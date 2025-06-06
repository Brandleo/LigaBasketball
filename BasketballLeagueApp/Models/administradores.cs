using System;
using System.Collections.Generic;

namespace BasketballLeagueApp.Models;

public partial class administradores
{
    public int id { get; set; }

    public string nombre { get; set; } = null!;

    public string apellido { get; set; } = null!;

    public string email { get; set; } = null!;

    public string password_hash { get; set; } = null!;

    public DateTime? fecha_registro { get; set; }
}
