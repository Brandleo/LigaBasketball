using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BasketballLeagueApp.Models;
using BasketballLeagueApp.Models.ViewModels;

namespace BasketballLeagueApp.Controllers
{
    public class estadisticas_jugadoresController : Controller
    {
        private readonly LigaBaloncestoContext _context;

        public estadisticas_jugadoresController(LigaBaloncestoContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index(int? temporadaId, int? equipoId)
        {
            ViewBag.SeHaBuscado = false;

            ViewBag.Temporadas = new SelectList(
                await _context.temporadas
                    .OrderByDescending(t => t.anio_inicio)
                    .ToListAsync(), "id", "nombre"
            );

            if (temporadaId != null)
            {
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

                ViewBag.Equipos = new SelectList(equipos, "id", "nombre");
            }
            else
            {
                ViewBag.Equipos = new SelectList(Enumerable.Empty<SelectListItem>());
            }

            if (temporadaId == null || equipoId == null)
                return View(new List<EstadisticaJugadorResultadoVM>());

            ViewBag.SeHaBuscado = true;

            // Obtener jugadores activos del equipo
            var jugadores = await _context.jugadores
                .Where(j => j.equipo_id == equipoId && j.estado == "activo")
                .Select(j => new { j.id, j.nombre, j.apellido })
                .ToListAsync();

            var estadisticas = new List<EstadisticaJugadorResultadoVM>();

            foreach (var jugador in jugadores)
            {
                var suma = await _context.estadisticas_jugadores
                    .Include(e => e.partido)
                    .Where(e => e.jugador_id == jugador.id && e.partido.temporada_id == temporadaId)
                    .GroupBy(e => e.jugador_id)
                    .Select(g => new
                    {
                        Minutos = g.Sum(x => x.minutos_jugados),
                        Puntos = g.Sum(x => x.puntos),
                        Rebotes = g.Sum(x => x.rebotes),
                        Asistencias = g.Sum(x => x.asistencias),
                        Robos = g.Sum(x => x.robos),
                        Bloqueos = g.Sum(x => x.bloqueos),
                        Faltas = g.Sum(x => x.faltas)
                    })
                    .FirstOrDefaultAsync();

                if (suma != null)
                {
                    estadisticas.Add(new EstadisticaJugadorResultadoVM
                    {
                        NombreJugador = $"{jugador.nombre} {jugador.apellido}",
                        Equipo = (await _context.equipos.FindAsync(equipoId))?.nombre ?? "",
                        Minutos = suma.Minutos,
                        Puntos = suma.Puntos,
                        Rebotes = suma.Rebotes,
                        Asistencias = suma.Asistencias,
                        Robos = suma.Robos,
                        Bloqueos = suma.Bloqueos,
                        Faltas = suma.Faltas
                    });
                }
            }

            return View(estadisticas);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerEquiposPorTemporada(int temporadaId)
        {
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
                .Select(e => new { e.id, e.nombre })
                .ToListAsync();

            return Json(equipos);
        }




        // GET: estadisticas_jugadores/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var estadisticas_jugadores = await _context.estadisticas_jugadores
                .Include(e => e.jugador)
                .Include(e => e.partido)
                .FirstOrDefaultAsync(m => m.id == id);
            if (estadisticas_jugadores == null)
            {
                return NotFound();
            }

            return View(estadisticas_jugadores);
        }

        // GET: estadisticas_jugadores/Create
        public IActionResult Create()
        {
            ViewData["jugador_id"] = new SelectList(_context.jugadores, "id", "id");
            ViewData["partido_id"] = new SelectList(_context.partidos, "id", "id");
            return View();
        }

        // POST: estadisticas_jugadores/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,partido_id,jugador_id,minutos_jugados,puntos,rebotes,asistencias,robos,bloqueos,perdidas,faltas")] estadisticas_jugadores estadisticas_jugadores)
        {
            if (ModelState.IsValid)
            {
                _context.Add(estadisticas_jugadores);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["jugador_id"] = new SelectList(_context.jugadores, "id", "id", estadisticas_jugadores.jugador_id);
            ViewData["partido_id"] = new SelectList(_context.partidos, "id", "id", estadisticas_jugadores.partido_id);
            return View(estadisticas_jugadores);
        }

        // GET: estadisticas_jugadores/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var estadisticas_jugadores = await _context.estadisticas_jugadores.FindAsync(id);
            if (estadisticas_jugadores == null)
            {
                return NotFound();
            }
            ViewData["jugador_id"] = new SelectList(_context.jugadores, "id", "id", estadisticas_jugadores.jugador_id);
            ViewData["partido_id"] = new SelectList(_context.partidos, "id", "id", estadisticas_jugadores.partido_id);
            return View(estadisticas_jugadores);
        }

        // POST: estadisticas_jugadores/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,partido_id,jugador_id,minutos_jugados,puntos,rebotes,asistencias,robos,bloqueos,perdidas,faltas")] estadisticas_jugadores estadisticas_jugadores)
        {
            if (id != estadisticas_jugadores.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(estadisticas_jugadores);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!estadisticas_jugadoresExists(estadisticas_jugadores.id))
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
            ViewData["jugador_id"] = new SelectList(_context.jugadores, "id", "id", estadisticas_jugadores.jugador_id);
            ViewData["partido_id"] = new SelectList(_context.partidos, "id", "id", estadisticas_jugadores.partido_id);
            return View(estadisticas_jugadores);
        }

        // GET: estadisticas_jugadores/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var estadisticas_jugadores = await _context.estadisticas_jugadores
                .Include(e => e.jugador)
                .Include(e => e.partido)
                .FirstOrDefaultAsync(m => m.id == id);
            if (estadisticas_jugadores == null)
            {
                return NotFound();
            }

            return View(estadisticas_jugadores);
        }

        // POST: estadisticas_jugadores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var estadisticas_jugadores = await _context.estadisticas_jugadores.FindAsync(id);
            if (estadisticas_jugadores != null)
            {
                _context.estadisticas_jugadores.Remove(estadisticas_jugadores);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool estadisticas_jugadoresExists(int id)
        {
            return _context.estadisticas_jugadores.Any(e => e.id == id);
        }
    }
}
