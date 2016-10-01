namespace EnterpriseDataPipeline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_PotentialCustomer : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PotentialCustomers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FirstName = c.String(nullable: false, unicode: false),
                        LastName = c.String(nullable: false, unicode: false),
                        Phone = c.String(nullable: false, unicode: false),
                        Email = c.String(nullable: false, unicode: false),
                        LikeKidsYouthProgram = c.String(unicode: false),
                        LikeITtrainingPlacement = c.String(unicode: false),
                        LikeBigDataIotCloudMarketPlace = c.String(unicode: false),
                        CreateDateTime = c.DateTime(precision: 0),
                        UpdateDateTime = c.DateTime(precision: 0),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.PotentialCustomers");
        }
    }
}
