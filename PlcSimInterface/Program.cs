using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TwinCAT.Ads;

namespace PlcSimInterface
{

    class Program
    {
        static void Main(string[] args)
        {
            PlcInterface plcInterface = new PlcInterface();

            plcInterface.Fetch();

            SimInterface simInterface = new SimInterface(plcInterface);
            simInterface.Execute();

            plcInterface.Dispose();
          

        }
    }
}
