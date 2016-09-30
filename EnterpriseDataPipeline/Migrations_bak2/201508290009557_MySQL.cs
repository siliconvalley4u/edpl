namespace EnterpriseDataPipeline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MySQL : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.JobServers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 1024, storeType: "nvarchar"),
                        IPAddress = c.String(nullable: false, maxLength: 160, storeType: "nvarchar"),
                        UserName = c.String(maxLength: 1024, storeType: "nvarchar"),
                        Password = c.String(maxLength: 1024, storeType: "nvarchar"),
                        JobDirectory = c.String(maxLength: 1024, storeType: "nvarchar"),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.JobStatus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Key = c.String(unicode: false),
                        Source = c.String(nullable: false, maxLength: 1024, storeType: "nvarchar"),
                        SourceIP = c.String(nullable: false, maxLength: 1024, storeType: "nvarchar"),
                        SourceTable = c.String(maxLength: 1024, storeType: "nvarchar"),
                        Destination = c.String(nullable: false, maxLength: 1024, storeType: "nvarchar"),
                        DestinationIP = c.String(nullable: false, maxLength: 1024, storeType: "nvarchar"),
                        DestinationTable = c.String(maxLength: 1024, storeType: "nvarchar"),
                        JobName = c.String(maxLength: 1024, storeType: "nvarchar"),
                        StartDateTime = c.DateTime(precision: 0),
                        EndDateTime = c.DateTime(precision: 0),
                        ByteTransfer = c.String(unicode: false),
                        TimeTaken = c.String(unicode: false),
                        Status = c.String(maxLength: 1024, storeType: "nvarchar"),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.JobTBs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Source = c.String(nullable: false, maxLength: 1024, storeType: "nvarchar"),
                        Destinatiion = c.String(nullable: false, maxLength: 1024, storeType: "nvarchar"),
                        JobName = c.String(maxLength: 1024, storeType: "nvarchar"),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ModuleStatus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Key = c.String(unicode: false),
                        ServerName = c.String(nullable: false, maxLength: 1024, storeType: "nvarchar"),
                        IPAddress = c.String(nullable: false, maxLength: 1024, storeType: "nvarchar"),
                        ModuleName = c.String(maxLength: 1024, storeType: "nvarchar"),
                        StartDateTime = c.DateTime(precision: 0),
                        EndDateTime = c.DateTime(precision: 0),
                        TimeTaken = c.String(unicode: false),
                        Status = c.String(maxLength: 1024, storeType: "nvarchar"),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ModuleTBs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 1024, storeType: "nvarchar"),
                        OrderId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PuppetServers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 1024, storeType: "nvarchar"),
                        IPAddress = c.String(nullable: false, maxLength: 160, storeType: "nvarchar"),
                        UserName = c.String(maxLength: 1024, storeType: "nvarchar"),
                        Password = c.String(maxLength: 1024, storeType: "nvarchar"),
                        ServerType = c.String(maxLength: 1024, storeType: "nvarchar"),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        Name = c.String(nullable: false, maxLength: 256, storeType: "nvarchar"),
                        Description = c.String(unicode: false),
                        Discriminator = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        RoleId = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.Storages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 160, storeType: "nvarchar"),
                        IPAddress = c.String(maxLength: 1024, storeType: "nvarchar"),
                        UserName = c.String(maxLength: 1024, storeType: "nvarchar"),
                        Password = c.String(maxLength: 1024, storeType: "nvarchar"),
                        DBName = c.String(maxLength: 160, storeType: "nvarchar"),
                        TableName = c.String(maxLength: 160, storeType: "nvarchar"),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        Email = c.String(maxLength: 256, storeType: "nvarchar"),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(unicode: false),
                        SecurityStamp = c.String(unicode: false),
                        PhoneNumber = c.String(unicode: false),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(precision: 0),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256, storeType: "nvarchar"),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        ClaimType = c.String(unicode: false),
                        ClaimValue = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        ProviderKey = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        UserId = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Storages");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.PuppetServers");
            DropTable("dbo.ModuleTBs");
            DropTable("dbo.ModuleStatus");
            DropTable("dbo.JobTBs");
            DropTable("dbo.JobStatus");
            DropTable("dbo.JobServers");
        }
    }
}
