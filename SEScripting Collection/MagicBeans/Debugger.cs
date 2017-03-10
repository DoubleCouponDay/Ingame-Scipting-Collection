using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicBeans
{
    class Debugger
    {
        static MagicBeans3.Program test = new MagicBeans3.Program();

        static void Main()
        {
            while (true)
            {
                test.Main ("hmm");
            }
        }
    }
}
