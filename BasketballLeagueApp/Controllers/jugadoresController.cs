using System;
using System.IO;
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
    public class jugadoresController : Controller
    {
        private readonly LigaBaloncestoContext _context;
        private readonly string _imgFolder = "imgJugadores";

        public jugadoresController(LigaBaloncestoContext context)
        {
            _context = context;
        }

        // INDEX
        public async Task<IActionResult> Index(string searchString, string equipoFiltro, string posicionFiltro, string estadoFiltro, string nacionalidadFiltro, int? page)
        {
            var jugadores = _context.jugadores.Include(j => j.equipo).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
                jugadores = jugadores.Where(j => j.nombre.Contains(searchString) || j.apellido.Contains(searchString));

            if (!string.IsNullOrEmpty(equipoFiltro))
                jugadores = jugadores.Where(j => j.equipo.nombre == equipoFiltro);

            if (!string.IsNullOrEmpty(posicionFiltro))
                jugadores = jugadores.Where(j => j.posicion.ToLower() == posicionFiltro.ToLower());

            if (!string.IsNullOrEmpty(estadoFiltro))
                jugadores = jugadores.Where(j => j.estado == estadoFiltro);

            if (!string.IsNullOrEmpty(nacionalidadFiltro))
                jugadores = jugadores.Where(j => j.nacionalidad == nacionalidadFiltro);

            ViewBag.Equipos = new SelectList(await _context.equipos.Select(e => e.nombre).Distinct().ToListAsync());
            ViewBag.Posiciones = new SelectList(new[] { "base", "escolta", "alero", "ala_pívot", "pívot" });
            ViewBag.Estados = new SelectList(new[] { "activo", "inactivo" });
            ViewBag.Nacionalidades = new SelectList(await _context.jugadores.Select(j => j.nacionalidad).Distinct().ToListAsync());

            int pageSize = 5;
            int pageNumber = page ?? 1;
            return View(jugadores.OrderBy(j => j.nombre).ToPagedList(pageNumber, pageSize));
        }

        // DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var jugador = await _context.jugadores.Include(j => j.equipo).FirstOrDefaultAsync(m => m.id == id);
            if (jugador == null) return NotFound();

            ViewBag.equipoImg = jugador.equipo?.img;
            return View(jugador);
        }

        // CREATE GET
        public IActionResult Create()
        {
            ViewBag.equipo = new SelectList(_context.equipos, "id", "nombre");
            return View();
        }

        // CREATE POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(jugadores jugador, IFormFile uploadFile)
        {
            ModelState.Remove("equipo");

            if (jugador.estatura < 1.5m || jugador.estatura > 2.5m)
                ModelState.AddModelError("estatura", "La estatura debe estar entre 1.5 y 2.5 metros.");

            if (string.IsNullOrWhiteSpace(jugador.nombre) || jugador.nombre.Length < 3 || jugador.nombre.Length > 100)
                ModelState.AddModelError("nombre", "El nombre debe tener entre 3 y 100 caracteres.");

            if (string.IsNullOrWhiteSpace(jugador.apellido) || jugador.apellido.Length < 3 || jugador.apellido.Length > 100)
                ModelState.AddModelError("apellido", "El apellido debe tener entre 3 y 100 caracteres.");

            var edadMinima = DateOnly.FromDateTime(DateTime.Now.AddYears(-19));
            if (jugador.fecha_nacimiento > edadMinima)
                ModelState.AddModelError("fecha_nacimiento", "El jugador debe tener al menos 19 años de edad.");

            if (_context.jugadores.Any(j => j.equipo_id == jugador.equipo_id && j.dorsal == jugador.dorsal))
                ModelState.AddModelError("dorsal", "Ya existe un jugador con ese dorsal en este equipo.");

            if (_context.jugadores.Any(j => j.nombre.ToLower().Trim() == jugador.nombre.ToLower().Trim()
                     && j.apellido.ToLower().Trim() == jugador.apellido.ToLower().Trim()))
                ModelState.AddModelError("", "Ya existe un jugador con el mismo nombre y apellido.");

            if (_context.jugadores.Count(j => j.equipo_id == jugador.equipo_id) >= 15)
                ModelState.AddModelError("equipo_id", "El equipo ya tiene 15 jugadores.");

            if (uploadFile != null && uploadFile.Length > 0)
            {
                string carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", _imgFolder);
                Directory.CreateDirectory(carpeta);

                string archivo = Guid.NewGuid() + Path.GetExtension(uploadFile.FileName);
                string ruta = Path.Combine(carpeta, archivo);

                using (var stream = new FileStream(ruta, FileMode.Create))
                    await uploadFile.CopyToAsync(stream);

                jugador.img = "/" + _imgFolder + "/" + archivo;
            }

            if (ModelState.IsValid)
            {
                _context.Add(jugador);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.equipo = new SelectList(_context.equipos, "id", "nombre", jugador.equipo_id);
            return View(jugador);
        }

        // EDIT GET
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var jugador = await _context.jugadores.Include(j => j.equipo).FirstOrDefaultAsync(j => j.id == id);
            if (jugador == null) return NotFound();

            var temporadaFinalizada = await _context.temporadas.OrderByDescending(t => t.anio_fin).FirstOrDefaultAsync();
            if (temporadaFinalizada == null || temporadaFinalizada.campeon_id == null)
            {
                TempData["ErrorMessage"] = "No se puede editar jugadores hasta que finalice la temporada (debe tener un campeón asignado).";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.equipo = new SelectList(_context.equipos, "id", "nombre", jugador.equipo_id);
            return View(jugador);
        }

        // EDIT POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, jugadores jugador, IFormFile? uploadFile)
        {
            if (id != jugador.id) return NotFound();

            var temporadaFinalizada = await _context.temporadas.OrderByDescending(t => t.anio_fin).FirstOrDefaultAsync();
            if (temporadaFinalizada == null || temporadaFinalizada.campeon_id == null)
            {
                TempData["ErrorMessage"] = "No se puede editar jugadores hasta que finalice la temporada (debe tener un campeón asignado).";
                return RedirectToAction(nameof(Index));
            }

            ModelState.Remove("equipo");

            var jugadorExistente = await _context.jugadores.FindAsync(id);
            if (jugadorExistente == null) return NotFound();

            if (jugador.nombre.Length < 3 || jugador.nombre.Length > 100)
                ModelState.AddModelError("nombre", "El nombre debe tener entre 3 y 100 caracteres.");

            if (jugador.apellido.Length < 3 || jugador.apellido.Length > 100)
                ModelState.AddModelError("apellido", "El apellido debe tener entre 3 y 100 caracteres.");

            if (_context.jugadores.Any(j => j.id != id && j.equipo_id == jugador.equipo_id && j.dorsal == jugador.dorsal))
                ModelState.AddModelError("dorsal", "Ya existe un jugador con ese dorsal en el equipo.");

            var edadMinima = DateOnly.FromDateTime(DateTime.Now.AddYears(-19));
            if (jugador.fecha_nacimiento > edadMinima)
                ModelState.AddModelError("fecha_nacimiento", "El jugador debe tener al menos 19 años de edad.");

            if (jugador.estatura < 1.5m || jugador.estatura > 2.5m)
                ModelState.AddModelError("estatura", "La estatura debe estar entre 1.5 y 2.5 metros.");

            if (uploadFile != null && uploadFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", _imgFolder);
                Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(uploadFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await uploadFile.CopyToAsync(stream);

                jugador.img = "/" + _imgFolder + "/" + fileName;
            }
            else
            {
                jugador.img = jugadorExistente.img;
            }

            if (ModelState.IsValid)
            {
                jugadorExistente.nombre = jugador.nombre;
                jugadorExistente.apellido = jugador.apellido;
                jugadorExistente.fecha_nacimiento = jugador.fecha_nacimiento;
                jugadorExistente.nacionalidad = jugador.nacionalidad;
                jugadorExistente.dorsal = jugador.dorsal;
                jugadorExistente.estatura = jugador.estatura;
                jugadorExistente.peso = jugador.peso;
                jugadorExistente.posicion = jugador.posicion;
                jugadorExistente.equipo_id = jugador.equipo_id;
                jugadorExistente.estado = jugador.estado;
                jugadorExistente.img = jugador.img;

                _context.Update(jugadorExistente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.equipo = new SelectList(_context.equipos, "id", "nombre", jugador.equipo_id);
            return View(jugador);
        }

        // FINALIZAR CONTRATO
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var jugador = await _context.jugadores.Include(j => j.equipo).FirstOrDefaultAsync(j => j.id == id);
            if (jugador == null) return NotFound();

            return View(jugador);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var jugador = await _context.jugadores.FindAsync(id);
            if (jugador == null) return NotFound();

            jugador.estado = "inactivo";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Contrato del jugador finalizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private bool jugadoresExists(int id)
        {
            return _context.jugadores.Any(e => e.id == id);
        }
    }
}
