using Microsoft.AspNetCore.Mvc.Rendering;

namespace BasketballLeagueApp.Models.ViewModels
{
    public class ReporteJugadoresDestacadosVM
    {
        public int? TemporadaSeleccionada { get; set; }
        public List<SelectListItem> Temporadas { get; set; } = new();
        public List<JugadorDestacadoVM> Jugadores { get; set; } = new();
        public string? NombreTemporada { get; set; } = "";


    }
}
