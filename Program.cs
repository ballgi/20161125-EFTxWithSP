using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _20161125_EFTxWithSP
{
    class Program
    {
        static void Main(string[] args)
        {
            EFTxWithSP obj = new EFTxWithSP();
            //Console.WriteLine("==== CallSPinExplicitTx ====");
            //obj.CallSpWithExplicitTx();
            //Console.WriteLine("==== CallSpWithImplicitTx ====");
            //obj.CallSpWithImplicitTx();
            Console.WriteLine("==== CallSpWithSameTxInContext ====");
            obj.CallSpWithSameTxInContext();
        }
    }
}
