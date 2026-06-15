namespace Bazy_danych.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddSprzetTableAndCreatedDate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Sprzet",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nazwa = c.String(),
                        Kategoria = c.String(),
                        Cena_wynajmu = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Status = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
        }

        public override void Down()
        {
            DropTable("dbo.Sprzet");
        }
    }
}
