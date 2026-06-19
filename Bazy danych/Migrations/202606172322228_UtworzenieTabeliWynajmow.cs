namespace Bazy_danych.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UtworzenieTabeliWynajmow : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Wynajems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DataWynajmu = c.DateTime(nullable: false),
                        DataZwrotu = c.DateTime(),
                        SprzetId = c.Int(nullable: false),
                        KlientId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Klients", t => t.KlientId, cascadeDelete: true)
                .ForeignKey("dbo.Sprzets", t => t.SprzetId, cascadeDelete: true)
                .Index(t => t.SprzetId)
                .Index(t => t.KlientId);
            
            AlterColumn("dbo.Klients", "Imie", c => c.String(nullable: false, maxLength: 10));
            AlterColumn("dbo.Klients", "Nazwisko", c => c.String(nullable: false, maxLength: 10));
            AlterColumn("dbo.Klients", "Telefon", c => c.String(maxLength: 10));
            AlterColumn("dbo.Klients", "Email", c => c.String(maxLength: 15));
            AlterColumn("dbo.Klients", "Adres", c => c.String(maxLength: 20));
            AlterColumn("dbo.Klients", "Firma", c => c.String(maxLength: 10));
            AlterColumn("dbo.Klients", "Uwagi", c => c.String(maxLength: 100));
            AlterColumn("dbo.Magazyns", "Nazwa", c => c.String(nullable: false, maxLength: 20));
            AlterColumn("dbo.Magazyns", "Lokalizacja", c => c.String(nullable: false, maxLength: 20));
            AlterColumn("dbo.Sprzets", "Nazwa", c => c.String(nullable: false, maxLength: 20));
            AlterColumn("dbo.Sprzets", "Kategoria", c => c.String(nullable: false, maxLength: 20));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Wynajems", "SprzetId", "dbo.Sprzets");
            DropForeignKey("dbo.Wynajems", "KlientId", "dbo.Klients");
            DropIndex("dbo.Wynajems", new[] { "KlientId" });
            DropIndex("dbo.Wynajems", new[] { "SprzetId" });
            AlterColumn("dbo.Sprzets", "Kategoria", c => c.String(nullable: false, maxLength: 256));
            AlterColumn("dbo.Sprzets", "Nazwa", c => c.String(nullable: false, maxLength: 256));
            AlterColumn("dbo.Magazyns", "Lokalizacja", c => c.String(nullable: false, maxLength: 256));
            AlterColumn("dbo.Magazyns", "Nazwa", c => c.String(nullable: false, maxLength: 256));
            AlterColumn("dbo.Klients", "Uwagi", c => c.String(maxLength: 1000));
            AlterColumn("dbo.Klients", "Firma", c => c.String(maxLength: 100));
            AlterColumn("dbo.Klients", "Adres", c => c.String(maxLength: 200));
            AlterColumn("dbo.Klients", "Email", c => c.String(maxLength: 100));
            AlterColumn("dbo.Klients", "Telefon", c => c.String(maxLength: 20));
            AlterColumn("dbo.Klients", "Nazwisko", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.Klients", "Imie", c => c.String(nullable: false, maxLength: 50));
            DropTable("dbo.Wynajems");
        }
    }
}
