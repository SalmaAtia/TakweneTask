using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TrackManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Artists",
                columns: new[] { "Id", "Country", "Email", "Name" },
                values: new object[,]
                {
                    { new Guid("a0000000-0000-0000-0000-000000000001"), "EG", "nour.elsayed@example.com", "Nour El Sayed" },
                    { new Guid("a0000000-0000-0000-0000-000000000002"), "SA", "faisal.alharbi@example.com", "Faisal Al-Harbi" },
                    { new Guid("a0000000-0000-0000-0000-000000000003"), "LB", "lina.haddad@example.com", "Lina Haddad" },
                    { new Guid("a0000000-0000-0000-0000-000000000004"), "MA", "yassine.benali@example.com", "Yassine Benali" }
                });

            migrationBuilder.InsertData(
                table: "Dsps",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("d0000000-0000-0000-0000-000000000001"), "Spotify" },
                    { new Guid("d0000000-0000-0000-0000-000000000002"), "Apple Music" },
                    { new Guid("d0000000-0000-0000-0000-000000000003"), "YouTube Music" }
                });

            migrationBuilder.InsertData(
                table: "Tracks",
                columns: new[] { "Id", "ArtistId", "Genre", "Isrc", "ReleaseDate", "Status", "Title" },
                values: new object[,]
                {
                    { new Guid("b0000000-0000-0000-0000-000000000001"), new Guid("a0000000-0000-0000-0000-000000000001"), "Mahraganat", "EG-NES-25-00001", new DateOnly(2025, 3, 14), "Distributed", "Shobra Nights" },
                    { new Guid("b0000000-0000-0000-0000-000000000002"), new Guid("a0000000-0000-0000-0000-000000000001"), "Mahraganat", "EG-NES-26-00002", new DateOnly(2026, 11, 27), "Submitted", "Tok Tok Anthem" },
                    { new Guid("b0000000-0000-0000-0000-000000000003"), new Guid("a0000000-0000-0000-0000-000000000001"), "Hip-Hop", "EG-NES-26-00003", new DateOnly(2027, 1, 15), "Draft", "Cairo Concrete" },
                    { new Guid("b0000000-0000-0000-0000-000000000004"), new Guid("a0000000-0000-0000-0000-000000000002"), "Khaleeji", "SA-FAH-25-00001", new DateOnly(2025, 6, 20), "Distributed", "Desert Rain" },
                    { new Guid("b0000000-0000-0000-0000-000000000005"), new Guid("a0000000-0000-0000-0000-000000000002"), "Khaleeji", "SA-FAH-26-00002", new DateOnly(2026, 9, 12), "Distributed", "Najd Moon" },
                    { new Guid("b0000000-0000-0000-0000-000000000006"), new Guid("a0000000-0000-0000-0000-000000000002"), "Arabic Pop", "SA-FAH-26-00003", new DateOnly(2027, 2, 1), "Draft", "Riyadh Lights" },
                    { new Guid("b0000000-0000-0000-0000-000000000007"), new Guid("a0000000-0000-0000-0000-000000000003"), "Arabic Pop", "LB-LHD-25-00001", new DateOnly(2025, 9, 5), "Distributed", "Beirut Balcony" },
                    { new Guid("b0000000-0000-0000-0000-000000000008"), new Guid("a0000000-0000-0000-0000-000000000003"), "Arabic Pop", "LB-LHD-26-00002", new DateOnly(2026, 11, 20), "Submitted", "Cedar Heart" },
                    { new Guid("b0000000-0000-0000-0000-000000000009"), new Guid("a0000000-0000-0000-0000-000000000004"), "Rai", "MA-YBN-26-00001", new DateOnly(2026, 12, 4), "Submitted", "Oujda Sunset" },
                    { new Guid("b0000000-0000-0000-0000-000000000010"), new Guid("a0000000-0000-0000-0000-000000000004"), "Hip-Hop", "MA-YBN-26-00002", new DateOnly(2027, 3, 10), "Draft", "Casa Flow" }
                });

            migrationBuilder.InsertData(
                table: "TrackDistributions",
                columns: new[] { "Id", "DspId", "Status", "SubmittedAt", "TrackId" },
                values: new object[,]
                {
                    { new Guid("c0000000-0000-0000-0000-000000000001"), new Guid("d0000000-0000-0000-0000-000000000001"), "Live", new DateTimeOffset(new DateTime(2025, 2, 20, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("b0000000-0000-0000-0000-000000000001") },
                    { new Guid("c0000000-0000-0000-0000-000000000002"), new Guid("d0000000-0000-0000-0000-000000000003"), "Live", new DateTimeOffset(new DateTime(2025, 2, 20, 9, 5, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("b0000000-0000-0000-0000-000000000001") },
                    { new Guid("c0000000-0000-0000-0000-000000000003"), new Guid("d0000000-0000-0000-0000-000000000002"), "Rejected", new DateTimeOffset(new DateTime(2025, 2, 20, 9, 10, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("b0000000-0000-0000-0000-000000000001") },
                    { new Guid("c0000000-0000-0000-0000-000000000004"), new Guid("d0000000-0000-0000-0000-000000000001"), "Pending", new DateTimeOffset(new DateTime(2026, 10, 15, 11, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("b0000000-0000-0000-0000-000000000002") },
                    { new Guid("c0000000-0000-0000-0000-000000000005"), new Guid("d0000000-0000-0000-0000-000000000003"), "Pending", new DateTimeOffset(new DateTime(2026, 10, 15, 11, 35, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("b0000000-0000-0000-0000-000000000002") },
                    { new Guid("c0000000-0000-0000-0000-000000000006"), new Guid("d0000000-0000-0000-0000-000000000001"), "Live", new DateTimeOffset(new DateTime(2025, 5, 28, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("b0000000-0000-0000-0000-000000000004") },
                    { new Guid("c0000000-0000-0000-0000-000000000007"), new Guid("d0000000-0000-0000-0000-000000000002"), "Live", new DateTimeOffset(new DateTime(2025, 5, 28, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("b0000000-0000-0000-0000-000000000004") },
                    { new Guid("c0000000-0000-0000-0000-000000000008"), new Guid("d0000000-0000-0000-0000-000000000003"), "Live", new DateTimeOffset(new DateTime(2025, 5, 28, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("b0000000-0000-0000-0000-000000000004") },
                    { new Guid("c0000000-0000-0000-0000-000000000009"), new Guid("d0000000-0000-0000-0000-000000000001"), "Live", new DateTimeOffset(new DateTime(2026, 8, 25, 14, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("b0000000-0000-0000-0000-000000000005") },
                    { new Guid("c0000000-0000-0000-0000-000000000010"), new Guid("d0000000-0000-0000-0000-000000000002"), "Pending", new DateTimeOffset(new DateTime(2026, 8, 25, 14, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("b0000000-0000-0000-0000-000000000005") },
                    { new Guid("c0000000-0000-0000-0000-000000000011"), new Guid("d0000000-0000-0000-0000-000000000002"), "Live", new DateTimeOffset(new DateTime(2025, 8, 18, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("b0000000-0000-0000-0000-000000000007") },
                    { new Guid("c0000000-0000-0000-0000-000000000012"), new Guid("d0000000-0000-0000-0000-000000000001"), "Live", new DateTimeOffset(new DateTime(2025, 8, 18, 10, 15, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("b0000000-0000-0000-0000-000000000007") },
                    { new Guid("c0000000-0000-0000-0000-000000000013"), new Guid("d0000000-0000-0000-0000-000000000002"), "Pending", new DateTimeOffset(new DateTime(2026, 10, 18, 16, 45, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("b0000000-0000-0000-0000-000000000008") },
                    { new Guid("c0000000-0000-0000-0000-000000000014"), new Guid("d0000000-0000-0000-0000-000000000001"), "Pending", new DateTimeOffset(new DateTime(2026, 10, 20, 7, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("b0000000-0000-0000-0000-000000000009") },
                    { new Guid("c0000000-0000-0000-0000-000000000015"), new Guid("d0000000-0000-0000-0000-000000000002"), "Pending", new DateTimeOffset(new DateTime(2026, 10, 20, 7, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("b0000000-0000-0000-0000-000000000009") },
                    { new Guid("c0000000-0000-0000-0000-000000000016"), new Guid("d0000000-0000-0000-0000-000000000003"), "Pending", new DateTimeOffset(new DateTime(2026, 10, 20, 7, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("b0000000-0000-0000-0000-000000000009") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000012"));

            migrationBuilder.DeleteData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000013"));

            migrationBuilder.DeleteData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000014"));

            migrationBuilder.DeleteData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000015"));

            migrationBuilder.DeleteData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000016"));

            migrationBuilder.DeleteData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: new Guid("b0000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: new Guid("b0000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: new Guid("b0000000-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "Dsps",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Dsps",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Dsps",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: new Guid("b0000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: new Guid("b0000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: new Guid("b0000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: new Guid("b0000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: new Guid("b0000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: new Guid("b0000000-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: new Guid("b0000000-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "Artists",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Artists",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Artists",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Artists",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000004"));
        }
    }
}
