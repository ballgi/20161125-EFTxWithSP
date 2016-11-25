using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _20161125_EFTxWithSP.Models;

namespace _20161125_EFTxWithSP
{
    public class EFTxWithSP
    {

        /// <summary>
        /// Stored Procedure 在單獨的 Transaction
        /// </summary>
        /// <remarks>
        /// [問題] Stored Procedure 預設會單獨 1 個 Transaction, 即使 EF 沒有 SaveChanges(), 仍會 Commit !
        /// </remarks>
        public void CallSPinExplicitTx()
        {
            using (EFTestDBEntities ctx = new EFTestDBEntities())
            {
                ObjectParameter orderno = new ObjectParameter("po_order_no", typeof(String));
                ctx.usp_get_order_no(orderno);
                Console.WriteLine(orderno.Value);

                ctx.MyOrders.Add(new MyOrder()
                {
                    OrderNo = orderno.Value.ToString(),
                    ShipName = "jasper",
                    ShipAddress = "taipei",
                    TotalAmt = 1000
                });
                //故意不作 SaveChange(), 查結果, 可以發現 OrderNoGenerators 這個 table 的資料有異動
                //ctx.SaveChanges();
            }
        }


    }
}
