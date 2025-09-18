using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using System.Data;
using System.Windows.Forms;
using System.Management;
using System.Reflection;
using System.Net;

using UtilityCore;

using System.Security.Cryptography;

using System.Diagnostics;

using ENROLLMENT_V3.WsBiometricsDGM;

//COMPONENTES DEL SDK DE TRABAJO
using Neurotec.Licensing;
using Neurotec.Biometrics;
using Neurotec.Biometrics.Client;
using Neurotec.Biometrics.Gui;
using Neurotec.Devices;
using Neurotec.Images;

using ENROLLMENT_V3.Properties;
using Newtonsoft.Json;

using System.Globalization;

using CapaEN;

using OpenCvSharp;
using System.Net.Http;

namespace ENROLLMENT_V3
{
    class FUNCIONES
    {
        public void Consola(TextBox textBox, string texto, int nuevasLineas)
        {
            textBox.Text += texto;
            for (int i = 0; i < nuevasLineas; i++) textBox.Text += Environment.NewLine;
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        public void CajaMensaje(string mensaje)
        {
            frmMessageBox frmensaje = new frmMessageBox();
            frmensaje.StartPosition = FormStartPosition.CenterScreen;
            frmensaje.setText(mensaje);
            frmensaje.ShowDialog();

        }
        public DataSet PersonaToDataTable(Movimiento movimiento)
        {
            DataSet ds = GetDsResultado();
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("no", typeof(int));
                dt.Columns.Add("imagen", typeof(Bitmap));
                dt.Columns.Add("rs", typeof(Bitmap));
                dt.Columns.Add("numerodocumento", typeof(string));
                dt.Columns.Add("nombrecompleto", typeof(string));
                int contador = 0;

                foreach(Persona menor in movimiento.menores)
                {
                    contador++;
                    DataRow dr = dt.NewRow();
                    dr["no"] = contador;

                    Bitmap bmp = (Bitmap)ImageFromBase64String(menor.RostroB64);
                    dr["imagen"] = new Bitmap(bmp, new System.Drawing.Size(60, 80));
                    dr["rs"] = null;
                    dr["numerodocumento"] = menor.numerodocumento;
                    dr["nombrecompleto"] = menor.nombrecompleto;
                    dt.Rows.Add(dr);
                }

                foreach (Persona mayor in movimiento.adultos)
                {
                    contador++;
                    DataRow dr = dt.NewRow();
                    dr["no"] = contador;

                    Bitmap bmp = (Bitmap)ImageFromBase64String(mayor.RostroB64);
                    dr["imagen"] = new Bitmap(bmp, new System.Drawing.Size(60, 80));
                    dr["rs"] = null;
                    dr["numerodocumento"] = mayor.numerodocumento;
                    dr["nombrecompleto"] = mayor.nombrecompleto;
                    dt.Rows.Add(dr);
                }

                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                ds.Tables[0].Rows[0]["DATOS"] = dt;
            }
            catch (Exception ex)
            {
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = "ProcessDocumentImage(). " + ex.Message + ". ";
            }
            return ds;
        }
        public void SetMarcador(PictureBox pictureBox, PARAMETRIZACION.EstatusIcono estatus, PictureBox pbxFuente)
        {
            try
            {
                pictureBox.Image = pbxFuente.Image;
                switch (estatus)
                {
                    case PARAMETRIZACION.EstatusIcono.Error:
                        pictureBox.Tag = "Warning";
                        break;

                    case PARAMETRIZACION.EstatusIcono.Carga:
                        pictureBox.Tag = "Loading";
                        break;

                    case PARAMETRIZACION.EstatusIcono.Exito:
                        pictureBox.Tag = "Check";
                        break;

                    default:
                        throw new Exception("Opción no válida para un marcador de seguridad");
                        break;
                }
            }
            catch (Exception ex) { throw new Exception("SetMarcador(). " + ex.Message); }
        }

        public DataSet EsEscanerHuellas442(NFScanner nFScanner)
        {
            DataSet ds = ArmarDsResultado();
            ds.Tables[0].Rows[0]["RESULTADO"] = false;
            bool Escanner442 = false;

            try
            {
                foreach (NFPosition item in nFScanner.GetSupportedPositions())
                {
                    if (!NBiometricTypes.IsPositionFourFingers(item))
                        continue;
                    Escanner442 = true;
                    break;
                }

                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["DATOS"] = Escanner442;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
            }
            catch (Exception ex)
            {
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = "EsEscanerHuellas442(). " + ex.Message;
            }

            return ds;
        }

        public readonly NFPosition[] NFPosition442 = new NFPosition[]
        {
            NFPosition.RightThumb,
            NFPosition.RightIndex,
            NFPosition.RightMiddle,
            NFPosition.RightRing,
            NFPosition.RightLittle,
            NFPosition.LeftThumb,
            NFPosition.LeftIndex,
            NFPosition.LeftMiddle,
            NFPosition.LeftRing,
            NFPosition.LeftLittle,
        };

        public DataSet ProcessDocumentImage(PictureBox pictureBox, Bitmap img)
        {
            DataSet ds = GetDsResultado();
            try
            {
                string rutaImagenSinComprimir = Path.Combine(Application.StartupPath, "ENROL", "json") + "\\" + pictureBox.Name + "_" + DateTime.Now.ToString("ddMMyyyyhhmmss", CultureInfo.InvariantCulture) + ".jpeg";
                string rutaImagenComprimida = Path.Combine(Application.StartupPath, "ENROL", "json") + "\\" + pictureBox.Name + "C_" + DateTime.Now.ToString("ddMMyyyyhhmmss", CultureInfo.InvariantCulture) + ".jpeg";

                try { if (File.Exists(rutaImagenSinComprimir)) File.Delete(rutaImagenSinComprimir); } catch (Exception ex) { throw new Exception("Error al eliminar imagen del pasaporte. Detalles: " + ex.Message); }

                try
                {
                    Bitmap bm = new Bitmap((Image)img.Clone());
                    bm.Save(rutaImagenSinComprimir, ImageFormat.Jpeg);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error al almacenar imagen del pasaporte: " + ex.Message);
                }

                DataSet dsComprimirImagen = new DataSet();
                Image imageCompresa;

                try
                {
                    dsComprimirImagen = ComprimirImagen((Image)img.Clone(), ".jpg", 25);
                    if (bool.Parse(dsComprimirImagen.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                        throw new Exception(dsComprimirImagen.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                    imageCompresa = ((Image)dsComprimirImagen.Tables[0].Rows[0]["DATOS"]);
                    //pictureBox.Image = imageCompresa;
                }
                catch (Exception)
                {
                    throw new Exception("Error al comprimir imagen del documento. ");
                }

                try { if (File.Exists(rutaImagenComprimida)) File.Delete(rutaImagenComprimida); } catch (Exception ex) { throw new Exception("Error al eliminar imagen del pasaporte. Detalles: " + ex.Message); }
                        ((Image)imageCompresa.Clone()).Save(rutaImagenComprimida, ImageFormat.Jpeg);

                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
            }
            catch (Exception ex)
            {
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = "ProcessDocumentImage(). " + ex.Message + ". ";
            }
            return ds;
        }

        public DataSet CargarLogo(PictureBox pictureBox)
        {
            DataSet ds = GetDsResultado();
            try
            {
                string appPath = Application.StartupPath + "\\ENROL\\img";
                string childPath = Path.Combine(appPath, "LOGO_IGM.png");
                Image logoImg = Image.FromFile(childPath);
                pictureBox.Image = logoImg;
                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;

                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
            }
            catch (Exception ex)
            {
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = "CargarLogo(). " + ex.Message + ". ";
            }
            return ds;
        }

        public long GetMSISDN(string cadena)
        {
            string msisdn = "-1";
            for (int i = 0; i < cadena.Length; i++)
                if (char.IsDigit(cadena.ToCharArray()[i]))
                    msisdn += cadena.ToCharArray()[i];

            return long.Parse(msisdn);
        }

        public class AsignacionSedeEquipoMemoria
        {
            public string codigo { get; set; }
            public string mensaje { get; set; }
            public string data { get; set; }

        }

        /** se agregó esta funcion para establecer la version del código fuente ***/
        public static string GetVersion()
        {
            string version = "Versión: 2025-07-16-01";
            return version;
        }
        public DataSet GetSede()
        {
            DataSet ds = GetDsResultado();
            try
            {
                string sede = Settings.Default.SEDE;
                if (sede == null || sede.Trim().Equals("") || sede.Trim().Equals(string.Empty)) throw new Exception("No se encontró una SEDE configurada. ");

                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                ds.Tables[0].Rows[0]["DATOS"] = sede;
            }
            catch (Exception ex)
            {
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = "GetSede(). " + ex.Message;
                ds.Tables[0].Rows[0]["DATOS"] = null;
            }
            return ds;
        }

        public DataSet GetBios()
        {
            DataSet ds = GetDsResultado();
            try
            {
                string item = "";
                foreach (ManagementBaseObject managementBaseObject in (new ManagementObjectSearcher("SELECT SerialNumber, SMBIOSBIOSVersion, ReleaseDate FROM Win32_BIOS")).Get())
                    item = (string)((ManagementObject)managementBaseObject)["SerialNumber"];

                if (item == null || item.Trim().Equals("") || item.Trim().Equals(string.Empty))
                    throw new Exception("No se encontró BIOS configurado. ");

                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                ds.Tables[0].Rows[0]["DATOS"] = item.Trim();
            }
            catch (Exception ex)
            {
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = "ObtenerBios(). " + ex.Message;
                ds.Tables[0].Rows[0]["DATOS"] = null;
            }
            return ds;
        }


        public class SedeEquipoMemoria
        {
            public string abreviaturaSede { get; set; }
            public string serialBios { get; set; }
            public string serialUsb { get; set; }
        }

        public string devApiMiros()
        {
            return Settings.Default.API_REST_MIROS;
        }
        public string ObtenerBios()
        {
            string item = "";
            foreach (ManagementBaseObject managementBaseObject in (new ManagementObjectSearcher("SELECT SerialNumber, SMBIOSBIOSVersion, ReleaseDate FROM Win32_BIOS")).Get())
            {
                item = (string)((ManagementObject)managementBaseObject)["SerialNumber"];
            }
            return item.Trim();
        }
        //CONSULTA DE ASIGNACIÓN DE CONSULADOS, EQUIPOS Y MEMORIAS
        public DataSet ConsultarAsignacionSeEqMe(string sede, string serialBios, string serialUsb)
        {
            DataSet dsResultado = ArmarDsResultado();
            try
            {

                SedeEquipoMemoria vParametros = new SedeEquipoMemoria();
                vParametros.abreviaturaSede = sede;
                vParametros.serialBios = serialBios;
                vParametros.serialUsb = serialUsb;

                string postString = JsonConvert.SerializeObject(vParametros);

                byte[] data = UTF8Encoding.UTF8.GetBytes(postString);

                HttpWebRequest request;
                request = WebRequest.Create(@"" + Settings.Default.IP_SERVICIOS_WEB + "/equipo_consulado_memoria") as HttpWebRequest;
                request.Timeout = 10 * 1000;
                request.Method = "POST";
                request.ContentLength = data.Length;
                request.ContentType = "application/json; charset=utf-8";

                var user = "migracion-pasaportes-enrollment-3.0";
                var password = "abc123";
                string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{user}:{password}"));
                request.Headers.Add("Authorization", "Basic " + credentials);


                Stream postStream = request.GetRequestStream();
                postStream.Write(data, 0, data.Length);

                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                StreamReader reader = new StreamReader(response.GetResponseStream());

                string body = reader.ReadToEnd();

                AsignacionSedeEquipoMemoria vAsignacion;
                vAsignacion = JsonConvert.DeserializeObject<AsignacionSedeEquipoMemoria>(body);

                if (!vAsignacion.codigo.Equals("200"))
                    throw new Exception("¡Código de servicio no válido: " + vAsignacion.codigo + " - " + vAsignacion.mensaje + "!");

                //MessageBox.Show("vAsignacion.data: " + vAsignacion.data);
                int id_memoria_equipo_consulado;
                if (int.TryParse((string)vAsignacion.data, out id_memoria_equipo_consulado))
                    if (id_memoria_equipo_consulado <= 0)
                        throw new Exception("¡ID de asignación no válido!");

                dsResultado.Tables[0].Rows[0]["DATOS"] = vAsignacion.data;
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;


            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "ConsultarAsignacionSeEqMe(). " + ex.Message;
            }
            return dsResultado;
        }

        public string DepartamentoDesdeCedula(string cedula)
        {
            try
            {
                if (cedula.ToUpper().Contains("A-1") || cedula.ToUpper().Contains("A-01") || cedula.ToUpper().Contains("A1") || cedula.ToUpper().Contains("A01"))
                    return "GUATEMALA";
                else if (cedula.ToUpper().Contains("B-2") || cedula.ToUpper().Contains("B-02") || cedula.ToUpper().Contains("B2") || cedula.ToUpper().Contains("B02"))
                    return "SACATEPEQUEZ";
                else if (cedula.ToUpper().Contains("C-3") || cedula.ToUpper().Contains("C-03") || cedula.ToUpper().Contains("C3") || cedula.ToUpper().Contains("C03"))
                    return "CHIMALTENANGO";
                else if (cedula.ToUpper().Contains("D-4") || cedula.ToUpper().Contains("D-04") || cedula.ToUpper().Contains("D4") || cedula.ToUpper().Contains("D04"))
                    return "EL PROGRESO";
                else if (cedula.ToUpper().Contains("E-5") || cedula.ToUpper().Contains("E-05") || cedula.ToUpper().Contains("E5") || cedula.ToUpper().Contains("E05"))
                    return "ESCUINTLA";
                else if (cedula.ToUpper().Contains("F-6") || cedula.ToUpper().Contains("F-06") || cedula.ToUpper().Contains("F6") || cedula.ToUpper().Contains("F06"))
                    return "SANTA ROSA";
                else if (cedula.ToUpper().Contains("G-7") || cedula.ToUpper().Contains("G-07") || cedula.ToUpper().Contains("G7") || cedula.ToUpper().Contains("G07"))
                    return "SOLOLA";
                else if (cedula.ToUpper().Contains("H-8") || cedula.ToUpper().Contains("H-08") || cedula.ToUpper().Contains("H8") || cedula.ToUpper().Contains("H08"))
                    return "TOTONICAPAN";
                else if (cedula.ToUpper().Contains("I-9") || cedula.ToUpper().Contains("I-09") || cedula.ToUpper().Contains("I9") || cedula.ToUpper().Contains("I09"))
                    return "QUETZALTENANGO";
                else if (cedula.ToUpper().Contains("J-10") || cedula.ToUpper().Contains("J10"))
                    return "SUCHITEPEQUEZ";
                else if (cedula.ToUpper().Contains("K-11") || cedula.ToUpper().Contains("K11"))
                    return "RETALHULEU";
                else if (cedula.ToUpper().Contains("L-12") || cedula.ToUpper().Contains("L12"))
                    return "SAN MARCOS";
                else if (cedula.ToUpper().Contains("M-13") || cedula.ToUpper().Contains("M13"))
                    return "HUEHUETENANGO";
                else if (cedula.ToUpper().Contains("N-14") || cedula.ToUpper().Contains("N14"))
                    return "QUICHE";
                else if (cedula.ToUpper().Contains("Ñ-15") || cedula.ToUpper().Contains("Ñ15"))
                    return "BAJA VERAPAZ";
                else if (cedula.ToUpper().Contains("O-16") || cedula.ToUpper().Contains("O16"))
                    return "ALTA VERAPAZ";
                else if (cedula.ToUpper().Contains("P-17") || cedula.ToUpper().Contains("P17"))
                    return "PETEN";
                else if (cedula.ToUpper().Contains("Q-18") || cedula.ToUpper().Contains("A18"))
                    return "IZABAL";
                else if (cedula.ToUpper().Contains("R-19") || cedula.ToUpper().Contains("R19"))
                    return "ZACAPA";
                else if (cedula.ToUpper().Contains("S-20") || cedula.ToUpper().Contains("S20"))
                    return "CHIQUIMULA";
                else if (cedula.ToUpper().Contains("T-21") || cedula.ToUpper().Contains("T21"))
                    return "JALAPA";
                else if (cedula.ToUpper().Contains("U-22") || cedula.ToUpper().Contains("U22"))
                    return "JUTIAPA";
                else
                    return "DESCONOCIDO";
            }
            catch (Exception ex)
            {
                throw new Exception("DepartamentoFromCedula(). " + ex.Message);
            }
        }

        public string Base64FromImage(Image image, ImageFormat imageFormat)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    Bitmap bitmap = new Bitmap(image);
                    bitmap.Save(ms, imageFormat);
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Base64DesdePng(). " + ex.Message);
            }
        }

        public byte[] ByteArrayFromImage(Image image, ImageFormat imageFormat)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    Bitmap bitmap = new Bitmap(image);
                    bitmap.Save(ms, imageFormat);
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("ByteArrayFromImage(). " + ex.Message);
            }
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

        private DataSet ArmarDsGenerarPlantilla()
        {
            DataSet ds = new DataSet();
            ds.Tables.Add(new DataTable());
            ds.Tables[0].Columns.Add("RESULTADO", typeof(bool));
            ds.Tables[0].Columns.Add("MSG_ERROR", typeof(string));

            ds.Tables[0].Columns.Add("ESTATUS", typeof(object));
            ds.Tables[0].Columns.Add("SUJETO", typeof(object));


            DataRow dr = ds.Tables[0].NewRow();
            ds.Tables[0].Rows.Add(dr);

            return ds;
        }

        public string ObtenerSerialUSB()
        {
            string serialUSB = "";
            ManagementObjectSearcher theSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive WHERE InterfaceType='USB'");
            foreach (ManagementObject currentObject in theSearcher.Get())
            {
                ManagementObject theSerialNumberObjectQuery = new ManagementObject("Win32_PhysicalMedia.Tag='" + currentObject["DeviceID"] + "'");
                serialUSB = theSerialNumberObjectQuery["SerialNumber"].ToString();
            }
            return serialUSB.Trim();
        }

        public DataSet GetUSBDriveLetters()
        {
            DataSet ds = GetDsResultado();
            try
            {
                var driveLetters = new List<string>();
                foreach (ManagementObject drive in new ManagementObjectSearcher("select * from Win32_DiskDrive where InterfaceType='USB'").Get())
                {
                    string serialUsbLocal = "" + new ManagementObject("Win32_PhysicalMedia.Tag='" + drive["DeviceID"] + "'")["SerialNumber"];
                    serialUsbLocal = serialUsbLocal.Trim();

                    //associate physical disks with partitions
                    foreach (ManagementObject partition in new ManagementObjectSearcher("ASSOCIATORS OF {Win32_DiskDrive.DeviceID='" + drive["DeviceID"] + "'} WHERE AssocClass = Win32_DiskDriveToDiskPartition").Get())
                    {
                        Console.WriteLine("Partition=" + partition["Name"]);

                        //associate partitions with logical disks(drive letter volumes)
                        foreach (ManagementObject disk in new ManagementObjectSearcher("ASSOCIATORS OF {Win32_DiskPartition.DeviceID='" + partition["DeviceID"] + "'} WHERE AssocClass = Win32_LogicalDiskToPartition").Get())
                        {
                            Console.WriteLine("Disk=" + disk["Name"]);
                            //Properties.Settings.Default.DRIVE_LETTER = "" + disk["Name"];
                            serialUsbLocal += ("-" + disk["Name"]).Trim();
                            driveLetters.Add(serialUsbLocal);
                        }
                    }
                }

                if (driveLetters == null || driveLetters.Count <= 0)
                    throw new Exception("No se encontró ningún dispositivo USB conectado. ");

                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                ds.Tables[0].Rows[0]["DATOS"] = driveLetters;
            }
            catch (Exception ex)
            {
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = "GetUSBDriveLetters(). " + ex.Message;
                ds.Tables[0].Rows[0]["DATOS"] = null;
            }
            return ds;
        }

        public string GetIpAddress()
        {
            string localIP;
            using (System.Net.Sockets.Socket socket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address.ToString();
            }
            return localIP;
        }
        public DataSet ValidarSedeEquipoMemoria(List<string> drives)
        {
            DataSet ds = GetDsResultado();
            try
            {
                if (Settings.Default.ConsultarAsignacionSeEqMe && Settings.Default.USB_DEVICE)
                {
                    foreach (string drive in drives)
                    {
                        Settings.Default.DRIVE_LETTER = null;
                        Settings.Default.SERIAL_USB = drive.Split('-')[0];
                        DataSet dsDrives = ConsultarAsignacionSeEqMe(Settings.Default.SEDE, Settings.Default.BIOS, Settings.Default.SERIAL_USB);

                        if (dsDrives.Tables[0].Rows.Count > 0)
                            if (dsDrives.Tables[0].Rows.Count > 0)
                                if (bool.Parse(dsDrives.Tables[0].Rows[0]["RESULTADO"].ToString()))
                                {
                                    Settings.Default.DRIVE_LETTER = drive.Split('-')[1];
                                    break;
                                }
                    }
                    if (Settings.Default.DRIVE_LETTER == null) throw new Exception("Error al validar la asignación SEDE/EQUIPO/USB. ");
                }

                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                ds.Tables[0].Rows[0]["DATOS"] = null;
            }
            catch (Exception ex)
            {
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = "ValidarSedeEquipoMemoria(). " + ex.Message + ". ";
                ds.Tables[0].Rows[0]["DATOS"] = null;
            }
            return ds;
        }

        public DataSet ValidarComponenteNeuro(String componente)
        {
            DataSet ds = GetDsResultado();
            try
            {
                NLicense.ObtainComponents(Settings.Default.SERVIDOR_NEURO, Settings.Default.PUERTO_NEURO, componente);
                bool componenteActivo = NLicense.IsComponentActivated(componente);

                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                ds.Tables[0].Rows[0]["DATOS"] = componenteActivo;
            }
            catch (Exception ex)
            {
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = "ValidarLicenciasNeuro(). " + ex.Message + ". ";
                ds.Tables[0].Rows[0]["DATOS"] = null;
            }
            return ds;
        }

        public DataSet IniciarClienteBiometriaHuellas(NBiometricClient cliente)
        {
            DataSet ds = GetDsResultado();
            try
            {
                cliente = new NBiometricClient { UseDeviceManager = true, BiometricTypes = NBiometricType.Finger };
                cliente.Initialize();

                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                ds.Tables[0].Rows[0]["DATOS"] = cliente;
            }
            catch (Exception ex)
            {
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = "IniciarClienteBiometriaHuellas(). " + ex.Message + ". ";
                ds.Tables[0].Rows[0]["DATOS"] = null;
            }
            return ds;
        }

        public DataSet IniciarClienteBiometriaRostro(NBiometricClient cliente)
        {
            DataSet ds = GetDsResultado();
            try
            {
                cliente = new NBiometricClient { UseDeviceManager = true, BiometricTypes = NBiometricType.Face };
                cliente.Initialize();

                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                ds.Tables[0].Rows[0]["DATOS"] = cliente;
            }
            catch (Exception ex)
            {
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = "IniciarClienteBiometriaRostro(). " + ex.Message + ". ";
                ds.Tables[0].Rows[0]["DATOS"] = null;
            }
            return ds;
        }

        public void AsignarDriveLetter(string serialUsbWs)
        {
            if (Properties.Settings.Default.USB_DEVICE)
            {

                foreach (ManagementObject drive in new ManagementObjectSearcher("select * from Win32_DiskDrive where InterfaceType='USB'").Get())
                {

                    string serialUsbLocal = "" + new ManagementObject("Win32_PhysicalMedia.Tag='" + drive["DeviceID"] + "'")["SerialNumber"];
                    if (serialUsbWs.Equals(serialUsbLocal.Trim()))
                    {
                        //associate physical disks with partitions
                        foreach (ManagementObject partition in new ManagementObjectSearcher("ASSOCIATORS OF {Win32_DiskDrive.DeviceID='" + drive["DeviceID"] + "'} WHERE AssocClass = Win32_DiskDriveToDiskPartition").Get())
                        {
                            Console.WriteLine("Partition=" + partition["Name"]);

                            //associate partitions with logical disks(drive letter volumes)
                            foreach (ManagementObject disk in new ManagementObjectSearcher("ASSOCIATORS OF {Win32_DiskPartition.DeviceID='" + partition["DeviceID"] + "'} WHERE AssocClass = Win32_LogicalDiskToPartition").Get())
                            {
                                Console.WriteLine("Disk=" + disk["Name"]);
                                Properties.Settings.Default.DRIVE_LETTER = "" + disk["Name"];
                            }
                        }
                    }
                    else
                        Properties.Settings.Default.DRIVE_LETTER = null;
                }
            }
            else
                Properties.Settings.Default.DRIVE_LETTER = Path.Combine(Application.StartupPath, "ENROL");
        }

        public string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }


        public DataSet ExisteDirectorioLlaves(string usuario, string nombreMaquina)
        {
            DataSet dsResultado = ArmarDsResultado();
            try
            {
                string msgErrorLlaves = string.Empty;

                //COMPROBANDO LOS DIRECTORIOS
                if (Properties.Settings.Default.DRIVE_LETTER == null || Properties.Settings.Default.DRIVE_LETTER == string.Empty || Properties.Settings.Default.DRIVE_LETTER.Equals(""))
                    throw new Exception("Unidad de encriptación no encontrada. ");

                string directorioEncriptarXml = Path.Combine(Properties.Settings.Default.DRIVE_LETTER, "DGMGT", "XMLKeys");
                if (Directory.Exists(directorioEncriptarXml) == false)
                    msgErrorLlaves += "¡Directorio de encriptación no encontrado!. ";
                else
                {
                    string llavePublica = "";

                    if (Properties.Settings.Default.SUFIJO_ENCRIPTACION.Equals("U"))
                        llavePublica = Path.Combine(Properties.Settings.Default.DRIVE_LETTER, "DGMGT", "XMLKeys", usuario + "_publi.xml");
                    else if (Properties.Settings.Default.SUFIJO_ENCRIPTACION.Equals("M"))
                        llavePublica = Path.Combine(Properties.Settings.Default.DRIVE_LETTER, "DGMGT", "XMLKeys", nombreMaquina + "_publi.xml");

                    if (File.Exists(llavePublica) == false)
                        msgErrorLlaves += "¡Archivos de configuración no encontrados (1)!. ";
                }

                string directorioFTP = Path.Combine(Properties.Settings.Default.DRIVE_LETTER, "DGMGT", "FTPKeys");
                if (Directory.Exists(directorioFTP) == false)
                    msgErrorLlaves += "¡Directorio de transferencia de archivos no encontrado!. ";
                else
                {
                    string archivoFTP = Path.Combine(Properties.Settings.Default.DRIVE_LETTER, "DGMGT", "FTPKeys", "FTPKeys.xml");
                    if (File.Exists(archivoFTP) == false || File.Exists(archivoFTP) == false)
                        msgErrorLlaves += "¡Archivos de configuración no encontrados (2)!. ";
                }

                string directorioDatosEquipo = Path.Combine(Properties.Settings.Default.DRIVE_LETTER, "DGMGT", "PCKeys");
                if (Directory.Exists(directorioDatosEquipo) == false)
                    msgErrorLlaves += "¡Directorio de datos de equipo no encontrado!. ";
                else
                {
                    string archivoPC = Path.Combine(Properties.Settings.Default.DRIVE_LETTER, "DGMGT", "PCKeys", "PCKeys.xml");
                    if (File.Exists(archivoPC) == false || File.Exists(archivoPC) == false)
                        msgErrorLlaves += "¡Archivos de configuración no encontrados (3)!. ";
                }

                if (msgErrorLlaves.Equals(string.Empty) == false)
                    throw new Exception(msgErrorLlaves);

                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                dsResultado.Tables[0].Rows[0]["DATOS"] = null;
            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "ExisteDirectorioLlaves(). " + ex.Message;
                dsResultado.Tables[0].Rows[0]["DATOS"] = null;
            }

            return dsResultado;
        }

        public Task<DataSet> EnvioFTPArchivo(string rutaArchivoEncriptado)
        {
            return Task.Run(() =>
            {
                DataSet dsResultado = ArmarDsResultado();
                try
                {
                    string FtpKeysFile = Path.Combine(Properties.Settings.Default.DRIVE_LETTER + "\\", "DGMGT", "FTPKeys", "FTPKeys.xml");

                    DataSet dsFTP = new DataSet();
                    dsFTP.ReadXml(FtpKeysFile);

                    FtpCore ftpCore = new FtpCore(dsFTP.Tables[0].Rows[0]["HOST"].ToString(), dsFTP.Tables[0].Rows[0]["USER"].ToString(), dsFTP.Tables[0].Rows[0]["PASSWORD"].ToString()) { uploadPath = dsFTP.Tables[0].Rows[0]["FOLDER"].ToString() };
                    ftpCore.InitCliet();
                    ftpCore.uploadFile(rutaArchivoEncriptado);
                    ftpCore.uploadFile(rutaArchivoEncriptado + ".done");
                    //SI LA CARPETA NO EXISTE HAY QUE CREARLA.
                    File.Move(rutaArchivoEncriptado, Path.Combine(Application.StartupPath, "ENROL", "XMLs", "Done") + "\\" + Path.GetFileNameWithoutExtension(rutaArchivoEncriptado) + ".txt");
                    File.Delete(rutaArchivoEncriptado + ".done");

                    dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
                    dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                    dsResultado.Tables[0].Rows[0]["DATOS"] = null;
                }
                catch (Exception ex)
                {
                    dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                    dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "EnvioFTPArchivo(). " + ex.Message + "Ruta";
                    dsResultado.Tables[0].Rows[0]["DATOS"] = null;
                }
                return dsResultado;
            });
        }

        public DataSet EsDsBiometriaValido(DataSet dsBiometria)
        {
            DataSet dsResultado = ArmarDsResultado();
            try
            {
                if (dsBiometria == null)
                    throw new Exception("Data set de comprobacion biométrica es nulo. ");

                if (dsBiometria.Tables.Count == 0)
                    throw new Exception("No existe tabla dentro del Data set de comprobacion biométrica. ");

                if (dsBiometria.Tables[0].Rows.Count == 0)
                    throw new Exception("No existe fila dentro de tabla del Data set de comprobacion biométrica. ");

                if (bool.Parse(dsBiometria.Tables[0].Rows[0]["LICENCIAS_INICIAR"].ToString()) == false)
                    throw new Exception("Error al validar licencias del cliente biométrico externo. LICENCIAS_RESULTADO: " + dsBiometria.Tables[0].Rows[0]["LICENCIAS_RESULTADO"].ToString() + ", LICENCIAS_ERROR_MSG: " + dsBiometria.Tables[0].Rows[0]["LICENCIAS_ERROR_MSG"].ToString());

                if (bool.Parse(dsBiometria.Tables[0].Rows[0]["CLIENTE_BIOMETRICO_INICIAR"].ToString()) == false)
                    throw new Exception("Error al iniciar el cliente biométrico externo. CLIENTE_BIOMETRICO_ERROR_MSG: " + dsBiometria.Tables[0].Rows[0]["CLIENTE_BIOMETRICO_ERROR_MSG"].ToString());

                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = MethodBase.GetCurrentMethod().Name + "(). Excepcion: " + ex.Message + ". InnerException: " + ex.InnerException;
            }

            return dsResultado;
        }

        public DataSet EsDsCoincidenciaABValido(DataSet dsCoincidenciaAB)
        {
            DataSet dsResultado = ArmarDsResultado();
            try
            {
                if (dsCoincidenciaAB == null)
                    throw new Exception("Data set de coincidencia biométrica es nulo. ");

                if (dsCoincidenciaAB.Tables.Count == 0)
                    throw new Exception("No existe tabla dentro del Data set de coincidencia biométrica. ");

                if (dsCoincidenciaAB.Tables[0].Rows.Count == 0)
                    throw new Exception("No existe fila dentro de tabla del Data set de coincidencia biométrica. ");

                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = MethodBase.GetCurrentMethod().Name + "(). Excepcion: " + ex.Message + ". InnerException: " + ex.InnerException;
            }
            return dsResultado;
        }

        private DataTable ArmarDtHuellasEncabezado()
        {
            DataTable dt = new DataTable("ENCABEZADO");
            dt.Columns.Add("HUELLAS_MDERECHA", typeof(int));
            dt.Columns.Add("HUELLAS_MIZQUIERDA", typeof(int));
            dt.Columns.Add("HUELLAS_TOTAL", typeof(int));
            dt.Columns.Add("HIT", typeof(bool));
            dt.Columns.Add("HIT_HUELLA1", typeof(int));
            dt.Columns.Add("IMAGEN_PNG_STRING_BASE64_1", typeof(string));
            dt.Columns.Add("IMAGEN_WSQ_STRING_BASE64_1", typeof(string));
            dt.Columns.Add("HIT_HUELLA2", typeof(int));
            dt.Columns.Add("IMAGEN_PNG_STRING_BASE64_2", typeof(string));
            dt.Columns.Add("IMAGEN_WSQ_STRING_BASE64_2", typeof(string));

            DataRow dr = dt.NewRow();
            dr["HUELLAS_MDERECHA"] = 0;
            dr["HUELLAS_MIZQUIERDA"] = 0;
            dr["HUELLAS_TOTAL"] = 0;
            dr["HIT"] = false;
            dr["HIT_HUELLA1"] = NFPosition.Unknown;
            dr["IMAGEN_PNG_STRING_BASE64_1"] = string.Empty;
            dr["IMAGEN_WSQ_STRING_BASE64_1"] = string.Empty;
            dr["HIT_HUELLA2"] = NFPosition.Unknown;
            dr["IMAGEN_PNG_STRING_BASE64_2"] = string.Empty;
            dr["IMAGEN_WSQ_STRING_BASE64_2"] = string.Empty;
            dt.Rows.Add(dr);

            return dt;
        }

        public DataTable ArmarDtHuellasDetalle()
        {
            DataTable dt = new DataTable("DETALLE");
            dt.Columns.Add("MANO", typeof(string));
            dt.Columns.Add("POSICION", typeof(int));
            dt.Columns.Add("HIT", typeof(bool));
            dt.Columns.Add("IMAGEN_PNG", typeof(Image));
            dt.Columns.Add("IMAGEN_PNG_STRING_BASE64", typeof(string));
            dt.Columns.Add("IMAGEN_WSQ_STRING_BASE64", typeof(string));



            //DataRow dr = dt.NewRow();
            //dt.Rows.Add(dr);

            return dt;
        }

        public DataSet GenerarHuellaPNG_STRING_BASE64(Image huellaDigital)
        {
            DataSet ds = new DataSet();
            ds.Tables.Add(new DataTable());
            ds.Tables[0].Columns.Add("RESULTADO", typeof(bool));
            ds.Tables[0].Columns.Add("MSG_ERROR", typeof(string));
            ds.Tables[0].Columns.Add("IMAGEN_PNG", typeof(Image));
            ds.Tables[0].Columns.Add("IMAGEN_PNG_STRING_BASE64", typeof(string));
            ds.Tables[0].Columns.Add("IMAGEN_WSQ_STRING_BASE64", typeof(string));

            DataRow dr = ds.Tables[0].NewRow();
            ds.Tables[0].Rows.Add(dr);

            try
            {
                Bitmap ImagenFinal;

                ImagenFinal = new Bitmap(416, 416);
                using (Graphics graph = Graphics.FromImage(ImagenFinal))
                {
                    System.Drawing.Rectangle ImageSize = new System.Drawing.Rectangle(0, 0, 416, 416);
                    graph.FillRectangle(Brushes.White, ImageSize);

                    //ImageSize = new Rectangle(79, 40, 258, 336);
                    //graph.FillRectangle(Brushes.Blue, ImageSize);

                    graph.DrawImage(huellaDigital, new System.Drawing.Rectangle(79, 40, 258, 336));
                }

                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                ds.Tables[0].Rows[0]["IMAGEN_PNG"] = ImagenFinal;

                MemoryStream ms = new MemoryStream();
                ImagenFinal.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ds.Tables[0].Rows[0]["IMAGEN_PNG_STRING_BASE64"] = Convert.ToBase64String(ms.ToArray());
                ms.Dispose();

                using (NImage imagenNeuro = NImage.FromBitmap((Bitmap)ImagenFinal))
                {
                    // Create WSQInfo to store bit rate
                    using (var info = (WsqInfo)NImageFormat.Wsq.CreateInfo(imagenNeuro))
                    {
                        var bitrate = WsqInfo.DefaultBitRate;
                        info.BitRate = bitrate;

                        string sWsqBase64 = Convert.ToBase64String(imagenNeuro.Save(NImageFormat.Wsq).ToArray());
                        ds.Tables[0].Rows[0]["IMAGEN_WSQ_STRING_BASE64"] = sWsqBase64;
                    }
                }
            }
            catch (Exception ex)
            {
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = "GenerarHuellaPNG_STRING_BASE64(). " + ex.Message;
                ds.Tables[0].Rows[0]["IMAGEN_PNG"] = null;
                ds.Tables[0].Rows[0]["IMAGEN_PNG_STRING_BASE64"] = string.Empty;
                ds.Tables[0].Rows[0]["IMAGEN_WSQ_STRING_BASE64"] = string.Empty;
            }
            return ds;
        }

        public DataSet GetPngFromStringRenapFomat(string sHuella)
        {
            DataSet ds = ArmarDsResultado();
            try
            {
                Image imgHuella = ImageFromBase64String(sHuella);
                Rectangle cropRect = new Rectangle(79, 40, 258, 336);
                Bitmap ImagenOriginal = imgHuella.Clone() as Bitmap;
                Bitmap cropedImage = ImagenOriginal.Clone(cropRect, ImagenOriginal.PixelFormat);

                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                ds.Tables[0].Rows[0]["DATOS"] = cropedImage;
            }
            catch (Exception ex)
            {
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = "GetPngFromStringRenapFomat(). " + ex.Message;
                ds.Tables[0].Rows[0]["DATOS"] = null;
            }
            return ds;
        }

        public DataSet MostrarHuellaDesdeBytes(byte[] bytes, NFingerView nfvHuella)
        {
            DataSet ds = ArmarDsResultado();
            ds.Tables[0].Rows[0]["RESULTADO"] = false;
            try
            {
                nfvHuella.Finger = null;
                if (bytes != null)
                {
                    // Create a finger object
                    var nfHuella = new NFinger { Image = NImage.FromMemory(bytes) };
                    nfvHuella.Finger = nfHuella;
                }

                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
            }
            catch (Exception ex)
            {
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = "MostrarHuellaDesdeBytes(). " + ex.Message;
            }

            return ds;
        }

        public async Task<DataSet> EscanearHuella(NFPosition nFPosition, NFingerView nFingerView, NBiometricClient nBiometricClient)
        {
            DataSet ds = ArmarDsResultado();
            ds.Tables[0].Rows[0]["RESULTADO"] = false;
            try
            {
                if (nBiometricClient.FingerScanner == null)
                    throw new Exception(@"Por favor, seleccione un escáner de la lista.");

                // Create a objeto tipo NFinger
                NFinger nFingerA = new NFinger();
                nFingerA.Position = nFPosition;

                // Agregar un dedo al sujeto y a la vista
                NSubject nSubject = new NSubject();
                nSubject.Fingers.Add(nFingerA);

                nFingerView.Finger = nFingerA;
                nFingerView.ShownImage = ShownImage.Original;

                CancelarOperacionBiometrica(nBiometricClient);

                // Begin capturing                
                NBiometricTask task = nBiometricClient.CreateTask(NBiometricOperations.Capture | NBiometricOperations.CreateTemplate, nSubject);
                var performedTask = await nBiometricClient.PerformTaskAsync(task);

                if (performedTask.Status != NBiometricStatus.Ok && performedTask.Status != NBiometricStatus.Canceled) throw new Exception("¡No fue posible realizar la extracción!, Estatus: " + performedTask.Status.ToString() + ". Error. " + performedTask.Error);
                nFingerView.Finger = nSubject.Fingers[0];

                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                ds.Tables[0].Rows[0]["DATOS"] = nSubject.Fingers[0];
            }
            catch (Exception ex)
            {
                //throw new Exception("ListarEscaners(). " + ex.Message);
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = "EscanearHuella(). " + ex.Message;
            }
            return ds;
        }

        public async Task<DataSet> ActivarCamara(NFaceView nFaceView, NBiometricClient nBiometricClient)
        {
            DataSet ds = ArmarDsResultado();
            ds.Tables[0].Rows[0]["RESULTADO"] = false;
            try
            {
                if (nBiometricClient.FaceCaptureDevice == null)
                    throw new Exception(@"Por favor, seleccione una cámara de la lista.");

                // Set face capture from stream
                var face = new NFace { CaptureOptions = NBiometricCaptureOptions.Stream | NBiometricCaptureOptions.Manual };

                // Agregar un dedo al sujeto y a la vista
                NSubject nSubject = new NSubject();
                nSubject.Faces.Add(face);
                nFaceView.Face = face;

                CancelarOperacionBiometrica(nBiometricClient);

                // Begin capturing                
                NBiometricTask task = nBiometricClient.CreateTask(NBiometricOperations.Capture | NBiometricOperations.CreateTemplate, nSubject);
                var performedTask = await nBiometricClient.PerformTaskAsync(task);

                //if (nBiometricClient != null) if (nBiometricClient.CurrentBiometric != null) {nBiometricClient.Cancel();}

                //var status = await nBiometricClient.CaptureAsync(nSubject);
                //if (status != NBiometricStatus.Ok && status != NBiometricStatus.Canceled) throw new Exception("Error al activar la cámara. " + status);
                if (performedTask.Status != NBiometricStatus.Ok && performedTask.Status != NBiometricStatus.Canceled) throw new Exception("¡No fue posible realizar la extracción!, Estatus: " + performedTask.Status.ToString() + ". Error. " + performedTask.Error);
                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                ds.Tables[0].Rows[0]["DATOS"] = string.Empty;
            }
            catch (Exception ex)
            {
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = "ActivarCamara(). " + ex.Message;
            }
            return ds;
        }

        public void ActivarCamaraOpenCV(PictureBox pbxRostro)
        {
            DataSet ds = ArmarDsResultado();
            ds.Tables[0].Rows[0]["RESULTADO"] = false;
            VideoCapture capture;
            Mat frame;
            System.Drawing.Bitmap image;

            try
            {
                frame = new Mat();
                capture = new VideoCapture();
                capture.Open(1);

                if (capture.IsOpened())
                {
                    while (true)//isCameraRunning)
                    {
                        capture.Read(frame);
                        image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(frame);
                        if (pbxRostro.Image != null)
                        {
                            //pbxRostro.Image = null;
                            pbxRostro.Image.Dispose();
                            
                        }
                        pbxRostro.Image = (Bitmap)image.Clone();
                    }
                }

                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                ds.Tables[0].Rows[0]["DATOS"] = string.Empty;
            }
            catch (Exception ex)
            {
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = "ActivarCamara(). " + ex.Message;
            }
            //return ds;
        }

        public DataSet GetDsResultado() { return ArmarDsResultado(); }

        public DataSet CancelarOperacionBiometrica(NBiometricClient cliente)
        {
            DataSet ds = GetDsResultado();
            try
            {
                NBiometric nBio = null;
                if (cliente != null)
                    nBio = cliente.CurrentBiometric;
                if (nBio != null) cliente.Cancel();

                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                ds.Tables[0].Rows[0]["DATOS"] = null;
            }
            catch (Exception ex)
            {
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = "CancelarOperacionBiometrica(). " + ex.Message + ". ";
                ds.Tables[0].Rows[0]["DATOS"] = null;
            }
            return ds;
        }

        public DataSet StringBase64ToFinger8b(string s)
        {
            DataSet ds = GetDsResultado();
            try
            {
                byte[] byteArray = Convert.FromBase64String(s);
                NImage nImage = NImage.FromMemory(byteArray);
                nImage.VertResolution = 500f;
                nImage.HorzResolution = 500f;

                NImage nImage8b = NImage.FromImage(NPixelFormat.Rgb8U, 0, nImage);
                nImage8b.ResolutionIsAspectRatio = false;

                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                ds.Tables[0].Rows[0]["DATOS"] = nImage8b;
            }
            catch (Exception ex)
            {
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = "StringBase64ToFinger8b(). " + ex.Message + ". ";
                ds.Tables[0].Rows[0]["DATOS"] = null;
            }
            return ds;
        }

        private NFinger ArrayToFinger(NFingerView[] nFingerViews, NFPosition nFPosition)
        {
            try
            {
                foreach (NFingerView nFingerView in nFingerViews)
                    if (nFingerView.Finger != null)
                        if (nFingerView.Finger.Position == nFPosition)
                            return (NFinger)nFingerView.Finger;
            }
            catch (Exception ex)
            {
                throw new Exception("ArrayToFinger(). " + ex.Message);
            }

            return null;
        }

        public DataSet GenerarDsSubject442(NFingerView[] nFingerViews, NFPosition[] nfPosition)
        {
            DataSet dsResultado = ArmarDsResultado();
            dsResultado.Tables[0].TableName = "RESULTADO";

            dsResultado.Tables.Add(ArmarDtHuellasEncabezado());
            dsResultado.Tables.Add(ArmarDtHuellasDetalle());

            try
            {
                NSubject nSubject = new NSubject();
                NFinger nFinger = new NFinger();

                foreach (NFPosition nFPosition in NFPosition442)
                {
                    nFinger = ArrayToFinger(nFingerViews, nFPosition);
                    if (nFinger == null)
                    {
                        nFinger = new NFinger();
                        nFinger.Position = NFPosition.Unknown;
                    }

                    if (nFinger.Position != NFPosition.Unknown)
                    {
                        byte[] byteArray = nFinger.Image.Save(NImageFormat.Png).ToArray();
                        Image iFinger;

                        using (var stream = new MemoryStream(byteArray, 0, byteArray.Length))
                        {
                            iFinger = Image.FromStream(stream);

                            DataSet dsHuellaFinal = GenerarHuellaPNG_STRING_BASE64(iFinger);

                            if (dsHuellaFinal == null || dsHuellaFinal.Tables.Count < 1 || dsHuellaFinal.Tables[0].Rows.Count < 1)
                                throw new Exception("¡Error al generar la huella " + nFinger.Position + "!");

                            if (bool.Parse(dsHuellaFinal.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                                throw new Exception("!Error al generar la huella derecha " + nFinger.Position + "!. " + dsHuellaFinal.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                            string mano = string.Empty;
                            bool encontrado = false;

                            foreach (NFPosition nFPositionF in NFPosition442)
                                if (!encontrado)
                                    if (nFinger.Position == NFPosition.RightThumb || nFinger.Position == NFPosition.RightIndex || nFinger.Position == NFPosition.RightMiddle || nFinger.Position == NFPosition.RightRing || nFinger.Position == NFPosition.RightLittle)
                                    {
                                        mano = "RIGHT";
                                        encontrado = true;
                                    }

                            encontrado = false;
                            if (mano.Equals(string.Empty))
                                foreach (NFPosition nFPositionF in NFPosition442)
                                    if (!encontrado)
                                        if (nFinger.Position == NFPosition.LeftThumb || nFinger.Position == NFPosition.LeftIndex || nFinger.Position == NFPosition.LeftMiddle || nFinger.Position == NFPosition.LeftRing || nFinger.Position == NFPosition.LeftLittle)
                                        {
                                            mano = "LEFT";
                                            encontrado = true;
                                        }

                            if (!mano.Equals("RIGHT") && !mano.Equals("LEFT"))
                                mano = "UNKNOWN";

                            DataRow dr = dsResultado.Tables["DETALLE"].NewRow();
                            dr["MANO"] = mano;
                            dr["POSICION"] = nFinger.Position;
                            dr["IMAGEN_PNG"] = dsHuellaFinal.Tables[0].Rows[0]["IMAGEN_PNG"];
                            dr["IMAGEN_PNG_STRING_BASE64"] = dsHuellaFinal.Tables[0].Rows[0]["IMAGEN_PNG_STRING_BASE64"].ToString();
                            dr["IMAGEN_WSQ_STRING_BASE64"] = dsHuellaFinal.Tables[0].Rows[0]["IMAGEN_WSQ_STRING_BASE64"].ToString();
                            dr["HIT"] = false;

                            dsResultado.Tables["DETALLE"].Rows.Add(dr);
                        }
                    }
                }

                int huellasMDer = 0;
                int huellasMIzq = 0;

                foreach (DataRow dr in dsResultado.Tables["DETALLE"].Rows)
                    if (dr["MANO"].Equals("RIGHT")) huellasMDer++; else if (dr["MANO"].Equals("LEFT")) huellasMIzq++;

                dsResultado.Tables["ENCABEZADO"].Rows[0]["HUELLAS_MDERECHA"] = huellasMDer;
                dsResultado.Tables["ENCABEZADO"].Rows[0]["HUELLAS_MIZQUIERDA"] = huellasMIzq;
                dsResultado.Tables["ENCABEZADO"].Rows[0]["HUELLAS_TOTAL"] = huellasMDer + huellasMIzq;

                dsResultado.Tables["RESULTADO"].Rows[0]["RESULTADO"] = true;
                dsResultado.Tables["RESULTADO"].Rows[0]["MSG_ERROR"] = string.Empty;
                dsResultado.Tables["RESULTADO"].Rows[0]["DATOS"] = null;
            }
            catch (Exception ex)
            {
                dsResultado.Tables["RESULTADO"].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables["RESULTADO"].Rows[0]["MSG_ERROR"] = "GenerarSubject442(). " + ex.Message;
                dsResultado.Tables["RESULTADO"].Rows[0]["DATOS"] = null;
            }
            return dsResultado;
        }

        public async Task<DataSet> HuellasIguales(NFinger fingerA, NFinger fingerB, NBiometricClient nBiometricClient)
        {

            DataSet ds = ArmarDsResultado();
            ds.Tables[0].Rows[0]["RESULTADO"] = false;

            try
            {
                NSubject _subjectA;
                NSubject _subjectB;

                //if (fingerA == null)
                //    fingerA = new NFinger();

                _subjectA = new NSubject();
                _subjectA.Fingers.Add(new NFinger { Image = fingerA.Image });

                _subjectB = new NSubject();
                _subjectB.Fingers.Add(new NFinger { Image = fingerB.Image });

                if (Properties.Settings.Default.MATCHING_MODE.Equals("SERVER"))
                {
                    WsBiometricsDGMSoapClient wsBiometricsDGM = new WsBiometricsDGMSoapClient(); ;

                    string huellaA = Convert.ToBase64String(fingerA.Image.Save(NImageFormat.Wsq).ToArray());
                    string huellaB = Convert.ToBase64String(fingerB.Image.Save(NImageFormat.Wsq).ToArray());
                    DataSet dsBiometria = wsBiometricsDGM.CompararDosHuellasStrBase64IMG_WSQ(huellaA, huellaB);

                    DataSet dsValidarDsBio = EsDsBiometriaValido(dsBiometria);
                    if (bool.Parse(dsValidarDsBio.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                        throw new Exception(dsValidarDsBio.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                    DataSet DsCoincidenciaAB = (DataSet)(dsBiometria.Tables[0].Rows[0]["METODO_RESULTADO"]);

                    dsValidarDsBio = EsDsCoincidenciaABValido(DsCoincidenciaAB);
                    if (bool.Parse(dsValidarDsBio.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                        throw new Exception(dsValidarDsBio.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                    if (bool.Parse(DsCoincidenciaAB.Tables[0].Rows[0]["COINCIDENCIA"].ToString()) == false)
                        throw new Exception("¡Las huellas son distintas!");

                }
                else if (Properties.Settings.Default.MATCHING_MODE.Equals("LOCAL"))
                {
                    if (nBiometricClient == null)
                    {
                        nBiometricClient = new NBiometricClient { UseDeviceManager = true, BiometricTypes = NBiometricType.Finger };
                        await nBiometricClient.InitializeAsync();
                    }

                    var status = await nBiometricClient.CreateTemplateAsync(_subjectA);

                    if (status != NBiometricStatus.Ok)
                        throw new Exception("Error al convertir huella DERECHA para comparación");

                    status = await nBiometricClient.CreateTemplateAsync(_subjectB);

                    if (status != NBiometricStatus.Ok)
                        throw new Exception("Error al convertir huella IZQUIERDA para comparación");

                    status = await nBiometricClient.VerifyAsync(_subjectA, _subjectB);
                    //status = _biometricFingerClient.Verify(_subjectA, _subjectB);

                    if (status != NBiometricStatus.Ok)
                        throw new Exception("¡Las huellas no coinciden!");
                }

                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
            }
            catch (Exception ex)
            {
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = "" + ex.Message;
            }

            return ds;
        }

        public async Task<DataSet> RostrosIguales(NFace faceA, NFace faceB, NBiometricClient nBiometricClient)
        {

            DataSet ds = ArmarDsResultado();
            ds.Tables[0].Rows[0]["RESULTADO"] = false;

            try
            {
                NSubject _subjectA;
                NSubject _subjectB;

                //if (fingerA == null)
                //    fingerA = new NFinger();

                _subjectA = new NSubject();
                _subjectA.Faces.Add(new NFace { Image = faceA.Image });

                _subjectB = new NSubject();
                _subjectB.Faces.Add(new NFace { Image = faceB.Image });

                if (Properties.Settings.Default.MATCHING_MODE.Equals("SERVER"))
                {
                    WsBiometricsDGMSoapClient wsBiometricsDGM = new WsBiometricsDGMSoapClient(); ;

                    string huellaA = Convert.ToBase64String(faceA.Image.Save(NImageFormat.Wsq).ToArray());
                    string huellaB = Convert.ToBase64String(faceB.Image.Save(NImageFormat.Wsq).ToArray());
                    DataSet dsBiometria = wsBiometricsDGM.CompararDosHuellasStrBase64IMG_WSQ(huellaA, huellaB);

                    DataSet dsValidarDsBio = EsDsBiometriaValido(dsBiometria);
                    if (bool.Parse(dsValidarDsBio.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                        throw new Exception(dsValidarDsBio.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                    DataSet DsCoincidenciaAB = (DataSet)(dsBiometria.Tables[0].Rows[0]["METODO_RESULTADO"]);

                    dsValidarDsBio = EsDsCoincidenciaABValido(DsCoincidenciaAB);
                    if (bool.Parse(dsValidarDsBio.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                        throw new Exception(dsValidarDsBio.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                    if (bool.Parse(DsCoincidenciaAB.Tables[0].Rows[0]["COINCIDENCIA"].ToString()) == false)
                        throw new Exception("¡Las huellas son distintas!");

                }
                else if (Properties.Settings.Default.MATCHING_MODE.Equals("LOCAL"))
                {
                    if (nBiometricClient == null)
                    {
                        nBiometricClient = new NBiometricClient { UseDeviceManager = true, BiometricTypes = NBiometricType.Finger };
                        await nBiometricClient.InitializeAsync();
                    }

                    var status = await nBiometricClient.CreateTemplateAsync(_subjectA);

                    if (status != NBiometricStatus.Ok)
                        throw new Exception("Error al convertir rostro A para comparación");

                    status = await nBiometricClient.CreateTemplateAsync(_subjectB);

                    if (status != NBiometricStatus.Ok)
                        throw new Exception("Error al convertir rostro B para comparación");

                    status = await nBiometricClient.VerifyAsync(_subjectA, _subjectB);
                    //status = _biometricFingerClient.Verify(_subjectA, _subjectB);

                    if (status != NBiometricStatus.Ok)
                        throw new Exception("¡Los rostros no coinciden!");
                }

                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
            }
            catch (Exception ex)
            {
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = "RostrosIguales(). " + ex.Message;
            }

            return ds;
        }

        public async Task<DataSet> SegmentarRostro(NFace faceA, NBiometricClient nBiometricClient)
        {

            DataSet ds = ArmarDsResultado();
            ds.Tables[0].Rows[0]["RESULTADO"] = false;

            try
            {
                NSubject _subjectA;
                NSubject _subjectB;

                //if (fingerA == null)
                //    fingerA = new NFinger();

                _subjectA = new NSubject();
                _subjectA.Faces.Add(new NFace { Image = faceA.Image });

                //_subjectB = new NSubject();
                //_subjectB.Faces.Add(new NFace { Image = faceB.Image });

                if (Properties.Settings.Default.MATCHING_MODE.Equals("SERVER"))
                {
                    WsBiometricsDGMSoapClient wsBiometricsDGM = new WsBiometricsDGMSoapClient(); ;

                    string huellaA = Convert.ToBase64String(faceA.Image.Save(NImageFormat.Wsq).ToArray());
                    string huellaB = "";// Convert.ToBase64String(faceB.Image.Save(NImageFormat.Wsq).ToArray());
                    DataSet dsBiometria = wsBiometricsDGM.CompararDosHuellasStrBase64IMG_WSQ(huellaA, huellaB);

                    DataSet dsValidarDsBio = EsDsBiometriaValido(dsBiometria);
                    if (bool.Parse(dsValidarDsBio.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                        throw new Exception(dsValidarDsBio.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                    DataSet DsCoincidenciaAB = (DataSet)(dsBiometria.Tables[0].Rows[0]["METODO_RESULTADO"]);

                    dsValidarDsBio = EsDsCoincidenciaABValido(DsCoincidenciaAB);
                    if (bool.Parse(dsValidarDsBio.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                        throw new Exception(dsValidarDsBio.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                    if (bool.Parse(DsCoincidenciaAB.Tables[0].Rows[0]["COINCIDENCIA"].ToString()) == false)
                        throw new Exception("¡Las huellas son distintas!");

                }
                else if (Properties.Settings.Default.MATCHING_MODE.Equals("LOCAL"))
                {
                    if (nBiometricClient == null)
                    {
                        nBiometricClient = new NBiometricClient { UseDeviceManager = true, BiometricTypes = NBiometricType.Finger };
                        await nBiometricClient.InitializeAsync();
                    }

                    var task = nBiometricClient.CreateTask(NBiometricOperations.Capture | NBiometricOperations.Segment | NBiometricOperations.CreateTemplate, _subjectA);

                    var performedTask = await nBiometricClient.PerformTaskAsync(task);

                    var status = await nBiometricClient.CreateTemplateAsync(_subjectA);

                    if (status != NBiometricStatus.Ok)
                        throw new Exception("Error al generar segmentación de rostro");
                }

                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["DATOS"] = _subjectA.Faces[1].Image.ToBitmap();
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
            }
            catch (Exception ex)
            {
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = ex.Message;
            }

            return ds;
        }

        public DataSet CitaControlFormulario(Control control, string valor, string nombreCampo)
        {
            DataSet ds = ArmarDsResultado();
            ds.Tables[0].Rows[0]["RESULTADO"] = false;

            try
            {
                if (control is ComboBox)
                {
                    ComboBox cmb = (ComboBox)control;

                    if (valor is null || valor.Equals(string.Empty) || valor.Equals(""))
                        throw new Exception("El campo " + nombreCampo + " viene vacío desde la cita, contacte al administrador del sistema.");

                    int index = cmb.FindStringExact(valor);
                    if (index >= 0)
                        cmb.SelectedIndex = index;
                    else
                    {
                        cmb.SelectedIndex = -1;
                        throw new Exception("El(La) " + nombreCampo + ": " + valor + " no existe en la aplicación de captura, contacte al adminstrador del sistema");
                    }
                }

                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
            }
            catch (Exception ex)
            {
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = "CitaControlFormulario(). " + ex.Message;
            }
            return ds;
        }

        public DataSet StringBase64ToImage(string stringBase64)
        {
            DataSet ds = ArmarDsResultado();
            ds.Tables[0].Rows[0]["RESULTADO"] = false;

            try
            {
                if (stringBase64 == null || stringBase64.Replace("data:image/png;base64,", "").Equals(string.Empty) || stringBase64.Replace("data:image/png;base64,", "").Equals(""))
                    throw new Exception("¡La consulta no devolvió fotografía!");

                // Convert Base64 String to byte[]
                byte[] imageBytes = Convert.FromBase64String(stringBase64.Replace("data:image/png;base64,", ""));
                MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);

                // Convert byte[] to Image
                ms.Write(imageBytes, 0, imageBytes.Length);
                Image image = Image.FromStream(ms, true);
                ms.Dispose();

                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                ds.Tables[0].Rows[0]["DATOS"] = image;
            }
            catch (Exception ex)
            {
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = "StringBase64ToImage(). " + ex.Message;
            }
            return ds;
        }

        public DataSet CalcularEdad(string fecha)
        {
            DataSet ds = ArmarDsResultado();
            ds.Tables[0].Rows[0]["RESULTADO"] = false;

            try
            {
                DateTime fechaNacimiento = DateTime.ParseExact(fecha, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                int edad = DateTime.Now.Year - fechaNacimiento.Year;

                //Obtengo la fecha de cumpleaños de este año.
                DateTime nacimientoAhora = fechaNacimiento.AddYears(edad);
                //Le resto un año si la fecha actual es anterior 
                //al día de nacimiento.
                if (DateTime.Now.CompareTo(nacimientoAhora) < 0)
                    edad--;

                if(edad < 0)
                {
                    String[] s = fecha.Split('/');
                    string temp = "19" + s[2].Substring(2);
                    fecha = s[0] + "/" + s[1] + "/" + temp;

                    fechaNacimiento = DateTime.ParseExact(fecha, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                    edad = DateTime.Now.Year - fechaNacimiento.Year;

                    nacimientoAhora = fechaNacimiento.AddYears(edad);
                    if (DateTime.Now.CompareTo(nacimientoAhora) < 0)
                        edad--;
                }

                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                ds.Tables[0].Rows[0]["DATOS"] = edad + "*" + fechaNacimiento;

            }
            catch (Exception ex)
            {
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = "CalcularEdad(). " + ex.Message;
            }
            return ds;
        }

        public DataSet EsMayorEdad(string fecha)
        {
            DataSet ds = ArmarDsResultado();
            ds.Tables[0].Rows[0]["RESULTADO"] = false;

            try
            {
                DateTime fechaNacimiento = DateTime.ParseExact(fecha, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                int edad = DateTime.Now.Year - fechaNacimiento.Year;

                //Obtengo la fecha de cumpleaños de este año.
                DateTime nacimientoAhora = fechaNacimiento.AddYears(edad);
                //Le resto un año si la fecha actual es anterior 
                //al día de nacimiento.
                if (DateTime.Now.CompareTo(nacimientoAhora) < 0)
                    edad--;
                
                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                ds.Tables[0].Rows[0]["DATOS"] = edad >= 18 ? true : false;

            }
            catch (Exception ex)
            {
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = "EsMayorEdad(). " + ex.Message;
            }
            return ds;
        }

        public DataSet ListarEscanersHuellas(ComboBox cmbEscanerHuellas, bool validarDisponible, bool filtrarEscaner, PARAMETRIZACION.TipoEscanerHuellas tipoEscaner, NDeviceManager nDeviceManager)
        {
            DataSet ds = ArmarDsResultado();
            try
            {
                DataTable dt = new DataTable("EscanerHuellas");
                dt.Columns.Add("ITEM", typeof(NFScanner));
                dt.Columns.Add("TIPO", typeof(PARAMETRIZACION.TipoEscanerHuellas));
                
                if (nDeviceManager == null)
                    throw new Exception("Problemas con el manejador de dispositivos de lectura de huellas (nDeviceManager es nulo). ");

                foreach (NDevice item in nDeviceManager.Devices)
                {
                    bool Escanner442 = false;
                    NFScanner nFScanner = (NFScanner)item;
                    foreach (NFPosition nFPosition in nFScanner.GetSupportedPositions())
                    {
                        if (!NBiometricTypes.IsPositionFourFingers(nFPosition)) continue;
                        Escanner442 = true;
                        break;
                    }

                    if(!filtrarEscaner)
                        dt.Rows.Add(new object[] { item, Escanner442 ? PARAMETRIZACION.TipoEscanerHuellas.Multidactilar : PARAMETRIZACION.TipoEscanerHuellas.Unidactilar });

                    else
                    {
                        if (tipoEscaner == PARAMETRIZACION.TipoEscanerHuellas.Multidactilar)
                            if (Escanner442)
                                dt.Rows.Add(new object[] { item, PARAMETRIZACION.TipoEscanerHuellas.Multidactilar });


                        if (tipoEscaner == PARAMETRIZACION.TipoEscanerHuellas.Unidactilar)
                            if (!Escanner442)
                                dt.Rows.Add(new object[] { item, PARAMETRIZACION.TipoEscanerHuellas.Unidactilar });
                    }
                }

                if (validarDisponible && dt.Rows.Count < 1)
                    throw new Exception("No se encontraron dispositivos de lectura de huellas disponibles. ");

                if(cmbEscanerHuellas != null)
                {
                    cmbEscanerHuellas.Items.Clear();
                    cmbEscanerHuellas.Enabled = cmbEscanerHuellas.Visible = true;

                    foreach (DataRow dr in dt.Rows)
                    {
                        NFScanner item = (NFScanner)dr["ITEM"];
                        cmbEscanerHuellas.Items.Add(item);
                    }

                    if (cmbEscanerHuellas.Items.Count == 1)
                    {
                        cmbEscanerHuellas.Enabled = false;
                        cmbEscanerHuellas.SelectedIndex = 0;
                    }
                }

                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;

                ds.Tables[0].Rows[0]["DATOS"] = dt;
            }
            catch (Exception ex)
            {
                //throw new Exception("ListarEscaners(). " + ex.Message);
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = "ListarEscanersHuellas(). " + ex.Message;
            }
            return ds;
        }

        public Image ImageFromBase64String(string imagenBase64String)
        {
            try
            {
                byte[] bytes = Convert.FromBase64String(imagenBase64String);

                Image image;
                //Convert byte[] into image
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    image = Image.FromStream(ms);
                }
                return image;
            }
            catch (Exception ex)
            {
                throw new Exception("ImageFromBase64String(). " + ex.Message);
            }
        }

        public Image ImageFromByteArray(byte[] bytes)
        {
            try
            {
                Image image;
                //Convert byte[] into image
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    image = Image.FromStream(ms);
                }
                return image;
            }
            catch (Exception ex)
            {
                throw new Exception("ImageFromByteArray(). " + ex.Message);
            }
        }

        public System.Drawing.Size DimensionPantalla(int anchoPantalla, int altoPantalla, PARAMETRIZACION.OrientancionPantalla orientacion)
        {
            System.Drawing.Size size;
            float relacionAspecto = float.Parse(anchoPantalla.ToString()) / float.Parse(altoPantalla.ToString());

            if (orientacion == PARAMETRIZACION.OrientancionPantalla.Horizontal)
            {
                int anchoVisor = Convert.ToInt16(altoPantalla * relacionAspecto);
                int altoVisor = altoPantalla - 20;
                size = new System.Drawing.Size(anchoVisor, altoVisor);
            }
            else
            {
                int anchoVisor = Convert.ToInt16(altoPantalla / relacionAspecto);
                int altoVisor = altoPantalla - 20;
                size = new System.Drawing.Size(altoVisor, anchoVisor);
            }

            return size;
        }

        public DataSet ProcesarMRZ(string sMrz)
        {
            DataSet dsResultado = ArmarDsResultado();
            dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;

            try
            {
                MRZ mrz = new MRZ();
                using (Desko.Mrz.MrzDescriptor desc = new Desko.Mrz.MrzDescriptor())
                {
                    desc.ParseMrz(sMrz);
                    foreach (Desko.Mrz.DocField v in Enum.GetValues(typeof(Desko.Mrz.DocField)))
                    {
                        PropertyInfo pInfo = mrz.GetType().GetProperty(v.ToString());
                        pInfo.SetValue(mrz, desc.GetField(v));
                    }
                }
                dsResultado.Tables[0].Rows[0]["DATOS"] = mrz;
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
            }
            catch (Exception ex)
            {                
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "ProcesarMRZ(). " + ex.Message;
            }
            return dsResultado;
        }

        public DataSet GetFotoPasaporte(string cui)
        {
            DataSet dsResultado = ArmarDsResultado();
            try
            {
                WsFotoPasaporte wsFotoPasaporte;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@"" + Settings.Default.IP_SERVICIOS_WEB + ":8080/ultimo_pasaporte_cui?cui=" + cui);

                var user = "migracion-pasaportes-enrollment-3.0";
                var password = "abc123";
                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{user}:{password}"));
                request.Headers.Add("Authorization", "Basic " + credentials);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    var json = reader.ReadToEnd();
                    wsFotoPasaporte = JsonConvert.DeserializeObject<WsFotoPasaporte>(json);
                }

                dsResultado.Tables[0].Rows[0]["DATOS"] = wsFotoPasaporte;
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;

            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "GetFotoPasaporte(). " + ex.Message;
            }

            return dsResultado;
        }

        public DataSet GetFotoPasaporteByLibreta(string noLibreta)
        {
            DataSet dsResultado = ArmarDsResultado();
            try
            {
                WsFotoPasaporte wsFotoPasaporte;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@"" + Settings.Default.IP_SERVICIOS_WEB + ":8080/ultimo_pasaporte_por_libreta?libreta=" + noLibreta);

                var user = "migracion-pasaportes-enrollment-3.0";
                var password = "abc123";
                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{user}:{password}"));
                request.Headers.Add("Authorization", "Basic " + credentials);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    var json = reader.ReadToEnd();
                    wsFotoPasaporte = JsonConvert.DeserializeObject<WsFotoPasaporte>(json);
                }

                dsResultado.Tables[0].Rows[0]["DATOS"] = wsFotoPasaporte;
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;

            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "GetFotoPasaporte(). " + ex.Message;
            }

            return dsResultado;
        }

        public DataSet GetConsultaPasaporteByLibreta(string noLibreta)
        {
            DataSet dsResultado = ArmarDsResultado();
            try
            {
                ConsultaPasaporte consultaPasaporte;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@"" + Settings.Default.CONSULTAR_CASO_PASAPORTE + "?libreta=" + noLibreta);

                var user = "migracion-pasaportes-enrollment-3.0";
                var password = "abc123";
                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{user}:{password}"));
                request.Headers.Add("Authorization", "Basic " + credentials);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    var json = reader.ReadToEnd();
                    consultaPasaporte = JsonConvert.DeserializeObject<ConsultaPasaporte>(json);
                }

                dsResultado.Tables[0].Rows[0]["DATOS"] = consultaPasaporte;
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;

            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "GetConsultaPasaporteByLibreta(). " + ex.Message;
            }

            return dsResultado;
        }

        public DataSet GetBiografiaByLibreta(string noLibreta, LoginData loginData)
        {
            DataSet dsResultado = ArmarDsResultado();
            try
            {
                ConsultaPasaporte consultaPasaporte;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@"" + Settings.Default.API_REST_MIROS + Settings.Default.API_PASAPORTE_BIOGRAFIA + "?numero_libreta=" + noLibreta);
                request.Headers.Add("Authorization", $"Bearer {loginData.JWT_TOKEN}");
            
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    var json = reader.ReadToEnd();
                    consultaPasaporte = JsonConvert.DeserializeObject<ConsultaPasaporte>(json);

                    if (consultaPasaporte.codigo != 200)
                        throw new Exception("Error al consultar PASAPORTE GUATEMALTECO (1): " + consultaPasaporte.mensaje);

                    if (consultaPasaporte.data.codigo != 200)
                        throw new Exception("Error al consultar PASAPORTE GUATEMALTECO (2): " + consultaPasaporte.data.mensaje);

                    dsResultado.Tables[0].Rows[0]["DATOS"] = consultaPasaporte;
                    dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
                }
            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "GetBiografiaByLibreta(). " + ex.Message;
            }

            return dsResultado;
        }

        public DataSet GetFotoByLibreta(string noLibreta, LoginData loginData)
        {
            DataSet dsResultado = ArmarDsResultado();
            try
            {
                ConsultaPasaporte consultaPasaporte;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@"" + Settings.Default.API_REST_MIROS + Settings.Default.API_PASAPORTE_FOTO + "?numero_libreta=" + noLibreta);
                request.Headers.Add("Authorization", $"Bearer {loginData.JWT_TOKEN}");

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    var json = reader.ReadToEnd();
                    consultaPasaporte = JsonConvert.DeserializeObject<ConsultaPasaporte>(json);
                }

                dsResultado.Tables[0].Rows[0]["DATOS"] = consultaPasaporte;
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;

            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "GetFotoByLibreta(). " + ex.Message;
            }

            return dsResultado;
        }

        public DataSet GetHuellasByLibreta(string noLibreta, LoginData loginData)
        {
            DataSet dsResultado = ArmarDsResultado();
            try
            {
                ConsultaPasaporte consultaPasaporte;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@"" + Settings.Default.API_REST_MIROS + Settings.Default.API_PASAPORTE_HUELLAS + "?numero_libreta=" + noLibreta);
                request.Headers.Add("Authorization", $"Bearer {loginData.JWT_TOKEN}");

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    var json = reader.ReadToEnd();
                    consultaPasaporte = JsonConvert.DeserializeObject<ConsultaPasaporte>(json);
                }

                dsResultado.Tables[0].Rows[0]["DATOS"] = consultaPasaporte;
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;

            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "GetHuellasByLibreta(). " + ex.Message;
            }

            return dsResultado;
        }

        public string IntToStringFinger(int valor)
        {
            switch (valor)
            {
                case 2: return "Indice derecho";
                case 7: return "Indice izquierdo";

                case 1: return "Pulgar derecho";
                case 6: return "Pulgar izquierdo";

                case 3: return "Medio derecho";
                case 8: return "Medio izquierdo";

                case 4: return "Anular derecho";
                case 9: return "Anular izquierdo";

                case 5: return "Meñique derecho";
                case 10: return "Meñique izquierdo";

                default: return "";
            }           
        }

        public DataSet ExisteCaso(string noCaso, string maquina, string usuario)
        {
            DataSet dsResultado = ArmarDsResultado();
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("RUTAXML", typeof(string));
                dt.Columns.Add("RUTAXML2", typeof(string));
                dt.Columns.Add("DIRECCION_ENCRYPT", typeof(string));
                dt.Columns.Add("DIRECCION_ENCRYPT2", typeof(string));                

                string rutaXML = Path.Combine(Application.StartupPath, "ENROL", "XMLs", "SftpData", "CasoPasaporte_" + noCaso.Trim() + ".xml");

                    if (File.Exists(rutaXML))
                        throw new Exception("El archivo ya existe");

                    string rutaXML2 = Path.Combine(Application.StartupPath, "ENROL", "XMLs", "SftpData", "Done", "CasoPasaporte_" + noCaso.Trim() + ".xml");

                    if (File.Exists(rutaXML2))
                        throw new Exception("El archivo ya existe (2)");

                    string direccionEncript = Path.Combine(Application.StartupPath, "ENROL", "XMLs") + "\\" + Path.GetFileNameWithoutExtension(rutaXML) + "_" + maquina + "_" + usuario + "_" + Properties.Settings.Default.SUFIJO_ENCRIPTACION + ".txt";

                    if (File.Exists(direccionEncript))
                        throw new Exception("El archivo ya existe (3)");

                    string direccionEncript2 = Path.Combine(Application.StartupPath, "ENROL", "XMLs", "Done") + "\\" + Path.GetFileNameWithoutExtension(rutaXML) + "_" + maquina + "_" + usuario + "_" + Properties.Settings.Default.SUFIJO_ENCRIPTACION + ".txt";

                    if (File.Exists(direccionEncript2))
                        throw new Exception("El archivo ya existe (4)");


                DataRow dr = dt.NewRow();
                dr["RUTAXML"] = rutaXML;
                dr["RUTAXML2"] = rutaXML2;
                dr["DIRECCION_ENCRYPT"] = direccionEncript;
                dr["DIRECCION_ENCRYPT2"] = direccionEncript2;
                dt.Rows.Add(dr);

                dsResultado.Tables[0].Rows[0]["DATOS"] = dt;
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;

            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "ExisteCaso(). " + ex.Message;
            }

            return dsResultado;
        }

        public DataSet ComprimirImagen(string inputFile, long compression)
        {
            DataSet dsResultado = ArmarDsResultado();
            try
            {
                Image image = Image.FromFile(inputFile);

                EncoderParameters eps = new EncoderParameters(1);
                eps.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, compression);


                string mimetype = GetMimeType(new System.IO.FileInfo(inputFile).Extension);
                ImageCodecInfo ici = GetEncoderInfo(mimetype);

                //try { if (File.Exists(ouputfile)) File.Delete(ouputfile); } catch (Exception) { }
                //image.Save(ouputfile, ici, eps);

                var ms = new MemoryStream();
                image.Save(ms, ici, eps);
                
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
                dsResultado.Tables[0].Rows[0]["DATOS"] = Image.FromStream(ms); ;
                
            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "ComprimirImagenFromFile(). " + ex.Message;
            }

            return dsResultado;
        }

        public DataSet ComprimirImagen(Image image, string extension, long compression)
        {
            DataSet dsResultado = ArmarDsResultado();
            try
            {
                EncoderParameters eps = new EncoderParameters(1);
                eps.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, compression);

                string mimetype = GetMimeType(extension);// new System.IO.FileInfo(inputFile).Extension);
                ImageCodecInfo ici = GetEncoderInfo(mimetype);

                //try { if (File.Exists(ouputfile)) File.Delete(ouputfile); } catch (Exception) { }
                //image.Save(ouputfile, ici, eps);

                var ms = new MemoryStream();
                image.Save(ms, ici, eps);

                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
                dsResultado.Tables[0].Rows[0]["DATOS"] = Image.FromStream(ms); ;

            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "ComprimirImagenFromFile(). " + ex.Message;
            }

            return dsResultado;
        }

        static string GetMimeType(string ext)
        {
            try
            {
                //    CodecName FilenameExtension FormatDescription MimeType 
                //    .BMP;*.DIB;*.RLE BMP ==> image/bmp 
                //    .JPG;*.JPEG;*.JPE;*.JFIF JPEG ==> image/jpeg 
                //    *.GIF GIF ==> image/gif 
                //    *.TIF;*.TIFF TIFF ==> image/tiff 
                //    *.PNG PNG ==> image/png 
                switch (ext.ToLower())
                {
                    case ".bmp":
                    case ".dib":
                    case ".rle":
                        return "image/bmp";

                    case ".jpg":
                    case ".jpeg":
                    case ".jpe":
                    case ".fif":
                        return "image/jpeg";

                    case "gif":
                        return "image/gif";
                    case ".tif":
                    case ".tiff":
                        return "image/tiff";
                    case ".png":
                        return "image/png";
                    default:
                        return "image/jpeg";
                }
            }
            catch (Exception ex)
            {
                throw new Exception("GetMimeType(). " + ex.Message);
            }
        }

        static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            try
            {
                ImageCodecInfo[] encoders;
                encoders = ImageCodecInfo.GetImageEncoders();

                ImageCodecInfo encoder = (from enc in encoders
                                          where enc.MimeType == mimeType
                                          select enc).First();
                return encoder;
            }
            catch (Exception ex)
            {
                throw new Exception("GetEncoderInfo(). " + ex.Message);
            }
        }
    }        
}
