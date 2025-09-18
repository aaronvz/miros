using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEN
{
    public class MovimientoSIOMRequest
    {
        public string primer_nombre { get; set; }
        public string segundo_nombre { get; set; }
        public string primer_apellido { get; set; }
        public string segundo_apellido { get; set; }
        public string fecha_nac { get; set; }
        public string icao { get; set; }
        public string tipoflujo { get; set; }
    }

    public class MovimientoSIOMResponse
    {
        public int codigo { get; set; }
        public DataMovimientoSIOM data { get; set; }
        public string mensaje { get; set; }
        public string codigoSys { get; set; }
    }


    public class DataMovimientoSIOM
    {
        public int validacionflujosiom { get; set; }
    }
}
