using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.UWP.Util
{
    public class SortOption
    {
        public SortProperties SortProperty { get; private set; }
        public string Description { get; private set; }
        private SortOption(SortProperties sort)
        {
            switch (sort)
            {
                case SortProperties.OriginalTitle:
                    Description = "названию";
                    break;
                case SortProperties.Year:
                    Description = "году";
                    break;
                case SortProperties.RatingIMDB:
                    Description = "оценке IMDB";
                    break;
                case SortProperties.RatingUser:
                    Description = "личной оценке";
                    break;
                case SortProperties.ViewDate:
                    Description = "дате просмотра";
                    break;
            }
            SortProperty = sort;
        }
        private static List<SortOption> sortOptions;
        public static List<SortOption> SortOptions
        {
            get
            {
                return sortOptions ?? (sortOptions = (
                    Enum.GetValues(typeof(SortProperties)) as SortProperties[])
                    .Select(x => new SortOption(x))
                    .ToList());
            }
        }
    }
    public enum SortProperties
    { OriginalTitle, Year, RatingIMDB, RatingUser, ViewDate }
}
