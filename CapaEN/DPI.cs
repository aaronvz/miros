using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;


namespace CapaEN
{
    public class DPI
    {
        public string CUI = string.Empty;
        public string DEPARTAMENTO_VECINDAD = string.Empty;
        public string MUNICIPIO_VECINDAD = string.Empty;
        public string FECHA_EMISION = string.Empty;
        public string FECHA_EXPIRA = string.Empty;
        public string SERIE_NUMERO = string.Empty;
        public string CEDULA_VECINDAD = string.Empty;
        public string DEPARTAMENTO_CEDULA = string.Empty;
        public string MUNICIPIO_CEDULA = string.Empty;
        public string PRIMER_NOMBRE = string.Empty;
        public string SEGUNDO_NOMBRE = string.Empty;
        public string TERCER_NOMBRE = string.Empty;
        public string PRIMER_APELLIDO = string.Empty;
        public string SEGUNDO_APELLIDO = string.Empty;
        public string CASADA_APELLIDO = string.Empty;
        public string FECHA_NACIMIENTO = string.Empty;
        public string PAIS_NACIMIENTO = string.Empty;
        public string DEPARTAMENTO_NACIMIENTO = string.Empty;
        public string MUNICIPIO_NACIMIENTO = string.Empty;
        public string SEXO = string.Empty;
        public string OCUPACION = string.Empty;
        public string FOLIO = string.Empty;
        public string LIBRO = string.Empty;
        public string PARTIDA = string.Empty;
        public string ESTADO_CIVIL = string.Empty;
        public string MRZ = string.Empty;
        public string NACIONALIDAD = string.Empty;
        public string ETNIA = string.Empty;
        public System.Drawing.Image IMAGE;
        public string FOTOGRAFIA_BASE_64 = string.Empty;

        //PARA PADRE O MADRE DE UN MENOR DE EDAD
        public string PARENTESCO = string.Empty;

        public string DEPARTAMENTO_EMISION_DPI = string.Empty;
        public string MUNICIPIO_EMISION_DPI = string.Empty;

        /*public string DPI_INTENTADO = "N";
        public string DPI_ = "N";
        public string DPI_SIN_HUELLAS = "S";
        public bool MOC = false;
        public string FACE_INTENTADO = "N";
        public string FACE = "N";*/

        public int INTENTOS_MOC = 0;

        public int INTENTOS_LECTURA = 0;
        public bool INFORMACION_DPI_LEIDA = false;
        public string MENSAJE_ERROR = string.Empty;

        /*public bool IDENTIDAD_VALIDA = false;
        public bool FOTOGRAFIA_VALIDA = false;
        public bool HUELLAS_VALIDAS = false;
        public bool FIRMA_VALIDA = false;
        public bool PROBATORIOS_VALIDOS = false;*/

    }

    public class DataWsRenap
    {
        public string cui { get; set; }
        public string primer_nombre { get; set; }
        public string segundo_nombre { get; set; }
        public string tercer_nombre { get; set; }
        public string primer_apellido { get; set; }
        public string segundo_apellido { get; set; }
        public string apellido_casada { get; set; }
        public string fecha_nacimiento { get; set; }
        public string fecha_vencimiento { get; set; }
        public int edad { get; set; }
        public string genero { get; set; }
        public string pais_nacimiento { get; set; }
        public string depto_nacimiento { get; set; }
        public string munic_nacimiento { get; set; }
        public string estado_civil { get; set; }
        public string nombre_padre { get; set; }
        public string nombre_madre { get; set; }
        public string foto { get; set; }
        public bool foto_renap { get; set; }
    }

    public class CuiWsRenap
    {
        public int status { get; set; }
        public string mensaje { get; set; }
        public DataWsRenap data { get; set; }
    }


    public class DataWsUsuariosDGM
    {
        public string id_usuario { get; set; }
        public string usuario { get; set; }
        public string clave { get; set; }
        public string nombres { get; set; }
        public string apellidos { get; set; }
        public int bloqueado { get; set; }
        public object sede_id { get; set; }
        public string sede_usuario { get; set; }
        public string descripcion { get; set; }
        public string huella1img { get; set; }
        public string huella1wsq { get; set; }
        public string huella2img { get; set; }
        public string huella2wsq { get; set; }
        public string foto { get; set; }
        public string activo { get; set; }
        public string tbl_ms_id_memoria_segura { get; set; }
        public string tbl_ms_activo { get; set; }
        public string tbl_ms_serial_number { get; set; }
        public object tbl_ms_fecha_inactivacion { get; set; }
        public string tbl_um_id_usuario_memoria { get; set; }
        public string tbl_um_id_usuario { get; set; }
        public string tbl_um_id_memoria { get; set; }
        public string tbl_um_activo { get; set; }
        public string tbl_um_fecha_inactivacion { get; set; }
        public string cambioclave { get; set; }
    }

    public class DataBioWsUsuariosDGM
    {
        public string id_usuario { get; set; }
        public string huella1 { get; set; }
        public string huella2 { get; set; }
        public string huella1wsq { get; set; }
        public string huella2wsq { get; set; }
        public string foto { get; set; }
    }

    public class InfoWsUsuarioDGM
    {
        public int codigo { get; set; }
        public string mensaje { get; set; }
        public List<DataWsUsuariosDGM> data { get; set; }
    }

    public class RespuestaWsUsuarioDGM
    {
        public string codigo { get; set; }
        public string mensaje { get; set; }
        public string data { get; set; }
    }

    public class Sibio
    {
        public string codigo { get; set; }
        public string mensaje { get; set; }
        public string data { get; set; }
    }

    public class SibioParametrosConsulta
    {
        public string cui { get; set; }
        public string fingerleft { get; set; }
        public string leftcod { get; set; }
        public string fingerright { get; set; }
        public string rightcod { get; set; }
    }

    public class ParametrosCasoPasaporte
    {
        public string caso { get; set; }
        public string idCita { get; set; }
    }

    public class InfoWsUsuarioDGMConsulta
    {
        public string usuario { get; set; }
        public string clave { get; set; }
    }

    public class RespuestaEnrolamiento
    {
        public string codigo { get; set; }
        public string mensaje { get; set; }
        public List<DetalleRespuestaEnrolamiento> detalle { get; set; }
    }

    public class DetalleRespuestaEnrolamiento
    {
        public string clave { get; set; }
        public string valor { get; set; }
    }
}
