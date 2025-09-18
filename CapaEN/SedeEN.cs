using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEN
{
    public class SedeRequest
    {
        public int id { get; set; }
    }

    public class SedeResponse
    {
        public int codigo { get; set; }
        public SedeData[] data { get; set; }
        public string mensaje { get; set; }
        public string codigoSys { get; set; }
    }

    public class SedeData
    {
        public int iddelegacion { get; set; }
        public string nombredelegacion { get; set; }
        public string clave { get; set; }
        public string segmento { get; set; }
        public string estado { get; set; }
        public int idtipodelegacion { get; set; }
        public string nombretipodelegacion { get; set; }
        public int idciudad { get; set; }
        public string nombreciudad { get; set; }
    }
}
