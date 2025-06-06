namespace BasketballLeagueApp.Models.ViewModels
{
    public class PartidoProgramado
    {
        public string Equipos { get; set; } = "";
        public DateTime Fecha { get; set; }
        public TimeOnly Hora { get; set; }
    }
}
