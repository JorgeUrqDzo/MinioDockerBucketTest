using Microsoft.EntityFrameworkCore.Migrations;

namespace MinioDockerBucketTest.Migrations
{
    public partial class internalname : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InternalName",
                table: "UserFiles",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InternalName",
                table: "UserFiles");
        }
    }
}
