using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;




namespace R.Models
{
  public  class RContext : DbContext
    {
        public RContext() : base("RDBConnectionString")
        {
            Database.SetInitializer<RContext>(new CreateDatabaseIfNotExists<RContext>());

            Database.SetInitializer(new MigrateDatabaseToLatestVersion<RContext, R.Migrations.Configuration>("RDBConnectionString"));

        }



        public DbSet<House> Houses { get; set; }

        public DbSet<HousePrice> HousePrices { get; set; }

        public DbSet<Result> Results { get; set; }

        public DbSet<Building> Buildings { get; set; }

        //////////////////////////////////
        
        public DbSet<Address> Addresses { get; set; }

        public DbSet<Phone> Phones { get; set; }
        
        public DbSet<Email> Emails { get; set; }
        
        public DbSet<Website1> webSite1s { get; set; }

        public DbSet<Organization> Organizations { get; set; }


        //////////////////////////////////

        public DbSet<Email2> Email2s { get; set; }
        public DbSet<Phone2> Phone2s { get; set; }

        public DbSet<Website2> Website2s { get; set; }

        public DbSet<Individual> Individuals { get; set; }

        ///////////////////////////////////////////////
        public DbSet<Address2> Address2s { get; set; }
        
        public DbSet<Photo> Photoes { get; set; }

        public DbSet<Parking> Parkings { get; set; }



        public DbSet<Property> Properties { get; set; }
        
        public DbSet<Land> Lands { get; set; }


        public DbSet<AlternateURL> AlternateURLs { get; set; }

        public DbSet<Pin> Pins { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

        }



    }
}
