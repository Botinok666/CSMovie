using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Model
{
    public class Context : DbContext
    {
        public static bool IsLocalDB { get; set; } = true;
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Viewing> Viewings { get; set; }
        public DbSet<MovieActor> MovieActor { get; set; }
        public DbSet<MovieDirector> MovieDirector { get; set; }
        public DbSet<MovieScreenwriter> MovieScreenwriter { get; set; }
        public DbSet<MovieCountry> MovieCountry { get; set; }
        public DbSet<MovieGenre> MovieGenre { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (IsLocalDB)
                optionsBuilder.UseLazyLoadingProxies().UseSqlite(@"Filename=Movies.db");
            else
                optionsBuilder.UseLazyLoadingProxies().UseSqlServer("Server=lenovo-pc;User id=User0;password=User0;database=Movies");
        }
    }
}
