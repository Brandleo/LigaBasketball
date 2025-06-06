namespace BasketballLeagueApp.Models.ViewModels
{
    public class ReporteFilaClasificacion
    {

        public string Equipo { get; set; }
        public int PartidosJugados { get; set; }
        public int PartidosGanados { get; set; }
        public int PartidosPerdidos { get; set; }
        public int PuntosFavor { get; set; }
        public int PuntosContra { get; set; }
        public int Diferencia { get; set; }
        public int PuntosTotales { get; set; }
    }
}
