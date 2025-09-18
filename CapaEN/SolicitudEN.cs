using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;


namespace CapaEN
{    

    public class SolicitudEN
    {
        public int codigo { get; set; }
        public string mensaje { get; set; }
        public int id { get; set; }
        public string caso { get; set; }
        public string sede_texto { get; set; }
        public int sede_id { get; set; }
        public int moc { get; set; }
        public int validada { get; set; }
        public int tipo_pasaporte_id { get; set; }
        public string tipo_pasaporte { get; set; }
        public string numero_pasaporte { get; set; }
        public string numero_recibo { get; set; }
        public string primer_nombre { get; set; }
        public string segundo_nombre { get; set; }
        public string primer_apellido { get; set; }
        public string segundo_apellido { get; set; }
        public string apellido_casada { get; set; }
        public string sexo { get; set; }
        public string direccion1 { get; set; }
        public string direccion2 { get; set; }
        public string direccion3 { get; set; }
        public string direccion_ciudad { get; set; }
        public string direccion_estado { get; set; }
        public string direccion_codigo_postal { get; set; }
        public int direccion_departamento { get; set; }
        public int direccion_municipio { get; set; }
        public string sede_entrega_texto { get; set; }
        public int sede_entrega_id { get; set; }
        public string direccion_envio1 { get; set; }
        public string direccion_envio2 { get; set; }
        public string direccion_envio_ciudad { get; set; }
        public string direccion_envio_estado { get; set; }
        public string direccion_envio_codigo_postal { get; set; }
        public string telefono_residencia { get; set; }
        public string telefono_trabajo { get; set; }
        public string telefono_celular { get; set; }
        public string telefono_envio { get; set; }
        public string correo_electronico { get; set; }
        public string fecha_nacimiento { get; set; }
        public int departamento_id { get; set; }
        public int municipio_id { get; set; }
        public int pais_id { get; set; }
        public int estado_civil_id { get; set; }
        public int nacionalidad { get; set; }
        public string ocupacion_detalle { get; set; }
        public int pais_residencia { get; set; }
        public int pais_envio { get; set; }
        public int color_ojos_id { get; set; }
        public int color_tez_id { get; set; }
        public int color_cabello_id { get; set; }
        public int estatura { get; set; }
        public int tipo_identificacion_id { get; set; }
        public string numero_identificacion { get; set; }
        public string serie_identificacion { get; set; }
        public string cui { get; set; }
        public int departamento_emision { get; set; }
        public int municipio_emision { get; set; }
        public string fecha_emision { get; set; }
        public int menor { get; set; }
        public string nombres_padre { get; set; }
        public string apellidos_padre { get; set; }
        public int tipo_id_padre { get; set; }
        public string numero_id_padre { get; set; }
        public string nombres_madre { get; set; }
        public string apellidos_madre { get; set; }
        public int tipo_id_madre { get; set; }
        public string numero_id_madre { get; set; }
        public string partida { get; set; }
        public string folio { get; set; }
        public string libro { get; set; }
        public string delegado_autoriza { get; set; }
        public string imagen_solicitud { get; set; }
        public string estado { get; set; }
        public string detalle_estado { get; set; }
        public string tramite { get; set; }
        public string numero_libreta { get; set; }
        public DateTime fecha_anterior { get; set; }
        public DateTime fecha_vencimiento { get; set; }
        public string libreta_anterior { get; set; }
        public string foto { get; set; }
        public string solictiud { get; set; }
        public string firma { get; set; }
        public string huella_png1 { get; set; }
        public string huella_png2 { get; set; }
        public string huella_pos1 { get; set; }
        public string huella_pos2 { get; set; }
        public string huella_obs1 { get; set; }
        public string huella_obs2 { get; set; }
        public string huella_wsq1 { get; set; }
        public string huella_wsq2 { get; set; }
        public string imagen_postal1 { get; set; }
        public string imagen_postal2 { get; set; }
        public string imagen_postal3 { get; set; }
        public string firma_forzada { get; set; }
        public string foto_forzada { get; set; }
        public string dpi_intentado { get; set; }
        public string dpi_lectura { get; set; }
        public string dpi_sin_huellas { get; set; }
        public string cui_menor { get; set; }
        public string validacion_afis { get; set; }
        public string estado_plastico { get; set; }
        public string observaciones { get; set; }
        public string usuario { get; set; }
        public string tracking { get; set; }
        public string created_by { get; set; }
        public string updated_by { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public bool enviado { get; set; }
        public int asignado_correlativo { get; set; }
        public int mocf { get; set; }
        public int cbs { get; set; }
        public string cbs_posicion_dedo { get; set; }
        public int id_pais_sede { get; set; }
    }    
}
