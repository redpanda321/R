using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;




namespace R.Models
{
    class RContext : DbContext
    {

        public RContext() : base("RDBConnectionString")
        {
            Database.SetInitializer<RContext>(new CreateDatabaseIfNotExists<RContext>());

            Database.SetInitializer(new MigrateDatabaseToLatestVersion<RContext, R.Migrations.Configuration>("RDBConnectionString"));
         
        }


        public DbSet<House> Houses { get; set; }

        public DbSet<HousePrice> HousePrices { get; set; }




        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

        }


    }
}
