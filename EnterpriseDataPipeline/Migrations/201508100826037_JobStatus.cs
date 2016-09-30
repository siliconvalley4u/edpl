namespace EnterpriseDataPipeline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class JobStatus : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JobStatus", "SourceIP", c => c.String(nullable: false, maxLength: 1024));
            AddColumn("dbo.JobStatus", "SourceTable", c => c.String(maxLength: 1024));
            AddColumn("dbo.JobStatus", "Destination", c => c.String(nullable: false, maxLength: 1024));
            AddColumn("dbo.JobStatus", "DestinationIP", c => c.String(nullable: false, maxLength: 1024));
            AddColumn("dbo.JobStatus", "DestinationTable", c => c.String(maxLength: 1024));
            AddColumn("dbo.JobStatus", "StartDateTime", c => c.DateTime(nullable: false));
            AddColumn("dbo.JobStatus", "EndDateTime", c => c.DateTime(nullable: false));
            DropColumn("dbo.JobStatus", "Destinatiion");
        }
        
        public override void Down()
        {
            AddColumn("dbo.JobStatus", "Destinatiion", c => c.String(nullable: false, maxLength: 1024));
            DropColumn("dbo.JobStatus", "EndDateTime");
            DropColumn("dbo.JobStatus", "StartDateTime");
            DropColumn("dbo.JobStatus", "DestinationTable");
            DropColumn("dbo.JobStatus", "DestinationIP");
            DropColumn("dbo.JobStatus", "Destination");
            DropColumn("dbo.JobStatus", "SourceTable");
            DropColumn("dbo.JobStatus", "SourceIP");
        }
    }
}
