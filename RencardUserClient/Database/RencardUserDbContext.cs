using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RencardUserClient.Models.About;
using RencardUserClient.Models.Identity;
using RencardUserClient.Models.Location;
using RencardUserClient.Models.Photos;
using RencardUserClient.Models.Preferences;

namespace RencardUserClient.Database
{
    public class RencardUserDbContext : IdentityDbContext<User>
    {
        public RencardUserDbContext(DbContextOptions<RencardUserDbContext> options) : base(options) { }

        public DbSet<UserPreferences> UserPreferences { get; set; }
        public DbSet<UserLocation> UserLocations { get; set; }
        public DbSet<AboutUser> AboutUsers { get; set; }
        public DbSet<UserPhoto> UserPhotos { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Настройка первичных ключей для зависимых сущностей
            builder.Entity<UserPreferences>()
                .HasKey(up => up.UserId); // UserId = PK

            builder.Entity<UserLocation>()
                .HasKey(ul => ul.UserId); // UserId = PK

            builder.Entity<AboutUser>()
                .HasKey(au => au.UserId); // UserId = PK

            builder.Entity<UserPhoto>()
                .HasKey(up => up.UserId); // UserId = PK 


            // Настройка 1:1 связей
            builder.Entity<User>()
                .HasOne(u => u.Preferences)
                .WithOne(p => p.User)
                .HasForeignKey<UserPreferences>(up => up.UserId);

            builder.Entity<User>()
                .HasOne(u => u.Location)
                .WithOne(p => p.User)
                .HasForeignKey<UserLocation>(ul => ul.UserId);

            builder.Entity<User>()
                .HasOne(u => u.About)
                .WithOne(p => p.User)
                .HasForeignKey<AboutUser>(au => au.UserId);

            builder.Entity<User>()
                .HasOne(u => u.Photo)
                .WithOne(p => p.User)
                .HasForeignKey<UserPhoto>(up => up.UserId);

            // Настройка PostGIS
            builder.HasPostgresExtension("postgis");
            builder.Entity<UserLocation>()
                .Property(ul => ul.Geography)
                .HasColumnType("geography (Point)");
        }
    }
}