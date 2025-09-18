using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEN
{
    public class UsuariosEN
    {
        public string NOMBRE { get; set; }
        public string USUARIO { get; set; }
        public string HUELLA_WSQ { get; set; }

        public string ID_USUARIO { get; set; }
        public string CUI { get; set; }
        public string NOMBRES { get; set; }
        public string APELLIDOS { get; set; }
        public string CONTRASEÑA { get; set; }
        public byte[] INDICE_DERECHO_BMP { get; set; }
        public byte[] INDICE_DERECHO_BINARIZADA { get; set; }
        public byte[] INDICE_DERECHO_PLANTILLA { get; set; }
        public byte[] INDICE_DERECHO_WSQ { get; set; }
    }
}
