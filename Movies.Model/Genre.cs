using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Model
{
    public class Genre
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short ID { get; set; }
        [Required]
        [MaxLength(64)]
        public string Name { get; set; }
        public virtual List<MovieGenre> Genres { get; set; }
    }
    public class GenreComparer : IEqualityComparer<Genre>
    {
        public bool Equals(Genre x, Genre y)
        {
            return x.ID == y.ID;
        }
        public int GetHashCode(Genre obj)
        {
            return obj.ID.GetHashCode() + obj.Name.GetHashCode();
        }
    }
}
