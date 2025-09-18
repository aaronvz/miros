using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;


namespace CapaEN
{
    public class JsonResponseMovimiento
    {
        public int codigo { get; set; }
        public string mensaje { get; set; }
        
        public Data[] data { get; set; }

    }

    public class Data
    {
        public string nombrecompleto { get; set; }
        public string numerodocumento { get; set; }
        public long idflujo { get; set; }
        public long idsegundarevision { get; set; }
    }
}
