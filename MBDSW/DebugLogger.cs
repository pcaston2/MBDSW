using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBDSW
{
    public class DebugLogger : ILog
    {
        public void Log(string message)
        {
            Debug.WriteLine(message);
        }

    }
}
