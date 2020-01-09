using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using EngineIO;
namespace PlcSimInterface
{
    class SimInterface
    {
        private PlcInterface plcInterface;
        private string plcHash, simuHash;
        private MD5 md5Hash;

        public SimInterface(PlcInterface plcInterface)
        {
            this.plcInterface = plcInterface;
        }

        public void Dispose()
        {
            ((IDisposable)md5Hash).Dispose();
        }

        public void Execute()
        {
            
            Console.WriteLine();
            Console.WriteLine("Press enter to end the program.");
            Console.WriteLine();
            md5Hash = MD5.Create();
           
            for (int i = 0; i < plcInterface.IBoolInPathCtr - 1; i++)
            {
                //Get inputs from the simulation and transfer them to plc.
                MemoryBit memoryBit = MemoryMap.Instance.GetBit(i, MemoryType.Input);
                simuHash = memoryBit.Name;
                //Search for the hash in the list of plc symbols

                for (int k = 0; k < plcInterface.IBoolInPathCtr - 1; k++)
                {
                    plcHash = plcInterface.GetMd5Hash(md5Hash, plcInterface.ArrBoolInPaths[k]);
                    if (string.Compare(plcHash, simuHash) == 0)
                    {
                        MemoryMap.Instance.Update();
                        //Write input value to plc input
                        plcInterface.writeBooleanInput(k, memoryBit.Value);
                        //Exit the loop
                        break;
                    }
                }
            }

            MemoryBit[] memoryBits = MemoryMap.Instance.GetBitMemories(MemoryType.Output);
            for (int i = 0; i < plcInterface.IBoolOutPathCtr - 1; i++)
            {
                //Get outputs from the plc and transfer them to the simulation.
                plcHash = plcInterface.GetMd5Hash(md5Hash, plcInterface.ArrBoolOutPaths[i]);
                for (int k = 0; k < plcInterface.IBoolOutPathCtr - 1; k++)
                {
                        
                    if (string.Compare(plcHash, memoryBits[k].Name) == 0)
                    {
                        //Get plc output and write it to simu output
                        memoryBits[k].Value = plcInterface.readBooleanOutput(i);
                        MemoryMap.Instance.Update();
                        //Exit the loop
                        break;
                    }
                }    
            }
        }
    }
}
