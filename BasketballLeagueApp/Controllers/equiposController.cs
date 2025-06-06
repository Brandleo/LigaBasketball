using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BasketballLeagueApp.Models;
using BasketballLeagueApp.enums;
using BasketballLeagueApp.Services;

namespace BasketballLeagueApp.Controllers
{
    public class equiposController : Controller
    {
        private readonly LigaBaloncestoContext _context;

        public equiposController(LigaBaloncestoContext context)
        {
            _context = context;
        }

        // GET: equipos
        [Autentication]


        public async Task<IActionResult> Index(string nombre, string sede, string estado)
        {
            var temporadaActivaSinCampeon = await _context.temporadas
                .OrderByDescending(t => t.anio_fin)
                .FirstOrDefaultAsync(t => t.campeon_id == null);

            ViewBag.BloquearEdicion = temporadaActivaSinCampeon != null;

            var query = _context.equipos.AsQueryable();

            if (!string.IsNullOrEmpty(nombre))
                query = query.Where(e => e.nombre.Contains(nombre));

            if (!string.IsNullOrEmpty(sede))
                query = query.Where(e => e.sede.Contains(sede));

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(e => e.estado == estado);

            var lista = await query.ToListAsync();
            return View(lista);
        }



        // GET: equipos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var equipos = await _context.equipos
                .FirstOrDefaultAsync(m => m.id == id);
            if (equipos == null)
            {
                return NotFound();
            }

            return View(equipos);
        }

        // GET: equipos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: equipos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,nombre,sede,estado")] equipos equipos, IFormFile ImagenEquipo)
        {
            if (ModelState.IsValid)
            {

                if (equipos == null) {

                    TempData["ErrorMessage"] = "Ingrese Datos.";
                    return View(equipos);
                }
                bool nombreDuplicado = await _context.equipos
               .AnyAsync(e => e.nombre == equipos.nombre);

                if (nombreDuplicado)
                {
                    TempData["ErrorMessage"] = "Ya existe un equipo con ese nombre.";
                    return View(equipos);
                }

                // Procesar la imagen si se subió
                if (ImagenEquipo != null && ImagenEquipo.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(ImagenEquipo.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/equipos", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImagenEquipo.CopyToAsync(stream);
                    }

                    equipos.img = fileName;
                }


                

                _context.equipos.Add(equipos);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Equipo registrado con éxito.";
                return RedirectToAction(nameof(Index));

            }
            return View(equipos);
        }
        // GET: equipos/Edit/5
        // EDIT GET
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var equipo = await _context.equipos.FindAsync(id);
            if (equipo == null) return NotFound();

            var temporadaFinalizada = await _context.temporadas
                .OrderByDescending(t => t.anio_fin)
                .FirstOrDefaultAsync();

            if (temporadaFinalizada == null || temporadaFinalizada.campeon_id == null)
            {
                TempData["ErrorMessage"] = "No se puede editar equipos hasta que finalice la temporada (debe tener un campeón asignado).";
                return RedirectToAction(nameof(Index));
            }

            return View(equipo);
        }


        // POST: equipos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: equipos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,nombre,sede,img,estado")] equipos equipo, IFormFile ImagenEquipo)
        {
            if (id != equipo.id)
                return NotFound();

            var equipoDB = await _context.equipos.FindAsync(id);
            if (equipoDB == null)
                return NotFound();

            var temporadaActual = await _context.temporadas
      .Where(t => t.campeon_id != null)
      .OrderByDescending(t => t.anio_fin)
      .FirstOrDefaultAsync();

            if (temporadaActual == null)
            {
                TempData["ErrorMessage"] = "No se puede editar el equipo porque aún no hay temporada con campeón.";
                equipo.img = equipoDB.img;
                ModelState.Remove("img");
                return View(equipo);
            }

            // Verificar si existe una temporada posterior a la actual (con o sin campeón)
            bool existeTemporadaPosterior = await _context.temporadas
                .AnyAsync(t => t.anio_inicio > temporadaActual.anio_fin);

            if (existeTemporadaPosterior)
            {
                TempData["ErrorMessage"] = "No se puede editar el equipo porque ya se ha generado una nueva temporada posterior.";
                equipo.img = equipoDB.img;
                ModelState.Remove("img");
                return View(equipo);
            }

            // Validación: nombre duplicado
            bool nombreDuplicado = await _context.equipos
                .AnyAsync(e => e.nombre == equipo.nombre && e.id != id);

            if (nombreDuplicado)
            {
                TempData["ErrorMessage"] = "Ya existe un equipo con ese nombre.";
                equipo.img = equipoDB.img;
                ModelState.Remove("img");
                return View(equipo);
            }

            ModelState.Remove("img");

            if (ModelState.IsValid)
            {
                equipoDB.nombre = equipo.nombre;
                equipoDB.sede = equipo.sede;
                equipoDB.estado = equipo.estado;

                if (ImagenEquipo != null && ImagenEquipo.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(ImagenEquipo.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/equipos", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImagenEquipo.CopyToAsync(stream);
                    }

                    // Eliminar imagen anterior
                    if (!string.IsNullOrEmpty(equipoDB.img))
                    {
                        var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/equipos", equipoDB.img);
                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }

                    equipoDB.img = fileName;
                }
                else
                {
                    equipoDB.img = equipo.img;
                }

                _context.Entry(equipoDB).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Equipo actualizado con éxito.";
                return RedirectToAction(nameof(Index));
            }

            equipo.img = equipoDB.img;
            ModelState.Remove("img");
            return View(equipo);
        }




        // GET: equipos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var equipo = await _context.equipos
                .FirstOrDefaultAsync(m => m.id == id);

            if (equipo == null)
                return NotFound();

            // Verificar si el equipo está activo
            if (equipo.estado == "activo")
            {
                TempData["ErrorMessage"] = "No se puede eliminar un equipo que está activo.";
                return RedirectToAction(nameof(Index));
            }

            return View(equipo);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var equipo = await _context.equipos.FindAsync(id);
            if (equipo == null)
                return NotFound();

            // 🔒 Validación: No eliminar si tiene jugadores asociados
            bool tieneJugadores = await _context.jugadores.AnyAsync(j => j.equipo_id == id);
            if (tieneJugadores)
            {
                TempData["ErrorMessage"] = "No se puede eliminar este equipo porque tiene jugadores registrados.";
                return RedirectToAction(nameof(Index));
            }

            // (Opcional) eliminar imagen del equipo
            if (!string.IsNullOrEmpty(equipo.img))
            {
                var imgPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/equipos", equipo.img);
                if (System.IO.File.Exists(imgPath))
                    System.IO.File.Delete(imgPath);
            }

            _context.equipos.Remove(equipo);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Equipo eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }





    }
}
