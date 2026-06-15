namespace Bazy_danych.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddPendingRegistrationTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PendingRegistrations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Email = c.String(nullable: false),
                        PasswordHash = c.String(nullable: false),
                        ConfirmationToken = c.String(nullable: false),
                        ExpiresAtUtc = c.DateTime(nullable: false),
                        CreatedAtUtc = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Email, unique: true, name: "IX_PendingRegistration_Email")
                .Index(t => t.ConfirmationToken, unique: true, name: "IX_PendingRegistration_Token");
        }

        public override void Down()
        {
            DropIndex("dbo.PendingRegistrations", "IX_PendingRegistration_Token");
            DropIndex("dbo.PendingRegistrations", "IX_PendingRegistration_Email");
            DropTable("dbo.PendingRegistrations");
        }
    }
}
