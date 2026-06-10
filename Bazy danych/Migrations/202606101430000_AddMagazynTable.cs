namespace Bazy_danych.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddMagazynTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Magazyns",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nazwa = c.String(nullable: false),
                        Lokalizacja = c.String(nullable: false),
                        Pojemnosc = c.Int(nullable: false),
                        ZajeteMiejsce = c.Int(nullable: false),
                        Status = c.String(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);

        }

        public override void Down()
        {
            DropTable("dbo.Magazyns");
        }
    }
}
