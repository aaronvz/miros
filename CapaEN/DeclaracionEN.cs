using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEN
{
    public class DeclaracionByLibretaRequest
    {
        public int id { get; set; }
        public string libreta { get; set; }
        public string fechaNac { get; set; }
        public string tipoMov { get; set; }
        public string nacionalidad { get; set; }
        public string fechaMov { get; set; }
        public int delegacion { get; set; }
    }

    public class DeclaracionByNumeroRequest
    {
        public string numeroDeclaracion { get; set; }
    }

    public class DeclaracionUpdateStatus
    {
        public long idDeclaracion { get; set; }
        public long movimiento { get; set; }
    }

    public class DeclaracionResponse
    {
        public int codigo { get; set; }
        public DeclaracionData data { get; set; }
        public string mensaje { get; set; }
        public string codigoSys { get; set; }
    }

    public class DeclaracionData
    {
        public DateTime creado { get; set; }
        public int id { get; set; }
        public string correlativo { get; set; }
        public bool equipoAcomp { get; set; }
        public int equipoNoAcomp { get; set; }
        public bool exoneracionTribuo { get; set; }
        public int valorDinero { get; set; }
        public bool traeAnimalesPlantas { get; set; }
        public bool traeQuimicosSustancias { get; set; }
        public bool traeMercaderiaDistintas { get; set; }
        public DateTime fechaMovimiento { get; set; }
        public DeclaracionPersona persona { get; set; }
        public DeclaracionViaje viaje { get; set; }
        public List<DeclaracionAcompaniante> acompanantes { get; set; }
        //public List<object> mercaderias { get; set; }
        //public List<object> detalleMonetario { get; set; }
        public List<Migrante> migrante { get; set; }

    } 

    public class DeclaracionPersona
    {
        public string primerNombre { get; set; }
        public object segundoNombre { get; set; }
        public string primerApellido { get; set; }
        public object segundoApellido { get; set; }
        public string sexo { get; set; }
        public DateTime fechaNacimiento { get; set; }
        public DeclaracionNacionalidad nacionalidad { get; set; }
        public string noDocumento { get; set; }
        public DeclaracionTipoDocumento tipoDocumento { get; set; }
        public string libreta { get; set; }
        public string identificacion { get; set; }
    }

    public class DeclaracionTipoDocumento
    {
        public int id { get; set; }
        public string nombre { get; set; }
    }

    public class DeclaracionNacionalidad
    {
        public string id { get; set; }
        public string nombre { get; set; }
    }

    public class DeclaracionViaje
    {
        public string empresaTrans { get; set; }
        public string noVueloEmb { get; set; }
        public string noViaje { get; set; }
        public string tipoMovimiento { get; set; }
        public DeclaracionDelegacion delegacion { get; set; }
        public DeclaracionPais paisProcedencia { get; set; }
        public DeclaracionPais paisDestino { get; set; }

    }

    public class DeclaracionDelegacion
    {
        public string id { get; set; }
        public string nombre { get; set; }
        public DeclaracionDelegacionesIGM[] delegacionesIGM { get; set; }
    }

    public class DeclaracionDelegacionesIGM
    {
        public int id { get; set; }
        public DeclaracionDelegacionIGM delegacion { get; set; }
    }

    public class DeclaracionDelegacionIGM
    {
        public int id { get; set; }
        public string nombre { get; set; }
    }

    public class DeclaracionPais
    {
        public string id { get; set; }
        public string nombre { get; set; }
    }

    public class DeclaracionAcompaniante
    {
        public DateTime creado { get; set; }
        public int estado { get; set; }
        public int id { get; set; }
        public string nombre { get; set; }
        public string numeroDoc { get; set; }
        public string identificacion { get; set; }
        public DeclaracionTipoDocumento tipoDocumento { get; set; }
        public DeclaracionPersona persona { get; set; }
        public DeclaracionNacionalidad nacionalidad { get; set; }
    }


    public class Migrante
    {
        public string nombre { get; set; }
        public object segundoNombre { get; set; }
        public string primerApellido { get; set; }
        public object segundoApellido { get; set; }
        public string sexo { get; set; }
        public DateTime fechaNacimiento { get; set; }
        public DeclaracionNacionalidad nacionalidad { get; set; }
        public string noDocumento { get; set; }
        public int acompananteID { get; set; }
        public DeclaracionTipoDocumento tipoDocumento { get; set; }
        public bool titular { get; set; }
        public string identificacion { get; set; }
        public string libreta { get; set; }
    }
}
