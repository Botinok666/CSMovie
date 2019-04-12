using System;

namespace Movies.UWP.Data
{
    public class PersonData
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public PersonData(Tuple<int, string> pair) { ID = pair.Item1; Name = pair.Item2; }
    }
}
