using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Movies.Model
{
    public class MovieDirector
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }
        public int MovieId { get; set; }
        public virtual Movie Movie { get; set; }
        public int DirectorId { get; set; }
        public virtual Person Director { get; set; }
    }
}
