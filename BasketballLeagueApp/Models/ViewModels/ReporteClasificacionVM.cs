using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
namespace BasketballLeagueApp.Models.ViewModels
{
    public class ReporteClasificacionVM
    {

        public List<SelectListItem> TemporadasDisponibles { get; set; }
        public int TemporadaSeleccionada { get; set; }
        public List<ReporteFilaClasificacion> Filas { get; set; }
    }
}
