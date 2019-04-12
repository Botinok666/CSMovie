using Movies.Model;
using Movies.UWP.Data;
using Movies.UWP.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.UWP.Controller
{
    public class MoviesController
    {
        private static MoviesController controller;
        private MoviesController() { }
        public static MoviesController GetInstance()
        {
            return controller ?? (controller = new MoviesController());
        }
        public List<GenreData> GetAllGenres()
        {
            using (var db = new Context())
            {
                return db.Genres.Select(x => new GenreData(new Tuple<int, string>(x.ID, x.Name))).ToList();
            }
        }
        public List<CountryData> GetAllCountries()
        {
            using (var db = new Context())
            {
                return db.Countries.Select(x => new CountryData(new Tuple<int, string>(x.ID, x.Name))).ToList();
            }
        }
        public PagedResult<MovieData> GetMovies(object param, int page, int count, Filters filter)
        {
            PagedResult<Movie> movies = new PagedResult<Movie>();
            using (var db = new Context())
            {
                switch (filter)
                {
                    case Filters.GetAll:
                        movies = db.Movies.GetPaged(page, count);
                        break;
                    case Filters.GetByActor:
                        if (param is PersonData)
                            movies = db.Movies.Where(x => 
                                x.Actors.Exists(y => y.ActorId == (param as PersonData).ID))
                                .GetPaged(page, count);
                        break;
                    case Filters.GetByCountry:
                        if (param is CountryData)
                            movies = db.Movies.Where(x => 
                                x.Countries.Exists(y => y.CountryId == (param as CountryData).ID))
                                .GetPaged(page, count);
                        break;
                    case Filters.GetByDirector:
                        if (param is PersonData)
                            movies = db.Movies.Where(x => 
                                x.Directors.Exists(y => y.DirectorId == (param as PersonData).ID))
                                .GetPaged(page, count);
                        break;
                    case Filters.GetByGenre:
                        if (param is GenreData)
                            movies = db.Movies.Where(x => 
                                x.Genres.Exists(y => y.GenreId == (param as GenreData).ID))
                                .GetPaged(page, count);
                        break;
                    case Filters.GetByStoryline:
                        if (param is string)
                            movies = db.Movies.Where(x =>
                                x.Storyline.Contains(param as string, StringComparison.OrdinalIgnoreCase))
                                .GetPaged(page, count);
                        break;
                    case Filters.GetByTitle:
                        if (param is string)
                            movies = db.Movies.Where(x => 
                                x.LocalizedTitle.Contains(param as string, StringComparison.OrdinalIgnoreCase) ||
                                x.OriginalTitle.Contains(param as string, StringComparison.OrdinalIgnoreCase))
                                .GetPaged(page, count);
                        break;
                    case Filters.GetByUser:
                        if (param is short)
                        {
                            var viewings = db.Viewings.Where(x =>
                                x.UserID == (short)param)
                                .OrderBy(x => x.Date)
                                .GetPaged(page, count);
                            movies = new PagedResult<Movie>()
                            {
                                CurrentPage = viewings.CurrentPage,
                                PageCount = viewings.PageCount,
                                PageSize = viewings.PageSize,
                                RowCount = viewings.RowCount,
                                Results = viewings.Results.Select(x => x.Movie).ToList()
                            };
                        }
                        break;
                    case Filters.GetByYearPeriod:
                        if (param is Tuple<short, short>)
                            movies = db.Movies.Where(x => 
                                x.Year >= (param as Tuple<short, short>).Item1 &&
                                x.Year <= (param as Tuple<short, short>).Item2)
                                .GetPaged(page, count);
                        break;
                }
            }
            return new PagedResult<MovieData>()
            {
                CurrentPage = movies.CurrentPage,
                PageCount = movies.PageCount,
                PageSize = movies.PageSize,
                RowCount = movies.RowCount,
                Results = movies.Results.Select(x => new MovieData() {
                    ID = x.ID,
                    LocalizedTitle = x.LocalizedTitle,
                    OriginalTitle = x.OriginalTitle,
                    Year = x.Year,
                    RatingIMDB = x.RatingIMDB
                }).ToList()
            };
        }
        public List<PersonData> GetPersons(string name)
        {
            using (var db = new Context())
            {
                return db.People
                    .Where(x => x.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                    .Take(50)
                    .Select(x => new PersonData(new Tuple<int, string>(x.ID, x.Name)))
                    .ToList();
            }
        }

        public async Task<int> SaveMovies(List<MovieData> movies)
        {
            using (var db = new Context())
            {
                HashSet<Country> countries = new HashSet<Country>(new CountryComparer());
                movies.ForEach(x => 
                    countries.UnionWith(x.Countries.Select(y => new Country() { ID = y.ID, Name = y.Name })));
                var countriesNew = countries.Where(x => !db.Countries.Contains(x));
                if (countriesNew.Count() > 0)
                    db.AddRange(countriesNew);

                HashSet<Genre> genres = new HashSet<Genre>(new GenreComparer());
                movies.ForEach(x => 
                    genres.UnionWith(x.Genres.Select(y => new Genre() { ID = y.ID, Name = y.Name })));
                var genresNew = genres.Where(x => !db.Genres.Contains(x));
                if (genresNew.Count() > 0)
                    db.AddRange(genresNew);

                HashSet<Person> people = new HashSet<Person>(new PersonComparer());
                movies.ForEach(x =>
                {
                    people.UnionWith(x.Actors.Select(y => new Person() { ID = y.ID, Name = y.Name }));
                    people.UnionWith(x.Directors.Select(y => new Person() { ID = y.ID, Name = y.Name }));
                    people.UnionWith(x.Screenwriters.Select(y => new Person() { ID = y.ID, Name = y.Name }));
                });
                var peopleNew = people.Where(x => !db.People.Contains(x));
                if (peopleNew.Count() > 0)
                    db.AddRange(peopleNew);

                await db.SaveChangesAsync();
                List<MovieActor> movieActors = new List<MovieActor>();
                List<MovieDirector> movieDirectors = new List<MovieDirector>();
                List<MovieScreenwriter> movieScreenwriters = new List<MovieScreenwriter>();
                List<MovieCountry> movieCountries = new List<MovieCountry>();
                List<MovieGenre> movieGenres = new List<MovieGenre>();
                movies = movies.Where(x => db.Movies.Count(y => y.ID == x.ID) == 0).ToList();
                if (movies.Count() > 0)
                {
                    db.AddRange(movies.Select(x => new Movie()
                        {
                            ID = x.ID,
                            TagLine = x.TagLine,
                            LocalizedTitle = x.LocalizedTitle,
                            OriginalTitle = x.OriginalTitle,
                            PosterLink = x.PosterLink,
                            RatingIMDB = x.RatingIMDB,
                            RatingKP = x.RatingKP,
                            Runtime = x.Runtime,
                            Storyline = x.Storyline,
                            Year = x.Year
                        }
                    ));
                    movies.ForEach(x => {
                        movieActors.AddRange(x.Actors.Take(16).Select(y => new MovieActor() 
                            { ID = (x.ID << 4) + x.Actors.IndexOf(y), MovieId = x.ID, ActorId = y.ID }));
                        movieDirectors.AddRange(x.Directors.Select(y => new MovieDirector() 
                            { ID = (x.ID << 4) + x.Directors.IndexOf(y), MovieId = x.ID, DirectorId = y.ID }));
                        movieScreenwriters.AddRange(x.Screenwriters.Select(y => new MovieScreenwriter() 
                            { ID = (x.ID << 4) + x.Screenwriters.IndexOf(y), MovieId = x.ID, ScreenwriterId = y.ID }));
                        movieCountries.AddRange(x.Countries.Select(y => new MovieCountry() 
                            { ID = (x.ID << 4) + x.Countries.IndexOf(y), MovieId = x.ID, CountryId = y.ID }));
                        movieGenres.AddRange(x.Genres.Select(y => new MovieGenre() 
                            { ID = (x.ID << 4) + x.Genres.IndexOf(y), MovieId = x.ID, GenreId = y.ID }));
                    });
                    db.AddRange(movieActors);
                    db.AddRange(movieDirectors);
                    db.AddRange(movieScreenwriters);
                    db.AddRange(movieCountries);
                    db.AddRange(movieGenres);
                    await db.SaveChangesAsync();
                }
                return movies.Count();
            }
        }
        public async Task<MovieData> GetMovie(int ID)
        {
            using (var db = new Context())
            {
                Movie movie = await db.Movies.FindAsync(ID);
                if (movie == null)
                    return null;
                return new MovieData()
                {
                    Actors = movie.Actors.Select(
                        x => new PersonData(new Tuple<int, string>(x.ActorId, x.Actor.Name))).ToList(),
                    Countries = movie.Countries.Select(
                        x => new CountryData(new Tuple<int, string>(x.CountryId, x.Country.Name))).ToList(),
                    Directors = movie.Directors.Select(
                        x => new PersonData(new Tuple<int, string>(x.DirectorId, x.Director.Name))).ToList(),
                    Genres = movie.Genres.Select(
                        x => new GenreData(new Tuple<int, string>(x.GenreId, x.Genre.Name))).ToList(),
                    ID = movie.ID,
                    LocalizedTitle = movie.LocalizedTitle,
                    OriginalTitle = movie.OriginalTitle,
                    PosterLink = movie.PosterLink,
                    RatingIMDB = movie.RatingIMDB,
                    RatingKP = movie.RatingKP,
                    Runtime = movie.Runtime,
                    Screenwriters = movie.Screenwriters.Select(
                        x => new PersonData(new Tuple<int, string>(x.ScreenwriterId, x.Screenwriter.Name))).ToList(),
                    Storyline = movie.Storyline,
                    TagLine = movie.TagLine,
                    Year = movie.Year
                };
            }
        }
        public async Task<int> SaveViewings(List<ViewingData> viewings)
        {
            using (var db = new Context())
            {
                db.AddRange(viewings.Select(
                    x => new Viewing() { Date = x.Date, MovieID = x.MovieID, Rating = x.Rating, UserID = x.UserID }));
                return await db.SaveChangesAsync();
            }
        }
        public List<ViewingData> GetViewings(int movieId, int userId)
        {
            using (var db = new Context())
            {
                return db.Viewings
                    .Where(x => x.MovieID == movieId && x.UserID == userId)
                    .OrderBy(x => x.Date)
                    .Select(x => new ViewingData(movieId, userId, x.Date, x.Rating))
                    .ToList();
            }
        }
        public short GetUserId(string name, string pwd)
        {
            using (var db = new Context())
            {
                return (short)(db.Users
                    .First(x => x.Name.Equals(name) && x.Pwd.Equals(pwd))
                    ?.ID
                    ?? -1);
            }
        }
    }
}
