namespace R.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ResultHistory : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ResultHistories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Date = c.String(),
                        ResultId = c.String(),
                        Price = c.Single(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            DropTable("dbo.HousePrices");
            DropTable("dbo.Houses");
        }
        
        public override void Down()
        {
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
                "dbo.HousePrices",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Date = c.String(),
                        HouseId = c.Int(nullable: false),
                        MlsNumber = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            DropTable("dbo.ResultHistories");
        }
    }
}
