using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.SqlClient;

namespace CapaEN
{
    public class Enrolamiento
    {
        public Guid Id { get; set; }
        public string TipoMovimiento { get; set; }

        //DATOS DE EGRESO / INGRESO
        public string NumeroVuelo { get; set; }
        public string Aerolinea { get; set; }
        public string PaisOrigen { get; set; }
        public string PaisDestino { get; set; }
        
        //NACIONALIDAD
        public string Nacionalidad { get; set; }
        public string PaisResidencia { get; set; }
        public string PaisNacimiento { get; set; }
        public string PaisExpedicion { get; set; }

        //DATOS DE IDENTIFICACIÓN
        public string TipoDocumento { get; set; }
        public int TipoPasaporte { get; set; }
        public string NumeroDocumento { get; set; }
        public string FechaNacimiento { get; set; }

        //DATOS GENERALES
        public string PrimerApellido { get; set; }
        public string SegundoApellido { get; set; }
        public string PrimerNombre { get; set; }
        public string SegundoNombre { get; set; }
        public string Sexo { get; set; }
        public string FechaVencimiento { get; set; }
        public int edad { get; set; }

        //DATOS MIGRATORIOS
        public int CategoriaMigratoria { get; set; }
        public int Caracteristicia { get; set; }
        public int TipoVisa { get; set; }
        public int Permanencia { get; set; }
        public int MotivoViaje { get; set; }
        public int Profesion { get; set; }
        public string Observaciones { get; set; }
        public int EnviarRS { get; set; }
        public string Mrz { get; set; }
        //public string Gestion { get; set; }
        public string Qr { get; set; }
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; }
        public string Sede { get; set; }
        public string Maquina { get; set; }
        public string Bios { get; set; }
        public string Ip { get; set; }
        public string LadoAVB64 { get; set; }
        public string LadoAUB64 { get; set; }
        public string LadoBVB64 { get; set; }
        public string LadoBUB64 { get; set; }
        public string RostroB64 { get; set; }
        public int PosicionH1 { get; set; }
        public string H1B64 { get; set; }
        public string H1Wsq { get; set; }
        public int PosicionH2 { get; set; }
        public string H2B64 { get; set; }
        public string H2Wsq { get; set; }
        public int CoincidenciaAlertas { get; set; }
        public List<Alerta> Alertas { get; set; }
        public int PPVT { get; set; }
        public string TipoEnrolamiento { get; set; }
        public int idUnidad { get; set; }
    }

    public class Movimiento
    {
        public JsonResponseMovimiento response;

        public string response_error;
        public Guid Id { get; set; }
        public List<Persona> adultos { get; set; }
        public List<Persona> menores { get; set; }
        public Viaje viaje { get; set; }
        public int iddelegado { get; set; }
	    public string usuarioingresa { get; set; }
        public string usuariomodifica { get; set; }
        public string fechamodifica { get; set; }
        public string usuarioelimina { get; set; }
        public string fechaelimina { get; set; }
        public DateTime fechaini { get; set; }
        public DateTime fechafin { get; set; }
        public double minutos { get; set; }
        public double segundos { get; set; }
        public string biosestacion { get; set; }
        public string makeScanner { get; set; }
        public string modelScanner { get; set; }
        public string serialScanner { get; set; }
        public string makeCamera { get; set; }
        public string modelCamera { get; set; }
        public string serialCamera { get; set; }

        public Movimiento()
        {
            adultos = new List<Persona>();
            menores = new List<Persona>();
        }
    }

    public class Probatorios
    {
        public string documentopadre { get; set; }
        public string documentomadre { get; set; }
    }
    public class Persona
    {
        public string primernombre { get; set; }
        public string segundonombre { get; set; }
        public string tercernombre { get; set; }
        public string primerapellido { get; set; }
        public string segundoapellido { get; set; }
        public string apellidocasada { get; set; }
        public int edad { get; set; }
        public string sexo { get; set; }
        public string fechanacimiento { get; set; }
        public string fechavencimientodoc { get; set; }
        public int documentovalido { get; set; }
        public int idtipodocumento { get; set; }
        public string tipodocumento { get; set; }
        public string icaopaisemision { get; set; }
        public string acuerdopais { get; set; }
        public string numerodocumento { get; set; }
        public int idnacionalidad { get; set; }
        public int? idtipovisa { get; set; }
        public string numerovisa { get; set; }
        public int idbusqueda {get; set;}
        public int idcategoria { get; set; }
        public int? idparentesco { get; set; }
        public int idciudad { get; set; }
        public int idprofesion { get; set; }
        public int enviarsegundarevision { get; set; }
        public int iddelegacion { get; set; }
        public string ipestacion { get; set; }
        public string clavedelegacion { get; set; }
        public List<Probatorios> probatorios { get; set; }
        public int? mna { get; set; }
        public string nombrecompleto { get; set; }
        public string declaracionjurada { get; set; }
        public int drjvtitular { get; set; }
        public string numerolibreta { get; set; }
        public string LadoAVB64 { get; set; }
        public string LadoAUB64 { get; set; }
        public string LadoBVB64 { get; set; }
        public string LadoBUB64 { get; set; } 
        public int RostroSegmentado { get; set; }
        //public int? categoriaimagenhuella { get; set; }
        public string codigocolorimagen { get; set; }
        public string algoritmocompresionimagen { get; set; }
        public int calidadhuella { get; set; }
        public string RostroB64 { get; set; }
        public string CoincideRostro { get; set; }
        public int PersonaSinHuellas { get; set; }
        public int CoincideHuella { get; set; }

        //MANO DERECHA DEDO 1 AL 5
        public int PosicionH1 { get; set; }
        public string H1B64 { get; set; }
        public string H1Wsq { get; set; }
        //public string H1Wsq_bdsp { get; set; }
        public int PosicionH2 { get; set; }
        public string H2B64 { get; set; }
        public string H2Wsq { get; set; }
        //public string H2Wsq_bdsp { get; set; }
        public int PosicionH3 { get; set; }
        public string H3B64 { get; set; }
        public string H3Wsq { get; set; }
        //public string H3Wsq_bdsp { get; set; }
        public int PosicionH4 { get; set; }
        public string H4B64 { get; set; }
        public string H4Wsq { get; set; }
        //public string H4Wsq_bdsp { get; set; }
        public int PosicionH5 { get; set; }
        public string H5B64 { get; set; }
        public string H5Wsq { get; set; }
        //public string H5Wsq_bdsp { get; set; }

        //MANO IZQUIERDA DEDO 6 AL 10
        public int PosicionH6 { get; set; }
        public string H6B64 { get; set; }
        public string H6Wsq { get; set; }
        //public string H6Wsq_bdsp { get; set; }
        public int PosicionH7 { get; set; }
        public string H7B64 { get; set; }
        public string H7Wsq { get; set; }
        //public string H7Wsq_bdsp { get; set; }
        public int PosicionH8 { get; set; }
        public string H8B64 { get; set; }
        public string H8Wsq { get; set; }
        //public string H8Wsq_bdsp { get; set; }
        public int PosicionH9 { get; set; }
        public string H9B64 { get; set; }
        public string H9Wsq { get; set; }
        //public string H9Wsq_bdsp { get; set; }
        public int PosicionH10 { get; set; }
        public string H10B64 { get; set; }
        public string H10Wsq { get; set; }
        //public string H10Wsq_bdsp { get; set; }
        //POSICIONH11
        public int PosicionH11 { get; set; }
        public string PDP11Wsq { get; set; }
        //POSICIONH12
        public int PosicionH12 { get; set; }
        public string PIP12Wsq { get; set; }
        //POSICIONH13
        public int PosicionH13 { get; set; }
        public string DCD13B64 { get; set; }
        public string DCD13Wsq { get; set; }
        //POSICIONH14
        public int PosicionH14 { get; set; }
        public string ICD14B64 { get; set; }
        public string ICD14Wsq { get; set; }
        //POSICIONH15
        public int PosicionH15 { get; set; }
        public string AP15B64 { get; set; }
        public string AP15Wsq { get; set; }
        public int validacionflujosiom { get; set; }
        public string mrzL1 { get; set; }
        public string mrzL2 { get; set; }
        public string mrzL3 { get; set; }
        public int mrzLines { get; set; }

        public string obs { get; set; }
        public EntrevistaVisaTrabajo entrevistavisatrabajo { get; set; }

        public Persona()
        {
            entrevistavisatrabajo = new EntrevistaVisaTrabajo();
        }
    }

    public class Viaje
    {
        public int idmotivoviaje { get; set; }
        public string nombremotivoviaje { get; set; }
        public string direccionresidencia { get; set; }
        public string direcionimprevista { get; set; }
        public int idtipoflujo { get; set; }
        public int idtipotransporte { get; set; }
        public int idciudadorigen { get; set; }
        public int idpaisorigen { get; set; }
        public int idciudaddestino { get; set; }
        public int idpaisdestino { get; set; }
        public int tiempo { get; set; }
        public int? iddetalletransporte { get; set; }
        public int idtransporte { get; set; }
        public int? idtipoclasificacionstatus { get; set; }
        public string icaopaisorigen { get; set; }
        public string icaopaisdestino { get; set; }
    }

    public class EntrevistaVisaTrabajo
    {
        //SI/NO/NR
        public string visatrabajadortemporal { get; set; }
        public string icaopaisdestinovisatrabajo { get; set; }
        public int? idempresareclutadora { get; set; }
    }

    public class Alerta
    {
        public string Id { get; set; }
        public string Fecha { get; set; }
        public string Origen { get; set; }
        public string Tipo { get; set; }
        public string Detalle { get; set; }
    }
}
