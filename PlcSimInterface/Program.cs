using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TwinCAT.Ads;

namespace PlcSimInterface
{

    class Program
    {

        static void Main(string[] args)
        {
            ArrayList plcInterfaceList = new ArrayList();
            ArrayList simInterfaceList = new ArrayList();

            string filename = "config.xml";
            var currentDirectory = Directory.GetCurrentDirectory();
            var configFilepath = Path.Combine(currentDirectory, filename);
            
            System.Xml.XmlDataDocument xmldoc = new System.Xml.XmlDataDocument();
            XmlNodeList xmlAddresses;
            
            string str = null;
            FileStream fs = new FileStream(configFilepath, FileMode.Open, FileAccess.Read);
            xmldoc.Load(fs);
            xmlAddresses = xmldoc.GetElementsByTagName("Address");

            var numOfPlcs = xmlAddresses.Count;
    
            for(int i = 0; i<numOfPlcs; i++)
            {
                string amsAddress = xmlAddresses[i].ChildNodes.Item(0).InnerText.Trim();
                Console.WriteLine("AmsNetId: {0}",amsAddress);
                PlcInterface plcInterface = new PlcInterface(amsAddress);
                plcInterfaceList.Add(plcInterface);
                plcInterface.Fetch();
                SimInterface simInterface = new SimInterface(plcInterface);
                simInterfaceList.Add(simInterface);
                simInterface.Init();
            }
            Console.WriteLine("Press enter to end the program.");
            Console.WriteLine();
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    if(key.Key == ConsoleKey.Enter)
                    {
                        break;
                    }
                }
            }

            Console.WriteLine("Cleaning up and exiting.");
            foreach(SimInterface simInterface in simInterfaceList)
            {
                    simInterface.Dispose();
            }
            
            foreach(PlcInterface plcInterface in plcInterfaceList)
            {
                    plcInterface.Dispose();
            }
        }
    }
}
