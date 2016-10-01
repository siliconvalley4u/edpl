namespace EnterpriseDataPipeline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update_Storage1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Storages", "Type", c => c.String(nullable: false, maxLength: 160, storeType: "nvarchar"));
            CreateIndex("dbo.Storages", "Type", name: "IX_Name_StorageType");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Storages", "IX_Name_StorageType");
            DropColumn("dbo.Storages", "Type");
        }
    }
}
