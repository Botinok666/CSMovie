using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using System.Globalization;
using Movies.UWP.Data;
using AngleSharp.Html.Parser;
using AngleSharp.Html.Dom;
using AngleSharp;
using AngleSharp.Dom;

namespace Movies.UWP.Util
{
    sealed class KPParser
    {
        private KPParser() { }

        public static MovieData ParseString(string html)
        {
            var document = new HtmlParser().ParseDocument(html);
            return Parse(document);
        }
        public async static Task<MovieData> ParseURL(string url)
        {
            var document = await BrowsingContext.New(Configuration.Default.WithDefaultLoader()).OpenAsync(url);
            var result = ParseString(document.ToHtml());
            document.Dispose();
            return result;
        }
        private static Tuple<int, string> ConvertToPair(IElement e)
        {
            if (!int.TryParse(new DirectoryInfo(e.GetAttribute("href")).Name, out int id))
                return new Tuple<int, string>(-1, WebUtility.HtmlDecode(e.InnerHtml));
            return new Tuple<int, string>(id, WebUtility.HtmlDecode(e.InnerHtml));
        }
        private static MovieData Parse(IHtmlDocument doc)
        {
            MovieData movie = new MovieData();
            StateEnum state = StateEnum.ID;
            try
            {
                IElement element = doc.QuerySelector("head > meta[property=og\\3A url]");
                movie.ID = int.Parse(new DirectoryInfo(element
                    .GetAttribute("content")).Name);
                
                state = StateEnum.Poster;
                movie.PosterLink = doc.QuerySelector("head > link[rel=image_src]")
                    .GetAttribute("href");

                state = StateEnum.Localized;
                element = doc.QuerySelector("div#viewFilmInfoWrapper");
                movie.LocalizedTitle = WebUtility.HtmlDecode(
                    element.QuerySelector("h1.moviename-big > span").InnerHtml);

                state = StateEnum.Original;
                movie.OriginalTitle = Regex.Replace
                    (WebUtility.HtmlDecode(element.QuerySelector("span.alternativeHeadline").InnerHtml), 
                    "\\([Вв]идео\\)|\\(ТВ\\)|в\\s3D",
                    "");

                state = StateEnum.Year;
                element = element.QuerySelector("table.info");
                var elements = element
                    .QuerySelectorAll("tr");
                movie.Year = short.Parse(elements[0]
                    .QuerySelector("a")
                    .InnerHtml);

                state = StateEnum.Country;
                movie.Countries = elements[1]
                    .QuerySelectorAll("a")
                    ?.Select(a => new CountryData(WebUtility.HtmlDecode(a.InnerHtml)))
                    .ToList()
                    ?? new List<CountryData>();

                state = StateEnum.TagLine;
                movie.TagLine = WebUtility.HtmlDecode(elements[2]
                    .Children[1]
                    .InnerHtml);

                state = StateEnum.Director;
                movie.Directors = elements[3]
                    .QuerySelectorAll("a")
                    ?.Where(a => !a.InnerHtml.Equals("..."))
                    .Select(a => new PersonData(ConvertToPair(a)))
                    .ToList()
                    ?? new List<PersonData>();

                state = StateEnum.Screenwriter;
                movie.Screenwriters = elements[4]
                    .QuerySelectorAll("a")
                    ?.Where(a => !a.InnerHtml.Equals("..."))
                    .Select(a => new PersonData(ConvertToPair(a)))
                    .ToList()
                    ?? new List<PersonData>();
                
                state = StateEnum.Genre;
                movie.Genres = element
                    .QuerySelectorAll("span[itemprop=genre] > a")
                    ?.Select(a => new GenreData(WebUtility.HtmlDecode(a.InnerHtml)))
                    .ToList()
                    ?? new List<GenreData>();
                
                state = StateEnum.Runtime;
                if (!short.TryParse(element
                        .QuerySelector("tr td.time")
                        .InnerHtml
                        .Split(' ')[0], out short runtime))
                    runtime = 0;
                movie.Runtime = runtime;

                state = StateEnum.Actor;
                movie.Actors = doc.QuerySelector("div#actorList > ul")
                    .Children
                    ?.Select(li => li.FirstElementChild)
                    .Where(li => !li.InnerHtml.Equals("..."))
                    .Select(li => new PersonData(ConvertToPair(li)))
                    .ToList()
                    ?? new List<PersonData>();

                state = StateEnum.Storyline;
                movie.Storyline = WebUtility.HtmlDecode(doc.QuerySelector("div.film-synopsys")
                    ?.InnerHtml
                    ?? "-");

                state = StateEnum.RatingKP;
                elements = doc.QuerySelectorAll("div.block_2 > div");
                if (!float.TryParse(elements[0]
                        .QuerySelector("span.rating_ball")
                        ?.InnerHtml
                        ?? "0", NumberStyles.Any, CultureInfo.InvariantCulture, out float rate))
                    rate = 0;
                movie.RatingKP = rate;
                
                state = StateEnum.RatingIMDB;
                if (!float.TryParse(elements[1]
                        .InnerHtml
                        .Split(' ')[1], NumberStyles.Any, CultureInfo.InvariantCulture, out rate))
                    rate = 0;
                movie.RatingIMDB = rate;

                return movie;
            }
            catch (Exception)
            {
                throw new FormatException("Error occurred when parsing " + state.ToString());
            }
        }
        private enum StateEnum
        {
            ID, Poster, Localized, Original, Year, Country, TagLine, Director,
            Screenwriter, Genre, Runtime, Actor, Storyline, RatingKP, RatingIMDB
        }
    }
}
