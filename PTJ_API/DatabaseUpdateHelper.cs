using Microsoft.EntityFrameworkCore;
using Models.Models;

namespace API
{
    public static class DatabaseUpdateHelper
    {
        public static void UpdateDatabase(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CarManagerContext>();
            
            try 
            {
                Console.WriteLine("Updating database: removing GSHT/IMEI columns...");

                // Helper SQL to drop default constraints and columns for:
                // 1. bulk_purchase_detail (has_gsht)
                // 2. vehicle (telematics_imei)
                // 3. vehicle_reception_record (telematics_imei)

                string[] dropCommands = new string[] 
                {
                    GenerateDropColumnSql("bulk_purchase_detail", "has_gsht"),
                    GenerateDropColumnSql("vehicle", "telematics_imei"),
                    GenerateDropColumnSql("vehicle_reception_record", "telematics_imei"),
                    GenerateDropColumnSql("bulk_purchase_detail", "has_camera_158")
                };

                foreach (var sql in dropCommands)
                {
                    context.Database.ExecuteSqlRaw(sql);
                }
                
                Console.WriteLine("Database columns updated successfully (GSHT removed).");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating database: {ex.Message}");
            }
        }

        private static string GenerateDropColumnSql(string tableName, string columnName)
        {
            return $@"
                DECLARE @ConstraintName nvarchar(200);
                SELECT @ConstraintName = Name FROM sys.default_constraints
                WHERE parent_object_id = OBJECT_ID('{tableName}')
                AND parent_column_id = (SELECT column_id FROM sys.columns WHERE name = '{columnName}' AND object_id = OBJECT_ID('{tableName}'));
                
                IF @ConstraintName IS NOT NULL
                    EXEC('ALTER TABLE {tableName} DROP CONSTRAINT ' + @ConstraintName);
                
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('{tableName}') AND name = '{columnName}')
                    BEGIN
                        EXEC('ALTER TABLE {tableName} DROP COLUMN {columnName}');
                    END
            ";
        }
    }
}
