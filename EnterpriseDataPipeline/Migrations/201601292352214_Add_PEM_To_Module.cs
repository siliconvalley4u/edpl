namespace EnterpriseDataPipeline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_PEM_To_Module : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ModuleServers", "PemFile", c => c.String(maxLength: 1024, storeType: "nvarchar"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ModuleServers", "PemFile");
        }
    }
}
