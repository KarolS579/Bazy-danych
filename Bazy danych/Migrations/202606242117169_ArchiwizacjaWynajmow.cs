using System;
using System.Data.Entity.Migrations;

namespace Bazy_danych.Migrations
{
    public partial class ArchiwizacjaWynajmow : DbMigration
    {
        public override void Up()
        {
            // 1. Próba utworzenia tabeli archiwalnej (jeśli jeszcze nie istnieje w bazie)
            Sql(@"
                IF OBJECT_ID('dbo.ArchiwumWynajmow', 'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.ArchiwumWynajmow (
                        Id INT NOT NULL,
                        DataWynajmu DATETIME NOT NULL,
                        DataZwrotu DATETIME NULL,
                        SprzetId INT NOT NULL,
                        KlientId INT NOT NULL,
                        DataArchiwizacji DATETIME NOT NULL DEFAULT GETDATE(),
                        CONSTRAINT PK_ArchiwumWynajmow PRIMARY KEY (Id)
                    );
                END
            ");

            // 2. Tworzenie lub aktualizacja procedury składowanej z poprawną nazwą tabeli (dbo.Wynajmy)
            Sql(@"
                CREATE OR ALTER PROCEDURE dbo.ZarchiwizujStareWynajmy
                    @DataGraniczna DATE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    
                    BEGIN TRANSACTION;
                    BEGIN TRY

                        -- KROK A: Kopiujemy stare, zamknięte wynajmy do tabeli Archiwum
                        INSERT INTO dbo.ArchiwumWynajmow (Id, DataWynajmu, DataZwrotu, SprzetId, KlientId)
                        SELECT Id, DataWynajmu, DataZwrotu, SprzetId, KlientId
                        FROM dbo.Wynajmy
                        WHERE DataZwrotu IS NOT NULL AND DataZwrotu <= @DataGraniczna;

                        -- KROK B: Usuwamy skopiowane rekordy z głównej tabeli operacyjnej
                        DELETE FROM dbo.Wynajmy
                        WHERE DataZwrotu IS NOT NULL AND DataZwrotu <= @DataGraniczna;

                        COMMIT TRANSACTION;
                    END TRY
                    BEGIN CATCH
                        -- Bezpieczne cofanie transakcji w przypadku błędu
                        IF @@TRANCOUNT > 0 
                        BEGIN
                            ROLLBACK TRANSACTION;
                        END
                        THROW;
                    END CATCH
                END
            ");
        }

        public override void Down()
        {
            Sql("DROP PROCEDURE IF EXISTS dbo.ZarchiwizujStareWynajmy");
            Sql("DROP TABLE IF EXISTS dbo.ArchiwumWynajmow");
        }
    }
}