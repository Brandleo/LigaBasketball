namespace BasketballLeagueApp.Models.ViewModels
{
    public class JugarPartidoViewModel
    {
        public int? PartidoId { get; set; }
        public string? EquipoLocalNombre { get; set; }
        public string? EquipoVisitanteNombre { get; set; }

        public List<EstadisticaJugadorViewModel> JugadoresLocal { get; set; }
        public List<EstadisticaJugadorViewModel> JugadoresVisitante { get; set; }

    }
}
