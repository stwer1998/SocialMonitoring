using Microsoft.EntityFrameworkCore;
using Models.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Models.DAL
{
    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Proxy> Proxies { get; set; }
        public DbSet<PhysicalPerson> PhyicalPeople { get; set; }
        public DbSet<LegalEntity> LegalEntities { get; set; }
        public DbSet<KadArbirtModel> KadArbirtModels { get; set; }
        public DbSet<FsspToken> FsspTokens { get; set; }
        public DbSet<FsspModel> FsspModels { get; set; }
        public DbSet<ErgulNalogModel> ErgulNalogModels { get; set; }
        public DbSet<DataHistory> DataHistories { get; set; }
        public DbSet<BoNalogModel> BoNalogModels { get; set; }
        public DbSet<MassAddress> MassAddresses { get; set; }
        public DbSet<MassOwners> MassOwners { get; set; }
        public DbSet<MassDirectors> MassDirectors { get; set; }
        public DbSet<TerosistLegal> TerosistLegals { get; set; }
        public DbSet<Terosist> Terosists { get; set; }

        public DbSet<ParserSetting> ParserSettings { get; set; }

        private const string connectionString = "";

        public ApplicationContext() : base() { Database.EnsureCreated(); }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectionString);
        }

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
            Database.EnsureCreated();   // создаем базу данных при первом обращении
        }
    }
}
