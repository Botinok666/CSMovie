using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Movies.Model
{
    public class MovieCountry
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }
        public int MovieId { get; set; }
        public virtual Movie Movie { get; set; }
        public short CountryId { get; set; }
        public virtual Country Country { get; set; }
    }
}
