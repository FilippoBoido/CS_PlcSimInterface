using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Threading; 
using EngineIO;
namespace PlcSimInterface
{
    class SimInterface
    {
        private PlcInterface plcInterface;
        private string plcHash, simuHash;
        private MD5 md5Hash;
        private int[] inputDecoder,outputDecoder;
        private MemoryBit[] memoryInputBits,memoryOutputBits;
        private Thread inputThread,outputThread;
        Boolean endThread;
        public SimInterface(PlcInterface plcInterface)
        {
            this.plcInterface = plcInterface;
            endThread = new Boolean();
            endThread = false;
            inputDecoder = new int[this.plcInterface.IBoolInPathCtr-1];
            outputDecoder = new int[this.plcInterface.IBoolOutPathCtr-1];
           
            for(int i = 0; i < this.plcInterface.IBoolInPathCtr -1 ; i++)
            {
                inputDecoder[i] = -1;
            }

            for(int i = 0; i < this.plcInterface.IBoolOutPathCtr -1 ; i++)
            {
                outputDecoder[i] = -1;
            }

            md5Hash = MD5.Create();
        }

        public void Dispose()
        {
            endThread = true;
            //Wait for threads to clean up
            while(inputThread.IsAlive || outputThread.IsAlive);

            Console.WriteLine("SimInterface disposed.");
            ((IDisposable)md5Hash).Dispose();
        }
        public void Init()
        {
            //MemoryMap.Instance.Update();
            memoryInputBits = MemoryMap.Instance.GetBitMemories(MemoryType.Input);
            for (int i = 0; i < memoryInputBits.Length - 1; i++)
            {
                MemoryBit memoryBit = memoryInputBits[i];
                simuHash = memoryBit.Name;
                if(simuHash.Equals("")
                    || simuHash.Length!=32)
                    continue;
                //Search for the hash in the list of plc symbols
                //Console.WriteLine("simuHash: {0} index: {1} ", simuHash, i );
                for (int k = 0; k < plcInterface.IBoolInPathCtr - 1; k++)
                {
                    plcHash = plcInterface.GetMd5Hash(md5Hash, plcInterface.ArrBoolInPaths[k]);
                    //Console.WriteLine("PLCHash: {0} SimuHash: {1}", plcHash, simuHash );
                    if (plcHash.Equals(simuHash))
                    {
                        
                        //Console.Write("plcHash: {0} index: {1} ", plcHash, k );
                        //Console.WriteLine("Memory input found: index plc: {0} index simulation: {1}", k, i );
                        //Store the simuHash index
                        inputDecoder[k] = i;
                        //Console.WriteLine("Input PLCHash: {0} Input SimuHash: {1} k: {2} inputDecoder[k]: {3}", plcHash, memoryInputBits[inputDecoder[k]].Name,k ,inputDecoder[k] );

                        //Exit the loop
                        break;
                    }
                }
            }

            //MemoryMap.Instance.Update();
            memoryOutputBits = MemoryMap.Instance.GetBitMemories(MemoryType.Output);
            for (int i = 0; i < memoryOutputBits.Length - 1; i++)
            {
                //Get outputs from the plc and transfer them to the simulation.
                if(memoryOutputBits[i].Name.Equals("")
                    || memoryOutputBits[i].Name.Length!=32)
                    continue;

                for (int k = 0; k < plcInterface.IBoolOutPathCtr - 1; k++)
                {
                    plcHash = plcInterface.GetMd5Hash(md5Hash, plcInterface.ArrBoolOutPaths[k]);    
                    if (plcHash.Equals(memoryOutputBits[i].Name) )
                    {

                   
                        outputDecoder[k] = i;
                         //Console.WriteLine("Output PLCHash: {0} Output SimuHash: {1} k: {2} ouputDecoder[k]: {3}", plcHash, memoryOutputBits[outputDecoder[k]].Name,k ,outputDecoder[k] );
                        //Exit the loop
                        break;
                    }
                }    
            }

            inputThread = new Thread(InputsExchange);
            outputThread = new Thread(OutputsExchange);
            inputThread.Start();
            outputThread.Start();
        }

        public void InputsExchange()
        {
            
            while(!endThread)
            {
                //var watch = System.Diagnostics.Stopwatch.StartNew();
                MemoryMap.Instance.Update();
           
                for (int k = 0; k < (plcInterface.IBoolInPathCtr - 1) ; k++)
                {
                    if(inputDecoder[k] == -1)
                        continue;
                    //plcHash = plcInterface.GetMd5Hash(md5Hash, plcInterface.ArrBoolInPaths[k]);
                    //Console.WriteLine("Input PLCHash: {0} Input SimuHash: {1} k: {2} inputDecoder[k]: {3}", plcHash, memoryInputBits[inputDecoder[k]].Name,k ,inputDecoder[k] );
                    plcInterface.writeBooleanInput(k, memoryInputBits[inputDecoder[k]].Value);              
                }
                  
                //watch.Stop();
                //var elapsedMs = watch.ElapsedMilliseconds;
                //Console.WriteLine("InputsExchange elapsed time: {0}",elapsedMs);
            }
        }
        public void OutputsExchange()
        {
            while(!endThread)
            {
                //var watch = System.Diagnostics.Stopwatch.StartNew();
                for (int k = 0; k < (plcInterface.IBoolOutPathCtr - 1) ; k++)
                {
                    if(outputDecoder[k] == -1)
                        continue;

                    memoryOutputBits[outputDecoder[k]].Value = plcInterface.readBooleanOutput(k);
               
                }
           
                MemoryMap.Instance.Update();
                //watch.Stop();
                //var elapsedMs = watch.ElapsedMilliseconds;
                //Console.WriteLine("OutputsExchange elapsed time: {0}",elapsedMs);
            }
        }

        public void Execute()
        {
            while(true)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                MemoryMap.Instance.Update();
           
                for (int k = 0; k < (plcInterface.IBoolInPathCtr - 1) ; k++)
                {
                    if(inputDecoder[k] == -1)
                        continue;
                    //plcHash = plcInterface.GetMd5Hash(md5Hash, plcInterface.ArrBoolInPaths[k]);
                    //Console.WriteLine("Input PLCHash: {0} Input SimuHash: {1} k: {2} inputDecoder[k]: {3}", plcHash, memoryInputBits[inputDecoder[k]].Name,k ,inputDecoder[k] );
                    plcInterface.writeBooleanInput(k, memoryInputBits[inputDecoder[k]].Value);              
                }
           
            
                for (int k = 0; k < (plcInterface.IBoolOutPathCtr - 1) ; k++)
                {
                    if(outputDecoder[k] == -1)
                        continue;

                    //plcHash = plcInterface.GetMd5Hash(md5Hash, plcInterface.ArrBoolOutPaths[k]);
                    //Console.WriteLine("Output PLCHash: {0} Output SimuHash: {1} k: {2} outputDecoder[k]: {3}", plcHash, memoryOutputBits[outputDecoder[k]].Name,k ,outputDecoder[k] );
                    memoryOutputBits[outputDecoder[k]].Value = plcInterface.readBooleanOutput(k);
               
                }
           
                MemoryMap.Instance.Update();
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine("Elapsed time: {0}",elapsedMs);
            }
        // the code that you want to measure comes here
           
        }
    }
}
