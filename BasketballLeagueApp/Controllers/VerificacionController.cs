using BasketballLeagueApp.Models;
using BasketballLeagueApp.Services;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;

namespace BasketballLeagueApp.Controllers
{
    [Route("Administradores")]
    public class VerificacionController : Controller
    {
        private readonly LigaBaloncestoContext _context;

        public VerificacionController(LigaBaloncestoContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            if (HttpContext.Session.GetInt32("id") != null)
            {
                return RedirectToAction("Principal", "Verificacion");
            }
            return View();
        }

        [HttpGet]
        [Route("Principal")]
        [Autentication]
        public IActionResult Principal()
        {
            return View();
        }

        [HttpPost]
        [Route("Autenticar")]
        public IActionResult Autenticar(string email, string password)
        {
            var administrador = (from a in _context.administradores
                                 where a.email == email
                                 select a).FirstOrDefault();

            if (administrador != null && BCrypt.Net.BCrypt.Verify(password, administrador.password_hash))
            {
                HttpContext.Session.SetInt32("id", administrador.id);
                HttpContext.Session.SetString("nombre", administrador.nombre);
                HttpContext.Session.SetString("apellido", administrador.apellido);
                return RedirectToAction("Principal", "Verificacion");
            }

            ViewData["ErrorMessage"] = "Usuario o contraseña incorrectos";
            ViewData["Email"] = email;
            return View("Index");
        }

        [HttpGet]
        [Route("Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}