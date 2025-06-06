using System.ComponentModel.DataAnnotations;

namespace BasketballLeagueApp.Models.ViewModels
{
    public class TemporadaCreateViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string? nombre { get; set; }

        [Required(ErrorMessage = "El año de inicio es obligatorio.")]
        [Range(2000, 2100, ErrorMessage = "El año de inicio debe estar entre 2000 y 2100.")]
        public int? anioInicio { get; set; }

        [Required(ErrorMessage = "El año de fin es obligatorio.")]
        [Range(2000, 2100, ErrorMessage = "El año de fin debe estar entre 2000 y 2100.")]
        public int? anioFin { get; set; }
        public List<int> EquiposSeleccionados { get; set; } = new List<int>();
        public List<equipos> EquiposActivos { get; set; } = new List<equipos>();
    }

}
