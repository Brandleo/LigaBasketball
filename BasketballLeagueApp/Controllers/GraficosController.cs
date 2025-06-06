using BasketballLeagueApp.Models;
using BasketballLeagueApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BasketballLeagueApp.Controllers
{
    public class GraficosController : Controller
    {

        private readonly LigaBaloncestoContext _context;


        public GraficosController(LigaBaloncestoContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new GraficoComparacionEquiposVM
            {
                EstadisticasDisponibles = new List<SelectListItem>
            {
                new SelectListItem { Value = "puntos", Text = "Puntos" },
                new SelectListItem { Value = "rebotes", Text = "Rebotes" },
                new SelectListItem { Value = "asistencias", Text = "Asistencias" }
            },
                TemporadasDisponibles = await _context.temporadas
                    .Select(t => new SelectListItem
                    {
                        Value = t.id.ToString(),
                        Text = t.nombre
                    })
                    .ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerDatosComparacion(int temporadaId, string estadistica)
        {
            var equiposIds = await _context.partidos
     .Where(p => p.temporada_id == temporadaId)
     .Select(p => p.equipo_local_id)
     .Union(_context.partidos
         .Where(p => p.temporada_id == temporadaId)
         .Select(p => p.equipo_visitante_id))
     .Distinct()
     .ToListAsync();

            // Traer solo esos equipos
            var equipos = await _context.equipos
                .Where(e => equiposIds.Contains(e.id))
                .ToListAsync();
            var valores = new List<int>();

            foreach (var equipo in equipos)
            {
                var valor = await _context.estadisticas_jugadores
                    .Include(e => e.partido)
                    .Where(e => e.partido.temporada_id == temporadaId && e.jugador.equipo_id == equipo.id)
                    .SumAsync(e => EF.Property<int>(e, estadistica));

                valores.Add(valor);
            }

            return Json(new
            {
                etiquetas = equipos.Select(e => e.nombre).ToList(),
                datos = valores
            });
        }

        public async Task<IActionResult> DistribucionPuntos()
        {
            var viewModel = new GraficoDistribucionPuntosVM
            {
                TemporadasDisponibles = await _context.temporadas
                    .Select(t => new SelectListItem
                    {
                        Value = t.id.ToString(),
                        Text = t.nombre
                    })
                    .ToListAsync(),
                EquiposDisponibles = await _context.equipos
                    .Select(e => new SelectListItem
                    {
                        Value = e.id.ToString(),
                        Text = e.nombre
                    })
                    .ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerDistribucionPuntos(int temporadaId, int equipoId)
        {
            var jugadores = await _context.jugadores
                .Where(j => j.equipo_id == equipoId && j.estado == "activo")
                .ToListAsync();

            var etiquetas = new List<string>();
            var datos = new List<int>();

            foreach (var jugador in jugadores)
            {
                var puntos = await _context.estadisticas_jugadores
                    .Include(e => e.partido)
                    .Where(e => e.jugador_id == jugador.id && e.partido.temporada_id == temporadaId)
                    .SumAsync(e => e.puntos);

                etiquetas.Add(jugador.nombre + " " + jugador.apellido);
                datos.Add(puntos ?? 0);
            }

            return Json(new { etiquetas, datos });
        }


        public async Task<IActionResult> ResultadosPorJornada()
        {
            var viewModel = new GraficoResultadosPorJornadaVM
            {
                TemporadasDisponibles = await _context.temporadas
                    .Select(t => new SelectListItem
                    {
                        Value = t.id.ToString(),
                        Text = t.nombre
                    })
                    .ToListAsync(),

                EquiposDisponibles = await _context.equipos
                    .Select(e => new SelectListItem
                    {
                        Value = e.id.ToString(),
                        Text = e.nombre
                    })
                    .ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerResultadosPorJornada(int temporadaId, int equipoId)
        {
            var jornadas = await _context.jornadas
                .Where(j => j.temporada_id == temporadaId)
                .OrderBy(j => j.numero_jornada)
                .ToListAsync();

            var etiquetas = new List<string>();
            var datos = new List<int>();

            foreach (var jornada in jornadas)
            {
                var partido = await _context.partidos
                    .Where(p => p.jornada_id == jornada.id &&
                                p.temporada_id == temporadaId &&
                                (p.equipo_local_id == equipoId || p.equipo_visitante_id == equipoId))
                    .FirstOrDefaultAsync();

                if (partido != null)
                {
                    int puntos = (partido.equipo_local_id == equipoId)
    ? (partido.puntos_local ?? 0)
    : (partido.puntos_visitante ?? 0);


                    etiquetas.Add("Jornada " + jornada.numero_jornada);
                    datos.Add(puntos);
                }
                else
                {
                    etiquetas.Add("Jornada " + jornada.numero_jornada);
                    datos.Add(0); // No jugó esa jornada
                }
            }

            return Json(new { etiquetas, datos });
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerEquiposPorTemporada(int temporadaId)
        {
            var equipos = await _context.partidos
                .Where(p => p.temporada_id == temporadaId)
                .Select(p => p.equipo_local_id)
                .Union(_context.partidos
                    .Where(p => p.temporada_id == temporadaId)
                    .Select(p => p.equipo_visitante_id))
                .Distinct()
                .ToListAsync();

            var lista = await _context.equipos
                .Where(e => equipos.Contains(e.id))
                .Select(e => new SelectListItem
                {
                    Value = e.id.ToString(),
                    Text = e.nombre
                })
                .ToListAsync();

            return Json(lista);
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
                })
                .ToListAsync();

            return Json(jugadores);
        }

        public async Task<IActionResult> RendimientoJugador()
        {
            var viewModel = new GraficoRendimientoJugadorVM
            {
                TemporadasDisponibles = await _context.temporadas
                    .Select(t => new SelectListItem
                    {
                        Value = t.id.ToString(),
                        Text = t.nombre
                    })
                    .ToListAsync(),
                EquiposDisponibles = new List<SelectListItem>(),
                JugadoresDisponibles = new List<SelectListItem>()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerRendimientoJugador(int temporadaId, int jugadorId)
        {
            var suma = await _context.estadisticas_jugadores
                .Include(e => e.partido)
                .Where(e => e.jugador_id == jugadorId && e.partido.temporada_id == temporadaId)
                .GroupBy(e => e.jugador_id)
                .Select(g => new
                {
                    Puntos = g.Sum(e => e.puntos),
                    Rebotes = g.Sum(e => e.rebotes),
                    Asistencias = g.Sum(e => e.asistencias),
                    Faltas = g.Sum(e => e.faltas),
                    Minutos = g.Sum(e => e.minutos_jugados)
                })
                .FirstOrDefaultAsync();

            return Json(new
            {
                etiquetas = new[] { "Puntos", "Rebotes", "Asistencias", "Faltas", "Minutos" },
                datos = suma != null
    ? new[] {
        suma.Puntos ?? 0,
        suma.Rebotes ?? 0,
        suma.Asistencias ?? 0,
        suma.Faltas ?? 0,
        suma.Minutos ?? 0
    }
    : new[] { 0, 0, 0, 0, 0 }

            });
        }



        public async Task<IActionResult> ClasificacionEquipos()
        {
            var viewModel = new GraficoComparacionEquiposVM
            {
                TemporadasDisponibles = await _context.temporadas
                    .Select(t => new SelectListItem
                    {
                        Value = t.id.ToString(),
                        Text = t.nombre
                    })
                    .ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerClasificacionEquipos(int temporadaId)
        {
            // Obtener IDs de equipos que participaron en partidos de la temporada
            var equipoIds = await _context.partidos
                .Where(p => p.temporada_id == temporadaId)
                .Select(p => p.equipo_local_id)
                .Union(_context.partidos
                    .Where(p => p.temporada_id == temporadaId)
                    .Select(p => p.equipo_visitante_id))
                .Distinct()
                .ToListAsync();

            var equipos = await _context.equipos
                .Where(e => equipoIds.Contains(e.id))
                .ToListAsync();

            var etiquetas = new List<string>();
            var datos = new List<int>();

            foreach (var equipo in equipos)
            {
                var puntosLocal = await _context.partidos
                    .Where(p => p.temporada_id == temporadaId && p.equipo_local_id == equipo.id)
                    .SumAsync(p => p.puntos_local ?? 0);

                var puntosVisitante = await _context.partidos
                    .Where(p => p.temporada_id == temporadaId && p.equipo_visitante_id == equipo.id)
                    .SumAsync(p => p.puntos_visitante ?? 0);

                etiquetas.Add(equipo.nombre);
                datos.Add(puntosLocal + puntosVisitante);
            }

            return Json(new { etiquetas, datos });
        }

        public IActionResult Dashboard()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> ObtenerTemporadas()
        {
            var temporadas = await _context.temporadas
                .Select(t => new SelectListItem
                {
                    Value = t.id.ToString(),
                    Text = t.nombre
                })
                .ToListAsync();

            return Json(temporadas);
        }

    }
}
