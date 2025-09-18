using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;


namespace CapaEN
{
    public class JsonEntregaPasaporteEN        
    {    
    }

    public class JsonRequestEntregaPasaporte
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

    }

    public class JsonComplementoEntrega
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

    public class JsonResponseEntregaPasaporte
    {
        public int codigo { get; set; }
        public string mensaje { get; set; }
        //public string data { get; set; }
    }
}
