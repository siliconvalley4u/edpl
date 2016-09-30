namespace EnterpriseDataPipeline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_KafkaTopcis : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.KafkaTopics",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Topics = c.String(maxLength: 1024, storeType: "nvarchar"),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.KafkaTopics");
        }
    }
}
