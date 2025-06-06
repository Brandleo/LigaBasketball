using System;
using System.Collections.Generic;

namespace BasketballLeagueApp.Models;

public partial class historial_entrenadores
{
    public int id { get; set; }

    public int entrenador_id { get; set; }

    public int equipo_id { get; set; }

    public int temporada_id { get; set; }

    public virtual entrenadores entrenador { get; set; } = null!;

    public virtual equipos equipo { get; set; } = null!;

    public virtual temporadas temporada { get; set; } = null!;
}
