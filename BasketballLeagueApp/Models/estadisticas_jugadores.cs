using System;
using System.Collections.Generic;

namespace BasketballLeagueApp.Models;

public partial class estadisticas_jugadores
{
    public int id { get; set; }

    public int partido_id { get; set; }

    public int jugador_id { get; set; }

    public int? minutos_jugados { get; set; }

    public int? puntos { get; set; }

    public int? rebotes { get; set; }

    public int? asistencias { get; set; }

    public int? robos { get; set; }

    public int? bloqueos { get; set; }

    public int? perdidas { get; set; }

    public int? faltas { get; set; }

    public virtual jugadores jugador { get; set; } = null!;

    public virtual partidos partido { get; set; } = null!;
}
