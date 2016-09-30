namespace EnterpriseDataPipeline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class JobStatusNew : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JobStatus", "Key", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.JobStatus", "Key");
        }
    }
}
