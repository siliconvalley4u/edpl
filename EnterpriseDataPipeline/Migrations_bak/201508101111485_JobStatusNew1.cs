namespace EnterpriseDataPipeline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class JobStatusNew1 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.JobStatus", "StartDateTime", c => c.DateTime());
            AlterColumn("dbo.JobStatus", "EndDateTime", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.JobStatus", "EndDateTime", c => c.DateTime(nullable: false));
            AlterColumn("dbo.JobStatus", "StartDateTime", c => c.DateTime(nullable: false));
        }
    }
}
