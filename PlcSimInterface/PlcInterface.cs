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
    class PlcInterface
    {
        public PlcInterface(string amsNetId)
        {
            this.amsNetId = amsNetId;
            readerReadBoolOut = new AdsBinaryReader(streamReadBoolOut);
        }

        private int
            hBoolInPaths,
            hBoolInPathCtr,
            hBoolOutPaths,
            hBoolOutPathCtr,
            iBoolInPathCtr,
            iBoolOutPathCtr;
        
        public int IBoolInPathCtr { get => iBoolInPathCtr; }
        public int IBoolOutPathCtr { get => iBoolOutPathCtr; }
        public string[] ArrBoolInPaths { get => arrBoolInPaths;  }
        public string[] ArrBoolOutPaths { get => arrBoolOutPaths;}

        private AdsStream streamReadBoolOut = new AdsStream(1);
        private AdsBinaryReader readerReadBoolOut; 
        private TcAdsClient tcClient;
        private string[] arrBoolInPaths = null; 
        private string[] arrBoolOutPaths = null;
        private string amsNetId;

        public void Dispose()
        {
            tcClient.Disconnect();
            tcClient.Dispose();
        }

        public string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        public void writeBooleanInput(int index, bool value)
        {
            int iHandle = tcClient.CreateVariableHandle(arrBoolInPaths[index]);
            tcClient.WriteAny(iHandle, value);
            tcClient.DeleteVariableHandle(iHandle);
        }

        public bool readBooleanOutput(int index)
        {
            int iHandle = tcClient.CreateVariableHandle(arrBoolOutPaths[index]);
            tcClient.Read(iHandle, streamReadBoolOut);
            bool val = readerReadBoolOut.ReadBoolean();
            streamReadBoolOut.Position = 0;
            tcClient.DeleteVariableHandle(iHandle);
            return val;
        }

        public void Fetch()
        {
            tcClient = new TcAdsClient();
            tcClient.Connect(amsNetId,851);
            try
            {
                //get the input paths

                hBoolInPathCtr = tcClient.CreateVariableHandle("SymbolPathStorage.uiBoolInPathCtr");
                AdsStream streamBoolInPathCtr = new AdsStream(2);
                AdsBinaryReader readerBoolInPathCtr = new AdsBinaryReader(streamBoolInPathCtr);
                tcClient.Read(hBoolInPathCtr, streamBoolInPathCtr);
                iBoolInPathCtr = readerBoolInPathCtr.ReadInt16();

                arrBoolInPaths = new string[iBoolInPathCtr - 1];

                Console.WriteLine("Input variables nr: {0}", IBoolInPathCtr);

                hBoolInPaths = tcClient.CreateVariableHandle("SymbolPathStorage.aBoolInPaths");

                AdsStream streamBoolInPaths = new AdsStream((IBoolInPathCtr - 1) * 256);
                BinaryReader readerBoolInPaths = new BinaryReader(streamBoolInPaths);

                tcClient.Read(hBoolInPaths, streamBoolInPaths);
                for (int i = 1; i < IBoolInPathCtr; i++)
                {
                    byte[] buffer = readerBoolInPaths.ReadBytes(256);
                    string var = Encoding.UTF8.GetString(buffer, 0, buffer.Length);

                    int index = var.IndexOf('\0');
                    var = var.Remove(index);
                    arrBoolInPaths[i - 1] = var;
                    using (MD5 md5Hash = MD5.Create())
                    {
                        string hash = GetMd5Hash(md5Hash, var);
                        Console.WriteLine("Input {0} : {1} - Hash: {2}", i, var, hash);
                    }
                }

                //get the output paths

                hBoolOutPathCtr = tcClient.CreateVariableHandle("SymbolPathStorage.uiBoolOutPathCtr");
                AdsStream streamBoolOutPathCtr = new AdsStream(2);
                AdsBinaryReader readerBoolOutPathCtr = new AdsBinaryReader(streamBoolOutPathCtr);
                tcClient.Read(hBoolOutPathCtr, streamBoolOutPathCtr);
                iBoolOutPathCtr = readerBoolOutPathCtr.ReadInt16();
                arrBoolOutPaths = new string[iBoolOutPathCtr - 1];
                Console.WriteLine("Output variables nr: {0}", IBoolOutPathCtr);

                hBoolOutPaths = tcClient.CreateVariableHandle("SymbolPathStorage.aBoolOutPaths");

                AdsStream streamBoolOutPaths = new AdsStream((IBoolOutPathCtr - 1) * 256);
                BinaryReader readerBoolOutPaths = new BinaryReader(streamBoolOutPaths);

                tcClient.Read(hBoolOutPaths, streamBoolOutPaths);
                for (int i = 1; i < IBoolOutPathCtr; i++)
                {
                    byte[] buffer = readerBoolOutPaths.ReadBytes(256);
                    string var = Encoding.UTF8.GetString(buffer, 0, buffer.Length);

                    int index = var.IndexOf('\0');
                    var = var.Remove(index);
                    arrBoolOutPaths[i - 1] = var;
                    using (MD5 md5Hash = MD5.Create())
                    {
                        string hash = GetMd5Hash(md5Hash, var);
                        Console.WriteLine("Output {0} : {1} - Hash: {2}", i, var, hash);
                    }
                }
                
            }
            catch (Exception err)
            {
                Console.WriteLine("Error while retrieving handle.");
            }
        }
    }
}
