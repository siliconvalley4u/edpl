namespace EnterpriseDataPipeline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModuleInstallationByPuppet : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ModuleStatus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Key = c.String(),
                        ServerName = c.String(nullable: false, maxLength: 1024),
                        IPAddress = c.String(nullable: false, maxLength: 1024),
                        ModuleName = c.String(maxLength: 1024),
                        StartDateTime = c.DateTime(),
                        EndDateTime = c.DateTime(),
                        TimeTaken = c.String(),
                        Status = c.String(maxLength: 1024),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ModuleTBs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 1024),
                        OrderId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PuppetServers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 1024),
                        IPAddress = c.String(nullable: false, maxLength: 160),
                        UserName = c.String(maxLength: 1024),
                        Password = c.String(maxLength: 1024),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.PuppetServers");
            DropTable("dbo.ModuleTBs");
            DropTable("dbo.ModuleStatus");
        }
    }
}
