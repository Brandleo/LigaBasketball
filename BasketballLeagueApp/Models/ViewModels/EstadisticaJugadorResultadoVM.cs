namespace BasketballLeagueApp.Models.ViewModels
{
    public class EstadisticaJugadorResultadoVM
    {

        public string  NombreJugador { get; set; } = "";
        public string Equipo { get; set; } = "";
        public int ? Minutos { get; set; }
        public int ? Puntos { get; set; }
        public int ? Rebotes { get; set; }
        public int ?Asistencias { get; set; }
        public int ?Robos { get; set; }
        public int ?Bloqueos { get; set; }
        public int ?Faltas { get; set; }
    }
}
