namespace BasketballLeagueApp.Models.ViewModels
{
    public class EstadisticaJugadorFila
    {

        public string NombreJugador { get; set; }
        public string Equipo { get; set; }
        public string Partido { get; set; } // "Fecha - Equipo A vs Equipo B"
        public int MinutosJugados { get; set; }
        public int Puntos { get; set; }
        public int Rebotes { get; set; }
        public int Asistencias { get; set; }
        public int Faltas { get; set; }
    }
}
