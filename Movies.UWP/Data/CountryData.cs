using System;

namespace Movies.UWP.Data
{
    public class CountryData
    {
        public short ID { get; set; }
        public string Name { get; set; }
        public CountryData(Tuple<int, string> pair) { ID = (short)pair.Item1; Name = pair.Item2; }
        public CountryData(string name) { Name = name; }
    }
}
