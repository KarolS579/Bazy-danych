namespace Bazy_danych.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ZwiekszenieDlugosciEmail : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Klients", "Telefon", c => c.String(maxLength: 30));
            AlterColumn("dbo.Klients", "Email", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Klients", "Email", c => c.String(maxLength: 50));
            AlterColumn("dbo.Klients", "Telefon", c => c.String(maxLength: 30));
        }
    }
}
