using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Movies.Model.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    ID = table.Column<short>(nullable: false),
                    Name = table.Column<string>(maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Genres",
                columns: table => new
                {
                    ID = table.Column<short>(nullable: false),
                    Name = table.Column<string>(maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genres", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Movies",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false),
                    LocalizedTitle = table.Column<string>(maxLength: 255, nullable: false),
                    OriginalTitle = table.Column<string>(maxLength: 255, nullable: false),
                    PosterLink = table.Column<string>(maxLength: 255, nullable: false),
                    Year = table.Column<short>(nullable: false),
                    TagLine = table.Column<string>(maxLength: 511, nullable: false),
                    Runtime = table.Column<short>(nullable: false),
                    Storyline = table.Column<string>(nullable: true),
                    RatingKP = table.Column<float>(nullable: false),
                    RatingIMDB = table.Column<float>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movies", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "People",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_People", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(maxLength: 64, nullable: false),
                    Pwd = table.Column<string>(maxLength: 64, nullable: false),
                    Role = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "MovieCountry",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false),
                    MovieId = table.Column<int>(nullable: false),
                    CountryId = table.Column<short>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieCountry", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MovieCountry_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovieCountry_Movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movies",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovieGenre",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false),
                    MovieId = table.Column<int>(nullable: false),
                    GenreId = table.Column<short>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieGenre", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MovieGenre_Genres_GenreId",
                        column: x => x.GenreId,
                        principalTable: "Genres",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovieGenre_Movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movies",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovieActor",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false),
                    MovieId = table.Column<int>(nullable: false),
                    ActorId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieActor", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MovieActor_People_ActorId",
                        column: x => x.ActorId,
                        principalTable: "People",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovieActor_Movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movies",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovieDirector",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false),
                    MovieId = table.Column<int>(nullable: false),
                    DirectorId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieDirector", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MovieDirector_People_DirectorId",
                        column: x => x.DirectorId,
                        principalTable: "People",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovieDirector_Movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movies",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovieScreenwriter",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false),
                    MovieId = table.Column<int>(nullable: false),
                    ScreenwriterId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieScreenwriter", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MovieScreenwriter_Movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movies",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovieScreenwriter_People_ScreenwriterId",
                        column: x => x.ScreenwriterId,
                        principalTable: "People",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Viewings",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MovieID = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Rating = table.Column<float>(nullable: false),
                    UserID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Viewings", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Viewings_Movies_MovieID",
                        column: x => x.MovieID,
                        principalTable: "Movies",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Viewings_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MovieActor_ActorId",
                table: "MovieActor",
                column: "ActorId");

            migrationBuilder.CreateIndex(
                name: "IX_MovieActor_MovieId",
                table: "MovieActor",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_MovieCountry_CountryId",
                table: "MovieCountry",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_MovieCountry_MovieId",
                table: "MovieCountry",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_MovieDirector_DirectorId",
                table: "MovieDirector",
                column: "DirectorId");

            migrationBuilder.CreateIndex(
                name: "IX_MovieDirector_MovieId",
                table: "MovieDirector",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_MovieGenre_GenreId",
                table: "MovieGenre",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_MovieGenre_MovieId",
                table: "MovieGenre",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_MovieScreenwriter_MovieId",
                table: "MovieScreenwriter",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_MovieScreenwriter_ScreenwriterId",
                table: "MovieScreenwriter",
                column: "ScreenwriterId");

            migrationBuilder.CreateIndex(
                name: "IX_Viewings_MovieID",
                table: "Viewings",
                column: "MovieID");

            migrationBuilder.CreateIndex(
                name: "IX_Viewings_UserID",
                table: "Viewings",
                column: "UserID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovieActor");

            migrationBuilder.DropTable(
                name: "MovieCountry");

            migrationBuilder.DropTable(
                name: "MovieDirector");

            migrationBuilder.DropTable(
                name: "MovieGenre");

            migrationBuilder.DropTable(
                name: "MovieScreenwriter");

            migrationBuilder.DropTable(
                name: "Viewings");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropTable(
                name: "Genres");

            migrationBuilder.DropTable(
                name: "People");

            migrationBuilder.DropTable(
                name: "Movies");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
