using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SimpleBankApi.Repository.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedToPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Cpf = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountNumber = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    OwnerId = table.Column<int>(type: "integer", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    RefreshToken = table.Column<string>(type: "text", nullable: true),
                    RefreshTokenExpiration = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accounts_Customers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false),
                    OperatingAccountId = table.Column<int>(type: "integer", nullable: false),
                    RelatedAccountId = table.Column<int>(type: "integer", nullable: true),
                    RelatedTransactionId = table.Column<int>(type: "integer", nullable: true),
                    TransactionType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Accounts_OperatingAccountId",
                        column: x => x.OperatingAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transactions_Accounts_RelatedAccountId",
                        column: x => x.RelatedAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transactions_Transactions_RelatedTransactionId",
                        column: x => x.RelatedTransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id");
                });

            #region CustomPopulate
            migrationBuilder.InsertData(
                table: "Customers",
                columns: ["Cpf", "Name"],
                values: new object[,]
                {
                    {"01234567890", "admin"}
                }
            );
            migrationBuilder.InsertData(
                table: "Accounts",
                columns: [
                    "AccountNumber",
                    "Active",
                    "CreatedAt",
                    "OwnerId",
                    "PasswordHash",
                    "RefreshToken",
                    "RefreshTokenExpiration",
                    "Role"
                ],
                values: new object[,]
                {
                    {
                        1724731066,
                        true,
                        DateTime.Parse("2024-06-05T12:00:00").ToUniversalTime(),
                        1,
                        "IKXqabRp2BpGqZv8IrWxGA==;LGEbV98LH8Mx0h92W6Fzmo2UfoMU6euVsK9rbVPQd9g=", //password plain text = admin
                        null,
                        null,
                        "Admin"
                    }
                }
            );
            #endregion

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_AccountNumber",
                table: "Accounts",
                column: "AccountNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_OwnerId",
                table: "Accounts",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Cpf",
                table: "Customers",
                column: "Cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_OperatingAccountId",
                table: "Transactions",
                column: "OperatingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_RelatedAccountId",
                table: "Transactions",
                column: "RelatedAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_RelatedTransactionId",
                table: "Transactions",
                column: "RelatedTransactionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
