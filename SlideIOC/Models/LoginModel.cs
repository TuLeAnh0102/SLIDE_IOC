using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlideIOC.Models
{
    public class LoginModel
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public string Token { get; set; }

        public string Version { get; set; }

        public string LastAccess { get; set; }

        public string DeviceModel { get; set; }

        public string Captcha { get; set; }
    }
    public class UserProfile
    {
        public int UserID { get; set; }
        public string TaiKhoan { get; set; }
        public string Image { get; set; }
        public string HoTen { get; set; }
        public long Id_DV { get; set; }
        public string Ten_DV { get; set; }
    }
}