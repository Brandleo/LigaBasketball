using System;
using System.Collections.Generic;

namespace BasketballLeagueApp.Models;

public partial class historial_jugadores
{
    public int id { get; set; }

    public int jugador_id { get; set; }

    public int equipo_id { get; set; }

    public int temporada_id { get; set; }

    public virtual equipos equipo { get; set; } = null!;

    public virtual jugadores jugador { get; set; } = null!;

    public virtual temporadas temporada { get; set; } = null!;
}
