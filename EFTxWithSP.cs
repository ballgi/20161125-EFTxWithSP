using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _20161125_EFTxWithSP.Models;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;

namespace _20161125_EFTxWithSP
{
    public class EFTxWithSP
    {

        // --------------------------------------------------
        // 如何在 SQL Server Profiler 觀察 TRANSACTION 
        // https://weblogs.asp.net/dixin/where-is-transaction-events-in-sql-server-profiler
        // --------------------------------------------------

        /// <summary>
        /// Stored Procedure 在單獨的 Transaction
        /// </summary>
        /// <remarks>
        /// [問題] Stored Procedure 預設會單獨 1 個 Transaction, 即使 EF 沒有 SaveChanges(), 仍會 Commit !
        /// </remarks>
        public void CallSpWithExplicitTx()
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

        /// <summary>
        /// Stored Procedure 仍然在單獨的 Transaction
        /// </summary>
        /// <remarks>
        /// [探索1] 試一下 StackOverflow 的解法, 用 SQL Server Profiler 檢查, 沒有 BEGIN TRANSACTION 了, 
        /// 但因為 SQL Server 預設沒有 TRANSACTION 的 INSERT / UPDATE / DELETE 就會作 COMMIT, 所以沒有解決問題
        /// http://stackoverflow.com/questions/19991609/ef6-wraps-every-single-stored-procedure-call-in-its-own-transaction-how-to-prev
        /// </remarks>
        public void CallSpWithImplicitTx()
        {
            using (EFTestDBEntities ctx = new EFTestDBEntities())
            {
                //
                ctx.Configuration.EnsureTransactionsForFunctionsAndCommands = false;
                //
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

        /// <summary>
        /// Stored Procedure 與 EF Context 在相同的 Transaction
        /// 利用 EF 建立的 context 物件, 去產生 connection / transaction
        /// </summary>
        /// <remarks>
        /// [探索2] 試一下 MSDN 的解法, EF 6.X 以後, 可自行控制 TRANSACTION, 可以解決
        /// https://msdn.microsoft.com/en-us/library/dn456843(v=vs.113).aspx
        /// 注意: using (MyDBEntities ctx = new MyDBEntities()) 還沒有 open connection
        /// 注意: using (var tx = ctx.Database.BeginTransaction()) 會先 open connection 至 DB, 再 BEGIN TX
        /// </remarks>
        public void CallSpWithSameTxInContext()
        {
            using (EFTestDBEntities ctx = new EFTestDBEntities())
            {
                using (var tx = ctx.Database.BeginTransaction())
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
                    //試一下 SaveChanges(), 再 RollbackTransaction(), 看一下結果
                    try
                    {
                        ctx.SaveChanges();
                        //tx.Commit();
                        tx.Rollback();
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// 測試案例_4: 自行先建立 conn 及 tx 物件, 再建立 EF 的 context 物件; 再將 conn / tx 指派到 EF 的 context
        /// </summary>
        /// <remarks>
        /// [探索3] 試一下 MSDN 的解法, 先以 ADO.NET 執行 Stored Procedure, 再執行 EF 的工作
        /// https://msdn.microsoft.com/en-us/library/dn456843(v=vs.113).aspx
        /// Database First 的 .edmx, 無法如同範例程式設定 conn 及 contextOwnsConnection; 只有 Code First 才能作到 !!
        /// http://stackoverflow.com/questions/22102082/error-passing-existing-connections-to-dbcontext-constructor-when-using-database
        /// </remarks>
        public void CallSp_MSDN_02()
        {
            string connStr = ConfigurationManager.ConnectionStrings["EFTestDB"].ConnectionString;
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();

                //using (var tx = conn.BeginTransaction(System.Data.IsolationLevel.Snapshot))
                using (var tx = conn.BeginTransaction())
                {
                    var sqlCommand = new SqlCommand("usp_get_order_no");
                    sqlCommand.Connection = conn;
                    sqlCommand.Transaction = tx;
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    var orderno = new SqlParameter
                    {
                        ParameterName = "po_order_no",
                        SqlDbType = SqlDbType.NVarChar,
                        Size = 16,
                        Direction = ParameterDirection.Output
                    };
                    sqlCommand.Parameters.Add(orderno);
                    sqlCommand.ExecuteNonQuery();
                    Console.WriteLine(orderno.Value);

                    //using (var db = new DbContext(conn, contextOwnsConnection: false))
                    //{
                    //    //EFTestDBEntities ctx = new EFTestDBEntities();
                    //    //這裡產生的 ctx 是 null, 所以不能這樣作 !
                    //    EFTestDBEntities ctx = db as EFTestDBEntities;
                    //    ctx.MyOrders.Add(new MyOrder()
                    //    {
                    //        OrderNo = orderno.Value.ToString(),
                    //        ShipName = "jasper",
                    //        ShipAddress = "taipei",
                    //        TotalAmt = 1000
                    //    });
                    //    //試一下 SaveChanges(), 再 RollbackTransaction(), 看一下結果
                    //    try
                    //    {
                    //        ctx.SaveChanges();
                    //        //tx.Commit();
                    //        tx.Rollback();
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        tx.Rollback();
                    //        Console.WriteLine(ex.ToString());
                    //    }
                    //}

                }
            }
        }   //END method

    }   //END Class
}   //END namespace
