using System;
using System.Collections.Generic;

namespace BasketballLeagueApp.Models;

public partial class entrenadores
{
    public int id { get; set; }

    public string nombre { get; set; } = null!;

    public string apellido { get; set; } = null!;

    public string nacionalidad { get; set; } = null!;


    public virtual ICollection<historial_entrenadores> historial_entrenadores { get; set; } = new List<historial_entrenadores>();
}
