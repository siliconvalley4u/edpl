namespace SV4U.EDPL.DynamicMVC.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Storage : DbMigration
    {
        public override void Up()
        {
            //CreateTable(
            //    "dbo.JobServers",
            //    c => new
            //        {
            //            Id = c.Int(nullable: false, identity: true),
            //            Name = c.String(nullable: false, maxLength: 1024),
            //            IPAddress = c.String(nullable: false, maxLength: 160),
            //            UserName = c.String(maxLength: 1024),
            //            Password = c.String(maxLength: 1024),
            //        })
            //    .PrimaryKey(t => t.Id);
            
            //CreateTable(
            //    "dbo.JobStatus",
            //    c => new
            //        {
            //            Id = c.Int(nullable: false, identity: true),
            //            Source = c.String(nullable: false, maxLength: 1024),
            //            Destinatiion = c.String(nullable: false, maxLength: 1024),
            //            JobName = c.String(maxLength: 1024),
            //            Status = c.String(maxLength: 1024),
            //        })
            //    .PrimaryKey(t => t.Id);
            
            //CreateTable(
            //    "dbo.JobTBs",
            //    c => new
            //        {
            //            Id = c.Int(nullable: false, identity: true),
            //            Source = c.String(nullable: false, maxLength: 1024),
            //            Destinatiion = c.String(nullable: false, maxLength: 1024),
            //            JobName = c.String(maxLength: 1024),
            //        })
            //    .PrimaryKey(t => t.Id);
            
            //CreateTable(
            //    "dbo.Storages",
            //    c => new
            //        {
            //            Id = c.Int(nullable: false, identity: true),
            //            Name = c.String(nullable: false, maxLength: 160),
            //            IPAddress = c.String(maxLength: 1024),
            //            UserName = c.String(maxLength: 1024),
            //            Password = c.String(maxLength: 1024),
            //            DBName = c.String(maxLength: 160),
            //            TableName = c.String(maxLength: 160),
            //        })
            //    .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            //DropTable("dbo.Storages");
            //DropTable("dbo.JobTBs");
            //DropTable("dbo.JobStatus");
            //DropTable("dbo.JobServers");
        }
    }
}
