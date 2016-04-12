namespace R.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Address2",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AddressText = c.String(),
                        Longitude = c.String(),
                        Latitude = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Addresses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AddressText = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AlternateURLs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BrochureLink = c.String(),
                        DetailsLink = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Buildings",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BathroomTotal = c.String(),
                        Bedrooms = c.String(),
                        SizeInterior = c.String(),
                        StoriesTotal = c.String(),
                        Type = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Email2",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ContactId = c.String(),
                        Individual_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Individuals", t => t.Individual_Id)
                .Index(t => t.Individual_Id);
            
            CreateTable(
                "dbo.Emails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ContactId = c.String(),
                        Organization_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Organizations", t => t.Organization_Id)
                .Index(t => t.Organization_Id);
            
            CreateTable(
                "dbo.HousePrices",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Date = c.String(),
                        HouseId = c.Int(nullable: false),
                        MlsNumber = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Houses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ImageUrl = c.String(),
                        Url = c.String(),
                        Address = c.String(),
                        Community = c.String(),
                        PostCode = c.String(),
                        City = c.String(),
                        Province = c.String(),
                        CurrentPrice = c.Single(nullable: false),
                        Bedroom = c.Single(nullable: false),
                        Bedroom1 = c.Single(nullable: false),
                        Bedroom2 = c.Single(nullable: false),
                        Bashroom = c.Single(nullable: false),
                        Type = c.String(),
                        MlsNumber = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Individuals",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IndividualID = c.Int(nullable: false),
                        Name = c.String(),
                        Photo = c.String(),
                        Position = c.String(),
                        PermitFreetextEmail = c.Boolean(nullable: false),
                        FirstName = c.String(),
                        LastName = c.String(),
                        CorporationDisplayTypeId = c.String(),
                        CccMember = c.Boolean(),
                        DesignationCodes = c.String(),
                        Organization_Id = c.Int(),
                        Result_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Organizations", t => t.Organization_Id)
                .ForeignKey("dbo.Results", t => t.Result_Id)
                .Index(t => t.Organization_Id)
                .Index(t => t.Result_Id);
            
            CreateTable(
                "dbo.Organizations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OrganizationID = c.Int(nullable: false),
                        Name = c.String(),
                        HasEmail = c.Boolean(nullable: false),
                        PermitFreetextEmail = c.Boolean(nullable: false),
                        PermitShowListingLink = c.Boolean(nullable: false),
                        Logo = c.String(),
                        Designation = c.String(),
                        Address_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Addresses", t => t.Address_Id)
                .Index(t => t.Address_Id);
            
            CreateTable(
                "dbo.Phones",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PhoneType = c.String(),
                        PhoneNumber = c.String(),
                        AreaCode = c.String(),
                        PhoneTypeId = c.String(),
                        Organization_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Organizations", t => t.Organization_Id)
                .Index(t => t.Organization_Id);
            
            CreateTable(
                "dbo.Website1",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Website = c.String(),
                        WebsiteTypeId = c.String(),
                        Organization_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Organizations", t => t.Organization_Id)
                .Index(t => t.Organization_Id);
            
            CreateTable(
                "dbo.Phone2",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PhoneType = c.String(),
                        PhoneNumber = c.String(),
                        AreaCode = c.String(),
                        PhoneTypeId = c.String(),
                        Extension = c.String(),
                        Individual_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Individuals", t => t.Individual_Id)
                .Index(t => t.Individual_Id);
            
            CreateTable(
                "dbo.Website2",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Website = c.String(),
                        WebsiteTypeId = c.String(),
                        Individual_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Individuals", t => t.Individual_Id)
                .Index(t => t.Individual_Id);
            
            CreateTable(
                "dbo.Lands",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LandscapeFeatures = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Parkings",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Spaces = c.String(),
                        Property_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Properties", t => t.Property_Id)
                .Index(t => t.Property_Id);
            
            CreateTable(
                "dbo.Photos",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SequenceId = c.String(),
                        HighResPath = c.String(),
                        MedResPath = c.String(),
                        LowResPath = c.String(),
                        LastUpdated = c.String(),
                        Property_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Properties", t => t.Property_Id)
                .Index(t => t.Property_Id);
            
            CreateTable(
                "dbo.Pins",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        key = c.String(),
                        propertyId = c.String(),
                        count = c.Int(nullable: false),
                        longitude = c.String(),
                        latitude = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Properties",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Price = c.String(),
                        Type = c.String(),
                        TypeId = c.String(),
                        OwnershipType = c.String(),
                        AmmenitiesNearBy = c.String(),
                        Address_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Address2", t => t.Address_Id)
                .Index(t => t.Address_Id);
            
            CreateTable(
                "dbo.Results",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        MlsNumber = c.String(),
                        PublicRemarks = c.String(),
                        PostalCode = c.String(),
                        RelativeDetailsURL = c.String(),
                        AlternateURL_Id = c.Int(),
                        Building_Id = c.Int(),
                        Land_Id = c.Int(),
                        Property_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AlternateURLs", t => t.AlternateURL_Id)
                .ForeignKey("dbo.Buildings", t => t.Building_Id)
                .ForeignKey("dbo.Lands", t => t.Land_Id)
                .ForeignKey("dbo.Properties", t => t.Property_Id)
                .Index(t => t.AlternateURL_Id)
                .Index(t => t.Building_Id)
                .Index(t => t.Land_Id)
                .Index(t => t.Property_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Results", "Property_Id", "dbo.Properties");
            DropForeignKey("dbo.Results", "Land_Id", "dbo.Lands");
            DropForeignKey("dbo.Individuals", "Result_Id", "dbo.Results");
            DropForeignKey("dbo.Results", "Building_Id", "dbo.Buildings");
            DropForeignKey("dbo.Results", "AlternateURL_Id", "dbo.AlternateURLs");
            DropForeignKey("dbo.Photos", "Property_Id", "dbo.Properties");
            DropForeignKey("dbo.Parkings", "Property_Id", "dbo.Properties");
            DropForeignKey("dbo.Properties", "Address_Id", "dbo.Address2");
            DropForeignKey("dbo.Website2", "Individual_Id", "dbo.Individuals");
            DropForeignKey("dbo.Phone2", "Individual_Id", "dbo.Individuals");
            DropForeignKey("dbo.Individuals", "Organization_Id", "dbo.Organizations");
            DropForeignKey("dbo.Website1", "Organization_Id", "dbo.Organizations");
            DropForeignKey("dbo.Phones", "Organization_Id", "dbo.Organizations");
            DropForeignKey("dbo.Emails", "Organization_Id", "dbo.Organizations");
            DropForeignKey("dbo.Organizations", "Address_Id", "dbo.Addresses");
            DropForeignKey("dbo.Email2", "Individual_Id", "dbo.Individuals");
            DropIndex("dbo.Results", new[] { "Property_Id" });
            DropIndex("dbo.Results", new[] { "Land_Id" });
            DropIndex("dbo.Results", new[] { "Building_Id" });
            DropIndex("dbo.Results", new[] { "AlternateURL_Id" });
            DropIndex("dbo.Properties", new[] { "Address_Id" });
            DropIndex("dbo.Photos", new[] { "Property_Id" });
            DropIndex("dbo.Parkings", new[] { "Property_Id" });
            DropIndex("dbo.Website2", new[] { "Individual_Id" });
            DropIndex("dbo.Phone2", new[] { "Individual_Id" });
            DropIndex("dbo.Website1", new[] { "Organization_Id" });
            DropIndex("dbo.Phones", new[] { "Organization_Id" });
            DropIndex("dbo.Organizations", new[] { "Address_Id" });
            DropIndex("dbo.Individuals", new[] { "Result_Id" });
            DropIndex("dbo.Individuals", new[] { "Organization_Id" });
            DropIndex("dbo.Emails", new[] { "Organization_Id" });
            DropIndex("dbo.Email2", new[] { "Individual_Id" });
            DropTable("dbo.Results");
            DropTable("dbo.Properties");
            DropTable("dbo.Pins");
            DropTable("dbo.Photos");
            DropTable("dbo.Parkings");
            DropTable("dbo.Lands");
            DropTable("dbo.Website2");
            DropTable("dbo.Phone2");
            DropTable("dbo.Website1");
            DropTable("dbo.Phones");
            DropTable("dbo.Organizations");
            DropTable("dbo.Individuals");
            DropTable("dbo.Houses");
            DropTable("dbo.HousePrices");
            DropTable("dbo.Emails");
            DropTable("dbo.Email2");
            DropTable("dbo.Buildings");
            DropTable("dbo.AlternateURLs");
            DropTable("dbo.Addresses");
            DropTable("dbo.Address2");
        }
    }
}
