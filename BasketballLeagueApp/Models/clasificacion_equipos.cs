using System;
using System.Collections.Generic;

namespace BasketballLeagueApp.Models;

public partial class clasificacion_equipos
{
    public int id { get; set; }

    public int temporada_id { get; set; }

    public int equipo_id { get; set; }

    public int? juegos_ganados { get; set; }

    public int? juegos_perdidos { get; set; }

    public decimal? porcentaje_victorias { get; set; }

    public decimal? diferencia_con_lider { get; set; }

    public int? local_ganados { get; set; }

    public int? local_perdidos { get; set; }

    public int? visitante_ganados { get; set; }

    public int? visitante_perdidos { get; set; }

    public int? ultimos10_ganados { get; set; }

    public int? ultimos10_perdidos { get; set; }

    public string? racha { get; set; }

    public virtual equipos equipo { get; set; } = null!;

    public virtual temporadas temporada { get; set; } = null!;
}
