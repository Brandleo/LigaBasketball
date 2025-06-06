namespace BasketballLeagueApp.Models.ViewModels
{
    public class EstadisticaJugadorViewModel
    {
        public int JugadorId { get; set; }
        public string NombreCompleto { get; set; }
        public int MinutosJugados { get; set; }
        public int Puntos { get; set; }
        public int Rebotes { get; set; }
        public int Asistencias { get; set; }
        public int Robos { get; set; }
        public int Bloqueos { get; set; }
        public int Perdidas { get; set; }
        public int Faltas { get; set; }


    }
}
