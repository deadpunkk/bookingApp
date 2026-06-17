using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUserBookingRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "CreatedByUserId",
                table: "Bookings",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CreatedByUserId",
                table: "Bookings",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Users_CreatedByUserId",
                table: "Bookings",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Users_CreatedByUserId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_CreatedByUserId",
                table: "Bookings");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByUserId",
                table: "Bookings",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");
        }
    }
}
