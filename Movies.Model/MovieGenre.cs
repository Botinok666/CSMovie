using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Movies.Model
{
    public class MovieGenre
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }
        public int MovieId { get; set; }
        public virtual Movie Movie { get; set; }
        public short GenreId { get; set; }
        public virtual Genre Genre { get; set; }
    }
}
