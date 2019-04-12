using System.Collections.Generic;

namespace Movies.UWP.Data
{
    public class MovieData
    {
        public int ID { get; set; }
        public string LocalizedTitle { get; set; }
        public string OriginalTitle { get; set; }
        public string PosterLink { get; set; }
        public short Year { get; set; }
        public List<CountryData> Countries { get; set; }
        public List<PersonData> Directors { get; set; }
        public List<PersonData> Screenwriters { get; set; }
        public string TagLine { get; set; }
        public List<GenreData> Genres { get; set; }
        public short Runtime { get; set; }
        public List<PersonData> Actors { get; set; }
        public string Storyline { get; set; }
        public float RatingKP { get; set; }
        public float RatingIMDB { get; set; }
    }
}