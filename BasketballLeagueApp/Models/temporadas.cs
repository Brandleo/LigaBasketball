using System;
using System.Collections.Generic;

namespace BasketballLeagueApp.Models;

public partial class temporadas
{
    public int id { get; set; }

    public string nombre { get; set; } = null!;

    public int anio_inicio { get; set; }

    public int anio_fin { get; set; }

    public int? campeon_id { get; set; }

    public virtual equipos? campeon { get; set; }

    public virtual ICollection<clasificacion_equipos> clasificacion_equipos { get; set; } = new List<clasificacion_equipos>();

    public virtual ICollection<historial_entrenadores> historial_entrenadores { get; set; } = new List<historial_entrenadores>();

    public virtual ICollection<historial_jugadores> historial_jugadores { get; set; } = new List<historial_jugadores>();

    public virtual ICollection<jornadas> jornadas { get; set; } = new List<jornadas>();

    public virtual ICollection<partidos> partidos { get; set; } = new List<partidos>();

    public virtual ICollection<playoffs> playoffs { get; set; } = new List<playoffs>();
}
