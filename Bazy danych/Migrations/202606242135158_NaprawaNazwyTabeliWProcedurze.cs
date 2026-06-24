using System;
using System.Data.Entity.Migrations;

namespace Bazy_danych.Migrations
{
    public partial class NaprawaNazwyTabeliWProcedurze : DbMigration
    {
        public override void Up()
        {
            // Aktualizujemy kod procedury w bazie, podmieniając błędne Wynajmys na poprawne Wynajmy
            Sql(@"
                ALTER PROCEDURE dbo.ZarchiwizujStareWynajmy
                    @DataGraniczna DATE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    
                    BEGIN TRANSACTION;
                    BEGIN TRY

                        -- KROK A: Kopiujemy stare dane z poprawnej tabeli
                        INSERT INTO dbo.ArchiwumWynajmow (Id, DataWynajmu, DataZwrotu, SprzetId, KlientId)
                        SELECT Id, DataWynajmu, DataZwrotu, SprzetId, KlientId
                        FROM dbo.Wynajmy
                        WHERE DataZwrotu IS NOT NULL AND DataZwrotu <= @DataGraniczna;

                        -- KROK B: Usuwamy stare rekordy z poprawnej tabeli
                        DELETE FROM dbo.Wynajmy
                        WHERE DataZwrotu IS NOT NULL AND DataZwrotu <= @DataGraniczna;

                        COMMIT TRANSACTION;
                    END TRY
                    BEGIN CATCH
                        -- Bezpieczne sprzątanie transakcji
                        IF @@TRANCOUNT > 0 
                        BEGIN
                            ROLLBACK TRANSACTION;
                        END
                        -- Rzucamy błąd dalej, by aplikacja wiedziała, że coś poszło nie tak
                        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
                        RAISERROR(@ErrorMessage, 16, 1);
                    END CATCH
                END
            ");
        }

        public override void Down()
        {
            // W razie wycofania, nie musimy robić nic szczególnego
        }
    }
}