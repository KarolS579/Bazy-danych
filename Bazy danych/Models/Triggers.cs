using System;
using Bazy_danych.Models;

namespace Bazy_danych
{
    public static class Triggers
    {
        public static void Initialize(ApplicationDbContext context)
        {
            try
            {
                // USUWANIE STAREGO TRIGGERA
                context.Database.ExecuteSqlCommand(@"
                    IF OBJECT_ID('[dbo].[trg_AutomatycznyStatusPoWynajmie]', 'TR') IS NOT NULL
                    BEGIN
                        DROP TRIGGER [dbo].[trg_AutomatycznyStatusPoWynajmie];
                    END
                ");

                // TWORZENIE NOWEGO, NIEZAWODNEGO TRIGGERA
                context.Database.ExecuteSqlCommand(@"
                    CREATE TRIGGER [dbo].[trg_AutomatycznyStatusPoWynajmie]
                    ON [dbo].[Wynajems]
                    AFTER INSERT, DELETE
                    AS
                    BEGIN
                        SET NOCOUNT ON;

                        -- REAKCJA NA DODANIE WYNAJMU (INSERT)
                        IF EXISTS (SELECT 1 FROM inserted)
                        BEGIN
                            UPDATE s
                            SET s.Status = 'Wynajęty'
                            FROM [dbo].[Sprzets] s
                            INNER JOIN inserted i ON s.Id = i.SprzetId;
                        END

                        -- REAKCJA NA USUNIĘCIE WYNAJMU (DELETE)
                        IF EXISTS (SELECT 1 FROM deleted)
                        BEGIN
                            UPDATE s
                            SET s.Status = 'Dostępny'
                            FROM [dbo].[Sprzets] s
                            INNER JOIN deleted d ON s.Id = d.SprzetId
                            -- Zabezpieczenie: Przywracaj 'Dostępny' TYLKO jeśli sprzęt faktycznie miał status 'Wynajęty'
                            -- Dzięki temu nie nadpiszemy statusu 'Serwis', jeśli ktoś go tam ręcznie wysłał!
                            WHERE s.Status = 'Wynajęty'; 
                        END
                    END
                ");

                context.Database.ExecuteSqlCommand(@"CREATE TRIGGER TR_Magazyny_StatusUpdate
                    ON Magazyns
                    AFTER INSERT, UPDATE
                    AS
                    BEGIN
                        SET NOCOUNT ON;
    
                        UPDATE m
                        SET m.Status = CASE 
                            WHEN m.ZajeteMiejsce >= m.Pojemnosc THEN 'Zapełniony'
                            ELSE 'Aktywny'
                        END
                        FROM Magazyns m
                        INNER JOIN inserted i ON m.Id = i.Id;
                    END
                ");

                context.Database.ExecuteSqlCommand(@"
                    IF NOT EXISTS (
                        SELECT * FROM sys.columns 
                        WHERE object_id = OBJECT_ID('dbo.Sprzets')
                        AND name = 'NumerSeryjny'
                    )
                    BEGIN
                        ALTER TABLE dbo.Sprzets ADD NumerSeryjny NVARCHAR(50) NULL;
                    END
                ");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Błąd inicjalizacji/naprawy bazy: " + ex.Message);
            }
        }
    }
}