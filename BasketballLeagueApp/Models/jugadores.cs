using System;
using System.Collections.Generic;

namespace BasketballLeagueApp.Models;

public partial class jugadores
{
    public int id { get; set; }

    public string nombre { get; set; } = null!;

    public string apellido { get; set; } = null!;

    public DateOnly fecha_nacimiento { get; set; }

    public string nacionalidad { get; set; } = null!;

    public int dorsal { get; set; }

    public decimal? estatura { get; set; }

    public decimal? peso { get; set; }

    public string? posicion { get; set; }

    public int equipo_id { get; set; }

    public string? estado { get; set; }

    public string? img { get; set; }

    public virtual equipos equipo { get; set; } = null!;

    public virtual ICollection<estadisticas_jugadores> estadisticas_jugadores { get; set; } = new List<estadisticas_jugadores>();

    public virtual ICollection<eventos_partido> eventos_partido { get; set; } = new List<eventos_partido>();

    public virtual ICollection<historial_jugadores> historial_jugadores { get; set; } = new List<historial_jugadores>();
}
