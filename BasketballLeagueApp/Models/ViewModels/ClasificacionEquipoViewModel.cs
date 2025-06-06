namespace BasketballLeagueApp.Models.ViewModels
{
    
        public class ClasificacionEquipoViewModel
        {

        public string EquipoNombre { get; set; } = "";
        public int JuegosGanados { get; set; }
        public int JuegosPerdidos { get; set; }
        public decimal? PorcentajeVictorias { get; set; }
        public decimal? DiferenciaConLider { get; set; }
        public int LocalGanados { get; set; }
        public int LocalPerdidos { get; set; }
        public int VisitanteGanados { get; set; }
        public int VisitantePerdidos { get; set; }
        public int Ultimos10Ganados { get; set; }
        public int Ultimos10Perdidos { get; set; }
        public string? Racha { get; set; }
    }

   

}
