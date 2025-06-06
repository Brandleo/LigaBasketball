using Microsoft.AspNetCore.Mvc.Rendering;

namespace BasketballLeagueApp.Models.ViewModels
{
    public class ReporteCalendarioJornadaVM
    {
        public int? TemporadaSeleccionada { get; set; }
        public List<SelectListItem> Temporadas { get; set; } = new();
        public List<JornadaConPartidos> Jornadas { get; set; } = new();

    }
}
