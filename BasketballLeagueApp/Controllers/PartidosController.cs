using BasketballLeagueApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BasketballLeagueApp.Models.ViewModels;
using System.Linq;
using BasketballLeagueApp.Services;

namespace BasketballLeagueApp.Controllers
{
    public class PartidosController : Controller
    {
        private readonly LigaBaloncestoContext _context;

        public PartidosController(LigaBaloncestoContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var partidos = await _context.partidos
                .Include(p => p.equipo_local)
                .Include(p => p.equipo_visitante)
                .Include(p => p.temporada)
                .Where(p => p.estado == "Finalizado" && p.temporada != null)
                .OrderBy(p => p.fecha)
                .ThenBy(p => p.hora)
                .ToListAsync();

            return View(partidos);
        }


        [HttpGet]
        [Autentication]
        public async Task<IActionResult> ListarJornadas(int? temporadaId)
        {
            var temporadas = await _context.temporadas
                .OrderBy(t => t.anio_inicio)
                .ToListAsync();

            ViewBag.Temporadas = new SelectList(temporadas, "id", "nombre", temporadaId);
            ViewBag.TemporadaSeleccionada = temporadaId;

            List<jornadas> jornadas = new();

            if (temporadaId.HasValue)
            {
                jornadas = await _context.jornadas
                    .Where(j => j.temporada_id == temporadaId.Value)
                    .OrderBy(j => j.numero_jornada)
                    .ToListAsync();

                if (!jornadas.Any())
                    TempData["ErrorMessage"] = "No hay jornadas registradas para esta temporada.";
            }

            return View(jornadas);
        }

        [HttpGet]
        public async Task<IActionResult> VerPartidos(int jornadaId)
        {
            if (jornadaId <= 0)
            {
                TempData["ErrorMessage"] = "ID de jornada no válido.";
                return RedirectToAction("ListarJornadas", "Partidos");
            }

            var partidos = await _context.partidos
                .Include(p => p.equipo_local)
                .Include(p => p.equipo_visitante)
                .Include(p => p.jornada)
                .Where(p => p.jornada_id == jornadaId)
                .ToListAsync();

            if (!partidos.Any())
            {
                TempData["ErrorMessage"] = "No hay partidos registrados para esta jornada.";
                return RedirectToAction("ListarJornadas", "Partidos");
            }

            var jornada = await _context.jornadas.FindAsync(jornadaId);
            if (jornada == null)
            {
                TempData["ErrorMessage"] = "La jornada no existe.";
                return RedirectToAction("ListarJornadas", "Partidos");
            }

            var seriesPrincipales = await _context.playoffs
                .Where(p => p.partido_id == null && p.temporada_id == jornada.temporada_id)
                .ToListAsync();

            var seriesFinalizadas = new HashSet<int>();

            foreach (var serie in seriesPrincipales)
            {
                var partidosSerie = await _context.partidos
                    .Where(p =>
                        p.temporada_id == serie.temporada_id &&
                        p.es_playoff &&
                        (
                            (p.equipo_local_id == serie.equipo_a_id && p.equipo_visitante_id == serie.equipo_b_id) ||
                            (p.equipo_local_id == serie.equipo_b_id && p.equipo_visitante_id == serie.equipo_a_id)
                        ))
                    .ToListAsync();

                int victoriasA = partidosSerie.Count(p => p.ganador_id == serie.equipo_a_id);
                int victoriasB = partidosSerie.Count(p => p.ganador_id == serie.equipo_b_id);

                if (victoriasA >= 3 || victoriasB >= 3)
                {
                    int clasificadoId = (int)(victoriasA > victoriasB ? serie.equipo_a_id : serie.equipo_b_id);
                    serie.clasificado_id = clasificadoId;

                    foreach (var p in partidosSerie.Where(p => p.estado == "Programado"))
                    {
                        p.estado = "Finalizado";
                        seriesFinalizadas.Add(p.id);
                    }

                    var registrosPlayoffs = await _context.playoffs
                        .Where(pl => pl.temporada_id == serie.temporada_id && partidosSerie.Select(pp => pp.id).Contains(pl.partido_id ?? 0))
                        .ToListAsync();

                    foreach (var registro in registrosPlayoffs.Where(r => r.clasificado_id == null))
                    {
                        registro.clasificado_id = clasificadoId;
                    }

                    _context.playoffs.Update(serie);
                    await _context.SaveChangesAsync();
                }
            }

            ViewBag.Jornada = jornada;
            ViewBag.SeriesFinalizadas = seriesFinalizadas;

            return View(partidos);
        }







        public async Task<IActionResult> Resultados(int temporadaId = 1)
        {
            var jornadas = await _context.jornadas
      .Where(j => j.temporada_id == temporadaId)
      .OrderBy(j => j.numero_jornada)
      .ToListAsync();

            if (!jornadas.Any())
            {
                TempData["ErrorMessage"] = "No hay jornadas registradas para esta temporada.";
            }

            ViewBag.TemporadaId = temporadaId;
            return View(jornadas);
        }

        [HttpGet]
        public async Task<IActionResult> JugarPartido(int id)
        {
            var partido = await _context.partidos
                .Include(p => p.equipo_local)
                .Include(p => p.equipo_visitante)
                .FirstOrDefaultAsync(p => p.id == id);

            if (partido == null)
                return NotFound();

            var jugadoresLocal = await _context.jugadores
                .Where(j => j.equipo_id == partido.equipo_local_id && j.estado == "activo")
                .Select(j => new EstadisticaJugadorViewModel
                {
                    JugadorId = j.id,
                    NombreCompleto = j.nombre + " " + j.apellido
                }).ToListAsync();

            var jugadoresVisitante = await _context.jugadores
                .Where(j => j.equipo_id == partido.equipo_visitante_id && j.estado == "activo")
                .Select(j => new EstadisticaJugadorViewModel
                {
                    JugadorId = j.id,
                    NombreCompleto = j.nombre + " " + j.apellido
                }).ToListAsync();

            var vm = new JugarPartidoViewModel
            {
                PartidoId = partido.id,
                EquipoLocalNombre = partido.equipo_local.nombre,
                EquipoVisitanteNombre = partido.equipo_visitante.nombre,
                JugadoresLocal = jugadoresLocal,
                JugadoresVisitante = jugadoresVisitante
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> JugarPartido(JugarPartidoViewModel model)
        {
            var partido = await _context.partidos
                .Include(p => p.equipo_local)
                .Include(p => p.equipo_visitante)
                .FirstOrDefaultAsync(p => p.id == model.PartidoId);

            if (partido == null)
                return NotFound();

            // Bloquear si ya fue finalizado o cancelado
            if (partido.estado == "Finalizado" || partido.estado == "Cancelado")
            {
                TempData["ErrorMessage"] = "⚠ Este partido ya fue registrado anteriormente.";
                return RedirectToAction("ListarJornadas");
            }

            // Cancelación
            if (Request.Form["cancelar"] == "true")
            {
                partido.estado = "Cancelado";
                partido.puntos_local = 0;
                partido.puntos_visitante = 0;
                partido.ganador_id = null;

                var estadisticasCancelado = model.JugadoresLocal.Concat(model.JugadoresVisitante)
                    .Select(j => new estadisticas_jugadores
                    {
                        partido_id = partido.id,
                        jugador_id = j.JugadorId,
                        minutos_jugados = 0,
                        puntos = 0,
                        rebotes = 0,
                        asistencias = 0,
                        robos = 0,
                        bloqueos = 0,
                        perdidas = 0,
                        faltas = 0
                    }).ToList();

                _context.estadisticas_jugadores.AddRange(estadisticasCancelado);
                _context.partidos.Update(partido);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "⚠ Partido cancelado correctamente. Todos los valores fueron guardados como cero.";
                return RedirectToAction("ListarJornadas");
            }

            // Calcular puntos totales
            int totalLocal = model.JugadoresLocal.Sum(j => j.Puntos);
            int totalVisitante = model.JugadoresVisitante.Sum(j => j.Puntos);

            // Validar empate
            if (totalLocal == totalVisitante)
            {
                TempData["ErrorMessage"] = "⚠ No se permite empate. Uno de los equipos debe ganar.";

                var partidoCompleto = await _context.partidos
                    .Include(p => p.equipo_local)
                    .Include(p => p.equipo_visitante)
                    .FirstOrDefaultAsync(p => p.id == model.PartidoId);

                model.EquipoLocalNombre = partidoCompleto.equipo_local.nombre;
                model.EquipoVisitanteNombre = partidoCompleto.equipo_visitante.nombre;

                model.JugadoresLocal = await _context.jugadores
                    .Where(j => j.equipo_id == partidoCompleto.equipo_local_id && j.estado == "activo")
                    .Select(j => new EstadisticaJugadorViewModel
                    {
                        JugadorId = j.id,
                        NombreCompleto = j.nombre + " " + j.apellido
                    }).ToListAsync();

                model.JugadoresVisitante = await _context.jugadores
                    .Where(j => j.equipo_id == partidoCompleto.equipo_visitante_id && j.estado == "activo")
                    .Select(j => new EstadisticaJugadorViewModel
                    {
                        JugadorId = j.id,
                        NombreCompleto = j.nombre + " " + j.apellido
                    }).ToListAsync();

                return View(model);
            }

            // Validaciones extra por jugador
            if (model.JugadoresLocal.Any(j => j.Puntos > 40 || j.MinutosJugados > 48) ||
                model.JugadoresVisitante.Any(j => j.Puntos > 40 || j.MinutosJugados > 48))
            {
                TempData["ErrorMessage"] = "⚠ Un jugador excede los valores permitidos (máx 40 pts o 60 min).";
                return View(model);
            }

            int minutosLocal = model.JugadoresLocal.Sum(j => j.MinutosJugados);
            int minutosVisitante = model.JugadoresVisitante.Sum(j => j.MinutosJugados);

            if (totalLocal < 2 || totalLocal > 150 || totalVisitante < 2 || totalVisitante > 150 ||
                minutosLocal > 240 || minutosVisitante > 240)
            {
                TempData["ErrorMessage"] = "⚠ Los totales del equipo exceden los valores válidos.";
                return View(model);
            }

            // Guardar resultado
            partido.puntos_local = totalLocal;
            partido.puntos_visitante = totalVisitante;
            partido.estado = "Finalizado";
            partido.ganador_id = totalLocal > totalVisitante ? partido.equipo_local_id : partido.equipo_visitante_id;

            // Definir texto del ganador antes de usarlo más abajo
            string ganadorTexto = partido.ganador_id == partido.equipo_local_id
                ? partido.equipo_local?.nombre ?? "Equipo local"
                : partido.equipo_visitante?.nombre ?? "Equipo visitante";

            var estadisticas = new List<estadisticas_jugadores>();

            estadisticas.AddRange(model.JugadoresLocal.Select(j => new estadisticas_jugadores
            {
                partido_id = partido.id,
                jugador_id = j.JugadorId,
                minutos_jugados = j.MinutosJugados,
                puntos = j.Puntos,
                rebotes = j.Rebotes,
                asistencias = j.Asistencias,
                robos = j.Robos,
                bloqueos = j.Bloqueos,
                perdidas = j.Perdidas,
                faltas = j.Faltas
            }));

            estadisticas.AddRange(model.JugadoresVisitante.Select(j => new estadisticas_jugadores
            {
                partido_id = partido.id,
                jugador_id = j.JugadorId,
                minutos_jugados = j.MinutosJugados,
                puntos = j.Puntos,
                rebotes = j.Rebotes,
                asistencias = j.Asistencias,
                robos = j.Robos,
                bloqueos = j.Bloqueos,
                perdidas = j.Perdidas,
                faltas = j.Faltas
            }));

            _context.estadisticas_jugadores.AddRange(estadisticas);
            _context.partidos.Update(partido);
            await _context.SaveChangesAsync();

            // Actualizar clasificación general
            await _context.Database.ExecuteSqlRawAsync("EXEC sp_actualizar_clasificacion_por_partido {0}", partido.id);

            // 👑 Verificar si es playoff y actualizar serie
            if (partido.es_playoff)
            {
                var serie = await _context.playoffs.FirstOrDefaultAsync(p =>
                    p.temporada_id == partido.temporada_id &&
                    p.partido_id == null &&
                    (
                        (p.equipo_a_id == partido.equipo_local_id && p.equipo_b_id == partido.equipo_visitante_id) ||
                        (p.equipo_a_id == partido.equipo_visitante_id && p.equipo_b_id == partido.equipo_local_id)
                    ));

                if (serie != null && serie.clasificado_id == null)
                {
                    bool ganoA = partido.ganador_id == serie.equipo_a_id;
                    bool ganoB = partido.ganador_id == serie.equipo_b_id;

                    if (ganoA) serie.victorias_equipo_a++;
                    if (ganoB) serie.victorias_equipo_b++;

                    if (serie.victorias_equipo_a == 3)
                        serie.clasificado_id = serie.equipo_a_id;
                    else if (serie.victorias_equipo_b == 3)
                        serie.clasificado_id = serie.equipo_b_id;

                    _context.playoffs.Update(serie);
                    await _context.SaveChangesAsync();

                    // 🏆 Si esta serie es la final, actualizar el campeón de la temporada
                    if (serie.ronda == "Final" && serie.clasificado_id != null)
                    {
                        var temporada = await _context.temporadas.FindAsync(partido.temporada_id);
                        if (temporada != null)
                        {
                            temporada.campeon_id = serie.clasificado_id;
                            _context.temporadas.Update(temporada);
                            await _context.SaveChangesAsync();

                            TempData["SuccessMessage"] += $" 🏆 ¡{ganadorTexto} se ha coronado campeón de la temporada!";
                        }
                    }
                }
            }

            TempData["SuccessMessage"] = $"✅ Resultado guardado exitosamente. El equipo ganador es: {ganadorTexto}";

            return RedirectToAction("ListarJornadas");
        }


        [HttpGet]
        public async Task<IActionResult> EditarPartido(int id)
        {
            var partido = await _context.partidos
                .Include(p => p.equipo_local)
                .Include(p => p.equipo_visitante)
                .FirstOrDefaultAsync(p => p.id == id);

            if (partido == null)
                return NotFound();

            return View(partido);
        }

        [HttpPost]
        public async Task<IActionResult> EditarPartido(partidos partidoEditado)
        {
            var partido = await _context.partidos.FindAsync(partidoEditado.id);

            if (partido == null)
                return NotFound();

            // Cargar la jornada desde la base de datos
            var jornada = await _context.jornadas.FindAsync(partido.jornada_id);

            if (jornada == null)
                return NotFound();

            // Validar que la nueva fecha esté dentro del rango permitido
            if (partidoEditado.fecha.Date < jornada.fecha_inicio.Date || partidoEditado.fecha.Date > jornada.fecha_fin.Date)
            {
                ModelState.AddModelError("fecha", $"La fecha del partido debe estar entre {jornada.fecha_inicio:dd/MM/yyyy} y {jornada.fecha_fin:dd/MM/yyyy}.");

                // Recarga manual de equipos para que no explote la vista
                partidoEditado.equipo_local = await _context.equipos.FindAsync(partido.equipo_local_id);
                partidoEditado.equipo_visitante = await _context.equipos.FindAsync(partido.equipo_visitante_id);

                return View(partidoEditado);
            }

            partido.fecha = partidoEditado.fecha;
            partido.hora = partidoEditado.hora;

            if (partido.estado == "Cancelado")
                partido.estado = "Programado";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Partido actualizado correctamente.";
            return RedirectToAction("VerPartidos", new { jornadaId = partido.jornada_id });
        }

        [HttpGet]
        public async Task<IActionResult> CalendarioJornadas()
        {
            var viewModel = new ReporteCalendarioJornadaVM
            {
                Temporadas = await _context.temporadas
                    .Select(t => new SelectListItem
                    {
                        Value = t.id.ToString(),
                        Text = t.nombre
                    }).ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CalendarioJornadas(ReporteCalendarioJornadaVM model)
        {
            var jornadas = await _context.jornadas
                .Where(j => j.temporada_id == model.TemporadaSeleccionada)
                .OrderBy(j => j.numero_jornada)
                .ToListAsync();

            var lista = new List<JornadaConPartidos>();

            foreach (var jornada in jornadas)
            {
                var partidos = await _context.partidos
                    .Include(p => p.equipo_local)
                    .Include(p => p.equipo_visitante)
                    .Where(p => p.jornada_id == jornada.id)
                    .OrderBy(p => p.fecha)
                    .ToListAsync();

                lista.Add(new JornadaConPartidos
                {
                    NumeroJornada = jornada.numero_jornada,
                    FechaInicio = jornada.fecha_inicio,
                    FechaFin = jornada.fecha_fin,
                    Partidos = partidos.Select(p => new PartidoProgramado
                    {
                        Equipos = $"{p.equipo_local.nombre} vs {p.equipo_visitante.nombre}",
                        Fecha = p.fecha,
                        Hora = p.hora
                    }).ToList()
                });
            }

            model.Temporadas = await _context.temporadas
                .Select(t => new SelectListItem
                {
                    Value = t.id.ToString(),
                    Text = t.nombre
                }).ToListAsync();

            model.Jornadas = lista;

            return View(model);
        }




    }






}

