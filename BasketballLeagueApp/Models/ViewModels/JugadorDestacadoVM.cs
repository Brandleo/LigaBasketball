namespace BasketballLeagueApp.Models.ViewModels
{
    public class JugadorDestacadoVM
    {
        public string NombreJugador { get; set; } = "";
        public string Equipo { get; set; } = "";
        public int TotalPuntos { get; set; }
        public int TotalRebotes { get; set; }
        public int TotalAsistencias { get; set; }
        public int TotalFaltas { get; set; }
        public double PromedioMinutos { get; set; }
    }
}
