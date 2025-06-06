using System;
using System.Collections.Generic;

namespace BasketballLeagueApp.Models;

public partial class jornadas
{
    public int id { get; set; }

    public int temporada_id { get; set; }

    public int numero_jornada { get; set; }

    public DateTime fecha_inicio { get; set; }

    public DateTime fecha_fin { get; set; }

    public virtual ICollection<partidos> partidos { get; set; } = new List<partidos>();

    public virtual temporadas temporada { get; set; } = null!;
}
