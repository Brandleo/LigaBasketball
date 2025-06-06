using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BasketballLeagueApp.Services
{
    public class AutenticationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var usuarioId = context.HttpContext.Session.GetInt32("id");

            if (usuarioId == null || usuarioId == 0)
            {
                context.Result = new RedirectToActionResult("Index", "Verificacion", null);
            }

            base.OnActionExecuting(context);

        }
    }
}
