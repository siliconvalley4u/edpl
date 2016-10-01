namespace EnterpriseDataPipeline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update_Storage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Storages", "Port", c => c.String(maxLength: 160, storeType: "nvarchar"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Storages", "Port");
        }
    }
}
