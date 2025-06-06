using Microsoft.AspNetCore.Mvc.Rendering;

namespace BasketballLeagueApp.Models.ViewModels
{
    public class GraficoComparacionEquiposVM
    {

        public List<string> NombresEquipos { get; set; }
        public List<int> ValoresEstadistica { get; set; }
        public string Estadistica { get; set; }
        public int TemporadaId { get; set; }
        public List<SelectListItem> EstadisticasDisponibles { get; set; }
        public List<SelectListItem> TemporadasDisponibles { get; set; }
    }
}
