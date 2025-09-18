using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.SqlClient;

namespace CapaEN
{
    public class FotoPasaporte
    {
        public string nombre { get; set; }
        public string cui { get; set; }
        public string foto { get; set; }
    }

    public class WsFotoPasaporte
    {
        public int codigo { get; set; }
        public string mensaje { get; set; }
        public FotoPasaporte data { get; set; }
    }

    public class PasaporteEntregaEN
    {
        public string fecha { get; set; }
        public string usuario { get; set; }
        public string no_libreta { get; set; }
        public string no_caso { get; set; }
        public int solicitud_id { get; set; }
        public string imagen_postal1 { get; set; }
        public string imagen_postal2 { get; set; }
        public string imagen_postal3 { get; set; }
        public string estacion { get; set; }
        public ComplementoEntrega complemento { get; set; }

        public string id_pais_sede { get; set; }
        public string msisdn { get; set; }
        public string nombre_completo { get; set; }

    }

    public class ComplementoEntrega
    {
        public string img_pasaporte_visible { get; set; }
        public string img_pasaporte_uv { get; set; }
        public int intento_coincidir_huella { get; set; }
        public int huella_coincide { get; set; }
        public int huella_pos { get; set; }
        public string tipo_entrega { get; set; }
        //public string foto_entrega { get; set; }
        public string sede { get; set; }
        public string bios { get; set; }
        public List<string> probatorios { get; set; }
    }

    public class ConsultaPasaporte
    {
        public int codigo { get; set; }
        public string mensaje { get; set; }
        public SolicitudEN data { get; set; }
    }

}
