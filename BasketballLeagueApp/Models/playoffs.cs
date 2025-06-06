using System;
using System.Collections.Generic;

namespace BasketballLeagueApp.Models;

public partial class playoffs
{
    public int id { get; set; }

    public int? temporada_id { get; set; }

    public string? ronda { get; set; } = null!;

    public int? partido_id { get; set; }

    public int? victorias_equipo_a { get; set; }

    public int? victorias_equipo_b { get; set; }

    public int? clasificado_id { get; set; }

    public int? equipo_a_id { get; set; }
    public int? equipo_b_id { get; set; }


    public virtual equipos? clasificado { get; set; }

    public virtual partidos partido { get; set; } = null!;

    public virtual temporadas temporada { get; set; } = null!;
}
