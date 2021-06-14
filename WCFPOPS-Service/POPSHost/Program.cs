using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace POPSHost
{
    class Program
    {
        static void Main(string[] args)
        {
            using (ServiceHost svc = new ServiceHost(typeof(WCFPOPS.POPS)))
            {
                svc.Open();
                Console.WriteLine("Server Has Started.");
                Console.WriteLine("Press Enter Key to stop server.");
                Console.ReadLine();
                svc.Close();
            }
        }
    }
}
