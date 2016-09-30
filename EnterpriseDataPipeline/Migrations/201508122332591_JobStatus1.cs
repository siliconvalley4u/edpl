namespace EnterpriseDataPipeline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class JobStatus1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JobStatus", "ByteTransfer", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.JobStatus", "ByteTransfer");
        }
    }
}
