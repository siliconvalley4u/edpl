namespace EnterpriseDataPipeline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update_PotentialCustomer : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.PotentialCustomers", "LikeKidsYouthProgram", c => c.Boolean(nullable: false));
            AlterColumn("dbo.PotentialCustomers", "LikeITtrainingPlacement", c => c.Boolean(nullable: false));
            AlterColumn("dbo.PotentialCustomers", "LikeBigDataIotCloudMarketPlace", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.PotentialCustomers", "LikeBigDataIotCloudMarketPlace", c => c.String(unicode: false));
            AlterColumn("dbo.PotentialCustomers", "LikeITtrainingPlacement", c => c.String(unicode: false));
            AlterColumn("dbo.PotentialCustomers", "LikeKidsYouthProgram", c => c.String(unicode: false));
        }
    }
}
