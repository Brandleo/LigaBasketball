using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BasketballLeagueApp.Models;

using BasketballLeagueApp.Models.ViewModels;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;


namespace BasketballLeagueApp.Controllers
{
    public class temporadasController : Controller
    {
        private readonly LigaBaloncestoContext _context;

        public temporadasController(LigaBaloncestoContext context)
        {
            _context = context;
        }

        // GET: temporadas
        public async Task<IActionResult> Index()
        {
            var ligaBaloncestoContext = _context.temporadas.Include(t => t.campeon);
            return View(await ligaBaloncestoContext.ToListAsync());
        }

        // GET: temporadas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var temporadas = await _context.temporadas
                .Include(t => t.campeon)
                .FirstOrDefaultAsync(m => m.id == id);
            if (temporadas == null)
            {
                return NotFound();
            }

            return View(temporadas);
        }

        // GET: temporadas/Create
        public IActionResult Create()
        {
            var equiposActivos = _context.equipos
                                         .Where(e => e.estado == "activo")
                                         .ToList();

            if (equiposActivos.Count < 18)
                ViewBag.MensajeAdvertencia = "Hay menos de 18 equipos activos disponibles.";

            var viewModel = new TemporadaCreateViewModel
            {
                EquiposActivos = equiposActivos // ← Asegura asignar aquí correctamente
            };

            return View(viewModel);
        }

        // POST: temporadas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TemporadaCreateViewModel model, DateTime fechaInicioCalendario)
        {
            foreach (var equipoId in model.EquiposSeleccionados)
            {
                var equipo = await _context.equipos
                    .Where(e => e.id == equipoId)
                    .Select(e => new { e.nombre })
                    .FirstOrDefaultAsync();

                int cantidadJugadores = await _context.jugadores
                    .CountAsync(j => j.equipo_id == equipoId);

                if (cantidadJugadores < 6)
                {
                    TempData["ErrorMessage"] = $"El equipo \"{equipo?.nombre ?? "Desconocido"}\" tiene solo {cantidadJugadores} jugadores. Cada equipo debe tener al menos 6.";
                    return RedirectToAction("Index");
                }
            }

            bool temporadaDuplicada = _context.temporadas
                .Any(t => t.anio_inicio == model.anioInicio && t.anio_fin == model.anioFin);

            if (temporadaDuplicada)
                ModelState.AddModelError("", "Ya existe una temporada con estos años.");

            if (model.EquiposSeleccionados == null || model.EquiposSeleccionados.Count != 18)
                ModelState.AddModelError("", "Debes seleccionar exactamente 18 equipos activos.");

            // VALIDACIÓN 1: Año de fecha de inicio del calendario debe coincidir con año_inicio
            if (fechaInicioCalendario.Year != model.anioInicio)
                ModelState.AddModelError("", "La fecha de inicio del calendario debe coincidir con el año de inicio de la temporada.");

            // VALIDACIÓN 2: Verificar si hay una temporada anterior sin campeón
            var ultimaTemporada = await _context.temporadas
                .OrderByDescending(t => t.anio_fin)
                .FirstOrDefaultAsync();

            if (ultimaTemporada != null)
            {
                // Si hay una temporada previa, debe tener campeón
                if (ultimaTemporada.campeon_id == null)
                    ModelState.AddModelError("", "No se puede crear una nueva temporada hasta que la anterior tenga un campeón.");

                // VALIDACIÓN 3: Validar continuidad entre temporadas (anio_inicio debe ser igual a anio_fin anterior)
                if (model.anioInicio != ultimaTemporada.anio_fin)
                {
                    ModelState.AddModelError("", $"La nueva temporada debe iniciar en el año {ultimaTemporada.anio_fin}.");
                }
            }

            if (!ModelState.IsValid)
            {
                model.EquiposActivos = _context.equipos.Where(e => e.estado == "activo").ToList();
                return View(model);
            }

            // Crear temporada
            var temporada = new temporadas
            {
                nombre = model.nombre,
                anio_inicio = (int)model.anioInicio,
                anio_fin = (int)model.anioFin
            };

            _context.temporadas.Add(temporada);
            await _context.SaveChangesAsync();

            foreach (var equipoId in model.EquiposSeleccionados)
            {
                _context.clasificacion_equipos.Add(new clasificacion_equipos
                {
                    temporada_id = temporada.id,
                    equipo_id = equipoId,
                    juegos_ganados = 0,
                    juegos_perdidos = 0,
                    porcentaje_victorias = 0,
                    diferencia_con_lider = 0,
                    local_ganados = 0,
                    local_perdidos = 0,
                    visitante_ganados = 0,
                    visitante_perdidos = 0,
                    ultimos10_ganados = 0,
                    ultimos10_perdidos = 0,
                    racha = "N"
                });
            }

            await _context.SaveChangesAsync();

            await GenerarCalendarioAutomatico(temporada.id, fechaInicioCalendario, model.EquiposSeleccionados);

            TempData["SuccessMessage"] = "Temporada creada y calendario generado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<RedirectToActionResult> GenerarCalendarioAutomatico(int temporadaId, DateTime fechaInicio, List<int> equiposSeleccionados)

        {
         


            if (equiposSeleccionados.Count != 18)
            {
                TempData["ErrorMessage"] = "La liga debe tener exactamente 18 equipos para generar el calendario.";
                return RedirectToAction("Index");
            }

            try
            {
                int totalJornadas = 34;
                int partidosPorJornada = equiposSeleccionados.Count / 2;
                var jornadas = new List<jornadas>();

                // Crear jornadas inicialmente
                for (int i = 1; i <= totalJornadas; i++)
                {
                    jornadas.Add(new jornadas
                    {
                        temporada_id = temporadaId,
                        numero_jornada = i,
                        fecha_inicio = fechaInicio.AddDays((i - 1) * 4),
                        fecha_fin = fechaInicio.AddDays((i - 1) * 4 + 1)
                    });
                }

                await _context.jornadas.AddRangeAsync(jornadas);
                await _context.SaveChangesAsync();

                var jornadasGeneradas = await _context.jornadas
                    .Where(j => j.temporada_id == temporadaId)
                    .OrderBy(j => j.numero_jornada)
                    .ToListAsync();

                var emparejamientosIda = GenerarRoundRobin(equiposSeleccionados);
                var emparejamientosVuelta = emparejamientosIda
    .SelectMany(jornada => jornada.Select(partido => (local: partido.visitante, visitante: partido.local)))
    .Chunk(partidosPorJornada)
    .Select(chunk => chunk.ToList())
    .ToList();

                // Juntar partidos de ida y vuelta
                var todasJornadas = emparejamientosIda.Concat(emparejamientosVuelta).ToList();

                // Registrar partidos en cada jornada
                for (int i = 0; i < totalJornadas; i++)
                {
                    foreach (var partido in todasJornadas[i])
                    {
                        _context.partidos.Add(new partidos
                        {
                            temporada_id = temporadaId,
                            jornada_id = jornadasGeneradas[i].id,
                            equipo_local_id = partido.local,
                            equipo_visitante_id = partido.visitante,
                            fecha = jornadasGeneradas[i].fecha_inicio,
                            hora = new TimeOnly(18, 00),
                            estado = "Programado",
                            es_playoff = false
                        });
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Calendario generado exitosamente.";
                return RedirectToAction("ListarJornadas", new { temporadaId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al generar el calendario: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // Función Round-Robin estándar
        private List<List<(int local, int visitante)>> GenerarRoundRobin(List<int> equipos)
        {
            var totalEquipos = equipos.Count;
            var totalRondas = totalEquipos - 1;
            var partidosPorRonda = totalEquipos / 2;

            var listaRondas = new List<List<(int local, int visitante)>>();

            for (int ronda = 0; ronda < totalRondas; ronda++)
            {
                var rondaActual = new List<(int local, int visitante)>();

                for (int partido = 0; partido < partidosPorRonda; partido++)
                {
                    int local = equipos[partido];
                    int visitante = equipos[totalEquipos - 1 - partido];

                    if (ronda % 2 == 0)
                        rondaActual.Add((local, visitante));
                    else
                        rondaActual.Add((visitante, local));
                }

                listaRondas.Add(rondaActual);

                // Rotar equipos excepto el primero
                var ultimoEquipo = equipos[totalEquipos - 1];
                equipos.RemoveAt(totalEquipos - 1);
                equipos.Insert(1, ultimoEquipo);
            }

            return listaRondas;
        }





        // GET: temporadas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var temporadas = await _context.temporadas.FindAsync(id);
            if (temporadas == null)
            {
                return NotFound();
            }
            ViewData["campeon_id"] = new SelectList(_context.equipos, "id", "id", temporadas.campeon_id);
            return View(temporadas);
        }

        // POST: temporadas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,nombre,anio_inicio,anio_fin,campeon_id")] temporadas temporadas)
        {
            if (id != temporadas.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(temporadas);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!temporadasExists(temporadas.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["campeon_id"] = new SelectList(_context.equipos, "id", "id", temporadas.campeon_id);
            return View(temporadas);
        }

        // GET: temporadas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var temporadas = await _context.temporadas
                .Include(t => t.campeon)
                .FirstOrDefaultAsync(m => m.id == id);
            if (temporadas == null)
            {
                return NotFound();
            }

            return View(temporadas);
        }

        // POST: temporadas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var temporadas = await _context.temporadas.FindAsync(id);
            if (temporadas != null)
            {
                _context.temporadas.Remove(temporadas);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool temporadasExists(int id)
        {
            return _context.temporadas.Any(e => e.id == id);
        }
        [HttpPost]
        public async Task<IActionResult> PlayOff(int temporadaId)
        {
            if (temporadaId <= 0)
            {
                TempData["ErrorMessage"] = "ID de temporada inválido.";
                return RedirectToAction("ListarJornadas", "Partidos");
            }

            bool temporadaTerminada = await _context.partidos
                .Where(p => p.temporada_id == temporadaId && !p.es_playoff)
                .AllAsync(p => p.estado == "Finalizado");

            if (!temporadaTerminada)
            {
                TempData["ErrorMessage"] = "La temporada regular aún no ha finalizado.";
                return RedirectToAction("ListarJornadas", "Partidos");
            }

            bool yaExistenSeries = await _context.playoffs
                .AnyAsync(p => p.temporada_id == temporadaId && p.ronda == "Cuartos" && p.partido_id == null);

            if (yaExistenSeries)
            {
                TempData["ErrorMessage"] = "Ya se generaron los Cuartos de Final.";
                return RedirectToAction("ListarJornadas", "Partidos");
            }

            var clasificados = await _context.clasificacion_equipos
                .Where(c => c.temporada_id == temporadaId)
                .OrderByDescending(c => c.porcentaje_victorias)
                .Take(8)
                .Select(c => c.equipo_id)
                .ToListAsync();

            if (clasificados.Count < 8)
            {
                TempData["ErrorMessage"] = "No hay suficientes equipos clasificados (mínimo 8).";
                return RedirectToAction("ListarJornadas", "Partidos");
            }

            var ultimaFecha = await _context.partidos
                .Where(p => p.temporada_id == temporadaId)
                .MaxAsync(p => p.fecha);

            var fechaInicio = ultimaFecha.AddDays(3);

            var jornada = await _context.jornadas
                .FirstOrDefaultAsync(j => j.temporada_id == temporadaId && j.numero_jornada == 100);

            if (jornada == null)
            {
                jornada = new jornadas
                {
                    temporada_id = temporadaId,
                    numero_jornada = 100,
                    fecha_inicio = fechaInicio,
                    fecha_fin = fechaInicio.AddDays(10)
                };
                _context.jornadas.Add(jornada);
                await _context.SaveChangesAsync();
            }

            var emparejamientos = new List<(int equipoA, int equipoB)>
    {
        (clasificados[0], clasificados[7]),
        (clasificados[1], clasificados[6]),
        (clasificados[2], clasificados[5]),
        (clasificados[3], clasificados[4])
    };

            foreach (var (equipoA, equipoB) in emparejamientos)
            {
                // Crear serie (cabecera)
                var serie = new playoffs
                {
                    temporada_id = temporadaId,
                    ronda = "Cuartos",
                    victorias_equipo_a = 0,
                    victorias_equipo_b = 0,
                    clasificado_id = null,
                    partido_id = null,
                    equipo_a_id = equipoA,
                    equipo_b_id = equipoB
                };


                _context.playoffs.Add(serie);
                await _context.SaveChangesAsync(); // Necesario para obtener el ID de la serie si luego lo necesitas

                for (int i = 0; i < 5; i++)
                {
                    var fecha = fechaInicio.AddDays(i * 2);
                    bool localEsA = (i % 2 == 0);

                    var partido = new partidos
                    {
                        temporada_id = temporadaId,
                        jornada_id = jornada.id,
                        equipo_local_id = localEsA ? equipoA : equipoB,
                        equipo_visitante_id = localEsA ? equipoB : equipoA,
                        fecha = fecha,
                        hora = new TimeOnly(18, 00),
                        estado = "Programado",
                        es_playoff = true
                    };

                    _context.partidos.Add(partido);
                    await _context.SaveChangesAsync();

                    // Aquí puedes guardar en otra tabla si estás usando una tabla intermedia de relación partido-serie,
                    // o puedes agregar un campo "playoff_id" en "partidos" si quieres una relación directa
                }
            }

            TempData["SuccessMessage"] = "Cuartos de Final generados correctamente.";
            return RedirectToAction("ListarJornadas", "Partidos");
        }
     
        [HttpPost]
        public async Task<IActionResult> SimularCuartos(int temporadaId)
        {
            var series = await _context.playoffs
                .Where(p => p.temporada_id == temporadaId && p.ronda == "Cuartos" && p.partido_id == null)
                .ToListAsync();

            if (!series.Any())
            {
                TempData["ErrorMessage"] = "No hay series de Cuartos para simular.";
                return RedirectToAction("ListarJornadas", "Partidos");
            }

            var rand = new Random();

            foreach (var serie in series)
            {
                int equipoA = (int)serie.equipo_a_id;
                int equipoB = (int)serie.equipo_b_id;

                var partidos = await _context.partidos
                    .Where(p => p.temporada_id == temporadaId &&
                                p.es_playoff &&
                                p.estado != "Finalizado" &&
                                (
                                    (p.equipo_local_id == equipoA && p.equipo_visitante_id == equipoB) ||
                                    (p.equipo_local_id == equipoB && p.equipo_visitante_id == equipoA)
                                ))
                    .OrderBy(p => p.fecha)
                    .ToListAsync();

                int victoriasA = 0;
                int victoriasB = 0;

                foreach (var partido in partidos)
                {
                    if (victoriasA >= 3 || victoriasB >= 3)
                        break;

                    int puntosLocal = rand.Next(70, 111);
                    int puntosVisitante = rand.Next(70, 111);

                    partido.estado = "Finalizado";
                    partido.puntos_local = puntosLocal;
                    partido.puntos_visitante = puntosVisitante;

                    bool localGana = puntosLocal > puntosVisitante;
                    int ganadorId = localGana ? partido.equipo_local_id : partido.equipo_visitante_id;
                    partido.ganador_id = ganadorId;

                    if (ganadorId == equipoA) victoriasA++;
                    else if (ganadorId == equipoB) victoriasB++;

                    _context.partidos.Update(partido);
                    await _context.SaveChangesAsync();

                    // Estadísticas por jugador
                    var jugadoresLocal = await _context.jugadores
                        .Where(j => j.equipo_id == partido.equipo_local_id && j.estado == "Activo")
                        .ToListAsync();

                    var jugadoresVisitante = await _context.jugadores
                        .Where(j => j.equipo_id == partido.equipo_visitante_id && j.estado == "Activo")
                        .ToListAsync();

                    var jugadoresTotales = jugadoresLocal.Concat(jugadoresVisitante).ToList();

                    foreach (var jugador in jugadoresTotales)
                    {
                        var estadistica = new estadisticas_jugadores
                        {
                            partido_id = partido.id,
                            jugador_id = jugador.id,
                            minutos_jugados = rand.Next(10, 36),
                            puntos = rand.Next(0, 21),
                            rebotes = rand.Next(0, 11),
                            asistencias = rand.Next(0, 8),
                            robos = rand.Next(0, 4),
                            bloqueos = rand.Next(0, 3),
                            perdidas = rand.Next(0, 4),
                            faltas = rand.Next(0, 5)
                        };

                        _context.estadisticas_jugadores.Add(estadistica);
                    }

                    await _context.SaveChangesAsync();
                }

                // Actualizar resultados de la serie
                if (victoriasA >= 3 || victoriasB >= 3)
                {
                    serie.victorias_equipo_a = victoriasA;
                    serie.victorias_equipo_b = victoriasB;
                    serie.clasificado_id = victoriasA > victoriasB ? equipoA : equipoB;

                    _context.playoffs.Update(serie);
                    await _context.SaveChangesAsync();
                }
            }

            TempData["SuccessMessage"] = "Simulación de Cuartos de Final completada.";
            return RedirectToAction("ListarJornadas", "Partidos");
        }


        [HttpPost]
        public async Task<IActionResult> GenerarSemifinales(int temporadaId)
        {
            if (temporadaId <= 0)
            {
                TempData["ErrorMessage"] = "ID de temporada inválido.";
                return RedirectToAction("ListarJornadas", "Partidos");
            }

            var semifinalesExistentes = await _context.playoffs
             .AnyAsync(p => p.temporada_id == temporadaId && p.ronda == "Semifinales" && p.partido_id == null);

            if (semifinalesExistentes)
            {
                TempData["ErrorMessage"] = "⚠ Ya se han generado las Semifinales para esta temporada.";
                return RedirectToAction("ListarJornadas", "Partidos");
            }

            // Obtener series de Cuartos (sin partido_id son las series principales)
            var cuartos = await _context.playoffs
                .Where(p => p.temporada_id == temporadaId && p.ronda == "Cuartos" && p.partido_id == null)
                .ToListAsync();

            if (cuartos.Count < 4 || cuartos.Any(p => p.clasificado_id == null))
            {
                TempData["ErrorMessage"] = "Aún hay series de Cuartos sin equipo clasificado.";
                return RedirectToAction("ListarJornadas", "Partidos");
            }

            // Obtener clasificados únicos
            var clasificados = cuartos
                .Select(p => p.clasificado_id!.Value)
                .Distinct()
                .ToList();

            if (clasificados.Count != 4)
            {
                TempData["ErrorMessage"] = "Error al obtener 4 clasificados únicos.";
                return RedirectToAction("ListarJornadas", "Partidos");
            }

            // Obtener fecha base
            var ultimaFecha = await _context.partidos
                .Where(p => p.temporada_id == temporadaId)
                .MaxAsync(p => p.fecha);
            var fechaInicio = ultimaFecha.AddDays(3);

            // Verificar o crear jornada 101 (Semifinales)
            var jornada = await _context.jornadas
                .FirstOrDefaultAsync(j => j.temporada_id == temporadaId && j.numero_jornada == 101);

            if (jornada == null)
            {
                jornada = new jornadas
                {
                    temporada_id = temporadaId,
                    numero_jornada = 101,
                    fecha_inicio = fechaInicio,
                    fecha_fin = fechaInicio.AddDays(10)
                };
                _context.jornadas.Add(jornada);
                await _context.SaveChangesAsync();
            }

            // Emparejar G1 vs G4 y G2 vs G3
            var emparejamientos = new List<(int equipoA, int equipoB)>
    {
        (clasificados[0], clasificados[3]),
        (clasificados[1], clasificados[2])
    };

            foreach (var (equipoA, equipoB) in emparejamientos)
            {
                if (equipoA == equipoB)
                    continue;

                // Insertar la serie principal
                var serie = new playoffs
                {
                    temporada_id = temporadaId,
                    ronda = "Semifinales",
                    victorias_equipo_a = 0,
                    victorias_equipo_b = 0,
                    clasificado_id = null,
                    partido_id = null,
                    equipo_a_id = equipoA,
                    equipo_b_id = equipoB
                };

                _context.playoffs.Add(serie);
                await _context.SaveChangesAsync();

                // Insertar 5 partidos programados
                for (int i = 0; i < 5; i++)
                {
                    var fecha = fechaInicio.AddDays(i * 2);
                    bool localEsA = (i % 2 == 0);

                    var partido = new partidos
                    {
                        temporada_id = temporadaId,
                        jornada_id = jornada.id,
                        equipo_local_id = localEsA ? equipoA : equipoB,
                        equipo_visitante_id = localEsA ? equipoB : equipoA,
                        fecha = fecha,
                        hora = new TimeOnly(18, 00),
                        estado = "Programado",
                        es_playoff = true
                    };

                    _context.partidos.Add(partido);
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Semifinales generadas correctamente.";
            return RedirectToAction("ListarJornadas", "Partidos");
        }



        [HttpPost]
        public async Task<IActionResult> GenerarFinal(int temporadaId)
        {
            if (temporadaId <= 0)
            {
                TempData["ErrorMessage"] = "ID de temporada inválido.";
                return RedirectToAction("ListarJornadas", "Partidos");
            }

            // Verificar si ya existe la serie de la Final
            var serieExistente = await _context.playoffs
                .FirstOrDefaultAsync(p => p.temporada_id == temporadaId && p.ronda == "Final" && p.partido_id == null);

            if (serieExistente != null)
            {
                TempData["ErrorMessage"] = "⚠ Ya se ha generado la Final para esta temporada.";
                return RedirectToAction("ListarJornadas", "Partidos");
            }


            // Verificar si todas las series de Semifinales tienen clasificado
            var semifinales = await _context.playoffs
                .Where(p => p.temporada_id == temporadaId && p.ronda == "Semifinales" && p.partido_id == null)
                .ToListAsync();

            if (semifinales.Count < 2 || semifinales.Any(p => p.clasificado_id == null))
            {
                TempData["ErrorMessage"] = "Aún hay series de Semifinales sin equipo clasificado.";
                return RedirectToAction("ListarJornadas", "Partidos");
            }

            // Obtener los 2 finalistas
            var finalistas = semifinales
                .OrderBy(p => p.id)
                .Select(p => p.clasificado_id!.Value)
                .ToList();

            if (finalistas.Count != 2)
            {
                TempData["ErrorMessage"] = "No se pudo determinar a los dos finalistas.";
                return RedirectToAction("ListarJornadas", "Partidos");
            }

            // Calcular fecha base para la Final
            var ultimaFecha = await _context.partidos
                .Where(p => p.temporada_id == temporadaId)
                .MaxAsync(p => p.fecha);
            var fechaInicio = ultimaFecha.AddDays(3);

            // Verificar o crear la jornada 102
            var jornada = await _context.jornadas
                .FirstOrDefaultAsync(j => j.temporada_id == temporadaId && j.numero_jornada == 102);

            if (jornada == null)
            {
                jornada = new jornadas
                {
                    temporada_id = temporadaId,
                    numero_jornada = 102,
                    fecha_inicio = fechaInicio,
                    fecha_fin = fechaInicio.AddDays(10)
                };
                _context.jornadas.Add(jornada);
                await _context.SaveChangesAsync();
            }

          
            

            // Crear serie principal de la Final
            var serieFinal = new playoffs
            {
                temporada_id = temporadaId,
                ronda = "Final",
                victorias_equipo_a = 0,
                victorias_equipo_b = 0,
                clasificado_id = null,
                partido_id = null,
                equipo_a_id = finalistas[0],
                equipo_b_id = finalistas[1]
            };
            _context.playoffs.Add(serieFinal);
            await _context.SaveChangesAsync();

            // Crear 5 partidos (serie al mejor de 5)
            for (int i = 0; i < 5; i++)
            {
                var fecha = fechaInicio.AddDays(i * 2);
                bool localEsA = (i % 2 == 0);

                var partido = new partidos
                {
                    temporada_id = temporadaId,
                    jornada_id = jornada.id,
                    equipo_local_id = localEsA ? finalistas[0] : finalistas[1],
                    equipo_visitante_id = localEsA ? finalistas[1] : finalistas[0],
                    fecha = fecha,
                    hora = new TimeOnly(18, 00),
                    estado = "Programado",
                    es_playoff = true
                };

                _context.partidos.Add(partido);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Final generada correctamente.";
            return RedirectToAction("ListarJornadas", "Partidos");
        }




        [HttpPost]
        public async Task<IActionResult> SimularPartidosTemporada(int temporadaId)
        {
            var partidos = await _context.partidos
                .Where(p => p.temporada_id == temporadaId && p.estado == "Programado")
                .ToListAsync();

            var jugadoresPorEquipo = await _context.jugadores
                .Where(j => _context.clasificacion_equipos
                    .Any(c => c.temporada_id == temporadaId && c.equipo_id == j.equipo_id))
                .GroupBy(j => j.equipo_id)
                .ToDictionaryAsync(g => g.Key, g => g.ToList());

            var rand = new Random();

            foreach (var partido in partidos)
            {
                int puntosLocal = rand.Next(70, 120);
                int puntosVisitante = rand.Next(70, 120);

                partido.estado = "Finalizado";
                partido.puntos_local = puntosLocal;
                partido.puntos_visitante = puntosVisitante;
                partido.ganador_id = puntosLocal > puntosVisitante ? partido.equipo_local_id : partido.equipo_visitante_id;

                var jugadoresLocales = jugadoresPorEquipo.ContainsKey(partido.equipo_local_id)
                    ? jugadoresPorEquipo[partido.equipo_local_id]
                    : new List<jugadores>();

                var jugadoresVisitantes = jugadoresPorEquipo.ContainsKey(partido.equipo_visitante_id)
                    ? jugadoresPorEquipo[partido.equipo_visitante_id]
                    : new List<jugadores>();

                foreach (var jugador in jugadoresLocales)
                {
                    _context.estadisticas_jugadores.Add(new estadisticas_jugadores
                    {
                        partido_id = partido.id,
                        jugador_id = jugador.id,
                        puntos = rand.Next(0, 25),
                        asistencias = rand.Next(0, 7),
                        rebotes = rand.Next(0, 10),
                        minutos_jugados = rand.Next(15, 35),
                        robos = rand.Next(0, 5),
                        bloqueos = rand.Next(0, 3),
                        perdidas = rand.Next(0, 4),
                        faltas = rand.Next(0, 5)
                    });
                }

                foreach (var jugador in jugadoresVisitantes)
                {
                    _context.estadisticas_jugadores.Add(new estadisticas_jugadores
                    {
                        partido_id = partido.id,
                        jugador_id = jugador.id,
                        puntos = rand.Next(0, 25),
                        asistencias = rand.Next(0, 7),
                        rebotes = rand.Next(0, 10),
                        minutos_jugados = rand.Next(15, 35),
                        robos = rand.Next(0, 5),
                        bloqueos = rand.Next(0, 3),
                        perdidas = rand.Next(0, 4),
                        faltas = rand.Next(0, 5)
                    });
                }
            }

            await _context.SaveChangesAsync();

            // 🔄 Actualizar clasificación
            var clasificaciones = await _context.clasificacion_equipos
                .Where(c => c.temporada_id == temporadaId)
                .ToListAsync();

            foreach (var clasificacion in clasificaciones)
            {
                var equipoId = clasificacion.equipo_id;

                var partidosJugados = await _context.partidos
                    .Where(p => p.temporada_id == temporadaId &&
                                p.estado == "Finalizado" &&
                                (p.equipo_local_id == equipoId || p.equipo_visitante_id == equipoId))
                    .OrderByDescending(p => p.fecha)
                    .ToListAsync();

                int ganados = 0, perdidos = 0;
                int localGanados = 0, localPerdidos = 0;
                int visitanteGanados = 0, visitantePerdidos = 0;

                foreach (var partido in partidosJugados)
                {
                    bool esLocal = partido.equipo_local_id == equipoId;
                    bool gano = partido.ganador_id == equipoId;

                    if (gano)
                    {
                        ganados++;
                        if (esLocal) localGanados++;
                        else visitanteGanados++;
                    }
                    else
                    {
                        perdidos++;
                        if (esLocal) localPerdidos++;
                        else visitantePerdidos++;
                    }
                }

                int totalJugados = ganados + perdidos;
                clasificacion.juegos_ganados = ganados;
                clasificacion.juegos_perdidos = perdidos;
                clasificacion.local_ganados = localGanados;
                clasificacion.local_perdidos = localPerdidos;
                clasificacion.visitante_ganados = visitanteGanados;
                clasificacion.visitante_perdidos = visitantePerdidos;
                clasificacion.porcentaje_victorias = totalJugados > 0
                    ? Math.Round((decimal)ganados / totalJugados * 100, 1)
                    : 0;

                // Últimos 10 partidos
                var ultimos10 = partidosJugados.Take(10);
                int ultGanados = ultimos10.Count(p => p.ganador_id == equipoId);
                int ultPerdidos = ultimos10.Count() - ultGanados;

                clasificacion.ultimos10_ganados = ultGanados;
                clasificacion.ultimos10_perdidos = ultPerdidos;

                // Racha
                string? rachaTipo = null;
                int rachaCantidad = 0;

                foreach (var partido in partidosJugados)
                {
                    bool gano = partido.ganador_id == equipoId;

                    if (rachaTipo == null)
                    {
                        rachaTipo = gano ? "W" : "L";
                        rachaCantidad = 1;
                    }
                    else if ((gano && rachaTipo == "W") || (!gano && rachaTipo == "L"))
                    {
                        rachaCantidad++;
                    }
                    else break;
                }

                clasificacion.racha = $"{rachaTipo}{rachaCantidad}";
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Todos los partidos han sido simulados con resultados, estadísticas y clasificación actualizada.";
            return RedirectToAction("ListarJornadas", "Partidos");
        }




    }
}
