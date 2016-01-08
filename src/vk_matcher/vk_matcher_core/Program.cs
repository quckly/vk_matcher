using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VKMatcher.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var logger = new QUtils.QLogger(@"./logs-core.txt"))
            {
                new VkMatcherServer(logger).Run();
            }
        }
    }
}
