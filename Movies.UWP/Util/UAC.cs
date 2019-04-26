using Movies.Model;
using Movies.UWP.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.UWP.Util
{
    public class UAC
    {
        private static UAC uac;
        private Data.UserData user = null;
        private UAC() { }
        public static UAC GetInstance()
        {
            return uac ?? (uac = new UAC());
        }
        public int UserId { get => user?.ID ?? -1; }
        public Roles? UserRole { get => user?.Role; }
        public bool Authorize(string name, string pass)
        {
            user = MoviesController.GetInstance().GetUser(name, pass);
            return UserId != -1;
        }
        public void LogOut()
        {
            user = null;
        }
    }
}
