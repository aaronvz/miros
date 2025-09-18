using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CapaEN
{
    public class TipoVisaRequest
    {
        public int id { get; set; }
    }

    public class TipoVisaResponse
    {
        public int codigo { get; set; }
        public TipoVisaData[] data { get; set; }
        public string mensaje { get; set; }
        public string codigoSys { get; set; }
    }
     
    public class TipoVisaData
    {
        public int idcategoriadocumento { get; set; }
        public string nombrecategoria { get; set; }
    }

    [XmlRoot(ElementName = "Nodo")]
    public class NodoTipoVisa
    {

        [XmlElement(ElementName = "idcategoriadocumento")]
        public int idcategoriadocumento { get; set; }

        [XmlElement(ElementName = "nombrecategoria")]
        public string nombrecategoria { get; set; }
    }

    [XmlRoot(ElementName = "Lista")]
    public class ListaTipoVisa
    {

        [XmlElement(ElementName = "Nodo")]
        public List<NodoTipoVisa> Nodo { get; set; }
    }
}
