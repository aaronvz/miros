using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEN
{
    public class XmlDatosEN
    {
        public string SedeEnrolamiento { get; set; }
        public string FechaCaptura { get; set; }
        public string Usuario { get; set; }
        public string NumeroCaso { get; set; }
        public string TipoTramite { get; set; }
        public string TipoPasaporte { get; set; }
        public string NoRecibo { get; set; }
        public string NoPasaporte { get; set; }
        public string CUI { get; set; }
        public string Nombre1 { get; set; }
        public string Nombre2 { get; set; }
        public string Nombre3 { get; set; }
        public string Apellido1 { get; set; }
        public string Apellido2 { get; set; }
        public string DeCasada { get; set; }
        public string Sexo { get; set; }
        public string EstadoCivil { get; set; }
        public string Ocupacion { get; set; }
        public string FechaNac { get; set; }
        public string PaisNac { get; set; }
        public string DepartamentoNac { get; set; }
        public string MunicipioNac { get; set; }
        public string DireccionResidencia1 { get; set; }
        public string DireccionResidencia2 { get; set; }
        public string DireccionResidencia3 { get; set; }
        public string MunicipioResidencia { get; set; }
        public string DeptoResidencia { get; set; }
        public string EstadoResidencia { get; set; }
        public string CiudadResidencia { get; set; }
        public string ZipResidencia { get; set; }
        public string PaisResidencia { get; set; }
        public string TelefonoCasa { get; set; }
        public string TelefonoTrabajo { get; set; }
        public string TelefonoCelular { get; set; }
        public string Email { get; set; }
        public string SedeEntrega { get; set; }
        public string DireccionEnvio1 { get; set; }
        public string DireccionEnvio2 { get; set; }
        public string EstadoEnvio { get; set; }
        public string CiudadEnvio { get; set; }
        public string ZipEnvio { get; set; }
        public string TelefonoEnvio { get; set; }
        public string Nacionalidad { get; set; }
        public string TipoId { get; set; }
        public string NumeroId { get; set; }
        public string NumeroSerie { get; set; }
        public string MunicipioEmision { get; set; }
        public string DeptoEmision { get; set; }
        public string Ojos { get; set; }
        public string Tez { get; set; }
        public string Pelo { get; set; }
        public string Estatura { get; set; }
        public string NombresPadre { get; set; }
        public string ApellidosPadre { get; set; }
        public string TipoIdPadre { get; set; }
        public string NumeroIdPadre { get; set; }
        public string NombresMadre { get; set; }
        public string ApellidosMadre { get; set; }
        public string TipoIdMadre { get; set; }
        public string NumeroIdMadre { get; set; }
        public string LibroMenor { get; set; }
        public string FolioMenor { get; set; }
        public string PartidaMenor { get; set; }
        public string CUIMenor { get; set; }
        public string Foto { get; set; }
        public string FotoForzada { get; set; }
        public string FotoObs { get; set; }
        public string HuellaPos1 { get; set; }
        public string HuellaObs1 { get; set; }
        public string HuellaPNG1 { get; set; }
        public string HuellaWSQ1 { get; set; }
        public string HuellaPos2 { get; set; }
        public string HuellaObs2 { get; set; }
        public string HuellaPNG2 { get; set; }
        public string HuellaWSQ2 { get; set; }
        public string Firma { get; set; }
        public string FirmaForzada { get; set; }
        public string DPIIntentado { get; set; }
        public string DPI { get; set; }
        public string DPISinHuellas { get; set; }
        public string MOC { get; set; }
        public string FaceIntentado { get; set; }
        public string Face { get; set; }
        public string DPIPadreIntentado { get; set; }
        public string DPIPadre { get; set; }
        public string DPIPadreSinHuellas { get; set; }
        public string MOCPadre { get; set; }
        public string FacePadreIntentado { get; set; }
        public string FacePadre { get; set; }
        public string DPIMadreIntentado { get; set; }
        public string DPIMadre { get; set; }
        public string DPIMadreSinHuellas { get; set; }
        public string MOCMadre { get; set; }
        public string FaceMadreIntentado { get; set; }
        public string FaceMadre { get; set; }
        public String[] Probatorios { get; set; }
        public string Version { get; set; }        
        public String[] Privilegios_Usuario { get; set; } 
        public string Jefe_Encargado { get; set; }
        public string MOCF { get; set; }
        public string HIT { get; set; }        
        public string RightThumbPngB64 { get; set; }
        public string RightThumbWsqB64 { get; set; }
        public string RightIndexPngB64 { get; set; }
        public string RightIndexWsqB64 { get; set; }
        public string RightMiddlePngB64 { get; set; }
        public string RightMiddleWsqB64 { get; set; }
        public string RightRingPngB64 { get; set; }
        public string RightRingWsqB64 { get; set; }
        public string RightLittlePngB64 { get; set; }
        public string RightLittleWsqB64 { get; set; }
        public string LeftThumbPngB64 { get; set; }
        public string LeftThumbWsqB64 { get; set; }
        public string LeftIndexPngB64 { get; set; }
        public string LeftIndexWsqB64 { get; set; }
        public string LeftMiddlePngB64 { get; set; }
        public string LeftMiddleWsqB64 { get; set; }
        public string LeftRingPngB64 { get; set; }
        public string LeftRingWsqB64 { get; set; }
        public string LeftLittlePngB64 { get; set; }
        public string LeftLittleWsqB64 { get; set; }
        
        //CONFIRMACION BIOMETRICA SOLICITANTE
        public string CBS { get; set; }
        public string CBS_POSICION_DEDO { get; set; }
        public string ConsultaCita { get; set; }
        public string HashCita { get; set; }
        public string idCita { get; set; }

    }
}
