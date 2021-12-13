using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlideIOC
{
    public static class Common
    {
        public static readonly string PathImage = "../../Image/";

        public static string GetImage(string imageName)
        {
            if (String.IsNullOrWhiteSpace(imageName))
            {
                return PathImage + "default.png";
            }
            else
            {
                return PathImage + imageName;
            }
        }
        
    }
}