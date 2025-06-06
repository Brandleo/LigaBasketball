using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BasketballLeagueApp.Models;

public partial class LigaBaloncestoContext : DbContext
{
    public LigaBaloncestoContext()
    {
    }

    public LigaBaloncestoContext(DbContextOptions<LigaBaloncestoContext> options)
        : base(options)
    {
    }

    public virtual DbSet<administradores> administradores { get; set; }

    public virtual DbSet<clasificacion_equipos> clasificacion_equipos { get; set; }

    public virtual DbSet<entrenadores> entrenadores { get; set; }

    public virtual DbSet<equipos> equipos { get; set; }

    public virtual DbSet<estadisticas_jugadores> estadisticas_jugadores { get; set; }

    public virtual DbSet<eventos_partido> eventos_partido { get; set; }

    public virtual DbSet<historial_entrenadores> historial_entrenadores { get; set; }

    public virtual DbSet<historial_jugadores> historial_jugadores { get; set; }

    public virtual DbSet<jornadas> jornadas { get; set; }

    public virtual DbSet<jugadores> jugadores { get; set; }

    public virtual DbSet<partidos> partidos { get; set; }

    public virtual DbSet<playoffs> playoffs { get; set; }

    public virtual DbSet<temporadas> temporadas { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=localhost;Initial Catalog=liga_baloncesto_bd;User Id=LigaBaloncestoUser;Password=123321;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<administradores>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__administ__3213E83F06CE404C");

            entity.HasIndex(e => e.email, "UQ__administ__AB6E616477270B00").IsUnique();

            entity.Property(e => e.apellido).HasMaxLength(100);
            entity.Property(e => e.email).HasMaxLength(100);
            entity.Property(e => e.fecha_registro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.nombre).HasMaxLength(100);
            entity.Property(e => e.password_hash).HasMaxLength(255);
        });

        modelBuilder.Entity<clasificacion_equipos>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__clasific__3213E83F63DC13D8");

            entity.Property(e => e.diferencia_con_lider)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.juegos_ganados).HasDefaultValue(0);
            entity.Property(e => e.juegos_perdidos).HasDefaultValue(0);
            entity.Property(e => e.local_ganados).HasDefaultValue(0);
            entity.Property(e => e.local_perdidos).HasDefaultValue(0);
            entity.Property(e => e.porcentaje_victorias)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 3)");
            entity.Property(e => e.racha).HasMaxLength(1);
            entity.Property(e => e.ultimos10_ganados).HasDefaultValue(0);
            entity.Property(e => e.ultimos10_perdidos).HasDefaultValue(0);
            entity.Property(e => e.visitante_ganados).HasDefaultValue(0);
            entity.Property(e => e.visitante_perdidos).HasDefaultValue(0);

            entity.HasOne(d => d.equipo).WithMany(p => p.clasificacion_equipos)
                .HasForeignKey(d => d.equipo_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__clasifica__equip__03F0984C");

            entity.HasOne(d => d.temporada).WithMany(p => p.clasificacion_equipos)
                .HasForeignKey(d => d.temporada_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__clasifica__tempo__02FC7413");
        });

        modelBuilder.Entity<entrenadores>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__entrenad__3213E83FE4F4AEAF");

            entity.Property(e => e.apellido).HasMaxLength(100);
            entity.Property(e => e.nacionalidad).HasMaxLength(50);
            entity.Property(e => e.nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<equipos>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__equipos__3213E83F6620B3F1");

            entity.HasIndex(e => e.nombre, "UQ__equipos__72AFBCC62EF5D615").IsUnique();

            entity.Property(e => e.estado).HasMaxLength(10);
            entity.Property(e => e.img).HasMaxLength(255);
            entity.Property(e => e.nombre).HasMaxLength(100);
            entity.Property(e => e.sede).HasMaxLength(100);
        });

        modelBuilder.Entity<estadisticas_jugadores>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__estadist__3213E83FBBED9D0A");

            entity.Property(e => e.asistencias).HasDefaultValue(0);
            entity.Property(e => e.bloqueos).HasDefaultValue(0);
            entity.Property(e => e.faltas).HasDefaultValue(0);
            entity.Property(e => e.perdidas).HasDefaultValue(0);
            entity.Property(e => e.puntos).HasDefaultValue(0);
            entity.Property(e => e.rebotes).HasDefaultValue(0);
            entity.Property(e => e.robos).HasDefaultValue(0);

            entity.HasOne(d => d.jugador).WithMany(p => p.estadisticas_jugadores)
                .HasForeignKey(d => d.jugador_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__estadisti__jugad__70DDC3D8");

            entity.HasOne(d => d.partido).WithMany(p => p.estadisticas_jugadores)
                .HasForeignKey(d => d.partido_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__estadisti__parti__6FE99F9F");
        });

        modelBuilder.Entity<eventos_partido>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__eventos___3213E83F1B8C990C");

            entity.Property(e => e.evento).HasMaxLength(100);

            entity.HasOne(d => d.jugador).WithMany(p => p.eventos_partido)
                .HasForeignKey(d => d.jugador_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__eventos_p__jugad__75A278F5");

            entity.HasOne(d => d.partido).WithMany(p => p.eventos_partido)
                .HasForeignKey(d => d.partido_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__eventos_p__parti__74AE54BC");
        });

        modelBuilder.Entity<historial_entrenadores>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__historia__3213E83FD295DAE3");

            entity.HasOne(d => d.entrenador).WithMany(p => p.historial_entrenadores)
                .HasForeignKey(d => d.entrenador_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__historial__entre__4F7CD00D");

            entity.HasOne(d => d.equipo).WithMany(p => p.historial_entrenadores)
                .HasForeignKey(d => d.equipo_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__historial__equip__5070F446");

            entity.HasOne(d => d.temporada).WithMany(p => p.historial_entrenadores)
                .HasForeignKey(d => d.temporada_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__historial__tempo__5165187F");
        });

        modelBuilder.Entity<historial_jugadores>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__historia__3213E83FEB099860");

            entity.HasOne(d => d.equipo).WithMany(p => p.historial_jugadores)
                .HasForeignKey(d => d.equipo_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__historial__equip__4BAC3F29");

            entity.HasOne(d => d.jugador).WithMany(p => p.historial_jugadores)
                .HasForeignKey(d => d.jugador_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__historial__jugad__4AB81AF0");

            entity.HasOne(d => d.temporada).WithMany(p => p.historial_jugadores)
                .HasForeignKey(d => d.temporada_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__historial__tempo__4CA06362");
        });

        modelBuilder.Entity<jornadas>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__jornadas__3213E83F5BF4BFB6");

            entity.HasOne(d => d.temporada).WithMany(p => p.jornadas)
                .HasForeignKey(d => d.temporada_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__jornadas__tempor__5441852A");
        });

        modelBuilder.Entity<jugadores>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__jugadore__3213E83FA0BC9F76");

            entity.HasIndex(e => new { e.dorsal, e.equipo_id }, "uc_jugador").IsUnique();

            entity.Property(e => e.apellido).HasMaxLength(100);
            entity.Property(e => e.estado).HasMaxLength(10);
            entity.Property(e => e.estatura).HasColumnType("decimal(3, 2)");
            entity.Property(e => e.img).HasMaxLength(255);
            entity.Property(e => e.nacionalidad).HasMaxLength(50);
            entity.Property(e => e.nombre).HasMaxLength(100);
            entity.Property(e => e.peso).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.posicion).HasMaxLength(50);

            entity.HasOne(d => d.equipo).WithMany(p => p.jugadores)
                .HasForeignKey(d => d.equipo_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__jugadores__equip__45F365D3");
        });

        modelBuilder.Entity<partidos>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__partidos__3213E83FB8B8F2D5");

            entity.Property(e => e.estado).HasMaxLength(50);
            entity.Property(e => e.fecha).HasColumnType("datetime");
            entity.Property(e => e.puntos_local).HasDefaultValue(0);
            entity.Property(e => e.puntos_visitante).HasDefaultValue(0);

            entity.HasOne(d => d.equipo_local).WithMany(p => p.partidosequipo_local)
                .HasForeignKey(d => d.equipo_local_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__partidos__equipo__5CD6CB2B");

            entity.HasOne(d => d.equipo_visitante).WithMany(p => p.partidosequipo_visitante)
                .HasForeignKey(d => d.equipo_visitante_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__partidos__equipo__5DCAEF64");

            entity.HasOne(d => d.ganador).WithMany(p => p.partidosganador)
                .HasForeignKey(d => d.ganador_id)
                .HasConstraintName("FK__partidos__ganado__5EBF139D");

            entity.HasOne(d => d.jornada).WithMany(p => p.partidos)
                .HasForeignKey(d => d.jornada_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__partidos__jornad__5AEE82B9");

            entity.HasOne(d => d.temporada).WithMany(p => p.partidos)
                .HasForeignKey(d => d.temporada_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__partidos__tempor__5BE2A6F2");
        });

        modelBuilder.Entity<playoffs>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__playoffs__3213E83FEC1C6C73");

            entity.Property(e => e.ronda).HasMaxLength(50);
            entity.Property(e => e.victorias_equipo_a).HasDefaultValue(0);
            entity.Property(e => e.victorias_equipo_b).HasDefaultValue(0);

            entity.HasOne(d => d.clasificado).WithMany(p => p.playoffs)
                .HasForeignKey(d => d.clasificado_id)
                .HasConstraintName("FK__playoffs__clasif__656C112C");

            entity.HasOne(d => d.partido).WithMany(p => p.playoffs)
                .HasForeignKey(d => d.partido_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__playoffs__partid__6477ECF3");

            entity.HasOne(d => d.temporada).WithMany(p => p.playoffs)
                .HasForeignKey(d => d.temporada_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__playoffs__tempor__6383C8BA");
        });

        modelBuilder.Entity<temporadas>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__temporad__3213E83F8994B0ED");

            entity.Property(e => e.nombre).HasMaxLength(100);

            entity.HasOne(d => d.campeon).WithMany(p => p.temporadas)
                .HasForeignKey(d => d.campeon_id)
                .HasConstraintName("FK__temporada__campe__3F466844");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
