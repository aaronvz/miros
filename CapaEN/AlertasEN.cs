using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEN
{
    public class AlertasByPersonaRequest
    {
        public string numerodocumento { get; set; }
        public string tipodocumento { get; set; }
        public string icaopaisemision { get; set; }
        public string primernombre { get; set; }
        public string segundonombre { get; set; }
        public string tercernombre { get; set; }
        public string primerapellido { get; set; }
        public string segundoapellido { get; set; }
        public string apellidocasada { get; set; }
        public string usuarioingresa { get; set; }
    }

    public class AlertasResponse
    {
        public int codigo { get; set; }
        public DataAlertas[] data { get; set; }
        public string mensaje { get; set; }
        public string codigoSys { get; set; }
    }


    public class DataAlertas
    {
        public int idbusqueda { get; set; }
    }
}
