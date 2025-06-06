namespace BasketballLeagueApp.Models.ViewModels
{
    public class PartidoResultadoVM
    {
        public string Equipos { get; set; } = "";
        public int? PuntosLocal { get; set; }
        public int? PuntosVisitante { get; set; }
        public string Ganador { get; set; } = "";
    }
}
