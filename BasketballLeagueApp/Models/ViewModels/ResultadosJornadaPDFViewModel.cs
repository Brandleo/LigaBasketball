namespace BasketballLeagueApp.Models.ViewModels
{
    public class ResultadosJornadaPDFViewModel
    {
        public string TemporadaNombre { get; set; } = "";
        public int NumeroJornada { get; set; }
        public List<PartidoResultadoVM> Partidos { get; set; } = new();
    }
}
