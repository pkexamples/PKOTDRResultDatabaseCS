namespace PhotonKinetics.ResultDatabase.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AttenuationWaveResult",
                c => new
                    {
                        AttenuationWaveResultId = c.Long(nullable: false, identity: true),
                        Wavelength = c.Double(nullable: false),
                        AttenuationCoefficient = c.Double(),
                        AttenResult_ResultId = c.Long(),
                    })
                .PrimaryKey(t => t.AttenuationWaveResultId)
                .ForeignKey("dbo.AttenuationResult", t => t.AttenResult_ResultId)
                .Index(t => t.AttenResult_ResultId);
            
            CreateTable(
                "dbo.Result",
                c => new
                    {
                        ResultId = c.Long(nullable: false, identity: true),
                        DateMeasured = c.DateTime(nullable: false),
                        FilePath = c.String(),
                        SpoolEnd = c.Int(),
                        Instrument_SerialNumber = c.String(maxLength: 128),
                        SetHeader_ResultSetHeaderId = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.ResultId)
                .ForeignKey("dbo.Instrument", t => t.Instrument_SerialNumber)
                .ForeignKey("dbo.ResultSetHeader", t => t.SetHeader_ResultSetHeaderId, cascadeDelete: true)
                .Index(t => t.Instrument_SerialNumber)
                .Index(t => t.SetHeader_ResultSetHeaderId);
            
            CreateTable(
                "dbo.Instrument",
                c => new
                    {
                        SerialNumber = c.String(nullable: false, maxLength: 128),
                        ModelNumber = c.String(),
                    })
                .PrimaryKey(t => t.SerialNumber);
            
            CreateTable(
                "dbo.ResultSetHeader",
                c => new
                    {
                        ResultSetHeaderId = c.Long(nullable: false, identity: true),
                        FiberIDString = c.String(nullable: false),
                        FiberIDTag = c.String(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                        EnteredLength = c.Double(),
                        OperatorID = c.String(),
                    })
                .PrimaryKey(t => t.ResultSetHeaderId);
            
            CreateTable(
                "dbo.ResultSetLabel",
                c => new
                    {
                        HeaderLabelId = c.Long(nullable: false, identity: true),
                        Tag = c.String(nullable: false),
                        Value = c.String(),
                        Header_ResultSetHeaderId = c.Long(),
                    })
                .PrimaryKey(t => t.HeaderLabelId)
                .ForeignKey("dbo.ResultSetHeader", t => t.Header_ResultSetHeaderId)
                .Index(t => t.Header_ResultSetHeaderId);
            
            CreateTable(
                "dbo.ModeFieldWaveResult",
                c => new
                    {
                        ModeFieldWaveResultId = c.Long(nullable: false, identity: true),
                        Wavelength = c.Double(nullable: false),
                        MfdStandard = c.Double(nullable: false),
                        ModeFieldResult_ResultId = c.Long(),
                    })
                .PrimaryKey(t => t.ModeFieldWaveResultId)
                .ForeignKey("dbo.ModeFieldResult", t => t.ModeFieldResult_ResultId)
                .Index(t => t.ModeFieldResult_ResultId);
            
            CreateTable(
                "dbo.LengthResult",
                c => new
                    {
                        ResultId = c.Long(nullable: false),
                        LengthMeasured = c.Double(nullable: false),
                        GroupIndex = c.Double(nullable: false),
                        LengthMethod = c.Int(nullable: false),
                        WavelengthUsed = c.Double(),
                    })
                .PrimaryKey(t => t.ResultId)
                .ForeignKey("dbo.Result", t => t.ResultId)
                .Index(t => t.ResultId);
            
            CreateTable(
                "dbo.AttenuationResult",
                c => new
                    {
                        ResultId = c.Long(nullable: false),
                        LengthUsed = c.Double(nullable: false),
                        AttenMethod = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ResultId)
                .ForeignKey("dbo.Result", t => t.ResultId)
                .Index(t => t.ResultId);
            
            CreateTable(
                "dbo.ModeFieldResult",
                c => new
                    {
                        ResultId = c.Long(nullable: false),
                        MfdMethod = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ResultId)
                .ForeignKey("dbo.Result", t => t.ResultId)
                .Index(t => t.ResultId);
            
            CreateTable(
                "dbo.SignatureResult",
                c => new
                    {
                        ResultId = c.Long(nullable: false),
                        Wavelength = c.Double(nullable: false),
                        GroupIndex = c.Double(nullable: false),
                        PulseWidthM = c.Double(nullable: false),
                        PointSpacingM = c.Double(nullable: false),
                        RangeKM = c.Double(nullable: false),
                        Direction = c.Int(nullable: false),
                        AverageType = c.Int(nullable: false),
                        AverageLocation = c.Double(),
                        AverageTarget = c.Double(),
                        AverageTime = c.Double(),
                        AverageCount = c.Double(),
                        Length = c.Double(),
                        Attenuation = c.Double(),
                        EndEvent_Location = c.Double(),
                        EndEvent_Loss = c.Double(),
                        EndEvent_Reflectance = c.Double(),
                        InsertionEvent_Location = c.Double(),
                        InsertionEvent_Loss = c.Double(),
                        InsertionEvent_Reflectance = c.Double(),
                        MaxLossEvent_Location = c.Double(),
                        MaxLossEvent_Loss = c.Double(),
                        MaxLossEvent_Reflectance = c.Double(),
                        MaxReflectanceEvent_Location = c.Double(),
                        MaxReflectanceEvent_Loss = c.Double(),
                        MaxReflectanceEvent_Reflectance = c.Double(),
                        MinLossEvent_Location = c.Double(),
                        MinLossEvent_Loss = c.Double(),
                        MinLossEvent_Reflectance = c.Double(),
                        MaxLsaDeviation_Location = c.Double(),
                        MaxLsaDeviation_Deviation = c.Double(),
                        MaxWindowAtten_Location = c.Double(),
                        MaxWindowAtten_Attenuation = c.Double(),
                        MinWindowAtten_Location = c.Double(),
                        MinWindowAtten_Attenuation = c.Double(),
                        MaxWindowUnif_Location = c.Double(),
                        MaxWindowUnif_Uniformity = c.Double(),
                        MinWindowUnif_Location = c.Double(),
                        MinWindowUnif_Uniformity = c.Double(),
                    })
                .PrimaryKey(t => t.ResultId)
                .ForeignKey("dbo.Result", t => t.ResultId)
                .Index(t => t.ResultId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SignatureResult", "ResultId", "dbo.Result");
            DropForeignKey("dbo.ModeFieldResult", "ResultId", "dbo.Result");
            DropForeignKey("dbo.AttenuationResult", "ResultId", "dbo.Result");
            DropForeignKey("dbo.LengthResult", "ResultId", "dbo.Result");
            DropForeignKey("dbo.ModeFieldWaveResult", "ModeFieldResult_ResultId", "dbo.ModeFieldResult");
            DropForeignKey("dbo.Result", "SetHeader_ResultSetHeaderId", "dbo.ResultSetHeader");
            DropForeignKey("dbo.ResultSetLabel", "Header_ResultSetHeaderId", "dbo.ResultSetHeader");
            DropForeignKey("dbo.Result", "Instrument_SerialNumber", "dbo.Instrument");
            DropForeignKey("dbo.AttenuationWaveResult", "AttenResult_ResultId", "dbo.AttenuationResult");
            DropIndex("dbo.SignatureResult", new[] { "ResultId" });
            DropIndex("dbo.ModeFieldResult", new[] { "ResultId" });
            DropIndex("dbo.AttenuationResult", new[] { "ResultId" });
            DropIndex("dbo.LengthResult", new[] { "ResultId" });
            DropIndex("dbo.ModeFieldWaveResult", new[] { "ModeFieldResult_ResultId" });
            DropIndex("dbo.ResultSetLabel", new[] { "Header_ResultSetHeaderId" });
            DropIndex("dbo.Result", new[] { "SetHeader_ResultSetHeaderId" });
            DropIndex("dbo.Result", new[] { "Instrument_SerialNumber" });
            DropIndex("dbo.AttenuationWaveResult", new[] { "AttenResult_ResultId" });
            DropTable("dbo.SignatureResult");
            DropTable("dbo.ModeFieldResult");
            DropTable("dbo.AttenuationResult");
            DropTable("dbo.LengthResult");
            DropTable("dbo.ModeFieldWaveResult");
            DropTable("dbo.ResultSetLabel");
            DropTable("dbo.ResultSetHeader");
            DropTable("dbo.Instrument");
            DropTable("dbo.Result");
            DropTable("dbo.AttenuationWaveResult");
        }
    }
}
