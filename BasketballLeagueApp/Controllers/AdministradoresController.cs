using BasketballLeagueApp.Models;
using BasketballLeagueApp.Services;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BasketballLeagueApp.Controllers
{
    [Route("Administradores")]
    public class AdministradoresController : Controller
    {
        private readonly LigaBaloncestoContext _context;

        public AdministradoresController(LigaBaloncestoContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("Crear")]
        [Autentication]
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        [Route("Crear")]
        [Autentication]
        public IActionResult Crear(administradores administrador)
        {
            if (string.IsNullOrEmpty(administrador.email) || string.IsNullOrEmpty(administrador.password_hash) || string.IsNullOrEmpty(administrador.nombre) || string.IsNullOrEmpty(administrador.apellido))
            {
                ViewData["ErrorMessage"] = "Todos los campos son obligatorios.";
                return View();
            }

            var existeAdministrador = _context.administradores.Any(a => a.email == administrador.email);
            if (existeAdministrador)
            {
                ViewData["ErrorMessage"] = "El correo ya está registrado.";
                return View();
            }

            try
            {
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(administrador.password_hash);
                var nuevoAdministrador = new administradores
                {
                    nombre = administrador.nombre,
                    apellido = administrador.apellido,
                    email = administrador.email,
                    password_hash = hashedPassword,
                    fecha_registro = DateTime.Now
                };

                _context.administradores.Add(nuevoAdministrador);
                _context.SaveChanges();
                var adminId = HttpContext.Session.GetInt32("id");

                if (adminId != 1)
                {
                    return RedirectToAction("Principal", "Verificacion");
                }
                return RedirectToAction("InfoAdmins");
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Error al registrar el administrador: {ex.Message}";
                return View();
            }
        }
        [HttpGet]
        [Route("InfoAdmins")]
        [Autentication]
        public IActionResult InfoAdmins(string searchEmail = "")
        {
            var adminId = HttpContext.Session.GetInt32("id");

            if (adminId != 1)
            {
                return RedirectToAction("Principal", "Verificacion");
            }
            var administradores = string.IsNullOrEmpty(searchEmail)
               ? _context.administradores.ToList()
               : _context.administradores
                   .Where(a => a.email.Contains(searchEmail))
                   .ToList();

            return View(administradores);
        }

        [HttpGet]
        [Route("Editar")]
        [Autentication]
        public IActionResult Editar(string email)
        {
            var adminId = HttpContext.Session.GetInt32("id");

            if (adminId != 1)
            {
                return RedirectToAction("Principal", "Verificacion");
            }

            var administrador = _context.administradores.FirstOrDefault(a=>a.email == email);
            if (administrador == null)
            {
                return NotFound();
            }

            return View(administrador);
        }

        [HttpPost]
        [Route("Editar")]
        [Autentication]
        public IActionResult Editar(int id, administradores administrador)
        {
            var adminId = HttpContext.Session.GetInt32("id");

            if (adminId != 1)
            {
                return RedirectToAction("Principal", "Verificacion");
            }

            var adminExistente = _context.administradores.Find(id);
            if (adminExistente == null)
            {
                return NotFound();
            }

            adminExistente.nombre = administrador.nombre;
            adminExistente.apellido = administrador.apellido;
            adminExistente.email = administrador.email;
            if (administrador.password_hash.IsNullOrEmpty())
            {
                _context.SaveChanges();
            }
            else
            {
                adminExistente.password_hash = BCrypt.Net.BCrypt.HashPassword(administrador.password_hash);

                _context.SaveChanges();
            }
                
            return RedirectToAction("InfoAdmins");
        }

        [HttpGet]
        [Route("Eliminar")]
        [Autentication]
        public IActionResult Eliminar(administradores admin)
        {
            var adminId = HttpContext.Session.GetInt32("id");

            if (adminId != 1)
            {
                return RedirectToAction("Principal", "Verificacion");
            }

            var administrador = _context.administradores.Find(admin.id);
            if (administrador == null)
            {
                return NotFound();
            }

            return View(administrador);
        }

        [HttpPost]
        [Route("Eliminar")]
        [Autentication]
        public IActionResult EliminarConfirmado(int id)
        {
            var adminId = HttpContext.Session.GetInt32("id");

            if (adminId != 1 || id == 1)
            {
                return RedirectToAction("Principal", "Verificacion");
            }

            var administrador = _context.administradores.Find(id);
            if (administrador == null)
            {
                return NotFound();
            }

            _context.administradores.Remove(administrador);
            _context.SaveChanges();

            return RedirectToAction("InfoAdmins");
        }

    }
}
