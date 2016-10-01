namespace EnterpriseDataPipeline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModuleInstallation : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ModuleServers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 1024, storeType: "nvarchar"),
                        IPAddress = c.String(nullable: false, maxLength: 160, storeType: "nvarchar"),
                        UserName = c.String(maxLength: 1024, storeType: "nvarchar"),
                        Password = c.String(maxLength: 1024, storeType: "nvarchar"),
                        ServerType = c.String(maxLength: 1024, storeType: "nvarchar"),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ModuleServers");
        }
    }
}
