using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Twissandnet.Models
{
    public class SessionUtil
    {
        public static void LogIn(Account account)
        {
            HttpContext.Current.Session["Account"] = account;
        }

        public static void LogOut()
        {
            if (IsLoggedIn())
            {
                HttpContext.Current.Session["Account"] = null;
            }
        }

        public static bool IsLoggedIn()
        {
            return HttpContext.Current.Session != null &&
                            HttpContext.Current.Session["Account"] != null;
        }

        public static Account GetAccount()
        {
            if (IsLoggedIn())
            {
                return ((Account)HttpContext.Current.Session["Account"]);
            }
            return null;
        }
    }
}