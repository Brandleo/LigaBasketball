using System;
using System.Collections.Generic;

namespace BasketballLeagueApp.Models;

public partial class partidos
{
    public int id { get; set; }

    public int jornada_id { get; set; }

    public int temporada_id { get; set; }

    public int equipo_local_id { get; set; }

    public int equipo_visitante_id { get; set; }

    public bool es_playoff { get; set; }

    public DateTime fecha { get; set; }

    public TimeOnly hora { get; set; }

    public int? puntos_local { get; set; }

    public int? puntos_visitante { get; set; }

    public string? estado { get; set; }

    public int? ganador_id { get; set; }

    public virtual equipos equipo_local { get; set; } = null!;

    public virtual equipos equipo_visitante { get; set; } = null!;

    public virtual ICollection<estadisticas_jugadores> estadisticas_jugadores { get; set; } = new List<estadisticas_jugadores>();

    public virtual ICollection<eventos_partido> eventos_partido { get; set; } = new List<eventos_partido>();

    public virtual equipos? ganador { get; set; }

    public virtual jornadas jornada { get; set; } = null!;

    public virtual ICollection<playoffs> playoffs { get; set; } = new List<playoffs>();

    public virtual temporadas temporada { get; set; } = null!;

    
}
