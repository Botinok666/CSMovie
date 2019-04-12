using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Movies.Model
{
    public class Movie
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }
        [Required]
        [MaxLength(255)]
        public string LocalizedTitle { get; set; }
        [Required]
        [MaxLength(255)]
        public string OriginalTitle { get; set; }
        [Required]
        [MaxLength(255)]
        public string PosterLink { get; set; }
        public short Year { get; set; }
        public virtual List<MovieCountry> Countries { get; set; }
        public virtual List<MovieDirector> Directors { get; set; }
        public virtual List<MovieScreenwriter> Screenwriters { get; set; }
        [Required]
        [MaxLength(511)]
        public string TagLine { get; set; }
        public virtual List<MovieGenre> Genres { get; set; }
        public short Runtime { get; set; }
        public virtual List<MovieActor> Actors { get; set; }
        public string Storyline { get; set; }
        public float RatingKP { get; set; }
        public float RatingIMDB { get; set; }
    }
}
