using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CapaEN
{
    public class PaisRequest
    {
        public int id { get; set; }
    }

    public class PaisResponse
    {
        public int codigo { get; set; }
        public PaisData[] data { get; set; }
        public string mensaje { get; set; }
        public string codigoSys { get; set; }
    }

    public class PaisData
    {
        public int idpais { get; set; }
        public string icaopais { get; set; }
        public string nombrepais { get; set; }
        public int idcategoriavisa { get; set; }
        public string categoriavisa { get; set; }
        public string acuerdopais { get; set; }
    }

    [XmlRoot(ElementName = "Nodo")]
    public class NodoPais
    {

        [XmlElement(ElementName = "ICAO")]
        public string ICAO { get; set; }

        [XmlElement(ElementName = "Codigo")]
        public int Codigo { get; set; }

        [XmlElement(ElementName = "Nombre")]
        public string Nombre { get; set; }

        [XmlElement(ElementName = "idcategoriavisa")]
        public int idcategoriavisa { get; set; }

        [XmlElement(ElementName = "categoriavisa")]
        public string categoriavisa { get; set; }

        [XmlElement(ElementName = "acuerdopais")]
        public string acuerdopais { get; set; }
    }

    [XmlRoot(ElementName = "Lista")]
    public class ListaPais
    {

        [XmlElement(ElementName = "Nodo")]
        public List<NodoPais> Nodo { get; set; }
    }
}
