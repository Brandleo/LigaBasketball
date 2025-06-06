using Microsoft.AspNetCore.Mvc.Rendering;

namespace BasketballLeagueApp.Models.ViewModels
{
    public class GraficoDistribucionPuntosVM
    {
        public List<SelectListItem> TemporadasDisponibles { get; set; }
        public List<SelectListItem> EquiposDisponibles { get; set; }

    }
}
