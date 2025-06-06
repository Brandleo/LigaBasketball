using System;
using System.Collections.Generic;

namespace BasketballLeagueApp.Models;

public partial class eventos_partido
{
    public int id { get; set; }

    public int partido_id { get; set; }

    public int jugador_id { get; set; }

    public int? minuto { get; set; }

    public string evento { get; set; } = null!;

    public virtual jugadores jugador { get; set; } = null!;

    public virtual partidos partido { get; set; } = null!;
}
