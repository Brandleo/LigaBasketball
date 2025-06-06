namespace BasketballLeagueApp.Models.ViewModels
{
    public class EstadisticasJugadorPDFViewModel
    {
        public string NombreJugador { get; set; } = "";
        public string TemporadaNombre { get; set; } = "";
        public List<EstadisticaJugadorResultado> Resultados { get; set; } = new();
    }
}
