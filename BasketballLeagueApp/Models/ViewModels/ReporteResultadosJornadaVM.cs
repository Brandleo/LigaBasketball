using Microsoft.AspNetCore.Mvc.Rendering;

namespace BasketballLeagueApp.Models.ViewModels
{
    public class ReporteResultadosJornadaVM
    {
        public int? TemporadaSeleccionada { get; set; }
        public int? JornadaSeleccionada { get; set; }

        public List<SelectListItem> Temporadas { get; set; } = new();
        public List<SelectListItem> Jornadas { get; set; } = new();
        public List<PartidoResultadoVM> Partidos { get; set; } = new();
    }
}
