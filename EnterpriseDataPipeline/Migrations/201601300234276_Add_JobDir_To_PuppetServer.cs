namespace EnterpriseDataPipeline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_JobDir_To_PuppetServer : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PuppetServers", "JobDirectory", c => c.String(maxLength: 1024, storeType: "nvarchar"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PuppetServers", "JobDirectory");
        }
    }
}
