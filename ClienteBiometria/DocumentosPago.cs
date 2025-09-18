using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENROLLMENT_V3
{
    public class CUI_BUSQUEDA_PAGO
    {
        public string cui { get; set; }
        public string documento { get; set; }
        public string mensaje { get; set; }
    }

    public class BOLETA
    {
        public string codigo { get; set; }
        public string mensaje { get; set; }
        public Data data { get; set; }
    }

    public class TRANSACCION
    {
        public string codigo { get; set; }
        public string mensaje { get; set; }
        public Data data { get; set; }
    }

    public class RECIBO
    {
        public string codigo { get; set; }
        public string mensaje { get; set; }
        public Data data { get; set; }
    }

    public class Data
    {
        public int boleta { get; set; }
        public int transaccion { get; set; }
        public int recibo { get; set; }
        public string documento { get; set; }
    }

}
