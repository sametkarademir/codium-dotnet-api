using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Codium.Template.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Initial_Create : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppPermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", maxLength: 256, nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeleterId = table.Column<Guid>(type: "uuid", maxLength: 256, nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppPermissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", maxLength: 256, nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeleterId = table.Column<Guid>(type: "uuid", maxLength: 256, nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PasswordHash = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    ShouldChangePasswordOnNextLogin = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordChangedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    FirstName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    LastName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", maxLength: 256, nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeleterId = table.Column<Guid>(type: "uuid", maxLength: 256, nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppRolePermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", maxLength: 256, nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeleterId = table.Column<Guid>(type: "uuid", maxLength: 256, nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRolePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppRolePermissions_AppPermissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "AppPermissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppRolePermissions_AppRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AppRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    EntityName = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppAuditLogs_AppUsers_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "AppUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AppHttpRequestLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HttpMethod = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    RequestPath = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    QueryString = table.Column<string>(type: "text", nullable: true),
                    RequestBody = table.Column<string>(type: "text", nullable: true),
                    RequestHeaders = table.Column<string>(type: "text", nullable: true),
                    StatusCode = table.Column<int>(type: "integer", nullable: true),
                    RequestTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResponseTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DurationMs = table.Column<long>(type: "bigint", nullable: true),
                    ClientIp = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    DeviceFamily = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DeviceModel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OsFamily = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OsVersion = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    BrowserFamily = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    BrowserVersion = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    IsMobile = table.Column<bool>(type: "boolean", nullable: false),
                    IsTablet = table.Column<bool>(type: "boolean", nullable: false),
                    IsDesktop = table.Column<bool>(type: "boolean", nullable: false),
                    ControllerName = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ActionName = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppHttpRequestLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppHttpRequestLogs_AppUsers_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "AppUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AppSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    RevokedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClientIp = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    UserAgent = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    DeviceFamily = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DeviceModel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OsFamily = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OsVersion = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    BrowserFamily = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    BrowserVersion = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    IsMobile = table.Column<bool>(type: "boolean", nullable: false),
                    IsDesktop = table.Column<bool>(type: "boolean", nullable: false),
                    IsTablet = table.Column<bool>(type: "boolean", nullable: false),
                    SnapshotId = table.Column<Guid>(type: "uuid", nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", maxLength: 256, nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeleterId = table.Column<Guid>(type: "uuid", maxLength: 256, nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppSessions_AppUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppUserRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", maxLength: 256, nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeleterId = table.Column<Guid>(type: "uuid", maxLength: 256, nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppUserRoles_AppRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AppRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppUserRoles_AppUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppEntityPropertyChanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PropertyTypeFullName = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    NewValue = table.Column<string>(type: "text", nullable: true),
                    OriginalValue = table.Column<string>(type: "text", nullable: true),
                    AuditLogId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppEntityPropertyChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppEntityPropertyChanges_AppAuditLogs_AuditLogId",
                        column: x => x.AuditLogId,
                        principalTable: "AppAuditLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppRefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ExpiryTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    ReplacedByToken = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false),
                    RevokedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", maxLength: 256, nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeleterId = table.Column<Guid>(type: "uuid", maxLength: 256, nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppRefreshTokens_AppSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "AppSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppRefreshTokens_AppUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppAuditLogs_CreationTime",
                table: "AppAuditLogs",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_AppAuditLogs_CreatorId",
                table: "AppAuditLogs",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_AppAuditLogs_EntityId",
                table: "AppAuditLogs",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_AppAuditLogs_EntityName",
                table: "AppAuditLogs",
                column: "EntityName");

            migrationBuilder.CreateIndex(
                name: "IX_AppAuditLogs_State",
                table: "AppAuditLogs",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_AppEntityPropertyChanges_AuditLogId",
                table: "AppEntityPropertyChanges",
                column: "AuditLogId");

            migrationBuilder.CreateIndex(
                name: "IX_AppEntityPropertyChanges_CreationTime",
                table: "AppEntityPropertyChanges",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_AppEntityPropertyChanges_CreatorId",
                table: "AppEntityPropertyChanges",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHttpRequestLogs_ActionName",
                table: "AppHttpRequestLogs",
                column: "ActionName");

            migrationBuilder.CreateIndex(
                name: "IX_AppHttpRequestLogs_BrowserFamily",
                table: "AppHttpRequestLogs",
                column: "BrowserFamily");

            migrationBuilder.CreateIndex(
                name: "IX_AppHttpRequestLogs_BrowserVersion",
                table: "AppHttpRequestLogs",
                column: "BrowserVersion");

            migrationBuilder.CreateIndex(
                name: "IX_AppHttpRequestLogs_ClientIp",
                table: "AppHttpRequestLogs",
                column: "ClientIp");

            migrationBuilder.CreateIndex(
                name: "IX_AppHttpRequestLogs_ControllerName",
                table: "AppHttpRequestLogs",
                column: "ControllerName");

            migrationBuilder.CreateIndex(
                name: "IX_AppHttpRequestLogs_CorrelationId",
                table: "AppHttpRequestLogs",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHttpRequestLogs_CreationTime",
                table: "AppHttpRequestLogs",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_AppHttpRequestLogs_CreatorId",
                table: "AppHttpRequestLogs",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHttpRequestLogs_DeviceFamily",
                table: "AppHttpRequestLogs",
                column: "DeviceFamily");

            migrationBuilder.CreateIndex(
                name: "IX_AppHttpRequestLogs_DeviceModel",
                table: "AppHttpRequestLogs",
                column: "DeviceModel");

            migrationBuilder.CreateIndex(
                name: "IX_AppHttpRequestLogs_HttpMethod",
                table: "AppHttpRequestLogs",
                column: "HttpMethod");

            migrationBuilder.CreateIndex(
                name: "IX_AppHttpRequestLogs_OsFamily",
                table: "AppHttpRequestLogs",
                column: "OsFamily");

            migrationBuilder.CreateIndex(
                name: "IX_AppHttpRequestLogs_OsVersion",
                table: "AppHttpRequestLogs",
                column: "OsVersion");

            migrationBuilder.CreateIndex(
                name: "IX_AppHttpRequestLogs_RequestPath",
                table: "AppHttpRequestLogs",
                column: "RequestPath");

            migrationBuilder.CreateIndex(
                name: "IX_AppHttpRequestLogs_SessionId",
                table: "AppHttpRequestLogs",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_AppPermissions_CreationTime",
                table: "AppPermissions",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_AppPermissions_CreatorId",
                table: "AppPermissions",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_AppPermissions_DeleterId",
                table: "AppPermissions",
                column: "DeleterId");

            migrationBuilder.CreateIndex(
                name: "IX_AppPermissions_DeletionTime",
                table: "AppPermissions",
                column: "DeletionTime");

            migrationBuilder.CreateIndex(
                name: "IX_AppPermissions_IsDeleted",
                table: "AppPermissions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_AppPermissions_LastModificationTime",
                table: "AppPermissions",
                column: "LastModificationTime");

            migrationBuilder.CreateIndex(
                name: "IX_AppPermissions_LastModifierId",
                table: "AppPermissions",
                column: "LastModifierId");

            migrationBuilder.CreateIndex(
                name: "IX_AppPermissions_NormalizedName",
                table: "AppPermissions",
                column: "NormalizedName",
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_AppRefreshTokens_CreationTime",
                table: "AppRefreshTokens",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_AppRefreshTokens_CreatorId",
                table: "AppRefreshTokens",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_AppRefreshTokens_DeleterId",
                table: "AppRefreshTokens",
                column: "DeleterId");

            migrationBuilder.CreateIndex(
                name: "IX_AppRefreshTokens_DeletionTime",
                table: "AppRefreshTokens",
                column: "DeletionTime");

            migrationBuilder.CreateIndex(
                name: "IX_AppRefreshTokens_IsDeleted",
                table: "AppRefreshTokens",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_AppRefreshTokens_LastModificationTime",
                table: "AppRefreshTokens",
                column: "LastModificationTime");

            migrationBuilder.CreateIndex(
                name: "IX_AppRefreshTokens_LastModifierId",
                table: "AppRefreshTokens",
                column: "LastModifierId");

            migrationBuilder.CreateIndex(
                name: "IX_AppRefreshTokens_SessionId",
                table: "AppRefreshTokens",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_AppRefreshTokens_UserId_Token",
                table: "AppRefreshTokens",
                columns: new[] { "UserId", "Token" },
                unique: true,
                filter: "\"IsDeleted\" = FALSE AND \"IsUsed\" = FALSE AND \"IsRevoked\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_AppRefreshTokens_UserId_Token_ExpiryTime_IsUsed_IsRevoked",
                table: "AppRefreshTokens",
                columns: new[] { "UserId", "Token", "ExpiryTime", "IsUsed", "IsRevoked" });

            migrationBuilder.CreateIndex(
                name: "IX_AppRolePermissions_CreationTime",
                table: "AppRolePermissions",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_AppRolePermissions_CreatorId",
                table: "AppRolePermissions",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_AppRolePermissions_DeleterId",
                table: "AppRolePermissions",
                column: "DeleterId");

            migrationBuilder.CreateIndex(
                name: "IX_AppRolePermissions_DeletionTime",
                table: "AppRolePermissions",
                column: "DeletionTime");

            migrationBuilder.CreateIndex(
                name: "IX_AppRolePermissions_IsDeleted",
                table: "AppRolePermissions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_AppRolePermissions_LastModificationTime",
                table: "AppRolePermissions",
                column: "LastModificationTime");

            migrationBuilder.CreateIndex(
                name: "IX_AppRolePermissions_LastModifierId",
                table: "AppRolePermissions",
                column: "LastModifierId");

            migrationBuilder.CreateIndex(
                name: "IX_AppRolePermissions_PermissionId",
                table: "AppRolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_AppRolePermissions_RoleId_PermissionId",
                table: "AppRolePermissions",
                columns: new[] { "RoleId", "PermissionId" },
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_AppRoles_CreationTime",
                table: "AppRoles",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_AppRoles_CreatorId",
                table: "AppRoles",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_AppRoles_DeleterId",
                table: "AppRoles",
                column: "DeleterId");

            migrationBuilder.CreateIndex(
                name: "IX_AppRoles_DeletionTime",
                table: "AppRoles",
                column: "DeletionTime");

            migrationBuilder.CreateIndex(
                name: "IX_AppRoles_IsDeleted",
                table: "AppRoles",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_AppRoles_LastModificationTime",
                table: "AppRoles",
                column: "LastModificationTime");

            migrationBuilder.CreateIndex(
                name: "IX_AppRoles_LastModifierId",
                table: "AppRoles",
                column: "LastModifierId");

            migrationBuilder.CreateIndex(
                name: "IX_AppRoles_NormalizedName",
                table: "AppRoles",
                column: "NormalizedName",
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_AppSessions_ClientIp",
                table: "AppSessions",
                column: "ClientIp");

            migrationBuilder.CreateIndex(
                name: "IX_AppSessions_CorrelationId",
                table: "AppSessions",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSessions_CreationTime",
                table: "AppSessions",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_AppSessions_CreatorId",
                table: "AppSessions",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSessions_DeleterId",
                table: "AppSessions",
                column: "DeleterId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSessions_DeletionTime",
                table: "AppSessions",
                column: "DeletionTime");

            migrationBuilder.CreateIndex(
                name: "IX_AppSessions_IsDeleted",
                table: "AppSessions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_AppSessions_IsDesktop",
                table: "AppSessions",
                column: "IsDesktop");

            migrationBuilder.CreateIndex(
                name: "IX_AppSessions_IsMobile",
                table: "AppSessions",
                column: "IsMobile");

            migrationBuilder.CreateIndex(
                name: "IX_AppSessions_IsTablet",
                table: "AppSessions",
                column: "IsTablet");

            migrationBuilder.CreateIndex(
                name: "IX_AppSessions_LastModificationTime",
                table: "AppSessions",
                column: "LastModificationTime");

            migrationBuilder.CreateIndex(
                name: "IX_AppSessions_LastModifierId",
                table: "AppSessions",
                column: "LastModifierId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSessions_SnapshotId",
                table: "AppSessions",
                column: "SnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSessions_UserId",
                table: "AppSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppUserRoles_CreationTime",
                table: "AppUserRoles",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_AppUserRoles_CreatorId",
                table: "AppUserRoles",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_AppUserRoles_DeleterId",
                table: "AppUserRoles",
                column: "DeleterId");

            migrationBuilder.CreateIndex(
                name: "IX_AppUserRoles_DeletionTime",
                table: "AppUserRoles",
                column: "DeletionTime");

            migrationBuilder.CreateIndex(
                name: "IX_AppUserRoles_IsDeleted",
                table: "AppUserRoles",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_AppUserRoles_LastModificationTime",
                table: "AppUserRoles",
                column: "LastModificationTime");

            migrationBuilder.CreateIndex(
                name: "IX_AppUserRoles_LastModifierId",
                table: "AppUserRoles",
                column: "LastModifierId");

            migrationBuilder.CreateIndex(
                name: "IX_AppUserRoles_RoleId_UserId",
                table: "AppUserRoles",
                columns: new[] { "RoleId", "UserId" },
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_AppUserRoles_UserId",
                table: "AppUserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_CreationTime",
                table: "AppUsers",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_CreatorId",
                table: "AppUsers",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_DeleterId",
                table: "AppUsers",
                column: "DeleterId");

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_DeletionTime",
                table: "AppUsers",
                column: "DeletionTime");

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_IsDeleted",
                table: "AppUsers",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_LastModificationTime",
                table: "AppUsers",
                column: "LastModificationTime");

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_LastModifierId",
                table: "AppUsers",
                column: "LastModifierId");

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_NormalizedEmail",
                table: "AppUsers",
                column: "NormalizedEmail",
                unique: true,
                filter: "\"IsDeleted\" = FALSE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppEntityPropertyChanges");

            migrationBuilder.DropTable(
                name: "AppHttpRequestLogs");

            migrationBuilder.DropTable(
                name: "AppRefreshTokens");

            migrationBuilder.DropTable(
                name: "AppRolePermissions");

            migrationBuilder.DropTable(
                name: "AppUserRoles");

            migrationBuilder.DropTable(
                name: "AppAuditLogs");

            migrationBuilder.DropTable(
                name: "AppSessions");

            migrationBuilder.DropTable(
                name: "AppPermissions");

            migrationBuilder.DropTable(
                name: "AppRoles");

            migrationBuilder.DropTable(
                name: "AppUsers");
        }
    }
}
