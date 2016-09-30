namespace EnterpriseDataPipeline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class JobStatusNew3 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.JobStatus", "TimeTaken", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.JobStatus", "TimeTaken", c => c.Time(precision: 7));
        }
    }
}
