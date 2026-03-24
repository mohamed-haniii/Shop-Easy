using Microsoft.EntityFrameworkCore.Migrations;

namespace ShopEasy.Migrations;

// US-034: Stored procedure GetPendingOrders created via migration
// Run: dotnet ef migrations add AddGetPendingOrdersSP
// Then: dotnet ef database update
public partial class AddGetPendingOrdersSP : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // US-034: Create stored procedure GetPendingOrders
        migrationBuilder.Sql(@"
            CREATE OR ALTER PROCEDURE shop.GetPendingOrders
            AS
            BEGIN
                SET NOCOUNT ON;
                SELECT *
                FROM   shop.Orders
                WHERE  Status = 'Pending'
                ORDER  BY PlacedAt DESC;
            END
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Drop procedure on rollback
        migrationBuilder.Sql("DROP PROCEDURE IF EXISTS shop.GetPendingOrders;");
    }
}
