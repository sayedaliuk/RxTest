using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rxTEst
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var svc = new OrchestratorService();
                svc.Start();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception : {ex.Message}. {ex.InnerException?.Message}. {ex.StackTrace}");
            }
            finally
            {
                Console.ReadKey();
            }
        }
    }
}
