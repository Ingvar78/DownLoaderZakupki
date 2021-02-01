using DownLoaderZakupki.Data.DB;
using DownLoaderZakupki.Data.Extention;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DownLoaderZakupki.Data.Access
{
    internal class GovDbContext : DbContext, IGovDbContext
    {
        private readonly string _connectionString;
        private readonly ILoggerFactory _loggerFactory;
        public GovDbContext(string connectionString, ILoggerFactory loggerFactory)
        {
            _connectionString = connectionString;
            _loggerFactory = loggerFactory;
        }

        public GovDbContext()
        {
            //Для миграции   
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
#if true && DEBUG
            optionsBuilder.UseLoggerFactory(_loggerFactory);
#endif
            optionsBuilder.UseNpgsql(_connectionString);
            //optionsBuilder.UseNpgsql("Host=192.168.1.60;Port=5432;Database=AimGov2;Username=postgres;Password=Zaq1Xsw2Z;Pooling=True");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UsePostgresConventions();
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<FileCashes> FileCashes { get; set; }
        public DbSet<NsiFileCashes> NsiFileCashes { get; set; }
        public DbSet<NsiAbandonedReason> NsiAReasons { get; set; }
        public DbSet<NsiEtps> NsiEtps { get; set; }

        public DbSet<NsiPlacingWays> NsiPlacingWays { get; set; }
        public DbSet<NsiOrganizations> NsiOrganizations { get; set; }
        public DbSet<Notifications> Notifications { get; set; }

        int IGovDbContext.SaveChanges()
        {
            return this.SaveChangesInternal();
        }

        private int SaveChangesInternal()
        {
            var result = base.SaveChanges();
            return result;
        }

    }
}
