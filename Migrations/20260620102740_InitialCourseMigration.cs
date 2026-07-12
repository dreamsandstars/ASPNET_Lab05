using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AspNetWeek3.Mvc.Migrations
{
    /// <inheritdoc />
    public partial class InitialCourseMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CourseCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CourseCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: ""),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AvailableSeats = table.Column<int>(type: "INTEGER", nullable: false),
                    CourseCategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Courses_CourseCategories_CourseCategoryId",
                        column: x => x.CourseCategoryId,
                        principalTable: "CourseCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Enrollments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudentId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Enrollments_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EnrollmentDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EnrollmentId = table.Column<int>(type: "INTEGER", nullable: false),
                    CourseId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnrollmentDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnrollmentDetails_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EnrollmentDetails_Enrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "Enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "CourseCategories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Programming" },
                    { 2, "Design" }
                });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "Id", "Email", "Name" },
                values: new object[,]
                {
                    { 1, "vana@gmail.com", "Nguyen Van A" },
                    { 2, "thib@gmail.com", "Tran Thi B" }
                });

            migrationBuilder.InsertData(
                table: "Courses",
                columns: new[] { "Id", "AvailableSeats", "CourseCategoryId", "CourseCode", "CreatedAt", "DeletedAt", "Description", "IsDeleted", "Name", "Price", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 15, 1, "CS101", new DateTime(2026, 6, 12, 12, 0, 0, 0, DateTimeKind.Unspecified), null, null, false, "Introduction to C#", 1200000m, null },
                    { 2, 3, 1, "CS201", new DateTime(2026, 6, 12, 12, 0, 0, 0, DateTimeKind.Unspecified), null, null, false, "ASP.NET Core Web MVC", 3500000m, null },
                    { 3, 8, 2, "DS101", new DateTime(2026, 6, 12, 12, 0, 0, 0, DateTimeKind.Unspecified), null, null, false, "UI/UX Design Fundamentals", 2000000m, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Courses_CourseCategoryId",
                table: "Courses",
                column: "CourseCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_CourseCode",
                table: "Courses",
                column: "CourseCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EnrollmentDetails_CourseId",
                table: "EnrollmentDetails",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_EnrollmentDetails_EnrollmentId",
                table: "EnrollmentDetails",
                column: "EnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_StudentId",
                table: "Enrollments",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnrollmentDetails");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "Enrollments");

            migrationBuilder.DropTable(
                name: "CourseCategories");

            migrationBuilder.DropTable(
                name: "Students");
        }
    }
}
