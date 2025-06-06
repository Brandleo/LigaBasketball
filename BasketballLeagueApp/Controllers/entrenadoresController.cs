using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BasketballLeagueApp.Models;
using X.PagedList;
using X.PagedList.Extensions;


namespace BasketballLeagueApp.Controllers
{
    public class entrenadoresController : Controller
    {
        private readonly LigaBaloncestoContext _context;

        public entrenadoresController(LigaBaloncestoContext context)
        {
            _context = context;
        }

        // GET: entrenadores

        public async Task<IActionResult> Index(string searchString, string nacionalidadFiltro, int? page)
        {
            var entrenadores = _context.entrenadores
                .Include(e => e.historial_entrenadores)
                    .ThenInclude(h => h.equipo)
                .Include(e => e.historial_entrenadores)
                    .ThenInclude(h => h.temporada)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                entrenadores = entrenadores.Where(e =>
                    e.nombre.Contains(searchString) || e.apellido.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(nacionalidadFiltro))
            {
                entrenadores = entrenadores.Where(e => e.nacionalidad == nacionalidadFiltro);
            }

            ViewBag.Nacionalidades = new SelectList(await _context.entrenadores
                .Select(e => e.nacionalidad)
                .Distinct()
                .ToListAsync());

            int pageSize = 8;
            int pageNumber = page ?? 1;

            // Ordenar por estado del equipo (inactivos primero)
            var listaOrdenada = await entrenadores
                .ToListAsync();

            var entrenadoresOrdenados = listaOrdenada
                .OrderBy(e =>
                    e.historial_entrenadores.FirstOrDefault()?.equipo.estado == "Activo")
                .ThenBy(e => e.nombre) // orden alfabético secundario
                .ToPagedList(pageNumber, pageSize);

            return View(entrenadoresOrdenados);
        }


        // GET: entrenadores/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var entrenador = await _context.entrenadores
                .Include(e => e.historial_entrenadores)
                    .ThenInclude(h => h.equipo)
                .Include(e => e.historial_entrenadores)
                    .ThenInclude(h => h.temporada)
                .FirstOrDefaultAsync(e => e.id == id);

            if (entrenador == null)
                return NotFound();

            return View(entrenador);
        }


        // GET: entrenadores/Create
        public IActionResult Create()
        {
            var equiposConEntrenador = _context.historial_entrenadores
                .Select(h => h.equipo_id)
                .Distinct()
                .ToList();

            var equiposDisponibles = _context.equipos
                .Where(e => !equiposConEntrenador.Contains(e.id))
                .ToList();

            ViewBag.Equipos = new SelectList(equiposDisponibles, "id", "nombre");
            ViewBag.Temporadas = new SelectList(_context.temporadas.ToList(), "id", "nombre");

            return View();
        }


        // POST: entrenadores/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(entrenadores entrenador, int equipo_id, int temporada_id)
        {
            // Validaciones básicas
            if (entrenador.nombre.Length < 3)
                ModelState.AddModelError("nombre", "El nombre debe tener al menos 3 caracteres.");

            if (entrenador.apellido.Length < 3)
                ModelState.AddModelError("apellido", "El apellido debe tener al menos 3 caracteres.");

            // Validar duplicado de nombre, apellido y nacionalidad
            bool existeEntrenador = await _context.entrenadores
                .AnyAsync(e => e.nombre == entrenador.nombre &&
                               e.apellido == entrenador.apellido &&
                               e.nacionalidad == entrenador.nacionalidad);

            if (existeEntrenador)
                ModelState.AddModelError("", "Ya existe un entrenador con los mismos datos.");

            // Validar que el equipo no tenga entrenador en la temporada seleccionada
            bool equipoAsignado = await _context.historial_entrenadores
                .AnyAsync(h => h.equipo_id == equipo_id);

            if (equipoAsignado)
                ModelState.AddModelError("equipo_id", "Este equipo ya tiene un entrenador asignado.");

            // Validar que no exista un historial con ese equipo y temporada ya registrado
            bool historialRepetido = await _context.historial_entrenadores
                .AnyAsync(h => h.equipo_id == equipo_id && h.temporada_id == temporada_id);

            if (historialRepetido)
                ModelState.AddModelError("", "Ya existe un historial para este equipo en esta temporada.");

            if (ModelState.IsValid)
            {
                // Guardar el entrenador
                _context.entrenadores.Add(entrenador);
                await _context.SaveChangesAsync();

                // Crear historial del entrenador
                var historial = new historial_entrenadores
                {
                    entrenador_id = entrenador.id,
                    equipo_id = equipo_id,
                    temporada_id = temporada_id
                };

                _context.historial_entrenadores.Add(historial);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // Si hay errores, volver a cargar listas
            var equiposConEntrenador = _context.historial_entrenadores
                .Select(h => h.equipo_id)
                .Distinct()
                .ToList();

            var equiposDisponibles = _context.equipos
                .Where(e => !equiposConEntrenador.Contains(e.id))
                .ToList();

            ViewBag.Equipos = new SelectList(equiposDisponibles, "id", "nombre", equipo_id);
            ViewBag.Temporadas = new SelectList(_context.temporadas.ToList(), "id", "nombre", temporada_id);

            return View(entrenador);
        }



        // GET: entrenadores/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var entrenador = await _context.entrenadores
                .Include(e => e.historial_entrenadores)
                    .ThenInclude(h => h.equipo)
                .Include(e => e.historial_entrenadores)
                    .ThenInclude(h => h.temporada)
                .FirstOrDefaultAsync(e => e.id == id);

            if (entrenador == null) return NotFound();

            var historial = entrenador.historial_entrenadores.FirstOrDefault();

            if ((historial.equipo.estado == "activo") || (historial.equipo.estado == "Activo"))
            {
                TempData["ErrorMessage"] = "No se puede editar un entrenador si su equipo está activo.";
                return RedirectToAction(nameof(Index));
            }

            // Evita equipos ya asignados a otros entrenadores
            var equiposDisponibles = _context.equipos
                .Where(e => !_context.historial_entrenadores.Any(h => h.equipo_id == e.id && h.entrenador_id != entrenador.id))
                .ToList();

            ViewBag.Equipos = new SelectList(equiposDisponibles, "id", "nombre", historial?.equipo_id);
            ViewBag.Temporadas = new SelectList(_context.temporadas, "id", "nombre", historial?.temporada_id);

            return View(entrenador);
        }



        // POST: entrenadores/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, entrenadores entrenador, int equipo_id, int temporada_id)
        {
            if (id != entrenador.id) return NotFound();

            // Validación básica de campos
            if (entrenador.nombre.Length < 3)
                ModelState.AddModelError("nombre", "El nombre debe tener al menos 3 caracteres.");

            if (entrenador.apellido.Length < 3)
                ModelState.AddModelError("apellido", "El apellido debe tener al menos 3 caracteres.");

            // Obtener historial actual
            var historial = await _context.historial_entrenadores
                .Include(h => h.equipo)
                .FirstOrDefaultAsync(h => h.entrenador_id == id);

            if (historial == null)
            {
                ModelState.AddModelError("", "No se encontró historial asociado al entrenador.");
            }
            else if ((historial.equipo.estado == "activo" ) || (historial.equipo.estado == "Activo"))
            {
                TempData["ErrorMessage"] = "No se puede editar un entrenador asociado a un equipo activo.";
                return RedirectToAction(nameof(Index));
            }

            // Validar duplicado en equipo y temporada
            bool duplicado = await _context.historial_entrenadores
                .Include(h => h.entrenador)
                .AnyAsync(h =>
                    h.entrenador_id != id &&
                    h.equipo_id == equipo_id &&
                    h.temporada_id == temporada_id &&
                    h.entrenador.nombre == entrenador.nombre &&
                    h.entrenador.apellido == entrenador.apellido &&
                    h.entrenador.nacionalidad == entrenador.nacionalidad);

            if (duplicado)
            {
                ModelState.AddModelError("", "Ya existe un entrenador con los mismos datos en el mismo equipo y temporada.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Actualizar datos
                    _context.Update(entrenador);

                    if (historial != null)
                    {
                        historial.equipo_id = equipo_id;
                        historial.temporada_id = temporada_id;
                        _context.Update(historial);
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.entrenadores.Any(e => e.id == entrenador.id))
                        return NotFound();
                    else
                        throw;
                }
            }

            ViewBag.Equipos = new SelectList(_context.equipos, "id", "nombre", equipo_id);
            ViewBag.Temporadas = new SelectList(_context.temporadas, "id", "nombre", temporada_id);
            ViewData["EquipoId"] = equipo_id;
            ViewData["TemporadaId"] = temporada_id;

            return View(entrenador);
        }


        // *********************** MÉTODO AUXILIAR: VERIFICAR SI EXISTE EL ENTRENADOR ***********************
        private bool EntrenadorExists(int id)
        {
            return _context.entrenadores.Any(e => e.id == id);
        }

        // GET: entrenadores/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var entrenador = await _context.entrenadores
                .Include(e => e.historial_entrenadores)
                    .ThenInclude(h => h.equipo)
                .FirstOrDefaultAsync(e => e.id == id);

            var historial = entrenador?.historial_entrenadores.FirstOrDefault();
            if ((historial.equipo.estado == "activo") || (historial.equipo.estado == "Activo"))
                return RedirectToAction(nameof(Index));

            return View(entrenador);
        }

        //deleted confirmed
        // POST: entrenadores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entrenador = await _context.entrenadores
                .Include(e => e.historial_entrenadores)
                .FirstOrDefaultAsync(e => e.id == id);

            if (entrenador == null)
                return NotFound();

            // Eliminar historial relacionado
            _context.historial_entrenadores.RemoveRange(entrenador.historial_entrenadores);

            // Eliminar el entrenador
            _context.entrenadores.Remove(entrenador);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
