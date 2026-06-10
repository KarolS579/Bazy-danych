namespace Bazy_danych.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddKlienci : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Klients",
                c => new
                {
                    Id       = c.Int(nullable: false, identity: true),
                    Imie     = c.String(nullable: false),
                    Nazwisko = c.String(nullable: false),
                    Telefon  = c.String(),
                    Email    = c.String(),
                    Adres    = c.String(),
                    Firma    = c.String(),
                    Uwagi    = c.String(),
                })
                .PrimaryKey(t => t.Id);
        }

        public override void Down()
        {
            DropTable("dbo.Klients");
        }
    }
}
