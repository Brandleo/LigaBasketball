namespace BasketballLeagueApp.Models.ViewModels
{
    public class JornadaConPartidos
    {
        public int NumeroJornada { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public List<PartidoProgramado> Partidos { get; set; } = new();
    }
}
