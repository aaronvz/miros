using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEN
{
    public class LoginRequest
    {
        public string username { get; set; }
        public string password { get; set; }
    }

    public class LoginResponse
    {
        public int codigo { get; set; }
        public LoginData[] data { get; set; }
        public string mensaje { get; set; }
        public string codigoSys { get; set; }
    }

    public class LoginData
    {
        public int ID_USUARIO { get; set; }
        public string USUARIO { get; set; }
        public string DESCRIPCION { get; set; }
        public string NOMBRES { get; set; }
        public string APELLIDOS { get; set; }
        public int STATUS { get; set; }
        public int LOGGEADO { get; set; }
        public int SEDE_ID { get; set; }
        public string PATHFOTO { get; set; }
        public int REINICIAR_BIOMETRIA { get; set; }
        public string CUI { get; set; }
        public string HUELLA1PNG { get; set; }
        public string HUELLA2PNG { get; set; }
        public string JWT_TOKEN { get; set; }
        public string biosestacion { get; set; }
    }
}
