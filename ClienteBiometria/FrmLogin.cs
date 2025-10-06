using AForge.Video;
using AForge.Video.DirectShow;
using CapaEN;
using ENROLLMENT_V3.Properties;
using ENROLLMENT_V3.WsBiometricsDGM;
using Neurotec.Biometrics;
using Neurotec.Biometrics.Client;
using Neurotec.Biometrics.Gui;
using Neurotec.Devices;
using Neurotec.Images;
//COMPONENTES DEL SDK DE TRABAJO
using Neurotec.Licensing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ENROLLMENT_V3
{
    public partial class FrmLogin : Form
    {
        FUNCIONES funciones = new FUNCIONES();
        LoginData loginData;
        EquipoData equipoData;
        SedeData sedeDataEquipo;

        WsBiometricsDGMSoapClient wsBiometricsDGM;        

        #region Declaración de variables para utilización del SDK
        //Cliente biométrico
        private NBiometricClient biometricClientFinger;
        private NBiometricClient biometricClientFace;
        private FilterInfoCollection filterInfoCollection;
        #endregion       

        public FrmLogin()
        {
            InitializeComponent();

            txtUsuario.Text = string.Empty;
            txtContrasenia.Text = string.Empty;
            cmbEscaners.Enabled = false;
            
            lversion.Text = FUNCIONES.GetVersion();
            l_api.Text = funciones.devApiMiros();
            InitializeResetPasswordLink();
        }

        private void InitializeResetPasswordLink()
        {
            try
            {
                // Additional styling for the reset password link if needed
                lnkResetPassword.VisitedLinkColor = System.Drawing.Color.White;
            }
            catch (Exception ex)
            {
                funciones.CajaMensaje("Error al inicializar enlace de restablecimiento: " + ex.Message);
            }
        }
        
        private Task<DataSet> SecuenciaArranque()
        {

            return Task.Run(() =>
            {
                DataSet ds = funciones.GetDsResultado();
                try
                {
                    string msgError = string.Empty;
                    Invoke(new MethodInvoker(() => { txtConsola.Text = string.Empty; }));


                    SetMarcador(PARAMETRIZACION.EstatusIcono.Error);
                    Consola(DateTime.Now.ToString("G", new CultureInfo("es-GT")), 1);
                    Consola("Cargando...", 1);
                    Consola("*** Ejecutando secuencia de arranque ***", 1);
                    SetMarcador(PARAMETRIZACION.EstatusIcono.Carga);

                    //1.Cargar ícono.
                    Consola("1. Cargar ícono-> ", 0);
                    ds = funciones.CargarLogo(pbxLogo);
                    bool resultado = bool.Parse(ds.Tables[0].Rows[0]["RESULTADO"].ToString());
                    if (!resultado) msgError += ds.Tables[0].Rows[0]["MSG_ERROR"].ToString();
                    Consola(resultado ? "Ok." : ds.Tables[0].Rows[0]["MSG_ERROR"].ToString(), 1);

                    //2.Obtener SEDE del equipo.
                    Consola("2. Obtener SEDE-> ", 0);
                    ds = funciones.GetSede();
                    resultado = bool.Parse(ds.Tables[0].Rows[0]["RESULTADO"].ToString());
                    if (!resultado) msgError += ds.Tables[0].Rows[0]["MSG_ERROR"].ToString();
                    else Settings.Default.SEDE = ds.Tables[0].Rows[0]["DATOS"].ToString();
                    Consola(resultado ? "Ok." : ds.Tables[0].Rows[0]["MSG_ERROR"].ToString(), 1);

                    //3.Obtener BIOS del equipo.
                    Consola("3. Obtener BIOS-> ", 0);
                    ds = funciones.GetBios();
                    resultado = bool.Parse(ds.Tables[0].Rows[0]["RESULTADO"].ToString());
                    if (!resultado) msgError += ds.Tables[0].Rows[0]["MSG_ERROR"].ToString();
                    else Settings.Default.BIOS = ds.Tables[0].Rows[0]["DATOS"].ToString();
                    Consola(resultado ? "Ok(" + ds.Tables[0].Rows[0]["DATOS"].ToString() + ")." : ds.Tables[0].Rows[0]["MSG_ERROR"].ToString(), 1);

                    //4.Obtener USB drives.
                    if (Settings.Default.USB_DEVICE)
                    {
                        ds = funciones.GetUSBDriveLetters();
                        resultado = bool.Parse(ds.Tables[0].Rows[0]["RESULTADO"].ToString());
                        if (!resultado)
                        {
                            msgError += ds.Tables[0].Rows[0]["MSG_ERROR"].ToString();
                            Consola("4.Obtener USB drives-> " + (ds.Tables[0].Rows[0]["MSG_ERROR"].ToString()), 1);
                        }
                        else
                        {
                            List<string> drives = (List<string>)ds.Tables[0].Rows[0]["DATOS"];
                            string lista = "";
                            foreach (string drive in drives)
                                lista += drive.Split('-')[1].Substring(0, 1) + "/" + drive.Split('-')[0] + ", ";
                            Consola("4.Obtener USB drives-> " + (resultado ? "Ok(" + lista.Substring(0, lista.Length - 2) + ")." : ds.Tables[0].Rows[0]["MSG_ERROR"].ToString()), 1);

                            //4.1. Existe SEDE / EQUIPO / MEMORIA.
                            ds = funciones.ValidarSedeEquipoMemoria(drives);
                            resultado = bool.Parse(ds.Tables[0].Rows[0]["RESULTADO"].ToString());
                            if (!resultado) msgError += ds.Tables[0].Rows[0]["MSG_ERROR"].ToString();
                            Consola("4.1. Validar SE/EQ/ME-> " + (resultado ? "Ok." : ds.Tables[0].Rows[0]["MSG_ERROR"].ToString()), 1);
                        }
                    }
                    else
                    {
                        Settings.Default.DRIVE_LETTER = Path.Combine(Application.StartupPath, "ENROL");
                        Consola("4.Obtener USB drives-> " + (resultado ? "Ok (" + Settings.Default.DRIVE_LETTER.Substring(0, 1) + ")." : ds.Tables[0].Rows[0]["MSG_ERROR"].ToString()), 1);
                    }

                    //5.Validar licencias de biometría.
                    Consola("5. Licencias biometría...", 1);
                    foreach (string componente in Settings.Default.COMPONENTES_NEURO.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        Consola(componente + "-> ", 0);
                        ds = funciones.ValidarComponenteNeuro(componente);
                        resultado = bool.Parse(ds.Tables[0].Rows[0]["RESULTADO"].ToString());
                        if (!resultado)
                        {
                            msgError += componente + "-> " + ds.Tables[0].Rows[0]["MSG_ERROR"].ToString();
                            Consola(ds.Tables[0].Rows[0]["MSG_ERROR"].ToString(), 1);
                        }
                        else
                        {
                            bool componenteActivo = bool.Parse(ds.Tables[0].Rows[0]["DATOS"].ToString());
                            Consola("Activo: " + componenteActivo.ToString(), 1);
                            if (!componenteActivo) msgError += componente + " no se encuentra activo. ";
                        }
                    }

                    //6.Iniciar cliente de biometría DACTILAR.
                    Consola("6. Iniciar cliente biometría DACTILAR-> ", 0);
                    ds = funciones.IniciarClienteBiometriaHuellas(biometricClientFinger);
                    resultado = bool.Parse(ds.Tables[0].Rows[0]["RESULTADO"].ToString());
                    if (!resultado) msgError += ds.Tables[0].Rows[0]["MSG_ERROR"].ToString();
                    else biometricClientFinger = (NBiometricClient)ds.Tables[0].Rows[0]["DATOS"];
                    Consola(resultado ? "Ok." : ds.Tables[0].Rows[0]["MSG_ERROR"].ToString(), 1);

                    //6.1 Iniciar cliente de biometría FACIAL.
                    Consola("6.1. Iniciar cliente biometría FACIAL-> ", 0);
                    ds = funciones.IniciarClienteBiometriaRostro(biometricClientFace);
                    resultado = bool.Parse(ds.Tables[0].Rows[0]["RESULTADO"].ToString());
                    if (!resultado) msgError += ds.Tables[0].Rows[0]["MSG_ERROR"].ToString();
                    else biometricClientFace = (NBiometricClient)ds.Tables[0].Rows[0]["DATOS"];
                    Consola(resultado ? "Ok." : ds.Tables[0].Rows[0]["MSG_ERROR"].ToString(), 1);

                    Consola("7. Listar escáner de huellas-> ", 0);
                    Invoke(new MethodInvoker(() =>
                    {
                        ds = funciones.ListarEscanersHuellas(cmbEscaners, true, Settings.Default.FILTRAR_ESCANER_HUELLAS, PARAMETRIZACION.TipoEscanerHuellas.Unidactilar, biometricClientFinger.DeviceManager);
                    }));
                    resultado = bool.Parse(ds.Tables[0].Rows[0]["RESULTADO"].ToString());
                    if (!resultado) msgError += ds.Tables[0].Rows[0]["MSG_ERROR"].ToString();
                    Consola(resultado ? "Ok." : ds.Tables[0].Rows[0]["MSG_ERROR"].ToString(), 1);

                    Consola("7. Listar camaras Disponibles -> ", 1);
                    filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                    if (filterInfoCollection.Count <= 0)
                    {
                        Consola("7.1 No se encontraron dispositivos de video disponibles!!!", 1);
                    }
                    else
                    {
                        string listadocamaras = "7.1 Camaras disponibles: ";
                        bool warningcamaras = false;
                        for (int i = 0; i < filterInfoCollection.Count; i++)
                        {

                            listadocamaras = listadocamaras + ";" + filterInfoCollection[i].Name;
                            if (filterInfoCollection[i].Name.Equals(Settings.Default.FILTRO_CAMARA))
                            {
                                warningcamaras = true;
                            }


                        }
                        if (warningcamaras)
                        {
                            Consola(listadocamaras, 1);
                        } else
                        {
                            Consola("!!!Warning!!!!" + listadocamaras, 1);
                            throw new Exception("El parametro FILTRO_CAMARA, no coincide con las camaras disponibles");
                        }
                        
                    }



                    ds.Tables[0].Rows[0]["RESULTADO"] = true;
                    ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;



                    SetMarcador(PARAMETRIZACION.EstatusIcono.Exito);
                    Consola("*** Secuencia de arranque exitosa ***", 0);
                }
                catch (Exception ex)
                {
                    ds.Tables[0].Rows[0]["RESULTADO"] = false;
                    ds.Tables[0].Rows[0]["MSG_ERROR"] =  ex.Message;
                    SetMarcador(PARAMETRIZACION.EstatusIcono.Error);
                    Consola("*** Secuencia de arranque con errores ***", 0);                
                }
                Consola("", 1);
                Consola(DateTime.Now.ToString("G", new CultureInfo("es-GT")), 0);
                return ds;
            }
           );            
        }        

        private async void btnVerificar_Click(object sender, EventArgs e)
        {
            btnVerificar.Enabled = btnLimpiar.Enabled = btnSalir.Enabled = false;
            try
            {
                string msgError = string.Empty;

                if (txtUsuario.Text.Trim().Equals(string.Empty) || txtUsuario.Text.Trim().Equals(""))
                    msgError += "Ingrese un usuario";

                if (txtContrasenia.Text.Trim().Equals(string.Empty) || txtContrasenia.Text.Trim().Equals(""))
                    if(msgError.Trim().Equals(string.Empty))
                    {
                        msgError += "Ingrese constraseña";
                    } else
                    {
                        msgError += " / Ingrese contraseña";
                    }


                if (biometricClientFinger.CurrentBiometric != null)
                    if (msgError.Trim().Equals(string.Empty))
                    {
                        msgError += "Registre su huella";
                    } else
                    {
                        msgError += " / Registre su huella";
                    }


                if (nFVDedoA.Finger == null)
                        msgError += "No ha ingresado la huella ";
                if (nFVDedoA.Finger.Image == null)
                {
                    msgError += "No ha ingresado la huella ";
                }
                
                if (msgError.Equals(string.Empty) == false)
                   throw new Exception(msgError);

                DataSet dsUsuario = ConsultaInformacionxUsuario(txtUsuario.Text, txtContrasenia.Text);
                
                if (bool.Parse(dsUsuario.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                    throw new Exception(dsUsuario.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                LoginResponse loginResponse = (LoginResponse)(dsUsuario.Tables[0].Rows[0]["DATOS"]);

                if (loginResponse.data.Length < 1)
                    throw new Exception("La colección de información de usuarios es 0.");

                loginData = loginResponse.data[0];
                
                Settings.Default.DRIVE_LETTER = Path.Combine(Application.StartupPath, "ENROL");
                Settings.Default.SUFIJO_ENCRIPTACION = "M";

                if(loginData == null)
                    throw new Exception("¡Información de usuario es null!");

                if (loginData.STATUS.ToString().Equals("0"))
                    throw new Exception("¡El usuario se encuentra INACTIVO!");


                if(Settings.Default.VALIDAR_SEQUIPO_SUSUARIO)
                    if (loginData.SEDE_ID != Settings.Default.SEDE_ID)
                        throw new Exception("¡La sede del equipo y la sede del usuario no coinciden (" + Settings.Default.SEDE_ID + "/" + loginData.SEDE_ID + ")!");

                //if (listDataWsUsuariosDGM[0].cambioclave == null)
                //    listDataWsUsuariosDGM[0].cambioclave = "0";

                if (Settings.Default.USB_DEVICE)
                {
                    if (!Settings.Default.ConsultarAsignacionSeEqMe)
                    {
                        //if (listDataWsUsuariosDGM[0].tbl_ms_serial_number == null)
                        //    throw new Exception("¡Sin dispositivo de cifrado asignado!. ");

                        //funciones.AsignarDriveLetter(listDataWsUsuariosDGM[0].tbl_ms_serial_number);
                        //funciones.AsignarDriveLetter(listDataWsUsuariosDGM[0].tbl_ms_serial_number);
                        //Settings.Default.SUFIJO_ENCRIPTACION = "U";
                    }
                    else Settings.Default.SUFIJO_ENCRIPTACION = "M";

                }
                else
                {
                    Settings.Default.DRIVE_LETTER = Path.Combine(Application.StartupPath, "ENROL");
                    Settings.Default.SUFIJO_ENCRIPTACION = "M";
                }

                DataSet dsDirectorioLlaves = funciones.ExisteDirectorioLlaves(loginData.USUARIO, Environment.MachineName);

                if (bool.Parse(dsDirectorioLlaves.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                    throw new Exception(dsDirectorioLlaves.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                //MessageBox.Show("Eliminar TEST, Eliminar Envía FTP, Verificar que todos los combos puedan seleccionarse con las flechas del cursor, Verificar que cuando el país de nacimiento sea distinto a Guatemala, valide el estado de nacimiento verficandolo en la impresión, Eliminar el país de nacimiento 'Guatemala', cuando sea pasaporte Diplomatico Artículo 98, Cuando es CASADO pide apellido de casada, Al seleccinar CASADA focus en txtApellidoCasada");
                //bool reiniciarContraseña = false;
                //listDataWsUsuariosDGM[0].activo = "0";
                //SI NO HAY BIOMETRÍA O ESTA MARCADA LA OPCION PARA CAMBIO DE CONTRASEÑA
                if (loginData.REINICIAR_BIOMETRIA == 1)
                {
                    //throw new Exception("Usuario sin biometría en el sistema, búsque un módulo de enrolamiento de pasaportes y enrole su biometría.");
                    Usuarios frmUsuario = new Usuarios(loginData);
                    this.Hide();

                    frmUsuario.ShowDialog();
                    this.Close();
                }//USUARIO NO ESTÁ BLOQUEADO Y LA BIOMETRÍA ESTÁ ACTIVA
                else// if (listDataWsUsuariosDGM[0].bloqueado.ToString().Equals("0") && listDataWsUsuariosDGM[0].activo.Equals("1") && (listDataWsUsuariosDGM[0].cambioclave.Equals("0")))
                {
                    try
                    {
                        funciones.CajaMensaje($"DEBUG Usuario:\nSEDE_ID del usuario: {loginData.SEDE_ID}\nUsuario: {loginData.USUARIO}");
                        DataSet dsSedeUsuario = GetSedeById(loginData.SEDE_ID);
                        if (bool.Parse(dsSedeUsuario.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                            throw new Exception(dsSedeUsuario.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                        DataSet dsBios = funciones.GetBios();
                        if (bool.Parse(dsBios.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                            throw new Exception(dsBios.Tables[0].Rows[0]["MSG_ERROR"].ToString());
                        
                        string bios = dsBios.Tables[0].Rows[0]["DATOS"].ToString();

                        loginData.biosestacion = bios;

                        DataSet dsEquipo = GetEquipoByBios(bios);
                        if (bool.Parse(dsEquipo.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                            throw new Exception(dsEquipo.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                        SedeData sedeDataUsuario = (SedeData)dsSedeUsuario.Tables[0].Rows[0]["DATOS"];
                        equipoData = (EquipoData)dsEquipo.Tables[0].Rows[0]["DATOS"];

                        funciones.CajaMensaje($"DEBUG Equipo:\nSEDE_ID del equipo: {equipoData.sede_id}\nEquipo: {equipoData.nombre}");
                        DataSet dsSedeEquipo = GetSedeById(equipoData.sede_id);
                        if (bool.Parse(dsSedeEquipo.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                            throw new Exception(dsSedeEquipo.Tables[0].Rows[0]["MSG_ERROR"].ToString());
                        
                        sedeDataEquipo = (SedeData)dsSedeEquipo.Tables[0].Rows[0]["DATOS"];

                        string sedeUsuario = sedeDataUsuario.iddelegacion + "-" + sedeDataUsuario.nombredelegacion;
                        string sedeEquipo = sedeDataEquipo.iddelegacion + "-" + sedeDataEquipo.nombredelegacion;
                        
                        if (sedeDataUsuario.iddelegacion != sedeDataEquipo.iddelegacion)
                            throw new Exception("Sede usuario y sede equipo no coinciden (" + sedeUsuario + " / " + sedeEquipo + ")");
                        //this.NuevaInstanciaEnrollment();
                      
                        string huellaLocal = Convert.ToBase64String(nFVDedoA.Finger.Image.Save(NImageFormat.Wsq).ToArray()); 
                        
                        if (Properties.Settings.Default.MATCHING_MODE.Equals("SERVER"))
                        {                            
                            DataSet ds = funciones.MostrarHuellaDesdeBytes(Convert.FromBase64String(loginData.HUELLA1PNG), nFVDedoB);
                            if (bool.Parse(ds.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                                throw new Exception(ds.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                            DataSet dsBiometria = wsBiometricsDGM.CompararDosHuellasStrBase64IMG_WSQ(huellaLocal, loginData.HUELLA1PNG);

                            DataSet dsValidarDsBio = funciones.EsDsBiometriaValido(dsBiometria);
                            if (bool.Parse(dsValidarDsBio.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                                throw new Exception(dsValidarDsBio.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                            DataSet DsCoincidenciaAB = (DataSet)(dsBiometria.Tables[0].Rows[0]["METODO_RESULTADO"]);

                            dsValidarDsBio = funciones.EsDsCoincidenciaABValido(DsCoincidenciaAB);
                            if (bool.Parse(dsValidarDsBio.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                                throw new Exception(dsValidarDsBio.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                            if (bool.Parse(DsCoincidenciaAB.Tables[0].Rows[0]["COINCIDENCIA"].ToString()) == false)
                            {                                
                                ds = funciones.MostrarHuellaDesdeBytes(Convert.FromBase64String(loginData.HUELLA2PNG), nFVDedoB);
                                if (bool.Parse(ds.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                                    throw new Exception(ds.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                                dsBiometria = wsBiometricsDGM.CompararDosHuellasStrBase64IMG_WSQ(huellaLocal, loginData.HUELLA2PNG);

                                dsValidarDsBio = funciones.EsDsBiometriaValido(dsBiometria);
                                if (bool.Parse(dsValidarDsBio.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                                    throw new Exception(dsValidarDsBio.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                                DsCoincidenciaAB = (DataSet)(dsBiometria.Tables[0].Rows[0]["METODO_RESULTADO"]);

                                dsValidarDsBio = funciones.EsDsCoincidenciaABValido(DsCoincidenciaAB);
                                if (bool.Parse(dsValidarDsBio.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                                    throw new Exception(dsValidarDsBio.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                                if (bool.Parse(DsCoincidenciaAB.Tables[0].Rows[0]["COINCIDENCIA"].ToString()) == false)
                                {
                                    CmbEscaners_SelectedIndexChanged(sender, e);
                                    throw new Exception("¡Las huellas no coinciden!. Puntuacion: " + DsCoincidenciaAB.Tables[0].Rows[0]["PUNTUACION"].ToString());
                                }

                                NuevaInstanciaEnrollment();
                            }
                            else
                                NuevaInstanciaEnrollment();
                        }
                        else if(Properties.Settings.Default.MATCHING_MODE.Equals("LOCAL"))
                        {
                            byte[] bytes = Convert.FromBase64String(loginData.HUELLA1PNG);
                            DataSet ds = funciones.MostrarHuellaDesdeBytes(bytes, nFVDedoB);
                            if (bool.Parse(ds.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                                throw new Exception(ds.Tables[0].Rows[0]["MSG_ERROR"].ToString());
                            
                            DataSet dsHuellasIguales = await funciones.HuellasIguales((NFinger)nFVDedoA.Finger, (NFinger)nFVDedoB.Finger, biometricClientFinger);
                            //await HuellasIguales(dsHuellasIguales, _subjectFingerDerecho, _subjectFingerIzquierdo);

                            string mensajeError = "";
                            if (dsHuellasIguales == null)
                                mensajeError += "Error al comparar 2 huellas. ";

                            if (dsHuellasIguales.Tables.Count < 1)
                                mensajeError += "Error al comparar 2 huellas (2). ";

                            if (dsHuellasIguales.Tables[0].Rows.Count < 1)
                                mensajeError += "Error al comparar 2 huellas (3). ";
                            
                            if (bool.Parse(dsHuellasIguales.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                            {
                                bytes = Convert.FromBase64String(loginData.HUELLA2PNG);
                                ds = funciones.MostrarHuellaDesdeBytes(bytes, nFVDedoB);
                                if (bool.Parse(ds.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                                    throw new Exception(ds.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                                dsHuellasIguales = await funciones.HuellasIguales((NFinger)nFVDedoA.Finger, (NFinger)nFVDedoB.Finger, biometricClientFinger);
                            }

                            mensajeError = "";
                            if (dsHuellasIguales == null)
                                mensajeError += "Error al comparar 2 huellas. ";
                                
                            if (dsHuellasIguales.Tables.Count < 1)
                                mensajeError += "Error al comparar 2 huellas (2). ";

                            if (dsHuellasIguales.Tables[0].Rows.Count < 1)
                                mensajeError += "Error al comparar 2 huellas (3). ";

                            if (bool.Parse(dsHuellasIguales.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                                mensajeError += dsHuellasIguales.Tables[0].Rows[0]["MSG_ERROR"].ToString();

                            if (!mensajeError.Equals(string.Empty))
                            {
                                CmbEscaners_SelectedIndexChanged(sender, e);
                                throw new Exception(mensajeError);
                            }

                            NuevaInstanciaEnrollment();                            
                        }
                    }
                    catch (Exception ex)
                    {
                        nFVDedoB.Finger = null;
                        throw new Exception("Error al validar la identidad del usuario: " + ex.Message);                        
                    }
                }               
            }
            catch (Exception ex)
            {
                //MessageBox.Show("btnVerificar_Click(). " + ex.Message);
                //MessageBox.Show(ex.Message, "Validación de credenciales", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                funciones.CajaMensaje(ex.Message);
               //MessageBox.Show(ex.Message);/
            
            }
            btnVerificar.Enabled = btnLimpiar.Enabled = btnSalir.Enabled = true;
            btnVerificar.Focus();
        }

        public void NuevaInstanciaEnrollment()
        {
            try
            {

                FrmEnrolamiento er = new FrmEnrolamiento(loginData, equipoData, sedeDataEquipo, this.biometricClientFinger, this.biometricClientFace);
                this.Hide();
                er.ShowDialog();
                this.Close();
            }
            catch(Exception ex)
            {
                //MessageBox.Show(ex.Message);
                funciones.CajaMensaje(ex.Message);
                //throw new Exception();
            }
        }

        public DataSet GetSedeById(int id)
        {
            DataSet dsResultado = ArmarDsResultado();
            try
            {
                SedeRequest sedeRequest = new SedeRequest();
                sedeRequest.id = id;

                // DEBUG: Mostrar datos del request
                string url = Settings.Default.API_REST_MIROS + Settings.Default.API_SEDE_BY_ID + "?id=" + sedeRequest.id;
                funciones.CajaMensaje($"DEBUG GetSedeById:\nSEDE_ID: {id}\nURL: {url}\nJWT: {loginData?.JWT_TOKEN?.Substring(0, 20)}...");

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Headers.Add("Authorization", $"Bearer {loginData.JWT_TOKEN}");

                using (WebResponse response = request.GetResponse())
                {
                    using (Stream strReader = response.GetResponseStream())
                    {
                        if (strReader == null)
                            throw new Exception("Respuesta nula desde el servidor. ");

                        using (StreamReader objReader = new StreamReader(strReader))
                        {
                            string body = objReader.ReadToEnd();
                            SedeResponse sedeResponse = JsonConvert.DeserializeObject<SedeResponse>(body);

                            if (sedeResponse.codigo != 200)
                                throw new Exception("Error al guardar la entrega. Código: " + sedeResponse.codigo + ", Mensaje: " + sedeResponse.mensaje);

                            if (sedeResponse.data.Length == 0)
                                throw new Exception("La colección está vacía. ");

                            if (sedeResponse.data[0] == null)
                                throw new Exception("Error de comunicación del API, intente guardar nuevamente y repórtelo al administrador");

                            if (!sedeResponse.data[0].estado.Equals("A"))
                                throw new Exception("Registro inactivo, esperado (A), obtenido (" + sedeResponse.data[0].estado +") ");

                            dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
                            dsResultado.Tables[0].Rows[0]["DATOS"] = sedeResponse.data[0];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "GetSedeById(). " + ex.Message;
            }

            return dsResultado;
        }

        public DataSet GetEquipoByBios(string bios)
        {
            DataSet dsResultado = ArmarDsResultado();
            try
            {
                EquipoRequest equipoRequest = new EquipoRequest();
                equipoRequest.bios = bios;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@"" + Settings.Default.API_REST_MIROS + Settings.Default.API_EQUIPO_BY_BIOS + "?bios=" + equipoRequest.bios);
                request.Headers.Add("Authorization", $"Bearer {loginData.JWT_TOKEN}");

                using (WebResponse response = request.GetResponse())
                {
                    using (Stream strReader = response.GetResponseStream())
                    {
                        if (strReader == null)
                            throw new Exception("Respuesta nula desde el servidor. ");

                        using (StreamReader objReader = new StreamReader(strReader))
                        {
                            string body = objReader.ReadToEnd();
                            EquipoResponse equipoResponse = JsonConvert.DeserializeObject<EquipoResponse>(body);

                            if (equipoResponse.codigo != 200)
                                throw new Exception("Error al guardar la entrega. Código: " + equipoResponse.codigo + ", Mensaje: " + equipoResponse.mensaje);

                            if (equipoResponse.data.Length == 0)
                                throw new Exception("La colección está vacía. ");

                            if (equipoResponse.data[0] == null)
                                throw new Exception("Error de comunicación del API, intente guardar nuevamente y repórtelo al administrador");

                            if (!equipoResponse.data[0].estado.Equals("A"))
                                throw new Exception("Registro inactivo, esperado (A), obtenido (" + equipoResponse.data[0].estado + ") ");

                            dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
                            dsResultado.Tables[0].Rows[0]["DATOS"] = equipoResponse.data[0];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "GetEquipoByBios(). " + ex.Message;
            }

            return dsResultado;
        }

        private DataSet ArmarDsResultado()
        {
            DataSet ds = new DataSet();
            ds.Tables.Add(new DataTable());
            ds.Tables[0].Columns.Add("RESULTADO", typeof(bool));
            ds.Tables[0].Columns.Add("MSG_ERROR", typeof(string));

            ds.Tables[0].Columns.Add("DATOS", typeof(object));
            ds.Tables[0].Columns.Add("DATOS_PAGO_PASAPORTE", typeof(object));
            ds.Tables[0].Columns.Add("DATOS_SIBIO", typeof(object));

            DataRow dr = ds.Tables[0].NewRow();
            ds.Tables[0].Rows.Add(dr);

            return ds;
        }

        public DataSet ConsultaInformacionxUsuario(string usuario, string contrasenia)
        {
            DataSet dsResultado = funciones.GetDsResultado();
            try
            {
                LoginRequest loginRequest = new LoginRequest();
                loginRequest.username = usuario;
                loginRequest.password = contrasenia;

                string postString = JsonConvert.SerializeObject(loginRequest);
                byte[] data = UTF8Encoding.UTF8.GetBytes(postString);

                HttpWebRequest request;
                string url = Settings.Default.API_REST_MIROS + @Settings.Default.API_LOGIN_USUARIO;
                request = WebRequest.Create(url) as HttpWebRequest;
                request.Timeout = 10 * 1000;
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Accept = "application/json";

                request.ContentLength = data.Length;

                Stream postStream = request.GetRequestStream();
                postStream.Write(data, 0, data.Length);

                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string body = reader.ReadToEnd();

                LoginResponse loginResponse;
                loginResponse = JsonConvert.DeserializeObject<LoginResponse>(body);

                if (loginResponse.codigo != 200)
                    throw new Exception("Ocurrió un error código " + loginResponse + ": " + loginResponse.mensaje);

                dsResultado.Tables[0].Rows[0]["DATOS"] = loginResponse;
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;

            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] =  ex.Message;
            }

            return dsResultado;
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            try{this.Close();}
            catch (Exception ex){ funciones.CajaMensaje(ex.Message); }
        }

        private void Ingreso_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (biometricClientFinger != null) biometricClientFinger.Cancel();
        }

        private void txtUsuario_Enter(object sender, EventArgs e)
        {
            txtUsuario.SelectAll();
        }

        private void txtContrasenia_Enter(object sender, EventArgs e)
        {
            txtContrasenia.SelectAll();
        }

        private void Consola(string texto, int nuevasLineas)
        {
            Invoke(new MethodInvoker(() =>
            {
                funciones.Consola(txtConsola, texto, nuevasLineas);
            }));            
        }

        private void SetMarcador(PARAMETRIZACION.EstatusIcono estatus)
        {
            SetMarcador(pbxLogin, estatus);
        }
        private void SetMarcador(PictureBox pictureBox, PARAMETRIZACION.EstatusIcono estatus)
        {
            switch(estatus)
            {
                case PARAMETRIZACION.EstatusIcono.Error:
                    funciones.SetMarcador(pictureBox, PARAMETRIZACION.EstatusIcono.Error, pbxWarning);
                    break;

                case PARAMETRIZACION.EstatusIcono.Carga:
                    funciones.SetMarcador(pictureBox, PARAMETRIZACION.EstatusIcono.Carga, pbxLoad);
                    break;

                case PARAMETRIZACION.EstatusIcono.Exito:
                    funciones.SetMarcador(pictureBox, PARAMETRIZACION.EstatusIcono.Exito, pbxCheck);
                    break;
            }
        }

        private async void CmbEscaners_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                funciones.MostrarHuellaDesdeBytes(null, nFVDedoA);
                funciones.MostrarHuellaDesdeBytes(null, nFVDedoB);

                biometricClientFinger.FingerScanner = cmbEscaners.SelectedItem as NFScanner;

                DataSet dsEscanerHuella = await funciones.EscanearHuella(NFPosition.RightIndex, nFVDedoA, biometricClientFinger);
                if (bool.Parse(dsEscanerHuella.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                    throw new Exception(dsEscanerHuella.Tables[0].Rows[0]["MSG_ERROR"].ToString());                
            }
            catch(Exception ex)
            {
                // MessageBox.Show("CmbEscaners_SelectedIndexChanged(). " + ex.Message);
                funciones.CajaMensaje(ex.Message);
            }
        }

        private void FrmLogin_Shown(object sender, EventArgs e)
        {
            pbxLogin_Click(sender, e);

            try
            {
                Process[] proc = Process.GetProcessesByName("ENROLLMENT_V3");

                if (proc.Length > 1)
                    for (int i = 0; i < proc.Length; i++)
                        proc[i].Kill();

                proc = Process.GetProcessesByName("READERDEMO");
                if (proc.Length > 0)
                    for (int i = 0; i < proc.Length; i++)
                        proc[i].Kill();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bienvenida_Load(). " + ex.Message);
            }
        }

        private void txtConsola_TextChanged(object sender, EventArgs e)
        {
            try
            {
                txtConsola.SelectionStart = txtConsola.Text.Length;
                txtConsola.ScrollToCaret();
            }
            catch (Exception ex){ MessageBox.Show("txtConsola_TextChanged(). " + ex.Message); }
        }

        private async void pbxLogin_Click(object sender, EventArgs e)
        {
            try
            {
                btnVerificar.Enabled = false;
                if (pbxLogin.Image == pbxLoad.Image) return;
                DataSet ds = await SecuenciaArranque();
                if (!bool.Parse(ds.Tables[0].Rows[0]["RESULTADO"].ToString())) throw new Exception(ds.Tables[0].Rows[0]["MSG_ERROR"].ToString());
                btnVerificar.Enabled = true;
                
            }
            catch (Exception ex){funciones.CajaMensaje(ex.Message);}
        }

        private void pbxLogo_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult dr = MessageBox.Show("¿Desea copiar el contenido de la consola al portapapeles?", "Enrollment", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.No) return;

                Clipboard.Clear();    //Clear if any old value is there in Clipboard        
                Clipboard.SetText(txtConsola.Text); //Copy text to Clipboard
                //string strClip = Clipboard.GetText(); //Get text from Clipboard
                MessageBox.Show("¡Copiado al portapapeles!");

            }
            catch (Exception ex){ MessageBox.Show("pbxLogo_Click(). "+ex.Message); }
        }

        private void lblEscanerHuellas_Click(object sender, EventArgs e)
        {
            try
            {
                DataSet ds=new DataSet();
                DialogResult dr = MessageBox.Show("¿Desea cargar el listado de escáner de huellas nuevamente?", "Enrollment", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.No) return;

                ds = funciones.ListarEscanersHuellas(cmbEscaners, true, Settings.Default.FILTRAR_ESCANER_HUELLAS, PARAMETRIZACION.TipoEscanerHuellas.Unidactilar, biometricClientFinger.DeviceManager);
                bool resultado = bool.Parse(ds.Tables[0].Rows[0]["RESULTADO"].ToString());
                if (!resultado) throw new Exception(ds.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                if (cmbEscaners.Items.Count > 1)
                {
                    funciones.MostrarHuellaDesdeBytes(null, nFVDedoA);
                    funciones.MostrarHuellaDesdeBytes(null, nFVDedoB);
                }
                MessageBox.Show("¡Realizado con éxito!");

            }
            catch (Exception ex) { MessageBox.Show("lblEscanerHuellas_Click(). " + ex.Message); }
        }

        private void btnLimpiar_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {

                DataSet ds = new DataSet();
                DialogResult dr = MessageBox.Show("¿Desea limpiar el formulario?", "Enrollment", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.No) return;

                txtUsuario.Text = txtContrasenia.Text = string.Empty;

                ds = funciones.ListarEscanersHuellas(cmbEscaners, true, Settings.Default.FILTRAR_ESCANER_HUELLAS, PARAMETRIZACION.TipoEscanerHuellas.Unidactilar, biometricClientFinger.DeviceManager);
                bool resultado = bool.Parse(ds.Tables[0].Rows[0]["RESULTADO"].ToString());
                if (!resultado) throw new Exception(ds.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                if (cmbEscaners.Items.Count > 1)
                {
                    funciones.MostrarHuellaDesdeBytes(null, nFVDedoA);
                    funciones.MostrarHuellaDesdeBytes(null, nFVDedoB);
                }
                MessageBox.Show("¡Realizado con éxito!");

            }
            catch (Exception ex) { MessageBox.Show("btnLimpiar_MouseClick(). " + ex.Message); }
        }

        private void lnkResetPassword_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                if (biometricClientFinger == null)
                {
                    funciones.CajaMensaje("El sistema biométrico no está inicializado. Por favor, espere a que termine la secuencia de arranque.");
                    return;
                }

                // Ocultar temporalmente el formulario de login
                this.Hide();

                // Crear y mostrar el formulario de restablecimiento de contraseña
                FrmResetPasswordHuella frmReset = new FrmResetPasswordHuella(biometricClientFinger);
                DialogResult result = frmReset.ShowDialog();

                // Mostrar nuevamente el formulario de login
                this.Show();

                // Si el restablecimiento fue exitoso, limpiar los campos
                if (result == DialogResult.OK)
                {
                    txtUsuario.Text = string.Empty;
                    txtContrasenia.Text = string.Empty;

                    // Limpiar las huellas mostradas
                    funciones.MostrarHuellaDesdeBytes(null, nFVDedoA);
                    funciones.MostrarHuellaDesdeBytes(null, nFVDedoB);

                    // Reinicializar el escáner forzando el evento
                    if (cmbEscaners.Items.Count > 0)
                    {
                        int currentIndex = cmbEscaners.SelectedIndex;
                        cmbEscaners.SelectedIndexChanged -= CmbEscaners_SelectedIndexChanged;
                        cmbEscaners.SelectedIndex = -1;
                        cmbEscaners.SelectedIndexChanged += CmbEscaners_SelectedIndexChanged;
                        cmbEscaners.SelectedIndex = currentIndex >= 0 ? currentIndex : 0;
                    }

                    funciones.CajaMensaje("Contraseña restablecida exitosamente. Puede iniciar sesión con su nueva contraseña.");
                }
            }
            catch (Exception ex)
            {
                this.Show(); // Asegurar que el formulario se muestre en caso de error
                funciones.CajaMensaje("Error al abrir el formulario de restablecimiento: " + ex.Message);
            }
        }

        private void lnkAutenticarSinHuella_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                this.Hide();
                FrmLogin2FA frmLogin2FA = new FrmLogin2FA();
                DialogResult result = frmLogin2FA.ShowDialog();

                if (result == DialogResult.OK)
                {
                    // Obtener datos del formulario 2FA
                    loginData = frmLogin2FA.loginData;
                    equipoData = frmLogin2FA.equipoData;
                    sedeDataEquipo = frmLogin2FA.sedeDataEquipo;

                    // Abrir FrmEnrolamiento
                    this.Hide();
                    FrmEnrolamiento frmEnrolamiento = new FrmEnrolamiento(loginData, equipoData, sedeDataEquipo, biometricClientFinger, biometricClientFace);
                    frmEnrolamiento.Show();
                }
                else
                {
                    this.Show();
                }
            }
            catch (Exception ex)
            {
                this.Show();
                funciones.CajaMensaje("Error: " + ex.Message);
            }
        }
    }
}
