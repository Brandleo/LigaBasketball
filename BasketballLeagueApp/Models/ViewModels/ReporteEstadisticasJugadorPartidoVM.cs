using Microsoft.AspNetCore.Mvc.Rendering;

namespace BasketballLeagueApp.Models.ViewModels
{
    public class ReporteEstadisticasJugadorPartidoVM
    {
        public int? TemporadaSeleccionada { get; set; }
        public int? EquipoSeleccionado { get; set; }
        public int? JugadorSeleccionado { get; set; }

        public List<SelectListItem> Temporadas { get; set; } = new();
        public List<SelectListItem> Equipos { get; set; } = new();
        public List<SelectListItem> Jugadores { get; set; } = new();

        public List<EstadisticaJugadorResultado> Resultados { get; set; } = new();
    }
}
