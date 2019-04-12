using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Model
{
    public class Country
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short ID { get; set; }
        [Required]
        [MaxLength(64)]
        public string Name { get; set; }
        public virtual List<MovieCountry> Countries { get; set; }
    }
    public class CountryComparer : IEqualityComparer<Country>
    {
        public bool Equals(Country x, Country y)
        {
            return x.ID == y.ID;
        }
        public int GetHashCode(Country obj)
        {
            return obj.ID.GetHashCode() + obj.Name.GetHashCode();
        }
    }
}
