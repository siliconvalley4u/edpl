namespace EnterpriseDataPipeline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
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
                        Key = c.String(),
                        Source = c.String(nullable: false, maxLength: 1024),
                        SourceIP = c.String(nullable: false, maxLength: 1024),
                        SourceTable = c.String(maxLength: 1024),
                        Destination = c.String(nullable: false, maxLength: 1024),
                        DestinationIP = c.String(nullable: false, maxLength: 1024),
                        DestinationTable = c.String(maxLength: 1024),
                        JobName = c.String(maxLength: 1024),
                        StartDateTime = c.DateTime(),
                        EndDateTime = c.DateTime(),
                        ByteTransfer = c.String(),
                        TimeTaken = c.String(),
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
                "dbo.KafkaServers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 1024),
                        IPAddress = c.String(nullable: false, maxLength: 160),
                        UserName = c.String(maxLength: 1024),
                        Password = c.String(maxLength: 1024),
                        ServerType = c.String(maxLength: 1024),
                        PemFile = c.String(maxLength: 1024),
                        JobLocation = c.String(maxLength: 1024),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.KafkaTopics",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Topics = c.String(maxLength: 1024),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ModuleServers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 1024),
                        IPAddress = c.String(nullable: false, maxLength: 160),
                        PrivateIPAddress = c.String(nullable: false, maxLength: 160),
                        UserName = c.String(maxLength: 1024),
                        Password = c.String(maxLength: 1024),
                        ServerType = c.String(maxLength: 1024),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ModuleStatus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Key = c.String(),
                        ServerName = c.String(nullable: false, maxLength: 1024),
                        IPAddress = c.String(nullable: false, maxLength: 1024),
                        ModuleName = c.String(maxLength: 1024),
                        StartDateTime = c.DateTime(),
                        EndDateTime = c.DateTime(),
                        TimeTaken = c.String(),
                        Status = c.String(maxLength: 1024),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ModuleTBs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 1024),
                        OrderId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PuppetServers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 1024),
                        IPAddress = c.String(nullable: false, maxLength: 160),
                        UserName = c.String(maxLength: 1024),
                        Password = c.String(maxLength: 1024),
                        ServerType = c.String(maxLength: 1024),
                        PemFile = c.String(maxLength: 1024),
                        JobLocation = c.String(maxLength: 1024),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                        Description = c.String(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
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
                        Name = c.String(nullable: false, maxLength: 160),
                        Type = c.String(nullable: false, maxLength: 160),
                        IPAddress = c.String(maxLength: 1024),
                        UserName = c.String(maxLength: 1024),
                        Password = c.String(maxLength: 1024),
                        DBName = c.String(maxLength: 160),
                        TableName = c.String(maxLength: 160),
                        Port = c.String(maxLength: 160),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Type, name: "IX_Name_StorageType");
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
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
            DropIndex("dbo.Storages", "IX_Name_StorageType");
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
            DropTable("dbo.ModuleServers");
            DropTable("dbo.KafkaTopics");
            DropTable("dbo.KafkaServers");
            DropTable("dbo.JobTBs");
            DropTable("dbo.JobStatus");
            DropTable("dbo.JobServers");
        }
    }
}
