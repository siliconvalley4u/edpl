namespace EnterpriseDataPipeline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModuleServerWithPrivateIP : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ModuleServers", "PrivateIPAddress", c => c.String(nullable: false, maxLength: 160, storeType: "nvarchar"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ModuleServers", "PrivateIPAddress");
        }
    }
}
