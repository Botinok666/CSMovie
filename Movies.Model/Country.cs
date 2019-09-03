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
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public short ID { get; set; }
        [Required]
        [MaxLength(64)]
        public string Name { get; set; }
        public virtual List<MovieCountry> Movies { get; set; }
    }
    public class CountryComparer : IEqualityComparer<Country>
    {
        public bool Equals(Country x, Country y)
        {
            return x.Name.Equals(y.Name);
        }
        public int GetHashCode(Country obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}
