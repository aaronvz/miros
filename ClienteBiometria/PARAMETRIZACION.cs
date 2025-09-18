using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENROLLMENT_V3
{    
    public class PARAMETRIZACION
    {
        public string IDENTIFICADOR_MAQUINA = "";
        public int INTENTOS_MOC_TITULAR = 4;
        public int INTENTOS_MOC_PADRE = 4;
        public int INTENTOS_MOC_MADRE = 4;

        public enum TipoEscanerHuellas
        {
            Unidactilar,
            Multidactilar
        }

        public enum OrientancionPantalla
        {
            Horizontal,
            Vertical
        }

        public enum Mano
        {
            Derecha,
            Izquierda
        }

        public enum EstatusIcono
        {
            Error,
            Carga,
            Exito
        }
    }
}
