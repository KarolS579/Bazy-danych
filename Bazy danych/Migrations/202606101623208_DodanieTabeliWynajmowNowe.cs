namespace Bazy_danych.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DodanieTabeliWynajmowNowe : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Sprzets",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nazwa = c.String(),
                        Kategoria = c.String(),
                        Cena_wynajmu = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Status = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Wynajems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DataWypozyczenia = c.DateTime(nullable: false),
                        DataZwrotu = c.DateTime(),
                        KlientId = c.Int(nullable: false),
                        SprzetId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Klients", t => t.KlientId, cascadeDelete: true)
                .ForeignKey("dbo.Sprzets", t => t.SprzetId, cascadeDelete: true)
                .Index(t => t.KlientId)
                .Index(t => t.SprzetId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Wynajems", "SprzetId", "dbo.Sprzets");
            DropForeignKey("dbo.Wynajems", "KlientId", "dbo.Klients");
            DropIndex("dbo.Wynajems", new[] { "SprzetId" });
            DropIndex("dbo.Wynajems", new[] { "KlientId" });
            DropTable("dbo.Wynajems");
            DropTable("dbo.Sprzets");
        }
    }
}
