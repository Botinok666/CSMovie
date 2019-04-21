using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Movies.UWP.Util
{
    public class FilterOption
    {
        public Filters Filter { get; private set; }
        public string Description { get; private set; }
        private FilterOption(Filters filter)
        {
            switch (filter)
            {
                case Filters.GetAll:
                    Description = "<нет>";
                    break;
                case Filters.GetByActor:
                    Description = "актёру";
                    break;
                case Filters.GetByCountry:
                    Description = "стране";
                    break;
                case Filters.GetByDirector:
                    Description = "режиссёру";
                    break;
                case Filters.GetByGenre:
                    Description = "жанру";
                    break;
                case Filters.GetByStoryline:
                    Description = "сюжету";
                    break;
                case Filters.GetByTitle:
                    Description = "названию";
                    break;
                case Filters.GetByYearPeriod:
                    Description = "году";
                    break;
                case Filters.GetByScreenwriter:
                    Description = "сценаристу";
                    break;
            }
            Filter = filter;
        }
        private static List<FilterOption> filterOptions;
        public static List<FilterOption> FilterOptions
        {
            get
            {
                return filterOptions ?? (filterOptions = (
                    Enum.GetValues(typeof(Filters)) as Filters[])
                    .Select(x => new FilterOption(x))
                    .ToList());
            }
        }
    }
    public enum Filters
    {
        GetAll,
        GetByActor,
        GetByCountry,
        GetByDirector,
        GetByScreenwriter,
        GetByGenre,
        GetByStoryline,
        GetByTitle,
        GetByYearPeriod
    }
}
