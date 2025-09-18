using System;
using System.Linq;
using System.Text;
using System.Management;
using System.Net.NetworkInformation;

namespace UtilityCore
{
    public class DataDivice
    {
        /// <summary>
        /// Optener numero de bios 
        /// </summary>
        /// <returns> Number bios string</returns>
        public static string getNumberBios(){
            ManagementObjectSearcher mSearcher = new ManagementObjectSearcher("SELECT SerialNumber, SMBIOSBIOSVersion, ReleaseDate FROM Win32_BIOS");
            ManagementObjectCollection collection = mSearcher.Get();
            string serial = "";
            foreach (ManagementObject obj in collection)
            {
                serial= (string)obj["SerialNumber"];
            }
            return serial;
             
          /*  SelectQuery query = new SelectQuery(@"Select * from Win32_ComputerSystem");
            string divice="";
            //initialize the searcher with the query it is supposed to execute
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                //execute the query
                foreach (ManagementObject process in searcher.Get())
                {
                    //print system info
                    process.Get();
                    divice = process["Model"].ToString();
                }
            }
            return divice;*/
        }
        /// <summary>
        ///  Optener el UUID
        /// </summary>
        /// <returns> return string </returns>
        public static string getUUID()
        {
            string ComputerName = "localhost";
            ManagementScope Scope;
            Scope = new ManagementScope(String.Format("\\\\{0}\\root\\CIMV2", ComputerName), null);
            Scope.Connect();
            ObjectQuery Query = new ObjectQuery("SELECT UUID FROM Win32_ComputerSystemProduct");
            ManagementObjectSearcher Searcher = new ManagementObjectSearcher(Scope, Query);
            string uuid="";

            foreach (ManagementObject WmiObject in Searcher.Get())
            {
                uuid = WmiObject["UUID"].ToString();
                //Console.WriteLine("{0,-35} {1,-40}", "UUID", WmiObject["UUID"]);// String                     
            }
            return uuid;
        }
        public static string getProcesorID(){
            string proc = "";
            var mbs = new ManagementObjectSearcher("Select ProcessorId From Win32_processor");
            ManagementObjectCollection mbsList = mbs.Get();
            foreach (ManagementObject mo in mbsList)
            {
                proc = mo["ProcessorId"].ToString();
                break;
            }
            return proc;
        }
        /// <summary>
        /// Optener Mac 
        /// </summary>
        /// <returns> return String </returns>
        public static string getMac()
        {
            String firstMacAddress = "";
             firstMacAddress = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Select(nic => nic.GetPhysicalAddress().ToString())
                .FirstOrDefault();

            return  firstMacAddress;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string getSerialVol()
        {


            string serialVol = string.Empty;
            string str1 = "C";
            StringBuilder stringBuilder = new StringBuilder();
            foreach (ManagementObject managementObject in (new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive")).Get())
            {
                ManagementObject managementObject1 = new ManagementObject(string.Concat("Win32_Logicaldisk=\"", str1, ":\""));
                try
                {
                    managementObject1.Properties["Volumename"].Value.ToString();
                    serialVol = managementObject1.Properties["Volumeserialnumber"].Value.ToString();
                }
                finally
                {
                    if (managementObject1 != null)
                    {
                        ((IDisposable)managementObject1).Dispose();
                    }
                }
            }            
            return serialVol;
        }
        public static string getDivice()
        {
            System.Management.SelectQuery query = new System.Management.SelectQuery(@"Select * from Win32_ComputerSystem");
            string device = string.Empty;
            //initialize the searcher with the query it is supposed to execute
            using (System.Management.ManagementObjectSearcher searcher = new System.Management.ManagementObjectSearcher(query))
            {
                //execute the query
                foreach (System.Management.ManagementObject process in searcher.Get())
                {
                    //print system info
                    process.Get();
                    device = process["Model"].ToString();
                }
            }
            return device;
        }



    }
}
