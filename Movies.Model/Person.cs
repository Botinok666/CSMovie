using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Model
{
    public class Person 
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
        public virtual List<MovieDirector> MovieDirectors { get; set; }
        public virtual List<MovieScreenwriter> MovieScreenwriters { get; set; }
        public virtual List<MovieActor> MovieActors { get; set; }
    }
    public class PersonComparer : IEqualityComparer<Person>
    {
        public bool Equals(Person x, Person y)
        {
            return x.ID == y.ID;
        }
        public int GetHashCode(Person obj)
        {
            return obj.ID.GetHashCode() + obj.Name.GetHashCode();
        }
    }
}
