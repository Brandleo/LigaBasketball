using BasketballLeagueApp.Models;
using BasketballLeagueApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BasketballLeagueApp.Controllers
{
    public class ClasificacionEquiposController : Controller
    {
        private readonly LigaBaloncestoContext _context;

        public ClasificacionEquiposController(LigaBaloncestoContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index(int? temporadaId, string orden = "porcentaje_desc")
        {
            ViewBag.Temporadas = new SelectList(await _context.temporadas.ToListAsync(), "id", "nombre", temporadaId);

            if (!temporadaId.HasValue)
                return View(new List<ClasificacionEquipoViewModel>());

            var clasificaciones = await (
                from ce in _context.clasificacion_equipos
                join e in _context.equipos on ce.equipo_id equals e.id
                where ce.temporada_id == temporadaId
                select new ClasificacionEquipoViewModel
                {
                    EquipoNombre = e.nombre,
                    JuegosGanados = ce.juegos_ganados ?? 0,
                    JuegosPerdidos = ce.juegos_perdidos ?? 0,
                    PorcentajeVictorias = ce.porcentaje_victorias ?? 0,
                    DiferenciaConLider = ce.diferencia_con_lider ?? 0,
                    LocalGanados = ce.local_ganados ?? 0,
                    LocalPerdidos = ce.local_perdidos ?? 0,
                    VisitanteGanados = ce.visitante_ganados ?? 0,
                    VisitantePerdidos = ce.visitante_perdidos ?? 0,
                    Ultimos10Ganados = ce.ultimos10_ganados ?? 0,
                    Ultimos10Perdidos = ce.ultimos10_perdidos ?? 0,
                    Racha = ce.racha
                }).ToListAsync();

            // Ordenamiento opcional
            clasificaciones = orden switch
            {
                "ganados_desc" => clasificaciones.OrderByDescending(c => c.JuegosGanados).ToList(),
                "ganados_asc" => clasificaciones.OrderBy(c => c.JuegosGanados).ToList(),
                "porcentaje_asc" => clasificaciones.OrderBy(c => c.PorcentajeVictorias).ToList(),
                _ => clasificaciones.OrderByDescending(c => c.PorcentajeVictorias).ToList()
            };

            ViewBag.Orden = orden;
            return View(clasificaciones);
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


    }
}
