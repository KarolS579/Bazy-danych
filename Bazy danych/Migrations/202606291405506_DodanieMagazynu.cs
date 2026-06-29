namespace Bazy_danych.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class DodanieMagazynu : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sprzets", "MagazynId", c => c.Int());

            CreateIndex("dbo.Sprzets", "MagazynId");
            AddForeignKey("dbo.Sprzets", "MagazynId", "dbo.Magazyns", "Id");
        }

        public override void Down()
        {
            DropForeignKey("dbo.Sprzets", "MagazynId", "dbo.Magazyns");
            DropIndex("dbo.Sprzets", new[] { "MagazynId" });
            DropColumn("dbo.Sprzets", "MagazynId");
        }
    }
}