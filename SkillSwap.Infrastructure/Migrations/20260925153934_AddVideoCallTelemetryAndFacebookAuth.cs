using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillSwap.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoCallTelemetryAndFacebookAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FacebookId",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ActualDurationMinutes",
                table: "SwapSessions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualEndTime",
                table: "SwapSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualStartTime",
                table: "SwapSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HostJoinedAt",
                table: "SwapSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HostLastHeartbeatAt",
                table: "SwapSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsHostInCall",
                table: "SwapSessions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsParticipantInCall",
                table: "SwapSessions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ParticipantJoinedAt",
                table: "SwapSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ParticipantLastHeartbeatAt",
                table: "SwapSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoomSecurityToken",
                table: "SwapSessions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FacebookId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ActualDurationMinutes",
                table: "SwapSessions");

            migrationBuilder.DropColumn(
                name: "ActualEndTime",
                table: "SwapSessions");

            migrationBuilder.DropColumn(
                name: "ActualStartTime",
                table: "SwapSessions");

            migrationBuilder.DropColumn(
                name: "HostJoinedAt",
                table: "SwapSessions");

            migrationBuilder.DropColumn(
                name: "HostLastHeartbeatAt",
                table: "SwapSessions");

            migrationBuilder.DropColumn(
                name: "IsHostInCall",
                table: "SwapSessions");

            migrationBuilder.DropColumn(
                name: "IsParticipantInCall",
                table: "SwapSessions");

            migrationBuilder.DropColumn(
                name: "ParticipantJoinedAt",
                table: "SwapSessions");

            migrationBuilder.DropColumn(
                name: "ParticipantLastHeartbeatAt",
                table: "SwapSessions");

            migrationBuilder.DropColumn(
                name: "RoomSecurityToken",
                table: "SwapSessions");
        }
    }
}
