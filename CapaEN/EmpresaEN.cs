using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CapaEN
{
    public class EmpresaRequest
    {
        public string filtro { get; set; }
    }

    public class EmpresaResponse
    {
        public int codigo { get; set; }
        public EmpresaData[] data { get; set; }
        public string mensaje { get; set; }
        public string codigoSys { get; set; }
    }

    public class EmpresaData
    {
        public int idempresa { get; set; }
        public string nombreempresa { get; set; }
    }

    [XmlRoot(ElementName = "Nodo")]
    public class NodoEmpresa
    {

        [XmlElement(ElementName = "idempresa")]
        public int idempresa { get; set; }

        [XmlElement(ElementName = "nombreempresa")]
        public string nombreempresa { get; set; }

    }

    [XmlRoot(ElementName = "Lista")]
    public class ListaEmpresa
    {

        [XmlElement(ElementName = "Nodo")]
        public List<NodoEmpresa> Nodo { get; set; }
    }
}
