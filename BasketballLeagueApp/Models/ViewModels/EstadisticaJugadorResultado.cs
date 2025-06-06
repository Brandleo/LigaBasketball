namespace BasketballLeagueApp.Models.ViewModels
{
    public class EstadisticaJugadorResultado
    {
        public string Partido { get; set; } = "";
        public string Fecha { get; set; } = "";
        public int? MinutosJugados { get; set; }
        public int? Puntos { get; set; }
        public int? Rebotes { get; set; }
        public int? Asistencias { get; set; }
        public int? Faltas { get; set; }
    }
}
