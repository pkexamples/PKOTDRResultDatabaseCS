namespace PhotonKinetics.ResultDatabase.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SlidingWindowsData : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.WindowAttenuation",
                c => new
                    {
                        WindowAttenuationId = c.Long(nullable: false, identity: true),
                        Location = c.Double(),
                        Attenuation = c.Double(),
                        SignatureResult_ResultId = c.Long(),
                    })
                .PrimaryKey(t => t.WindowAttenuationId)
                .ForeignKey("dbo.SignatureResult", t => t.SignatureResult_ResultId)
                .Index(t => t.SignatureResult_ResultId);
            
            CreateTable(
                "dbo.WindowUniformity",
                c => new
                    {
                        WindowUniformityId = c.Long(nullable: false, identity: true),
                        Location = c.Double(),
                        Uniformity = c.Double(),
                        SignatureResult_ResultId = c.Long(),
                    })
                .PrimaryKey(t => t.WindowUniformityId)
                .ForeignKey("dbo.SignatureResult", t => t.SignatureResult_ResultId)
                .Index(t => t.SignatureResult_ResultId);
            
            AddColumn("dbo.SignatureResult", "MaxWindowAtten_WindowAttenuationId", c => c.Long());
            AddColumn("dbo.SignatureResult", "MaxWindowUnif_WindowUniformityId", c => c.Long());
            AddColumn("dbo.SignatureResult", "MinWindowAtten_WindowAttenuationId", c => c.Long());
            AddColumn("dbo.SignatureResult", "MinWindowUnif_WindowUniformityId", c => c.Long());
            CreateIndex("dbo.SignatureResult", "MaxWindowAtten_WindowAttenuationId");
            CreateIndex("dbo.SignatureResult", "MaxWindowUnif_WindowUniformityId");
            CreateIndex("dbo.SignatureResult", "MinWindowAtten_WindowAttenuationId");
            CreateIndex("dbo.SignatureResult", "MinWindowUnif_WindowUniformityId");
            AddForeignKey("dbo.SignatureResult", "MaxWindowAtten_WindowAttenuationId", "dbo.WindowAttenuation", "WindowAttenuationId");
            AddForeignKey("dbo.SignatureResult", "MaxWindowUnif_WindowUniformityId", "dbo.WindowUniformity", "WindowUniformityId");
            AddForeignKey("dbo.SignatureResult", "MinWindowAtten_WindowAttenuationId", "dbo.WindowAttenuation", "WindowAttenuationId");
            AddForeignKey("dbo.SignatureResult", "MinWindowUnif_WindowUniformityId", "dbo.WindowUniformity", "WindowUniformityId");
            DropColumn("dbo.SignatureResult", "MaxWindowAtten_Location");
            DropColumn("dbo.SignatureResult", "MaxWindowAtten_Attenuation");
            DropColumn("dbo.SignatureResult", "MinWindowAtten_Location");
            DropColumn("dbo.SignatureResult", "MinWindowAtten_Attenuation");
            DropColumn("dbo.SignatureResult", "MaxWindowUnif_Location");
            DropColumn("dbo.SignatureResult", "MaxWindowUnif_Uniformity");
            DropColumn("dbo.SignatureResult", "MinWindowUnif_Location");
            DropColumn("dbo.SignatureResult", "MinWindowUnif_Uniformity");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SignatureResult", "MinWindowUnif_Uniformity", c => c.Double());
            AddColumn("dbo.SignatureResult", "MinWindowUnif_Location", c => c.Double());
            AddColumn("dbo.SignatureResult", "MaxWindowUnif_Uniformity", c => c.Double());
            AddColumn("dbo.SignatureResult", "MaxWindowUnif_Location", c => c.Double());
            AddColumn("dbo.SignatureResult", "MinWindowAtten_Attenuation", c => c.Double());
            AddColumn("dbo.SignatureResult", "MinWindowAtten_Location", c => c.Double());
            AddColumn("dbo.SignatureResult", "MaxWindowAtten_Attenuation", c => c.Double());
            AddColumn("dbo.SignatureResult", "MaxWindowAtten_Location", c => c.Double());
            DropForeignKey("dbo.SignatureResult", "MinWindowUnif_WindowUniformityId", "dbo.WindowUniformity");
            DropForeignKey("dbo.SignatureResult", "MinWindowAtten_WindowAttenuationId", "dbo.WindowAttenuation");
            DropForeignKey("dbo.SignatureResult", "MaxWindowUnif_WindowUniformityId", "dbo.WindowUniformity");
            DropForeignKey("dbo.SignatureResult", "MaxWindowAtten_WindowAttenuationId", "dbo.WindowAttenuation");
            DropForeignKey("dbo.WindowUniformity", "SignatureResult_ResultId", "dbo.SignatureResult");
            DropForeignKey("dbo.WindowAttenuation", "SignatureResult_ResultId", "dbo.SignatureResult");
            DropIndex("dbo.SignatureResult", new[] { "MinWindowUnif_WindowUniformityId" });
            DropIndex("dbo.SignatureResult", new[] { "MinWindowAtten_WindowAttenuationId" });
            DropIndex("dbo.SignatureResult", new[] { "MaxWindowUnif_WindowUniformityId" });
            DropIndex("dbo.SignatureResult", new[] { "MaxWindowAtten_WindowAttenuationId" });
            DropIndex("dbo.WindowUniformity", new[] { "SignatureResult_ResultId" });
            DropIndex("dbo.WindowAttenuation", new[] { "SignatureResult_ResultId" });
            DropColumn("dbo.SignatureResult", "MinWindowUnif_WindowUniformityId");
            DropColumn("dbo.SignatureResult", "MinWindowAtten_WindowAttenuationId");
            DropColumn("dbo.SignatureResult", "MaxWindowUnif_WindowUniformityId");
            DropColumn("dbo.SignatureResult", "MaxWindowAtten_WindowAttenuationId");
            DropTable("dbo.WindowUniformity");
            DropTable("dbo.WindowAttenuation");
        }
    }
}
