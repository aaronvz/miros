using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEN
{
    public class EquipoRequest
    {
        public string bios { get; set; }
    }

    public class EquipoResponse
    {
        public int codigo { get; set; }
        public EquipoData[] data { get; set; }
        public string mensaje { get; set; }
        public string codigoSys { get; set; }
    }

    public class EquipoData
    {
        public int equipo_id { get; set; }
        public string mac_address { get; set; }
        public string ip { get; set; }
        public int sede_id { get; set; }
        public string nombre { get; set; }
        public int tipo_flujo { get; set; }
        public string nombre_tipo_flujo { get; set; }
        public string estado { get; set; }
    }
}
