using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymFit.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MembershipOffers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Price = table.Column<double>(type: "REAL", nullable: false),
                    ValidityDays = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipOffers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    IsBlocked = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClientMemberships",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    MembershipOfferId = table.Column<int>(type: "INTEGER", nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PaymentStatus = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientMemberships_MembershipOffers_MembershipOfferId",
                        column: x => x.MembershipOfferId,
                        principalTable: "MembershipOffers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientMemberships_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainerProfiles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Biography = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Specializations = table.Column<string>(type: "TEXT", nullable: false),
                    PhotoUrl = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainerProfiles", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_TrainerProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupActivities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DurationInMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    CapacityLimit = table.Column<int>(type: "INTEGER", nullable: false),
                    TrainerId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupActivities_TrainerProfiles_TrainerId",
                        column: x => x.TrainerId,
                        principalTable: "TrainerProfiles",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupActivityReservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    GroupActivityId = table.Column<int>(type: "INTEGER", nullable: false),
                    BookingDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsPresent = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupActivityReservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupActivityReservations_GroupActivities_GroupActivityId",
                        column: x => x.GroupActivityId,
                        principalTable: "GroupActivities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupActivityReservations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientMemberships_MembershipOfferId",
                table: "ClientMemberships",
                column: "MembershipOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientMemberships_UserId",
                table: "ClientMemberships",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupActivities_TrainerId",
                table: "GroupActivities",
                column: "TrainerId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupActivityReservations_GroupActivityId",
                table: "GroupActivityReservations",
                column: "GroupActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupActivityReservations_UserId",
                table: "GroupActivityReservations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientMemberships");

            migrationBuilder.DropTable(
                name: "GroupActivityReservations");

            migrationBuilder.DropTable(
                name: "MembershipOffers");

            migrationBuilder.DropTable(
                name: "GroupActivities");

            migrationBuilder.DropTable(
                name: "TrainerProfiles");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
