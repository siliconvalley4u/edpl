namespace EnterpriseDataPipeline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_JobLocation_To_PuppetServer : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PuppetServers", "JobLocation", c => c.String(maxLength: 1024, storeType: "nvarchar"));
            DropColumn("dbo.PuppetServers", "JobDirectory");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PuppetServers", "JobDirectory", c => c.String(maxLength: 1024, storeType: "nvarchar"));
            DropColumn("dbo.PuppetServers", "JobLocation");
        }
    }
}
