namespace R.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Business : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Businesses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Results", "Business_Id", c => c.Int());
            CreateIndex("dbo.Results", "Business_Id");
            AddForeignKey("dbo.Results", "Business_Id", "dbo.Businesses", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Results", "Business_Id", "dbo.Businesses");
            DropIndex("dbo.Results", new[] { "Business_Id" });
            DropColumn("dbo.Results", "Business_Id");
            DropTable("dbo.Businesses");
        }
    }
}
