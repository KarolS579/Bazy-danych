namespace Bazy_danych.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DodanieTabeliSerwisu : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Serwises",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DataRozpoczecia = c.DateTime(nullable: false),
                        DataZakonczenia = c.DateTime(),
                        Opis = c.String(),
                        SprzetId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Sprzets", t => t.SprzetId, cascadeDelete: true)
                .Index(t => t.SprzetId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Serwises", "SprzetId", "dbo.Sprzets");
            DropIndex("dbo.Serwises", new[] { "SprzetId" });
            DropTable("dbo.Serwises");
        }
    }
}
