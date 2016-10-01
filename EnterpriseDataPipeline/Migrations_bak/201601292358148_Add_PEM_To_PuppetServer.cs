namespace EnterpriseDataPipeline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_PEM_To_PuppetServer : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PuppetServers", "PemFile", c => c.String(maxLength: 1024, storeType: "nvarchar"));
            DropColumn("dbo.ModuleServers", "PemFile");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ModuleServers", "PemFile", c => c.String(maxLength: 1024, storeType: "nvarchar"));
            DropColumn("dbo.PuppetServers", "PemFile");
        }
    }
}
