namespace EnterpriseDataPipeline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class JobStatusNew2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JobStatus", "TimeTaken", c => c.Time(precision: 7));
        }
        
        public override void Down()
        {
            DropColumn("dbo.JobStatus", "TimeTaken");
        }
    }
}
