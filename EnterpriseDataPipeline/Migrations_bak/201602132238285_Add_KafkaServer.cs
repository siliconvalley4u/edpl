namespace EnterpriseDataPipeline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_KafkaServer : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.KafkaServers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 1024, storeType: "nvarchar"),
                        IPAddress = c.String(nullable: false, maxLength: 160, storeType: "nvarchar"),
                        UserName = c.String(maxLength: 1024, storeType: "nvarchar"),
                        Password = c.String(maxLength: 1024, storeType: "nvarchar"),
                        ServerType = c.String(maxLength: 1024, storeType: "nvarchar"),
                        PemFile = c.String(maxLength: 1024, storeType: "nvarchar"),
                        JobLocation = c.String(maxLength: 1024, storeType: "nvarchar"),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.KafkaServers");
        }
    }
}
