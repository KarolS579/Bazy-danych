namespace Bazy_danych.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class WlaczenieBlokadyUsuwania : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Wynajems", "SprzetId", "dbo.Sprzets");
            AddForeignKey("dbo.Wynajems", "SprzetId", "dbo.Sprzets", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Wynajems", "SprzetId", "dbo.Sprzets");
            AddForeignKey("dbo.Wynajems", "SprzetId", "dbo.Sprzets", "Id", cascadeDelete: true);
        }
    }
}
