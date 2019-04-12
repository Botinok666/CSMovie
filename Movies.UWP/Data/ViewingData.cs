using System;

namespace Movies.UWP.Data
{
    public class ViewingData
    {
        public int ID { get; set; }
        public int MovieID { get; set; }
        public DateTime Date { get; set; }
        public float Rating { get; set; }
        public int UserID { get; set; }
        public ViewingData(int MovieID, int UserID, DateTime Date, float Rating)
        {
            this.MovieID = MovieID;
            this.UserID = UserID;
            this.Date = Date;
            this.Rating = Rating;
        }
    }
}
