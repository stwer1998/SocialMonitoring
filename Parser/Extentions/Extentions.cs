using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Extentions
{
    public static class Extentions
    {
        public static int TryToInt(this string str) 
        {
            try 
            {
                return int.Parse(str);
            }
            catch 
            {
                return 0;
            }
        }
    }
}
