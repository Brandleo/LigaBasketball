using BasketballLeagueApp.enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BasketballLeagueApp.Models;

public partial class equipos
{
    public int id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string nombre { get; set; } = null!;

    [Required(ErrorMessage = "La sede es obligatoria.")]
    public string sede { get; set; } = null!;

    [Required(ErrorMessage = "El estado es obligatorio.")]
    public string estado { get; set; } = null!;

    public string? img { get; set; } // Imagen puede ser nula


    public virtual ICollection<clasificacion_equipos> clasificacion_equipos { get; set; } = new List<clasificacion_equipos>();

    public virtual ICollection<historial_entrenadores> historial_entrenadores { get; set; } = new List<historial_entrenadores>();

    public virtual ICollection<historial_jugadores> historial_jugadores { get; set; } = new List<historial_jugadores>();

    public virtual ICollection<jugadores> jugadores { get; set; } = new List<jugadores>();

    public virtual ICollection<partidos> partidosequipo_local { get; set; } = new List<partidos>();

    public virtual ICollection<partidos> partidosequipo_visitante { get; set; } = new List<partidos>();

    public virtual ICollection<partidos> partidosganador { get; set; } = new List<partidos>();

    public virtual ICollection<playoffs> playoffs { get; set; } = new List<playoffs>();

    public virtual ICollection<temporadas> temporadas { get; set; } = new List<temporadas>();
}
