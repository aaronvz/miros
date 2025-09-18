using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEN
{
    public class BiometriaUsuarioRequest
    {
        public string id_usuario { get; set; }
        public string foto { get; set; }
        public Huellas[] huellas { get; set; }
    }

    public class Huellas
    {
        public int id_posicion { get; set; }
        public string huella_png { get; set; }
        public string huella_wsq { get; set; }
    }

    public class BiometriaUsuarioResponse
    {
        public int codigo { get; set; }
        public UsuarioData[] data { get; set; }
        public string mensaje { get; set; }
        public string codigoSys { get; set; }
    }

    public class UsuarioData
    {
    }
}
