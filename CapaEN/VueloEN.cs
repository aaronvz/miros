using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CapaEN
{
    public class VueloRequest
    {
        public int id { get; set; }
        public string tipo { get; set; }
        public string codigodelegacion { get; set; }
    }

    public class VueloResponse
    {
        public int codigo { get; set; }
        public VueloData[] data { get; set; }
        public string mensaje { get; set; }
        public string codigoSys { get; set; }
    }

    public class VueloData
    {
        public int idvuelo { get; set; }
        public int idtransporte { get; set; }
        public int idciudaddestino { get; set; }
        public int idpaisorigen { get; set; }
        public int idciudadorigen { get; set; }
        public string nombrepaisorigen { get; set; }
        public string icaopaisorigen { get; set; }
        public int idpaisdestino { get; set; }
        public string nombrepaisdestino { get; set; }
        public string descripcionvuelo { get; set; }
        public string transporte { get; set; }
        public string icaopaisdestino { get; set; }
    }

    [XmlRoot(ElementName = "Nodo")]
    public class NodoVuelo
    {

        [XmlElement(ElementName = "idvuelo")]
        public int idvuelo { get; set; }

        [XmlElement(ElementName = "idtrasporte")]
        public int idtransporte { get; set; }

        [XmlElement(ElementName = "idciudaddestino")]
        public int idciudaddestino { get; set; }

        [XmlElement(ElementName = "idpaisorigen")]
        public int idpaisorigen { get; set; }

        [XmlElement(ElementName = "idciudadorigen")]
        public int idciudadorigen { get; set; }

        [XmlElement(ElementName = "nombrepaisorigen")]
        public string nombrepaisorigen { get; set; }

        [XmlElement(ElementName = "icaopaisorigen")]
        public string icaopaisorigen { get; set; }

        [XmlElement(ElementName = "idpaisdestino")]
        public int idpaisdestino { get; set; }

        [XmlElement(ElementName = "nombrepaisdestino")]
        public string nombrepaisdestino { get; set; }

        [XmlElement(ElementName = "descripcionvuelo")]
        public string descripcionvuelo { get; set; }

        [XmlElement(ElementName = "transporte")]
        public string transporte { get; set; }

        [XmlElement(ElementName = "icaopaisdestino")]
        public string icaopaisdestino { get; set; }

    }

    [XmlRoot(ElementName = "Lista")]
    public class ListaVuelo
    {

        [XmlElement(ElementName = "Nodo")]
        public List<NodoVuelo> Nodo { get; set; }
    }
}
