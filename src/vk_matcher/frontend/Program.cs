﻿ using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VKMatcher.Frontend
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new FrontendServer();
            server.Run();
        }
    }
}
