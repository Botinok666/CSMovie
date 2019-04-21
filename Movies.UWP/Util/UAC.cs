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
        private UAC() { }
        public static UAC GetInstance()
        {
            return uac ?? (uac = new UAC() { UserId = -1 });
        }
        public short UserId { get; private set; }
        public bool Authorize(string name, string pass)
        {
            UserId = MoviesController.GetInstance().GetUserId(name, pass);
            return UserId != -1;
        }
        public void LogOut()
        {
            UserId = -1;
        }
    }
}
