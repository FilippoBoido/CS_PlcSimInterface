using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwinCAT.Ads;

namespace PlcSimInterface
{

    class Program
    {
        private static int  
            hBoolInPaths, 
            hBoolInPathCtr,
            hBoolOutPaths,
            hBoolOutPathCtr;

        private static TcAdsClient tcClient;
        private static ArrayList arrListBoolInPaths = new ArrayList();
        private static ArrayList arrListBoolOutPaths = new ArrayList();

        static void Main(string[] args)
        {
            tcClient = new TcAdsClient();
            tcClient.Connect(851);
            try
            {
                //get the input paths

                hBoolInPathCtr = tcClient.CreateVariableHandle("SymbolPathStorage.uiBoolInPathCtr");
                AdsStream streamBoolInPathCtr = new AdsStream(2);
                AdsBinaryReader readerBoolInPathCtr = new AdsBinaryReader(streamBoolInPathCtr);
                tcClient.Read(hBoolInPathCtr, streamBoolInPathCtr);
                int iBoolInPathCtr = readerBoolInPathCtr.ReadInt16();
                Console.WriteLine("Input variables nr: {0}", iBoolInPathCtr);

                hBoolInPaths = tcClient.CreateVariableHandle("SymbolPathStorage.aBoolInPaths");

                AdsStream streamBoolInPaths = new AdsStream((iBoolInPathCtr-1) * 256);
                BinaryReader readerBoolInPaths = new BinaryReader(streamBoolInPaths);
               
                tcClient.Read(hBoolInPaths, streamBoolInPaths);
                for (int i = 1; i < iBoolInPathCtr; i++)
                {
                    byte[] buffer = readerBoolInPaths.ReadBytes(256);
                    string var = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                    
                    int index = var.IndexOf('\0');                
                    var = var.Remove(index);
                    arrListBoolInPaths.Add(var);
                    Console.WriteLine("Input {0} : {1}", i, var);
                }

                //get the output paths

                hBoolOutPathCtr = tcClient.CreateVariableHandle("SymbolPathStorage.uiBoolOutPathCtr");
                AdsStream streamBoolOutPathCtr = new AdsStream(2);
                AdsBinaryReader readerBoolOutPathCtr = new AdsBinaryReader(streamBoolOutPathCtr);
                tcClient.Read(hBoolOutPathCtr, streamBoolOutPathCtr);
                int iBoolOutPathCtr = readerBoolOutPathCtr.ReadInt16();
                Console.WriteLine("Output variables nr: {0}", iBoolOutPathCtr);

                hBoolOutPaths = tcClient.CreateVariableHandle("SymbolPathStorage.aBoolOutPaths");

                AdsStream streamBoolOutPaths = new AdsStream((iBoolOutPathCtr - 1) * 256);
                BinaryReader readerBoolOutPaths = new BinaryReader(streamBoolOutPaths);

                tcClient.Read(hBoolOutPaths, streamBoolOutPaths);
                for (int i = 1; i < iBoolOutPathCtr; i++)
                {
                    byte[] buffer = readerBoolOutPaths.ReadBytes(256);
                    string var = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                    
                    int index = var.IndexOf('\0');
                    var = var.Remove(index);
                    arrListBoolOutPaths.Add(var);
                    Console.WriteLine("Output {0} : {1}", i, var);
                }
                Console.ReadKey();
            }
            catch (Exception err)
            {
                Console.WriteLine("Error while retrieving handle.");
            }
        }
    }
}
