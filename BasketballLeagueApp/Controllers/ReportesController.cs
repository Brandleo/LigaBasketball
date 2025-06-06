using Microsoft.AspNetCore.Mvc;
using BasketballLeagueApp.Models.ViewModels;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using BasketballLeagueApp.Models;
using System.Linq;
using DinkToPdf.Contracts;
using DinkToPdf;

namespace BasketballLeagueApp.Controllers
{
    public class ReportesController : Controller
    {
        private readonly LigaBaloncestoContext _context;

        private readonly IConverter _converter;

        public ReportesController(LigaBaloncestoContext context, IConverter converter)
        {
            _context = context;
            _converter = converter;
        }


        [HttpGet]
        public async Task<IActionResult> Clasificacion(bool esAdmin = false)
        {
        

            var viewModel = new ReporteClasificacionVM
            {
                TemporadasDisponibles = await _context.temporadas
                    .Select(t => new SelectListItem
                    {
                        Value = t.id.ToString(),
                        Text = t.nombre
                    }).ToListAsync(),
                Filas = new List<ReporteFilaClasificacion>()
            };

            return View(viewModel);
        }



        [HttpPost]
        public async Task<IActionResult> Clasificacion(ReporteClasificacionVM model, bool esAdmin = false)
        {
          
            var equiposIds = await _context.partidos
                .Where(p => p.temporada_id == model.TemporadaSeleccionada)
                .Select(p => p.equipo_local_id)
                .Union(_context.partidos
                    .Where(p => p.temporada_id == model.TemporadaSeleccionada)
                    .Select(p => p.equipo_visitante_id))
                .Distinct()
                .ToListAsync();

            var equipos = await _context.equipos
                .Where(e => equiposIds.Contains(e.id))
                .ToListAsync();

            var filas = new List<ReporteFilaClasificacion>();

            foreach (var equipo in equipos)
            {
                var partidos = await _context.partidos
                    .Where(p => p.temporada_id == model.TemporadaSeleccionada &&
                        (p.equipo_local_id == equipo.id || p.equipo_visitante_id == equipo.id))
                    .ToListAsync();

                int ganados = partidos.Count(p => p.ganador_id == equipo.id);
                int perdidos = partidos.Count - ganados;
                int puntosFavor = partidos.Sum(p =>
                    p.equipo_local_id == equipo.id ? p.puntos_local ?? 0 :
                    p.equipo_visitante_id == equipo.id ? p.puntos_visitante ?? 0 : 0);
                int puntosContra = partidos.Sum(p =>
                    p.equipo_local_id == equipo.id ? p.puntos_visitante ?? 0 :
                    p.equipo_visitante_id == equipo.id ? p.puntos_local ?? 0 : 0);

                filas.Add(new ReporteFilaClasificacion
                {
                    Equipo = equipo.nombre,
                    PartidosJugados = partidos.Count,
                    PartidosGanados = ganados,
                    PartidosPerdidos = perdidos,
                    PuntosFavor = puntosFavor,
                    PuntosContra = puntosContra,
                    Diferencia = puntosFavor - puntosContra,
                    PuntosTotales = ganados * 2 + perdidos * 1
                });
            }

            var viewModel = new ReporteClasificacionVM
            {
                TemporadasDisponibles = await _context.temporadas
                    .Select(t => new SelectListItem
                    {
                        Value = t.id.ToString(),
                        Text = t.nombre
                    }).ToListAsync(),
                TemporadaSeleccionada = model.TemporadaSeleccionada,
                Filas = filas
            };

            return View(viewModel);
        }



        [HttpPost]
        public async Task<IActionResult> GenerarPDF(int temporadaId)
        {
            // Reutiliza la lógica de tu método Clasificacion
            var equiposIds = await _context.partidos
                .Where(p => p.temporada_id == temporadaId)
                .Select(p => p.equipo_local_id)
                .Union(_context.partidos
                    .Where(p => p.temporada_id == temporadaId)
                    .Select(p => p.equipo_visitante_id))
                .Distinct()
                .ToListAsync();

            var equipos = await _context.equipos
                .Where(e => equiposIds.Contains(e.id))
                .ToListAsync();

            var filas = new List<ReporteFilaClasificacion>();

            foreach (var equipo in equipos)
            {
                var partidos = await _context.partidos
                    .Where(p => p.temporada_id == temporadaId &&
                                (p.equipo_local_id == equipo.id || p.equipo_visitante_id == equipo.id))
                    .ToListAsync();

                var ganados = partidos.Count(p => p.ganador_id == equipo.id);
                var perdidos = partidos.Count - ganados;
                var puntosFavor = partidos.Sum(p =>
                    p.equipo_local_id == equipo.id ? (p.puntos_local ?? 0) :
                    p.equipo_visitante_id == equipo.id ? (p.puntos_visitante ?? 0) : 0);
                var puntosContra = partidos.Sum(p =>
                    p.equipo_local_id == equipo.id ? (p.puntos_visitante ?? 0) :
                    p.equipo_visitante_id == equipo.id ? (p.puntos_local ?? 0) : 0);

                filas.Add(new ReporteFilaClasificacion
                {
                    Equipo = equipo.nombre,
                    PartidosJugados = partidos.Count,
                    PartidosGanados = ganados,
                    PartidosPerdidos = perdidos,
                    PuntosFavor = puntosFavor,
                    PuntosContra = puntosContra,
                    Diferencia = puntosFavor - puntosContra,
                    PuntosTotales = (ganados * 2) + (perdidos * 1)
                });
            }

            var html = await this.RenderViewAsync("ReporteClasificacionPDF", filas, true);

            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
            PaperSize = PaperKind.A4,
            Orientation = Orientation.Portrait
        },
                Objects = {
            new ObjectSettings {
                HtmlContent = html,
                WebSettings = { DefaultEncoding = "utf-8" }
            }
        }
            };

            var pdf = _converter.Convert(doc);
            return File(pdf, "application/pdf", $"Clasificacion_Temporada_{temporadaId}.pdf");
        }

        // Hasta aqui sirve


        [HttpGet]
        public async Task<IActionResult> EstadisticasJugadorPartido()
        {
            var viewModel = new ReporteEstadisticasJugadorPartidoVM
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
        public async Task<IActionResult> EstadisticasJugadorPartido(ReporteEstadisticasJugadorPartidoVM model)
        {
            var estadisticas = await _context.estadisticas_jugadores
                .Include(e => e.partido)
                    .ThenInclude(p => p.equipo_local)
                .Include(e => e.partido)
                    .ThenInclude(p => p.equipo_visitante)
                .Where(e => e.jugador_id == model.JugadorSeleccionado && e.partido.temporada_id == model.TemporadaSeleccionada)
                .OrderBy(e => e.partido.fecha)
                .ToListAsync();

            model.Resultados = estadisticas.Select(e => new EstadisticaJugadorResultado
            {
                Fecha = e.partido.fecha.ToString("dd/MM/yyyy"),
                Partido = $"{e.partido.equipo_local.nombre} vs {e.partido.equipo_visitante.nombre}",
                MinutosJugados = e.minutos_jugados,
                Puntos = e.puntos,
                Rebotes = e.rebotes,
                Asistencias = e.asistencias,
                Faltas = e.faltas
            }).ToList();

            model.Temporadas = await _context.temporadas
                .Select(t => new SelectListItem { Value = t.id.ToString(), Text = t.nombre })
                .ToListAsync();

            model.Equipos = await _context.equipos
                .Select(e => new SelectListItem { Value = e.id.ToString(), Text = e.nombre })
                .ToListAsync();

            model.Jugadores = await _context.jugadores
                .Where(j => j.equipo_id == model.EquipoSeleccionado)
                .Select(j => new SelectListItem
                {
                    Value = j.id.ToString(),
                    Text = j.nombre + " " + j.apellido
                }).ToListAsync();

            return View(model);
        }



        [HttpGet]
        public async Task<IActionResult> ObtenerEquiposPorTemporada(int temporadaId)
        {
            var equiposIds = await _context.partidos
                .Where(p => p.temporada_id == temporadaId)
                .Select(p => p.equipo_local_id)
                .Union(_context.partidos.Where(p => p.temporada_id == temporadaId).Select(p => p.equipo_visitante_id))
                .Distinct()
                .ToListAsync();

            var equipos = await _context.equipos
                .Where(e => equiposIds.Contains(e.id))
                .Select(e => new SelectListItem
                {
                    Value = e.id.ToString(),
                    Text = e.nombre
                }).ToListAsync();

            return Json(equipos);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerJugadoresPorEquipo(int equipoId)
        {
            var jugadores = await _context.jugadores
                .Where(j => j.equipo_id == equipoId && j.estado == "activo")
                .Select(j => new SelectListItem
                {
                    Value = j.id.ToString(),
                    Text = j.nombre + " " + j.apellido
                }).ToListAsync();

            return Json(jugadores);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerPartidosPorEquipoTemporada(int equipoId, int temporadaId)
        {
            var partidos = await _context.partidos
                .Include(p => p.equipo_local)
                .Include(p => p.equipo_visitante)
                .Where(p => p.temporada_id == temporadaId &&
                            (p.equipo_local_id == equipoId || p.equipo_visitante_id == equipoId))
                .Select(p => new SelectListItem
                {
                    Value = p.id.ToString(),
                    Text = p.fecha.ToString("dd/MM/yyyy") + " - " + p.equipo_local.nombre + " vs " + p.equipo_visitante.nombre
                }).ToListAsync();

            return Json(partidos);
        }


        [HttpGet]
        public async Task<IActionResult> ExportarEstadisticasJugadorPDF(int temporadaId, int jugadorId)
        {
            var jugador = await _context.jugadores.FindAsync(jugadorId);
            var temporada = await _context.temporadas.FindAsync(temporadaId);

            var estadisticas = await _context.estadisticas_jugadores
                .Include(e => e.partido)
                    .ThenInclude(p => p.equipo_local)
                .Include(e => e.partido)
                    .ThenInclude(p => p.equipo_visitante)
                .Where(e => e.jugador_id == jugadorId && e.partido.temporada_id == temporadaId)
                .OrderBy(e => e.partido.fecha)
                .ToListAsync();

            var lista = estadisticas.Select(e => new EstadisticaJugadorResultado
            {
                Fecha = e.partido.fecha.ToString("dd/MM/yyyy"),
                Partido = $"{e.partido.equipo_local.nombre} vs {e.partido.equipo_visitante.nombre}",
                MinutosJugados = e.minutos_jugados,
                Puntos = e.puntos,
                Rebotes = e.rebotes,
                Asistencias = e.asistencias,
                Faltas = e.faltas
            }).ToList();

            var viewModel = new EstadisticasJugadorPDFViewModel
            {
                NombreJugador = jugador?.nombre + " " + jugador?.apellido,
                TemporadaNombre = temporada?.nombre ?? "",
                Resultados = lista
            };

            var html = await this.RenderViewAsync("EstadisticasJugadorPDF", viewModel, true);

            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
            PaperSize = PaperKind.A4,
            Orientation = Orientation.Portrait,
            DocumentTitle = "Estadísticas del Jugador"
        },
                Objects = {
            new ObjectSettings {
                HtmlContent = html,
                WebSettings = { DefaultEncoding = "utf-8" }
            }
        }
            };

            var pdf = _converter.Convert(doc);
            return File(pdf, "application/pdf", $"Estadisticas_{viewModel.NombreJugador.Replace(" ", "_")}_T{temporadaId}.pdf");
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


        [HttpGet]
        public async Task<IActionResult> ResultadosPorJornada()
        {
            var model = new ReporteResultadosJornadaVM
            {
                Temporadas = await _context.temporadas
                    .Select(t => new SelectListItem
                    {
                        Value = t.id.ToString(),
                        Text = t.nombre
                    }).ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResultadosPorJornada(ReporteResultadosJornadaVM model)
        {
            var partidos = await _context.partidos
                .Include(p => p.equipo_local)
                .Include(p => p.equipo_visitante)
                .Include(p => p.ganador)
                .Where(p => p.jornada_id == model.JornadaSeleccionada)
                .ToListAsync();

            model.Partidos = partidos.Select(p => new PartidoResultadoVM
            {
                Equipos = $"{p.equipo_local.nombre} vs {p.equipo_visitante.nombre}",
                PuntosLocal = p.puntos_local,
                PuntosVisitante = p.puntos_visitante,
                Ganador = p.ganador != null ? p.ganador.nombre : "Empate / No definido"
            }).ToList();

            model.Temporadas = await _context.temporadas
                .Select(t => new SelectListItem
                {
                    Value = t.id.ToString(),
                    Text = t.nombre
                }).ToListAsync();

            model.Jornadas = await _context.jornadas
                .Where(j => j.temporada_id == model.TemporadaSeleccionada)
                .Select(j => new SelectListItem
                {
                    Value = j.id.ToString(),
                    Text = "Jornada " + j.numero_jornada
                }).ToListAsync();

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> ObtenerJornadasPorTemporada(int temporadaId)
        {
            var jornadas = await _context.jornadas
                .Where(j => j.temporada_id == temporadaId)
                .Select(j => new SelectListItem
                {
                    Value = j.id.ToString(),
                    Text = "Jornada " + j.numero_jornada
                }).ToListAsync();

            return Json(jornadas);
        }
        [HttpGet]
        public async Task<IActionResult> ExportarResultadosJornadaPDF(int temporadaId, int jornadaId)
        {
            var temporada = await _context.temporadas.FindAsync(temporadaId);
            var jornada = await _context.jornadas.FirstOrDefaultAsync(j => j.id == jornadaId);

            var partidos = await _context.partidos
                .Include(p => p.equipo_local)
                .Include(p => p.equipo_visitante)
                .Include(p => p.ganador)
                .Where(p => p.jornada_id == jornadaId)
                .ToListAsync();

            var lista = partidos.Select(p => new PartidoResultadoVM
            {
                Equipos = $"{p.equipo_local.nombre} vs {p.equipo_visitante.nombre}",
                PuntosLocal = p.puntos_local,
                PuntosVisitante = p.puntos_visitante,
                Ganador = p.ganador != null ? p.ganador.nombre : "Empate / N.D."
            }).ToList();

            var viewModel = new ResultadosJornadaPDFViewModel
            {
                TemporadaNombre = temporada?.nombre ?? "Desconocida",
                NumeroJornada = jornada?.numero_jornada ?? 0,
                Partidos = lista
            };

            var html = await this.RenderViewAsync("ResultadosJornadaPDF", viewModel, true);

            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
            PaperSize = PaperKind.A4,
            Orientation = Orientation.Portrait,
            DocumentTitle = "Resultados por Jornada"
        },
                Objects = {
            new ObjectSettings {
                HtmlContent = html,
                WebSettings = { DefaultEncoding = "utf-8" }
            }
        }
            };

            var pdf = _converter.Convert(doc);
            return File(pdf, "application/pdf", $"Resultados_Jornada_{viewModel.NumeroJornada}_{temporada?.nombre}.pdf");
        }
        [HttpGet]
        public async Task<IActionResult> JugadoresDestacados()
        {
            var model = new ReporteJugadoresDestacadosVM
            {
                Temporadas = await _context.temporadas
                    .Select(t => new SelectListItem
                    {
                        Value = t.id.ToString(),
                        Text = t.nombre
                    }).ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> JugadoresDestacados(ReporteJugadoresDestacadosVM model)
        {
            var estadisticas = await _context.estadisticas_jugadores
                .Include(e => e.jugador)
                    .ThenInclude(j => j.equipo)
                .Include(e => e.partido)
                .Where(e => e.partido.temporada_id == model.TemporadaSeleccionada)
                .ToListAsync();

            var destacados = estadisticas
                .GroupBy(e => e.jugador_id)
                .Select(g =>
                {
                    var jugador = g.First().jugador;
                    return new JugadorDestacadoVM
                    {
                        NombreJugador = jugador.nombre + " " + jugador.apellido,
                        Equipo = jugador.equipo?.nombre ?? "N/D",
                        TotalPuntos = g.Sum(x => x.puntos ?? 0),
                        TotalRebotes = g.Sum(x => x.rebotes ?? 0),
                        TotalAsistencias = g.Sum(x => x.asistencias ?? 0),
                        TotalFaltas = g.Sum(x => x.faltas ?? 0),
                        PromedioMinutos = g.Average(x => x.minutos_jugados ?? 0)
                    };
                })
                .OrderByDescending(j => j.TotalPuntos)
                .ToList();

            model.Temporadas = await _context.temporadas
                .Select(t => new SelectListItem
                {
                    Value = t.id.ToString(),
                    Text = t.nombre
                }).ToListAsync();

            model.Jugadores = destacados.Take(10).ToList();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ExportarJugadoresDestacadosPDF(int TemporadaSeleccionada)
        {
            var temporada = await _context.temporadas.FindAsync(TemporadaSeleccionada);

            var estadisticas = await _context.estadisticas_jugadores
                .Include(e => e.jugador).ThenInclude(j => j.equipo)
                .Include(e => e.partido)
                .Where(e => e.partido.temporada_id == TemporadaSeleccionada)
                .ToListAsync();

            var jugadores = estadisticas
                .GroupBy(e => new { e.jugador.id, e.jugador.nombre, e.jugador.apellido, Equipo = e.jugador.equipo.nombre })
                .Select(g => new JugadorDestacadoVM
                {
                    NombreJugador = g.Key.nombre + " " + g.Key.apellido,
                    Equipo = g.Key.Equipo,
                    TotalPuntos = g.Sum(x => x.puntos ?? 0),
                    TotalRebotes = g.Sum(x => x.rebotes ?? 0),
                    TotalAsistencias = g.Sum(x => x.asistencias ?? 0),
                    TotalFaltas = g.Sum(x => x.faltas ?? 0),
                    PromedioMinutos = g.Average(x => x.minutos_jugados ?? 0)
                })
                .OrderByDescending(j => j.TotalPuntos)
                .Take(10)
                .ToList();

            var viewModel = new ReporteJugadoresDestacadosVM
            {
                NombreTemporada = temporada?.nombre ?? "Desconocida",
                Jugadores = jugadores
            };

            var html = await this.RenderViewAsync("JugadoresDestacadosPDF", viewModel, true);

            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
            PaperSize = PaperKind.A4,
            Orientation = Orientation.Portrait,
            DocumentTitle = "Top 10 Jugadores Destacados"
        },
                Objects = {
            new ObjectSettings {
                HtmlContent = html,
                WebSettings = { DefaultEncoding = "utf-8" }
            }
        }
            };

            var pdf = _converter.Convert(doc);
            return File(pdf, "application/pdf", $"Top10_Jugadores_{temporada?.nombre}.pdf");
        }


    }
}
