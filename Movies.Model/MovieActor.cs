using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Movies.Model
{
    public class MovieActor
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }
        public int MovieId { get; set; }
        public virtual Movie Movie { get; set; }
        public int ActorId { get; set; }
        public virtual Person Actor { get; set; }
    }
}
