using System;

namespace Movies.UWP.Data
{
    public class GenreData
    {
        public short ID { get; set; }
        public string Name { get; set; }

        public GenreData(Tuple<int, string> pair) { ID = (short)pair.Item1; Name = pair.Item2; }
        public GenreData(string name) { Name = name; }
    }
}
