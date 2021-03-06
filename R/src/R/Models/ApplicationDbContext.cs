﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using R.Common.Models;


/*
public Configuration()
{

    AutomaticMigrationsEnabled = true;
}
*/

namespace R.Models
{
  public  class ApplicationDbContext : DbContext
    {

        public static string ConnectionString { get; set; } =
         //"Server=(localdb)\\mssqllocaldb; AttachDBFilename=|DataDirectory|\\R.mdf;Trusted_Connection=True;MultipleActiveResultSets=true";
         //  "Server=(localdb)\\mssqllocaldb;Database=R;Trusted_Connection=True;MultipleActiveResultSets=true";
         // "Server=x.x.x.x;uid=xx;pwd=xxx;Database=R;MultipleActiveResultSets=true";
         // "Server=(localdb)\\mssqllocaldb;Database=RX;Trusted_Connection=True;MultipleActiveResultSets=true";
         "Server=localhost\\SQLEXPRESS;AttachDbFileName=C:\\data\\MyDB1.mdf;Trusted_Connection=True;MultipleActiveResultSets=true";


        public ApplicationDbContext() : base(ConnectionString)
        {

            Database.SetInitializer<ApplicationDbContext>(new CreateDatabaseIfNotExists<ApplicationDbContext>());

            Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationDbContext, R.Migrations.Configuration>());

        }


        public DbSet<XResultHistory> XResultHistories { get; set; }
        public DbSet<ResultHistory> ResultHistories { get; set; }


        public DbSet<Address2> Address2s { get; set; }


        public DbSet<Address> Addresses { get; set; }


        public DbSet<AlternateURL> AlternateURLs { get; set; }


        public DbSet<Building> Buildings { get; set; }


        public DbSet<Business> Businesses { get; set; }


        public DbSet<Email> Emails { get; set; }


        public DbSet<Email2> Email2s { get; set; }


        public DbSet<Individual> Individuals { get; set; }


        public DbSet<Land> Lands { get; set; }


        public DbSet<Organization> Organizations { get; set; }


        public DbSet<Parking> Parkings { get; set; }


        public DbSet<Phone> Phones { get; set; }


        public DbSet<Phone2> Phone2s { get; set; }


        public DbSet<Photo> Photoes { get; set; }

        public DbSet<Pin> Pins { get; set; }



        public DbSet<Property> Properties { get; set; }



        public DbSet<Result> Results { get; set; }



        public DbSet<Website1> webSite1s { get; set; }


        public DbSet<Website2> Website2s { get; set; }



        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

        }



    }
}
