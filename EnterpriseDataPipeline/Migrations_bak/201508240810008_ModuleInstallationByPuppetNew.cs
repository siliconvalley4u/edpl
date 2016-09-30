namespace EnterpriseDataPipeline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModuleInstallationByPuppetNew : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PuppetServers", "ServerType", c => c.String(maxLength: 1024));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PuppetServers", "ServerType");
        }
    }
}
