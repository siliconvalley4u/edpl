namespace EnterpriseDataPipeline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class JobServer : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.JobServers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 1024),
                        IPAddress = c.String(nullable: false, maxLength: 160),
                        UserName = c.String(maxLength: 1024),
                        Password = c.String(maxLength: 1024),
                        JobDirectory = c.String(maxLength: 1024),
                    })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.JobStatus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Source = c.String(nullable: false, maxLength: 1024),
                        Destinatiion = c.String(nullable: false, maxLength: 1024),
                        JobName = c.String(maxLength: 1024),
                        Status = c.String(maxLength: 1024),
                    })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.JobTBs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Source = c.String(nullable: false, maxLength: 1024),
                        Destinatiion = c.String(nullable: false, maxLength: 1024),
                        JobName = c.String(maxLength: 1024),
                    })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.Storages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 160),
                        IPAddress = c.String(maxLength: 1024),
                        UserName = c.String(maxLength: 1024),
                        Password = c.String(maxLength: 1024),
                        DBName = c.String(maxLength: 160),
                        TableName = c.String(maxLength: 160),
                    })
                .PrimaryKey(t => t.Id);

            AddColumn("dbo.AspNetRoles", "Description", c => c.String());
            AddColumn("dbo.AspNetRoles", "Discriminator", c => c.String(nullable: false, maxLength: 128));
            //AddColumn("dbo.JobServers", "JobDirectory", c => c.String(maxLength: 1024));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetRoles", "Discriminator");
            DropColumn("dbo.AspNetRoles", "Description");
            DropTable("dbo.Storages");
            DropTable("dbo.JobTBs");
            DropTable("dbo.JobStatus");
            DropTable("dbo.JobServers");
        }
    }
}
