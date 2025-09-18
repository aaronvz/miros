using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Threading;
using System.Drawing.Imaging;

using System.Management;

//COMPONENTES DEL SDK DE TRABAJO
using Neurotec.Biometrics;
using Neurotec.Biometrics.Client;
using Neurotec.Biometrics.Gui;
using Neurotec.Devices;
using Neurotec.Images;
using Neurotec.Samples;
using Neurotec.IO;

//COMPONENTES
using CapaEN;
using CapaLN;

using DGMReaderNS;
using EncriptarXML_LN;
using System.Runtime.InteropServices;

using System.Net;
using Newtonsoft.Json;

using System.Xml.Linq;

using Microsoft.Reporting.WinForms;

using System.Diagnostics;

using ENROLLMENT_V3.Properties;
using ENROLLMENT_V3.Reportes;

using UtilityCore.Cryptography;

using ENROLLMENT_V3.WsBiometricsDGM;

using System.Reflection;
using System.Globalization;

using Desko.FullPage;
using ENROLLMENT_V3.ControlesDesko;
using SampleNet;

using TwainLib;
using System.Text.RegularExpressions;
using System.Drawing.Printing;

using ZXing;
using ZXing.Common;
using ZXing.QrCode;

using READERDEMO;
using ENROLLMENT_V3.Models;

using AForge.Video;
using AForge.Video.DirectShow;
using System.Xml.Serialization;
using Newtonsoft.Json.Linq;

//Penta Scanner 4x
using Desko.IDA;
using static System.Net.Mime.MediaTypeNames;

namespace ENROLLMENT_V3
{
    public partial class FrmEnrolamiento : Form
    {
        private ISession _session;
        Runtime _runtime = new Runtime();
        private bool _documentPresent = false;
        private IDocument _document;
        bool _waitingForFlip = false;
        private IDeviceStateInfo _deviceStateInfo = null;
        Queue<IImage> _enqueuedImages = new Queue<IImage>();
        Queue<IField> _enqueuedFields = new Queue<IField>();
        Queue<IValidation> _enqueuedValidations = new Queue<IValidation>();

        private Dispositivo dspEscanerHuellas;
        private Dispositivo camara;
        private Guid idMovimiento;
        Bitmap Evf_Bmp;
        int xi, yi, LVBw, LVBh, w, h;
        float LVBratio, LVration;

        int ErrCount;
        object ErrLock = new object();
        object LvLock = new object();

        #region Declaración de variables para utilización del SDK

        //Administrador de dispositivos
        //HUELLAS
        private NDeviceManager _deviceFingerManager;
        //ROSTROS
        private NDeviceManager _deviceFaceManager;

        //Cliente biométrico
        //HUELLAS
        private NBiometricClient _biometricFingerClient;
        //ROSTRO
        private NBiometricClient _biometricFaceClient;
        //private NBiometricClient _biometricFaceClientIcao;

        private ManualResetEvent _isIdle = new ManualResetEvent(true);

        //Sujetos
        private NSubject _subjectFinger;
        //private NSubject _subjectFace;

        private NSubject _subject442 = new NSubject();
        DataSet dsSubject442 = new DataSet();

        //Dedo
        private NFinger _subjectFingerDerecho;
        private NFinger _subjectFingerIzquierdo;

        //Rostro
        //private NFace _nFace;
        private NFace _nFaceSegmented;

        #endregion

        DataSet dsVuelo;
        DataSet dsPais;
        DataSet dsTipoVisa;
        bool busquedaCuiManual = false;

        private FilterInfoCollection filterInfoCollection;
        private VideoCaptureDevice videoCaptureDevice;

        Movimiento movimiento;
        bool clicAgregarPersona = false;
        bool formularioLimpio = false;

        DeclaracionData declaracionData = null;

        public readonly RegulaReader _regulaReader;
        public event EventHandler OnDataReady;
        private volatile bool _isDataReady;
        ScanDataModel scanDataModel;

        string rutaCarpetaJson = System.Windows.Forms.Application.StartupPath + "\\ENROL\\json\\";
        private bool esVerificacionGuardarVisible = false;
        SolicitudLN pasaporteEntregaLN;

        DataSet dsCatalogos;

        Sede sedeEstacion;
        LoginData loginData;
        EquipoData equipoData;
        SedeData sedeDataEquipo;

        int INDICE_ESCANEO;

        int x, y;
        int ancho = 100;
        int alto = 137;
        string rptIdentidadCBS;
        System.Drawing.Image huellaCBS;
        private bool Escanner442 = false;

        private readonly NFPosition[] _slaps = new NFPosition[]
        {
            NFPosition.PlainLeftFourFingers,
            NFPosition.PlainRightFourFingers,
            NFPosition.PlainThumbs,
        };
 
        WsBiometricsDGMSoapClient wsBiometricsDGM;

        public FrmEnrolamiento(LoginData _loginData, EquipoData _equipoData, SedeData _sedeDataEquipo, NBiometricClient _nBiometricClientFinger, NBiometricClient _nBiometricClientFace)
        {

            _biometricFingerClient = _nBiometricClientFinger;
            _biometricFaceClient = _nBiometricClientFace;

            equipoData = _equipoData;
            sedeDataEquipo = _sedeDataEquipo;
            loginData = _loginData;

            dsPais = GetPais();
            if (bool.Parse(dsPais.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                throw new Exception(dsPais.Tables[0].Rows[0]["MSG_ERROR"].ToString());

            if (sedeDataEquipo.idtipodelegacion == 1)
            {
                dsVuelo = GetVuelos();
                if (bool.Parse(dsVuelo.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                    throw new Exception(dsVuelo.Tables[0].Rows[0]["MSG_ERROR"].ToString());
            }

            movimiento = new Movimiento();
            string connectionString = @"Data Source=10.200.1.223;Initial Catalog=PasaportesGuatemala;User ID=ws_entrega;Password=wsentrega2022";
            pasaporteEntregaLN = new SolicitudLN(connectionString);

            InitializeComponent();

            xi = yi = 0;
            LVBw = pbxVideo.Width;
            LVBh = pbxVideo.Height;

            if (Settings.Default.ESCANER_PASAPORTES == "Regula")
            {
                _regulaReader = new RegulaReader();
                SubscribeEvents();
            }
            //para la version
            lversion.Text = Settings.Default.VERSION;

            lblEscaneos.Text = "0/0";
            
            this.lbl_usuario.Text = loginData.USUARIO;
            this.lblNombreUsuario.Text = loginData.NOMBRES + " " + loginData.APELLIDOS;

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(loginData.PATHFOTO);
                System.Drawing.Image image;

                using (var stream = new MemoryStream(bytes, 0, bytes.Length))
                {
                    image = System.Drawing.Image.FromStream(stream);
                    picb_usuario.Image = image;
                }

                bytes = Convert.FromBase64String(loginData.HUELLA1PNG);
                var nfHuella = new NFinger { Image = NImage.FromMemory(bytes) };
                NSubject nsSujeto = new NSubject();
                nsSujeto.Fingers.Add(nfHuella);
                nFVDedoA.Finger = nfHuella;


                bytes = Convert.FromBase64String(loginData.HUELLA2PNG);
                nfHuella = new NFinger { Image = NImage.FromMemory(bytes) };
                nsSujeto = new NSubject();
                nsSujeto.Fingers.Add(nfHuella);
                nFVDedoB.Finger = nfHuella;
            } catch 
            {
            }


            //this.picb_usuario.Image;

            x = y = 10; 
            //configuracion = new ConfEnrollment();            
        }

        public DataSet ArmarDsVuelos()
        {
            return new DataSet();
        }

        public DataSet GetVuelos()
        {
            MovimientoDiagnostico mv = new MovimientoDiagnostico();
            DateTime fechaIni = DateTime.Now;
            DateTime fechaFin = DateTime.Now;

            string jsonString = "";
            string body = "";

            DataSet dsResultado = ArmarDsResultado();
            try
            {
                VueloRequest vueloRequest = new VueloRequest();
                vueloRequest.id = 0;
                vueloRequest.tipo = equipoData.nombre_tipo_flujo.Substring(0, 1);//Settings.Default.TIPO_MOVIMIENTO.Substring(0, 1); //"R";// 
                vueloRequest.codigodelegacion = sedeDataEquipo.clave;
                
                jsonString = JsonConvert.SerializeObject(vueloRequest);

                byte[] data = UTF8Encoding.UTF8.GetBytes(jsonString);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@"" + Settings.Default.API_REST_MIROS + Settings.Default.API_GET_VUELOS);
                request.Headers.Add("Authorization", $"Bearer {loginData.JWT_TOKEN}");
                request.Timeout = 10 * 1000;
                request.Method = "POST";
                request.ContentLength = data.Length;
                request.ContentType = "application/json; charset=utf-8";

                Stream postStream = request.GetRequestStream();
                postStream.Write(data, 0, data.Length);

                using (WebResponse response = request.GetResponse())
                {
                    using (Stream strReader = response.GetResponseStream())
                    {
                        fechaFin = DateTime.Now;
                        if (strReader == null)
                            throw new Exception("Respuesta nula desde el servidor. ");

                        using (StreamReader objReader = new StreamReader(strReader))
                        {
                            body = objReader.ReadToEnd();
                            VueloResponse vueloResponse = JsonConvert.DeserializeObject<VueloResponse>(body);

                            if (vueloResponse.codigo != 200)
                                throw new Exception("Error al guardar la entrega. Código: " + vueloResponse.codigo + ", Mensaje: " + vueloResponse.mensaje);

                            if (vueloResponse.data == null)
                                throw new Exception("Error de comunicación del API, intente guardar nuevamente y repórtelo al administrador");
                            //
                            string ruta = System.Windows.Forms.Application.StartupPath + @"\Catalogos\Vuelo.xml";

                            ListaVuelo lista = new ListaVuelo();
                            lista.Nodo = new List<NodoVuelo>();

                            foreach(VueloData vueloData in vueloResponse.data)
                            {
                                NodoVuelo nodo = new NodoVuelo();
                                nodo.idvuelo = vueloData.idvuelo;
                                nodo.idtransporte = vueloData.idtransporte;
                                nodo.idciudaddestino = vueloData.idciudaddestino;
                                nodo.idpaisorigen = vueloData.idpaisorigen;
                                nodo.idciudadorigen = vueloData.idciudadorigen;
                                nodo.nombrepaisorigen = vueloData.nombrepaisorigen;
                                nodo.icaopaisorigen = vueloData.icaopaisorigen;
                                nodo.icaopaisdestino = vueloData.icaopaisdestino;
                                nodo.idpaisdestino = vueloData.idpaisdestino;
                                nodo.nombrepaisdestino = vueloData.nombrepaisdestino;
                                nodo.descripcionvuelo = vueloData.descripcionvuelo;
                                nodo.transporte = vueloData.transporte;
                                lista.Nodo.Add(nodo);
                            }

                            var encoding = Encoding.GetEncoding("ISO-8859-1");
                            var serializer = new XmlSerializer(typeof(ListaVuelo));
                            using (var writer = new StreamWriter(ruta, false, encoding))
                            {
                                serializer.Serialize(writer, lista);
                            }

                            DataSet dsTemp = new DataSet();
                            dsTemp.ReadXml(ruta);

                            dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
                            dsResultado.Tables[0].Rows[0]["DATOS"] = dsTemp.Tables[0];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "GetVuelos(). " + ex.Message;
            }

            if(Settings.Default.MODO_DIAGNÓSTICO)
            {
                mv.id_movimiento = idMovimiento.ToString();
                mv.tipo_documento = string.Empty;
                mv.numero_documento = string.Empty;
                mv.nombre = string.Empty;
                mv.fecha_nacimiento = string.Empty;
                mv.comando = "GetVuelos()";
                mv.segundos = 0;
                mv.fecha_ini = fechaIni;
                mv.fecha_fin = fechaFin;
                mv.request = jsonString;
                mv.response = body;

                ReportesDB reporte = new ReportesDB();
                reporte.InsertarRegistro(mv);
            }

            return dsResultado;
        }

        public DataSet GetPais()
        {
            DataSet dsResultado = ArmarDsResultado();
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@"" + Settings.Default.API_REST_MIROS + Settings.Default.API_GET_PAIS);
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
                            PaisResponse paisResponse = JsonConvert.DeserializeObject<PaisResponse>(body);

                            if (paisResponse.codigo != 200)
                                throw new Exception("Error al guardar la entrega. Código: " + paisResponse.codigo + ", Mensaje: " + paisResponse.mensaje);

                            if (paisResponse.data == null)
                                throw new Exception("Error de comunicación del API, intente guardar nuevamente y repórtelo al administrador");

                            string ruta = System.Windows.Forms.Application.StartupPath + @"\Catalogos\Pais.xml";

                            ListaPais lista = new ListaPais();
                            lista.Nodo = new List<NodoPais>();

                            foreach (PaisData paisData in paisResponse.data)
                            {
                                NodoPais nodo = new NodoPais();
                                nodo.ICAO = paisData.icaopais;
                                nodo.Codigo = paisData.idpais;
                                nodo.Nombre = paisData.nombrepais;
                                nodo.idcategoriavisa = paisData.idcategoriavisa;
                                nodo.categoriavisa = paisData.categoriavisa;
                                nodo.acuerdopais = paisData.acuerdopais == null ? "-1" : paisData.acuerdopais;
                                lista.Nodo.Add(nodo);
                            }

                            var encoding = Encoding.GetEncoding("ISO-8859-1");
                            var serializer = new XmlSerializer(typeof(ListaPais));
                            using (var writer = new StreamWriter(ruta, false, encoding))
                            {
                                serializer.Serialize(writer, lista);
                            }

                            DataSet dsTemp = new DataSet();
                            dsTemp.ReadXml(ruta);

                            dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
                            dsResultado.Tables[0].Rows[0]["DATOS"] = dsTemp.Tables[0];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "GetPais(). " + ex.Message;
            }

            return dsResultado;
        }

        public DataSet GetTipoVisa()
        {
            DataSet dsResultado = ArmarDsResultado();
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@"" + Settings.Default.API_REST_MIROS + Settings.Default.API_GET_TIPO_VISA + "?idtipodocumento=2");
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
                            TipoVisaResponse tipoVisaResponse = JsonConvert.DeserializeObject<TipoVisaResponse>(body);

                            if (tipoVisaResponse.codigo != 200)
                                throw new Exception("Error al guardar la entrega. Código: " + tipoVisaResponse.codigo + ", Mensaje: " + tipoVisaResponse.mensaje);

                            if (tipoVisaResponse.data == null)
                                throw new Exception("Error de comunicación del API, intente guardar nuevamente y repórtelo al administrador");

                            string ruta = System.Windows.Forms.Application.StartupPath + @"\Catalogos\TipoVisa.xml";

                            ListaTipoVisa lista = new ListaTipoVisa();
                            lista.Nodo = new List<NodoTipoVisa>();

                            foreach (TipoVisaData tipoVisaData in tipoVisaResponse.data)
                            {
                                NodoTipoVisa nodo = new NodoTipoVisa();
                                nodo.idcategoriadocumento = tipoVisaData.idcategoriadocumento;
                                nodo.nombrecategoria = tipoVisaData.nombrecategoria;
                                lista.Nodo.Add(nodo);
                            }

                            var encoding = Encoding.GetEncoding("ISO-8859-1");
                            var serializer = new XmlSerializer(typeof(ListaTipoVisa));
                            using (var writer = new StreamWriter(ruta, false, encoding))
                            {
                                serializer.Serialize(writer, lista);
                            }

                            DataSet dsTemp = new DataSet();
                            dsTemp.ReadXml(ruta);

                            dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
                            dsResultado.Tables[0].Rows[0]["DATOS"] = dsTemp.Tables[0];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "GetTipoVisa(). " + ex.Message;
            }

            return dsResultado;
        }

        public void SubscribeEvents()
        {
            OnDataReady += rr_OnDataReady;
            _regulaReader.OnProcessingFinished += rr_OnProcessingFinished;
            _regulaReader.OnNotificationOptical += rr_OnNotificationOptical;
            _regulaReader.OnNotificationRFID += rr_OnNotificationRFID;
            _regulaReader.OnRFIDRequest += RegulaReaderOnRfidRequest;
            _regulaReader.OnResultReady += RegulaReaderOnResultReady;
        }

        public void UnsubscribeEvents()
        {
            OnDataReady -= rr_OnDataReady;
            _regulaReader.OnProcessingFinished -= rr_OnProcessingFinished;
            _regulaReader.OnNotificationOptical -= rr_OnNotificationOptical;
            _regulaReader.OnNotificationRFID -= rr_OnNotificationRFID;
            _regulaReader.OnRFIDRequest -= RegulaReaderOnRfidRequest;
            _regulaReader.OnResultReady -= RegulaReaderOnResultReady;
        }

        private void RegulaReaderOnResultReady(int aType)
        {
            switch ((eRPRM_ResultType)aType)
            {
                case eRPRM_ResultType.RPRM_ResultType_Graphics:
                    //start counting time
                    break;
                case eRPRM_ResultType.RPRM_ResultType_Authenticity:
                    //stop counting time
                    break;
            }
            // all other processing you have 
        }

        private void RegulaReaderOnRfidRequest(object requestXml)
        {
            var xml = new XmlDocument();
            xml.LoadXml(requestXml.ToString());

            var elements = xml.GetElementsByTagName("SDK_Request");
            if (elements.Count > 0)
            {
                var item = elements.Item(0);

                if (item != null)
                {
                    var aType = item.Attributes?.GetNamedItem("Type");

                    if (aType != null)
                    {
                        XmlCDataSection cdata;
                        XmlNode aCert;
                        XmlNode aNode;

                        switch (aType.InnerText)
                        {
                            case "PA_Resources":
                                // getting parameters from request to find appropriate certificates in external storage
                                var temp = xml.GetElementsByTagName("Issuer");
                                if (temp.Count > 0)
                                {
                                    var issuer = temp.Item(0)?.Value;
                                }

                                temp = xml.GetElementsByTagName("SerialNumber");
                                if (temp.Count > 0)
                                {
                                    var serialNumber = temp.Item(0)?.Value;
                                }

                                temp = xml.GetElementsByTagName("SubjectKeyIdentifier");
                                if (temp.Count > 0)
                                {
                                    var subjectKeyIdentifier = temp.Item(0)?.Value;
                                }

                                // here you should find some certificates
                                // ...

                                // adding found corresponding certificates to XML response
                                var certFile = GetFile("C:\\RFID\\PA\\CSCAcert.der");
                                if (certFile != null)
                                {
                                    aCert = xml.CreateNode(XmlNodeType.Element, "PA_Certificate", "");
                                    aNode = xml.CreateNode(XmlNodeType.Element, "Data", "");
                                    cdata = xml.CreateCDataSection(Convert.ToBase64String(certFile));
                                    aNode.AppendChild(cdata);
                                    aCert.AppendChild(aNode);
                                    if (xml.DocumentElement != null) xml.DocumentElement.AppendChild(aCert);
                                    certFile = null;
                                }

                                certFile = GetFile("C:\\RFID\\PA\\SODcert.der");
                                if (certFile != null)
                                {
                                    aCert = xml.CreateNode(XmlNodeType.Element, "PA_Certificate", "");
                                    aNode = xml.CreateNode(XmlNodeType.Element, "Data", "");
                                    cdata = xml.CreateCDataSection(Convert.ToBase64String(certFile));
                                    aNode.AppendChild(cdata);
                                    aCert.AppendChild(aNode);
                                    if (xml.DocumentElement != null) xml.DocumentElement.AppendChild(aCert);
                                }

                                _regulaReader.RFID_ResponseXML = xml.InnerXml;
                                break;
                            case "TA_Resources":
                                var carElements = xml.GetElementsByTagName("CAR");
                                if (carElements.Count > 0)
                                {
                                    // getting CAR parameter from request to find appropriate certificates in external storage 
                                    var car = carElements.Item(0)?.Value;

                                    // here you should find apropriate certificates by using provided CAR

                                    // adding found certificates and private keys if available to the response XML
                                    /*
                                             aCert = xml.CreateNode(XmlNodeType.Element, "TA_Certificate", "");
                 
                                             aNode = xml.CreateNode(XmlNodeType.Element, "Data", "");
                                             cdata = xml.CreateCDataSection(Convert.ToBase64String(getfile("C:\\RFID\\TA\\CV01.cvcert")));
                                             aNode.AppendChild(cdata);
                                             aCert.AppendChild(aNode);
                                             /*
                                             aNode = xml.CreateNode(XmlNodeType.Element, "PrivateKey", "");
                                             cdata = xml.CreateCDataSection(Convert.ToBase64String(getfile("C:\\RFID\\TA\\CV01.pkcs8")));
                                             aNode.AppendChild(cdata);
                                             aCert.AppendChild(aNode);
                                             
                                             xml.DocumentElement.AppendChild(aCert);
                                             */
                                    var dv01Cert = GetFile("C:\\RFID\\TA\\DV01.cvcert");
                                    if (dv01Cert != null)
                                    {
                                        aCert = xml.CreateNode(XmlNodeType.Element, "TA_Certificate", "");

                                        aNode = xml.CreateNode(XmlNodeType.Element, "Data", "");
                                        cdata = xml.CreateCDataSection(Convert.ToBase64String(dv01Cert));
                                        aNode.AppendChild(cdata);
                                        aCert.AppendChild(aNode);
                                        /*
                                        aNode = xml.CreateNode(XmlNodeType.Element, "PrivateKey", "");
                                        cdata = xml.CreateCDataSection(Convert.ToBase64String(getfile("C:\\RFID\\TA\\DV01.pkcs8")));
                                        aNode.AppendChild(cdata);
                                        aCert.AppendChild(aNode);
                                        */
                                        if (xml.DocumentElement != null) xml.DocumentElement.AppendChild(aCert);
                                    }

                                    var is01Cert = GetFile("C:\\RFID\\TA\\IS01.cvcert");
                                    if (is01Cert != null)
                                    {
                                        aCert = xml.CreateNode(XmlNodeType.Element, "TA_Certificate", "");

                                        aNode = xml.CreateNode(XmlNodeType.Element, "Data", "");
                                        cdata = xml.CreateCDataSection(Convert.ToBase64String(is01Cert));
                                        aNode.AppendChild(cdata);
                                        aCert.AppendChild(aNode);
                                        /*   
                                        aNode = xml.CreateNode(XmlNodeType.Element, "PrivateKey", "");
                                        cdata = xml.CreateCDataSection(Convert.ToBase64String(getfile("C:\\RFID\\TA\\IS01.pkcs8")));
                                        aNode.AppendChild(cdata);
                                        aCert.AppendChild(aNode);
                                        */
                                        if (xml.DocumentElement != null) xml.DocumentElement.AppendChild(aCert);
                                    }

                                    _regulaReader.RFID_ResponseXML = xml.InnerXml;
                                }

                                break;
                            case "TA_Signature":
                                var challengeElements = xml.GetElementsByTagName("Challenge");
                                var hashElements = xml.GetElementsByTagName("HashValue");
                                if ((challengeElements.Count > 0) || (hashElements.Count > 0))
                                {
                                    // getting Challenge parameter from request to sign it on the external web-service 
                                    if (challengeElements.Count > 0)
                                    {
                                        var challenge = challengeElements.Item(0)?.Value;
                                    }

                                    // getting Hash parameter from request to sign it on the external web-service 
                                    if (hashElements.Count > 0)
                                    {
                                        var hashValue = hashElements.Item(0)?.Value;
                                    }

                                    // here you should sign the challenge or its hash provided on the external web-service and return a signature
                                    var signature = new MemoryStream();

                                    //adding signature to the response XML
                                    aCert = xml.CreateNode(XmlNodeType.Element, "TA_Signature", "");

                                    aNode = xml.CreateNode(XmlNodeType.Element, "Data", "");
                                    cdata = xml.CreateCDataSection(Convert.ToBase64String(signature.ToArray()));
                                    aNode.AppendChild(cdata);
                                    aCert.AppendChild(aNode);

                                    if (xml.DocumentElement != null) xml.DocumentElement.AppendChild(aCert);
                                }

                                break;
                        }
                    }
                }
            }
        }

        private static void rr_OnNotificationRFID(int aCode, int aValue)
        {
            switch (aCode)
            {
                case (int)eRFID_NotificationCodes.RFID_Notification_Progress:
                    // TSProgressBar.Value = AValue;
                    break;
                default:
                    break;
            }
        }

        private static void rr_OnNotificationOptical(int aCode, int aValue)
        {
            switch (aCode)
            {
                case (int)eRPRM_NotificationCodes.RPRM_Notification_DocumentReady:
                    switch (aValue)
                    {
                        case 0:
                            // "Document is not ready";
                            break;
                        case 1:
                            // "Document is ready";
                            break;
                    }
                    break;
            }
        }

        private void rr_OnProcessingFinished()
        {
            if (OnDataReady != null) OnDataReady(null, EventArgs.Empty);

            _isDataReady = true;
        }

        private void rr_OnDataReady(object sender, EventArgs e)
        {
            new Thread(ReadDataThread).Start();
        }

        private void ReadDataThread()
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)ReadData);
            }
            else ReadData();
        }

        private static byte[] GetFile(string fName)
        {
            if (File.Exists(fName))
            {
                var f = new FileStream(fName, FileMode.Open, FileAccess.Read);
                var ba = new byte[f.Length];
                f.Read(ba, 0, (int)f.Length);
                return ba;
            }
            else
                return null;
        }

        private void ClearResults()
        {
            //ImagesGrid.Rows.Clear();
            //ImagesGrid.Rows.Clear();
            //AnalyzeGrid.Rows.Clear();
            //GraphicFieldsGrid.Rows.Clear();
            //AuthenticityGrid.Rows.Clear();
        }

        public void ReadData()
        {
            lblEscaneos.Text = (int.Parse(lblEscaneos.Text.Split('/')[0].Trim()) + 1).ToString() + "/" + lblEscaneos.Text.Split('/')[1].Trim();

            this.btnGuardar.Enabled = false;
            Bitmap bmp;
            XmlElement item;
            XmlNodeList noList;

            var o = new XmlDocument();

            ClearResults(); // cleaning previous results if any

            // Getting page images
            List<Bitmap> lstImageVisible = new List<Bitmap>();
            List<Bitmap> lstImageUV = new List<Bitmap>();

            int total = _regulaReader.IsReaderResultTypeAvailable((int)(eRPRM_ResultType.RPRM_ResultType_RawImage));
            if (total > 0)
            {

                for (var i = (total - 1); i >= 0; i--)
                {
                    var str = new MemoryStream(_regulaReader.GetReaderFileImage(i));
                    bmp = new Bitmap(str);

                    var SigBase64 = Convert.ToBase64String(str.ToArray()); // Get Base64

                    if (eRPRM_Lights.GetName(typeof(eRPRM_Lights), _regulaReader.CheckReaderImageLight(i)).Equals("RPRM_Light_White_Full"))
                        lstImageVisible.Add(bmp);

                    if (eRPRM_Lights.GetName(typeof(eRPRM_Lights), _regulaReader.CheckReaderImageLight(i)).Equals("RPRM_Light_UV"))
                        lstImageUV.Add(bmp);
                }
            }

            if (lstImageVisible.ToArray().Length >= 1)
                scanDataModel.Anverso = lstImageVisible.ToArray()[0];

            if (lstImageVisible.ToArray().Length >= 2)
                scanDataModel.Reverso = lstImageVisible.ToArray()[1];

            if (lstImageUV.ToArray().Length >= 1)
                scanDataModel.AnversoUV = lstImageUV.ToArray()[0];

            if (lstImageUV.ToArray().Length >= 2)
                scanDataModel.ReversoUV = lstImageUV.ToArray()[1];

            // Getting text fields
            total = _regulaReader.IsReaderResultTypeAvailable((int)eRPRM_ResultType.RPRM_ResultType_OCRLexicalAnalyze);

            if (total > 0)
            {
                o.LoadXml(_regulaReader.CheckReaderResultXML((int)eRPRM_ResultType.RPRM_ResultType_OCRLexicalAnalyze, 0, 0));

                noList = o.GetElementsByTagName("Document_Field_Analysis_Info");

                for (int i = 0; i < noList.Count; i++)
                {
                    item = (XmlElement)noList.Item(i);
                    int fieldType = Convert.ToInt32(item.GetElementsByTagName("Type").Item(0).InnerText);

                    // this is text field LCID value. For Latin text is equal to 0
                    int lcid = Convert.ToInt32(item.GetElementsByTagName("LCID").Item(0)?.InnerText);
                    string caption = eVisualFieldType.GetName(typeof(eVisualFieldType), fieldType);

                    if (lcid > 0)
                        caption += string.Format("({0})", lcid);

                    if (caption.Equals("ft_MRZ_Strings"))
                        scanDataModel.MrzString = ((XmlElement)noList.Item(i)).GetElementsByTagName("Field_MRZ").Item(0)?.InnerText;
                }
            }
            ProcessScan(scanDataModel);
        }

        protected override bool ProcessCmdKey(ref Message msg, System.Windows.Forms.Keys keyData)
        {
            if (!this.Enabled)
                return false;

            if(!chkMRZ.Checked)// && !txtObservaciones.Focused)
            {
                if (keyData == System.Windows.Forms.Keys.N)
                this.btnNuevo_MouseClick(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));

                if (keyData == System.Windows.Forms.Keys.C)
                    this.pbxCONS_MouseClick(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));

                if (keyData == System.Windows.Forms.Keys.C)
                    this.pbxCONS_MouseClick(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));

                if (keyData == System.Windows.Forms.Keys.Enter)
                    if (btnGuardar.Visible)
                        if(busquedaCuiManual == false)
                            this.btnGuardar_MouseClick(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));

                if (keyData == System.Windows.Forms.Keys.Space)
                {
                    this.btnCapturarRostro_MouseClick(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));
                    btnCapturarRostro.Select();
                }

                if (keyData == System.Windows.Forms.Keys.Add)
                    this.btnAgregar_MouseClick(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));
            }
            else//if(chkMRZ.Checked)
            {
                if (keyData == (System.Windows.Forms.Keys.N | System.Windows.Forms.Keys.Control))
                    this.btnNuevo_MouseClick(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));

                if (keyData == (System.Windows.Forms.Keys.C | System.Windows.Forms.Keys.Control))
                    this.pbxCONS_MouseClick(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));

                if (keyData == (System.Windows.Forms.Keys.C | System.Windows.Forms.Keys.Control))
                    this.pbxCONS_MouseClick(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));

                if (keyData == (System.Windows.Forms.Keys.Enter | System.Windows.Forms.Keys.Control))
                    if (btnGuardar.Visible)
                        this.btnGuardar_MouseClick(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));

                if (keyData == (System.Windows.Forms.Keys.Space | System.Windows.Forms.Keys.Control))
                {
                    this.btnCapturarRostro_MouseClick(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));
                    btnCapturarRostro.Select();
                }

                if (keyData == (System.Windows.Forms.Keys.B | System.Windows.Forms.Keys.Control)){
                    this.txtNoDocumento_KeyDown(new object(), new KeyEventArgs(System.Windows.Forms.Keys.Multiply));
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        #region API Events        

        public void AsignarFotoRecortada(System.Drawing.Image imagen)
        {
            try
            {
                Bitmap bmp = (Bitmap)(imagen);

                NFace face = new NFace
                {
                    Image = NImage.FromBitmap(bmp),
                    CaptureOptions = NBiometricCaptureOptions.Stream
                };

                NSubject _newSubject = new NSubject();
                _newSubject.Faces.Add(face);
                //faceView2.Face = _newSubject.Faces.First();
                pbxRostroIcao.Image = _newSubject.Faces.First().Image.ToBitmap();
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "AsignarFotoRecortada(). " + ex.Message;
                MessageBox.Show("AsignarFotoRecortada(). " + ex.Message);
            }
        }

        #endregion       

        int tamanio;
        string[] array;
        List<string> listaProbatorios = new List<string>();

        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hwnd, int wmsg, int wparam, int lparam);

        private void picb_cerrar_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void panel_superior_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void lblFecha_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }



        private void pic_logo_dgm_MouseHover(object sender, EventArgs e)
        {

        }

        private void pic_logo_dgm_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void pic_txt_dgm_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        DialogResult salir;
        private void btn_cerrar_info_Click(object sender, EventArgs e)
        {
            cerrarAplicacion();
        }
        private PARAMETRIZACION parametrizacion;
        private FUNCIONES funciones;

        private DPI dpiTitular;
        private DPI dpiPadre;
        private DPI dpiMadre;

        private System.Drawing.Image fotoDPI;

        int intentosMOCTitular;
        int intentosMOCPadre;
        int intentosMOCMadre;

        VisorAlertas visorAlertas;
        VisorAlertas visorAlertasPadre;
        VisorAlertas visorAlertasMadre;



        #region Métodos privados

        private void ActivarControlesRostro(bool capturing)
        {
            btnCapturarRostro.Enabled = capturing;
            btnActualizarCamaras.Enabled = !capturing;

            CameraListBox.Enabled = true;

            if (capturing)
            {
                if (CameraListBox.Items.Count > 1)
                    CameraListBox.Enabled = !capturing;
                else
                    CameraListBox.Enabled = false;
            }


        }

        private void EnrollCompleto(NBiometricTask task, NSubject nSubject, NFinger nFinger, Label lblCalidad)
        {
            NBiometricStatus status = task.Status;

            if (status == NBiometricStatus.Canceled)
                return;

            if (status == NBiometricStatus.Ok)
                lblCalidad.Text = String.Format("Calidad: {0}", nFinger.Objects[0].Quality);
            else
            {
                MessageBox.Show(string.Format("La plantilla no fue extraída: {0}.", status), Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                nSubject = null;
                nFinger = null;
            }
        }

        private void ListarEscanersHuellas(ComboBox cmbEscanerHuellas, bool validarDisponible, bool filtrarEscaner, PARAMETRIZACION.TipoEscanerHuellas tipoEscaner)
        {
            try
            {
                cmbEscanerHuellas.Items.Clear();
                cmbEscanerHuellas.Enabled = cmbEscanerHuellas.Visible = true;

                //INICIANDO MANEJADOR DE DISPOSITIVOS, DE CAPTURA DE HUELLAS
                _deviceFingerManager = _biometricFingerClient.DeviceManager;

                if (_deviceFingerManager == null)
                    throw new Exception("Problemas con el manejador de dispositivos de lectura de huellas. ");

                foreach (NDevice item in _deviceFingerManager.Devices)
                {
                    Escanner442 = false;
                    if (filtrarEscaner)
                    {
                        NFScanner nFScanner = (NFScanner)item;
                        foreach (NFPosition nFPosition in nFScanner.GetSupportedPositions())
                        {
                            if (!NBiometricTypes.IsPositionFourFingers(nFPosition)) continue;
                            Escanner442 = true;
                            break;
                        }

                        if (tipoEscaner == PARAMETRIZACION.TipoEscanerHuellas.Multidactilar)
                        {
                            if (Escanner442)
                                cmbEscanerHuellas.Items.Add(item);
                        }
                        else if(tipoEscaner == PARAMETRIZACION.TipoEscanerHuellas.Unidactilar)
                        {
                            if (!Escanner442)
                                cmbEscanerHuellas.Items.Add(item);
                        }
                        
                    }
                    else
                        cmbEscanerHuellas.Items.Add(item);
                }                                    

                if (cmbEscanerHuellas.Items.Count == 1)
                {
                    cmbEscanerHuellas.Enabled = /*cmbEscanerHuellas.Visible =*/ false;
                    cmbEscanerHuellas.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("ListarEscaners(). " + ex.Message);
            }
        }

        private void ListarCamaras(ComboBox cmbCamaras, bool validarDisponible)
        {
            try
            {
                cmbCamaras.Items.Clear();
                cmbCamaras.Enabled = /*cmbCamaras.Visible =*/ true;

                //INICIANDO MANEJADOR DE DISPOSITIVOS, DE CAPTURA DE ROSTROS
                _deviceFaceManager = _biometricFaceClient.DeviceManager;


                if (_deviceFaceManager == null)
                    throw new Exception("Problemas con el manejador de dispositivos de captura de rostros. ");

                foreach (NDevice item in _deviceFaceManager.Devices)
                {
                    if (!Settings.Default.FILTRAR_CAMARA)
                        cmbCamaras.Items.Add(item);

                    else if (!Settings.Default.FILTRO_CAMARA.Equals(string.Empty))
                        if(item.DisplayName.Trim().ToUpper().Equals(Settings.Default.FILTRO_CAMARA.Trim().ToUpper()))
                            cmbCamaras.Items.Add(item);
                }

                if (cmbCamaras.Items.Count == 0 && validarDisponible)
                    throw new Exception("No se encontraron dispositivos de lectura de disponibles. FILTRAR_CAMARA: " + Settings.Default.FILTRAR_CAMARA + ", FILTRO_CAMARA: " + (Settings.Default.FILTRO_CAMARA.Trim().Equals(string.Empty) ? "Sin asignar" : Settings.Default.FILTRO_CAMARA.Trim()) + ".");

                if (cmbCamaras.Items.Count == 1)
                {
                    cmbCamaras.Enabled = /*cmbEscanerHuellas.Visible =*/ false;
                    cmbCamaras.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("ListarCamaras(). " + ex.Message);
            }
        }

        #endregion        

        private void enrollment_Load(object sender, EventArgs e)
        {
            try
            {
                CheckForIllegalCrossThreadCalls = false;

                if(Settings.Default.ESCANER_PASAPORTES == "Desko")
                {
                    Api.OnDevicePlugged += updatePlugState;
                    Api.OnDocumentPresent += onDocPresented;
                    Api.OnDocumentRemove += onDocRemoved;
                }

                funciones = new FUNCIONES();
                funciones.CargarLogo(pic_txt_dgm);

                wsBiometricsDGM = new WsBiometricsDGMSoapClient();
                lversion.Text = FUNCIONES.GetVersion();

                this.Enabled = false;

                string sedeEquipo = Properties.Settings.Default.SEDE;

                DataSet dsSede = CargarSede(sedeEquipo);

                if (bool.Parse(dsSede.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                    throw new Exception(dsSede.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                sedeEstacion = (Sede)(dsSede.Tables[0].Rows[0]["DATOS"]);

                //DatosLicencia();
                //DatosEquipo();

                this.lblNuevo.Text = "-1";
                this.btnNuevo_MouseClick(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));
                lblFecha.Text = DateTime.Now.ToLongDateString() + " - ";

                this.Enabled = true;

                NativeMethods.CHANGEFILTERSTRUCT changeFilter = new NativeMethods.CHANGEFILTERSTRUCT();
                changeFilter.size = (uint)Marshal.SizeOf(changeFilter);
                changeFilter.info = 0;
                if (!NativeMethods.ChangeWindowMessageFilterEx(this.Handle, NativeMethods.WM_COPYDATA, NativeMethods.ChangeWindowMessageFilterExAction.Allow, ref changeFilter))
                {
                    int error = Marshal.GetLastWin32Error();
                    MessageBox.Show(String.Format("The error {0} occured.", error));
                }

            }
            catch (Exception ex)
            {
                txtMensaje.Text = "enrollment_Load(). " + ex.Message;
                MessageBox.Show("enrollment_Load(). " + ex.Message);
            }
        }

        private DataSet CargarSede(string NombreSede)
        {
            DataSet ds = ArmarDsResultado();
            try
            {
                XDocument listaNodos = XDocument.Load(System.Windows.Forms.Application.StartupPath + "\\Catalogos\\Sedes.xml", LoadOptions.None);
                XElement lista = listaNodos.Element("Sedes");

                XElement xeSede;

                try
                {
                    xeSede = lista.Elements().Single(p => p.Element("Nombre").Value == NombreSede);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al leer la sede con nombre: " + NombreSede + ". Excepción: " + ex.Message);
                }

                listaNodos = XDocument.Load(System.Windows.Forms.Application.StartupPath + "\\Catalogos\\Pais.xml", LoadOptions.None);
                lista = listaNodos.Element("Lista");

                string Pais = xeSede.Element("Pais").Value;
                XElement xeCodigoPais;

                try
                {
                    xeCodigoPais = lista.Elements().Single(p => p.Element("Nombre").Value == Pais);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al leer el código de país con nombre: " + Pais + ". Excepción: " + ex.Message);
                }

                Sede sede = new Sede();
                sede.CODIGO_PAIS = xeCodigoPais.Element("Codigo").Value;
                sede.PAIS = xeSede.Element("Pais").Value;
                sede.MISION = xeSede.Element("Mision").Value;
                sede.CIUDAD = xeSede.Element("Ciudad").Value;
                sede.NOMBRE = xeSede.Element("Nombre").Value;

                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                ds.Tables[0].Rows[0]["DATOS"] = sede;
            }
            catch (Exception ex)
            {
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = "CargarSede(). " + ex.Message;
                ds.Tables[0].Rows[0]["DATOS"] = new object();
            }
            return ds;
        }

        private async Task<int> Iniciar_Cliente_Huellas(object sender, EventArgs e)
        {
            try
            {
                //INICIANDO EL CLIENTE DE BIOMETRÍA PARA DISPOSITIVOS DE CAPTURA DE HUELLAS
                //if (_biometricFingerClient != null && _biometricFingerClient.CurrentBiometric != null) _biometricFingerClient.Cancel();

                if(_biometricFingerClient == null)
                {
                    _biometricFingerClient = new NBiometricClient { UseDeviceManager = true, BiometricTypes = NBiometricType.Finger };
                    await _biometricFingerClient.InitializeAsync();
                }
                    
                return 0;
            }
            catch (Exception ex)
            {
                //UtilsLectorHuellas.GetException(ex);
                throw new Exception("Iniciar_Lector_Huellas(). " + ex.Message);
            }
        }

        private async Task<int> Iniciar_Cliente_Fotos(object sender, EventArgs e)
        {
            try
            {
                if(_biometricFaceClient == null)
                {
                    //INICIANDO EL CLIENTE DE BIOMETRÍA PARA DISPOSITIVOS DE CAPTURA DE HUELLAS
                    //if (_biometricFaceClient != null && _biometricFaceClient.CurrentBiometric != null) _biometricFaceClient.Cancel();

                    if (_biometricFaceClient == null)
                    {
                        _biometricFaceClient = new NBiometricClient { UseDeviceManager = true, BiometricTypes = NBiometricType.Face };
                        await _biometricFaceClient.InitializeAsync();
                    }
                }

                //if(_biometricFaceClientIcao == null)
                //{
                //    if (_biometricFaceClientIcao != null && _biometricFaceClientIcao.CurrentBiometric != null) _biometricFaceClientIcao.Cancel();
                    
                //    if(_biometricFaceClientIcao == null)
                //    {
                //        _biometricFaceClientIcao = new NBiometricClient { BiometricTypes = NBiometricType.Face };
                //        await _biometricFaceClientIcao.InitializeAsync();
                //    }
                //}
                
                return 0;
            }
            catch (Exception ex)
            {
                //UtilsLectorHuellas.GetException(ex);
                throw new Exception("Iniciar_Cliente_Fotos(). " + ex.Message);
            }
        }

        private void CargarComboBoxICAO(ComboBox cmb, bool SeleccionarIndice, string nombre)
        {
            try
            {
                cmb.SelectedIndex = -1;
                cmb.DataSource = null;
                cmb.DisplayMember = null;
                cmb.ValueMember = null;
                cmb.Items.Clear();

                string ruta = System.Windows.Forms.Application.StartupPath + "\\Catalogos\\" + nombre + ".xml";
                DataSet ds = LeerXmlCatalogos(ruta, "Lista", "Nodo", "ICAO", "Nombre");
                
                cmb.DisplayMember = "VALOR";
                cmb.ValueMember = "CODIGO";
                cmb.DataSource = ds.Tables[0];

                if (cmb.Items.Count > 0 && SeleccionarIndice)
                    cmb.SelectedIndex = 0;
                else
                    cmb.SelectedIndex = -1;

                DataSet dsTemp = new DataSet();
                dsTemp.ReadXml(ruta);
                dsTemp.Tables[0].TableName = nombre;

                if(dsCatalogos.Tables.Contains(nombre)) dsCatalogos.Tables.Remove(nombre);
                dsCatalogos.Tables.Add(dsTemp.Tables[0].Copy());
            }
            catch (Exception ex)
            {
                throw new Exception("CargarComboBoxICAO(" + nombre + "). " + ex.Message);
            }
        }

        private void CargarComboBox(ComboBox cmb, bool SeleccionarIndice, string nombre)
        {
            try
            {
                cmb.SelectedIndex = -1;
                cmb.DataSource = null;
                cmb.DisplayMember = null;
                cmb.ValueMember = null;
                cmb.Items.Clear();

                string ruta = System.Windows.Forms.Application.StartupPath + "\\Catalogos\\" + nombre + ".xml";
                DataSet ds = LeerXmlCatalogos(ruta, "Lista", "Nodo", "Codigo", "Nombre");

                cmb.DisplayMember = "VALOR";
                cmb.ValueMember = "CODIGO";
                cmb.DataSource = ds.Tables[0];

                if (cmb.Items.Count > 0 && SeleccionarIndice)
                    cmb.SelectedIndex = 0;
                else
                    cmb.SelectedIndex = -1;

                DataSet dsTemp = new DataSet();
                dsTemp.ReadXml(ruta);
                dsTemp.Tables[0].TableName = nombre;

                if (dsCatalogos.Tables.Contains(nombre)) dsCatalogos.Tables.Remove(nombre);
                dsCatalogos.Tables.Add(dsTemp.Tables[0].Copy());
            }
            catch (Exception ex)
            {
                throw new Exception("CargarComboBox(" + nombre + "). " + ex.Message);
            }
        }

        private void CargarComboBoxJson(ComboBox cmb, bool SeleccionarIndice, string nombre)
        {
            try
            {
                cmb.SelectedIndex = -1;
                cmb.DataSource = null;
                cmb.DisplayMember = null;
                cmb.ValueMember = null;
                cmb.Items.Clear();

                string ruta = System.Windows.Forms.Application.StartupPath + "\\Catalogos\\" + nombre + ".json";
                string jsonString = File.ReadAllText(System.Windows.Forms.Application.StartupPath + "\\Catalogos\\" + nombre + ".json");
                DataSet ds = JsonConvert.DeserializeObject<DataSet>(jsonString);
                cmb.DisplayMember = "VALOR";
                cmb.ValueMember = "CODIGO";
                cmb.DataSource = ds.Tables["data"];

                if (cmb.Items.Count > 0 && SeleccionarIndice)
                    cmb.SelectedIndex = 0;
                else
                    cmb.SelectedIndex = -1;

                DataSet dsTemp = JsonConvert.DeserializeObject<DataSet>(jsonString);
                dsTemp.Tables[0].TableName = nombre;

                if (dsCatalogos.Tables.Contains(nombre)) dsCatalogos.Tables.Remove(nombre);
                dsCatalogos.Tables.Add(dsTemp.Tables[0].Copy());
            }
            catch (Exception ex)
            {
                throw new Exception("CargarComboBox(" + nombre + "). " + ex.Message);
            }
        }

        private DataSet DatosLicencia()
        {
            DataSet ds = ArmarDsResultado();
            try
            {
                XDocument listaNodos = XDocument.Load(System.Windows.Forms.Application.StartupPath + "\\ENROL\\Conf\\DGM.xml", LoadOptions.None);
                XElement lista = listaNodos.Element("DGM_DEVICES");

                Equipo equipo = new Equipo();
                equipo.DEVICE_TYPE = lista.Elements().First().Element("DEVICE_TYPE").Value.ToString();
                equipo.BIOS_SERIAL = lista.Elements().First().Element("BIOS_SERIAL").Value.ToString();
                equipo.UUID = lista.Elements().First().Element("UUID").Value.ToString(); ;
                equipo.PROCESSOR_ID = lista.Elements().First().Element("PROCESSOR_ID").Value.ToString();
                equipo.DRIVE_VOLUME = lista.Elements().First().Element("DRIVE_VOLUME").Value.ToString();
                equipo.MAC = lista.Elements().First().Element("MAC").Value.ToString();

                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                ds.Tables[0].Rows[0]["DATOS"] = equipo;
            }
            catch (Exception ex)
            {
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = "DatosLicencia(). " + ex.Message;
                ds.Tables[0].Rows[0]["DATOS"] = new object();
            }
            return ds;
        }

        private DataSet DatosEquipo()
        {
            DataSet ds = ArmarDsResultado();
            try
            {

                //// create management class object
                //ManagementClass mc = new ManagementClass("Win32_ComputerSystem");

                ////collection to store all management objects
                //ManagementObjectCollection moc = mc.GetInstances();

                //Equipo equipo = new Equipo();
                //equipo.DEVICE_TYPE = mc.GetInstances().GetEnumerator().Current()[""].ToString();
                //equipo.BIOS_SERIAL = lista.Elements().First().Element("BIOS_SERIAL").Value.ToString();
                //equipo.UUID = lista.Elements().First().Element("UUID").Value.ToString(); ;
                //equipo.PROCESSOR_ID = lista.Elements().First().Element("PROCESSOR_ID").Value.ToString();
                //equipo.DRIVE_VOLUME = lista.Elements().First().Element("DRIVE_VOLUME").Value.ToString();
                //equipo.MAC = lista.Elements().First().Element("MAC").Value.ToString();

                //if (moc.Count != 0)
                //{

                //    foreach (ManagementObject mo in mc.GetInstances())

                //    {

                //        // display general system information

                //        Console.WriteLine("\nMachine Make: {0}\nMachine Model: {1}\nSystem Type: {2}\nHost Name: {3}\nLogon User Name: {4}\n",

                //                          mo["Manufacturer"].ToString(),

                //                          mo["Model"].ToString(),

                //                          mo["SystemType"].ToString(),

                //                          mo["DNSHostName"].ToString(),

                //                          mo["UserName"].ToString());

                //    }

                //}


                //XDocument listaNodos = XDocument.Load(Application.StartupPath + "\\Conf\\DGM.xml", LoadOptions.None);
                //XElement lista = listaNodos.Element("DGM_DEVICES");



                //ds.Tables[0].Rows[0]["RESULTADO"] = true;
                //ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                //ds.Tables[0].Rows[0]["DATOS"] = equipo;
            }
            catch (Exception ex)
            {
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = "DatosLicencia(). " + ex.Message;
                ds.Tables[0].Rows[0]["DATOS"] = new object();
            }
            return ds;
        }

        private void CargarCmbDedos(ComboBox cmb, bool SeleccionarIndice, string mano)
        {
            try
            {
                cmb.SelectedIndex = -1;
                cmb.DataSource = null;
                cmb.DisplayMember = null;
                cmb.ValueMember = null;
                cmb.Items.Clear();

                string[] dedosMano = new string[] { "Indice", "Pulgar", "Medio", "Anular", "Meñique", "Ninguno" };
                DataSet ds = new DataSet();
                ds.Tables.Add(new DataTable());
                ds.Tables[0].Columns.Add("CODIGO");
                ds.Tables[0].Columns.Add("VALOR");


                for (int i = 1; i <= 6; i++)
                {
                    DataRow dr = ds.Tables[0].NewRow();

                    if (mano.ToUpper().Equals("DERECHA"))
                        dr["CODIGO"] = i;
                    else
                        dr["CODIGO"] = i + 6;

                    dr["VALOR"] = i + " - " + dedosMano[i - 1];

                    ds.Tables[0].Rows.Add(dr);
                }

                cmb.DataSource = ds.Tables[0];
                cmb.DisplayMember = ds.Tables[0].Columns[1].ColumnName;
                cmb.ValueMember = ds.Tables[0].Columns[0].ColumnName;

                if (cmb.Items.Count > 0 && SeleccionarIndice)
                    cmb.Text = "1 - Indice";//cmb.SelectedIndex = 0;
                else
                    cmb.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                throw new Exception("CargarCmbDedos(). " + ex.Message);
            }
        }

        private DataSet LeerXmlCatalogos(string archivoXML, string nombreListaNodos, string nombreLista, string snCodigo, string snValor)
        {
            DataSet ds = new DataSet();
            ds.Tables.Add(new DataTable());
            ds.Tables[0].Columns.Add("CODIGO");
            ds.Tables[0].Columns.Add("VALOR");

            try
            {
                XmlDocument documentoXML = new XmlDocument();

                documentoXML.Load(archivoXML);

                XmlNodeList listaNodo = documentoXML.GetElementsByTagName(nombreListaNodos);
                XmlNodeList lista = ((XmlElement)listaNodo[0]).GetElementsByTagName(nombreLista);

                int i = 0;
                foreach (XmlElement nodo in lista)
                {
                    XmlNodeList nCodigo = nodo.GetElementsByTagName(snCodigo);
                    XmlNodeList nValor = nodo.GetElementsByTagName(snValor);

                    DataRow dr = ds.Tables[0].NewRow();

                    if(i == 0)
                    {
                        dr["CODIGO"] = "0";
                        dr["VALOR"] = "SELECCIONE";
                    }
                    else
                    {
                        dr["CODIGO"] = nCodigo[0].InnerText;
                        dr["VALOR"] = nValor[0].InnerText;
                    }

                    ds.Tables[0].Rows.Add(dr);

                    i++;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("LeerXmlCatalogos(). " + ex.Message);
            }

            return ds;
        }

        private DataSet LeerXmlLinqCatalogosFiltro(string archivoXML, string nombreListaNodos, string nombreLista, string snCampoFiltro, string snValorFiltro, string snCampoCodigo, string snCampoValor)
        {
            DataSet ds = new DataSet();
            ds.Tables.Add(new DataTable());
            ds.Tables[0].Columns.Add("CODIGO");
            ds.Tables[0].Columns.Add("VALOR");

            try
            {
                XDocument listaNodos = XDocument.Load(archivoXML, LoadOptions.None);
                XElement lista = listaNodos.Element(nombreListaNodos);

                foreach (XElement ZipEntry in lista.Elements().Where(p => p.Element(snCampoFiltro).Value == snValorFiltro))
                {
                    DataRow dr = ds.Tables[0].NewRow();
                    dr["CODIGO"] = ZipEntry.Element(snCampoCodigo).Value;
                    dr["VALOR"] = ZipEntry.Element(snCampoValor).Value;

                    ds.Tables[0].Rows.Add(dr);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("LeerXmlCatalogos(). " + ex.Message);
            }

            return ds;
        }

        public Task<DataSet> InsertarMovimiento(Movimiento movimiento)
        {
            return Task.Run(() =>
            {
                MovimientoDiagnostico mv = new MovimientoDiagnostico();
                DateTime fechaIni = DateTime.Now;
                DateTime fechaFin = DateTime.Now;

                string jsonString = "";
                string body = "";

                DataSet dsResultado = ArmarDsResultado();
                try
                {
                    jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(movimiento);
                    byte[] data = UTF8Encoding.UTF8.GetBytes(jsonString);
                    
                    var url = Settings.Default.API_REST_MIROS + @Settings.Default.API_INSERTAR_MOVIMIENTO;
                    var request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "POST";
                    request.ContentType = "application/json";
                    request.Accept = "application/json";
                    request.Headers.Add("Authorization", $"Bearer {loginData.JWT_TOKEN}");

                    Stream postStream = request.GetRequestStream();
                    postStream.Write(data, 0, data.Length);
                    
                    using (WebResponse response = request.GetResponse())
                    {
                        using (Stream strReader = response.GetResponseStream())
                        {
                            if (strReader == null)
                                throw new Exception("Respuesta nula desde el servidor. ");

                            using (StreamReader objReader = new StreamReader(strReader))
                            {
                                body = objReader.ReadToEnd();
                                JsonResponseMovimiento respuestaEntrega;
                                fechaFin = DateTime.Now;
                                try
                                {
                                    respuestaEntrega = JsonConvert.DeserializeObject<JsonResponseMovimiento>(body);
                                }
                                catch (Exception)
                                {
                                    throw new Exception("Error al deserealizar respuesta: " + body);
                                }
                                

                                if (respuestaEntrega.codigo != 200)
                                    throw new Exception("Error al guardar la entrega. Código: " + respuestaEntrega.codigo + ", Mensaje: " + respuestaEntrega.mensaje);

                                if (respuestaEntrega.data == null)
                                    throw new Exception("Error de comunicación del API, intente guardar nuevamente y repórtelo al administrador");

                                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
                                dsResultado.Tables[0].Rows[0]["DATOS"] = respuestaEntrega;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                    dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "InsertarMovimiento(). " + ex.Message;
                }

                if (Settings.Default.MODO_DIAGNÓSTICO)
                {
                    mv.id_movimiento = idMovimiento.ToString();
                    mv.tipo_documento = string.Empty;
                    mv.numero_documento = string.Empty;
                    mv.nombre = string.Empty;
                    mv.fecha_nacimiento = string.Empty;
                    mv.comando = "InsertarMovimiento()";
                    mv.segundos = 0;
                    mv.fecha_ini = fechaIni;
                    mv.fecha_fin = fechaFin;
                    mv.request = jsonString;
                    mv.response = body;

                    ReportesDB reporte = new ReportesDB();
                    reporte.InsertarRegistro(mv);
                }

                return dsResultado;
            }
            );
        }

        public DataSet GenerarDetalleReporte()
        {
            DataSet dsResultado = ArmarDsResultado();
            try
            {
                int iCorrelativo = 0;
                string horaCaptura = DateTime.Now.ToString("HH:mm:ss");
                string fechaCaptura = DateTime.Now.ToString("dd/MM/yyyy");

                DateTime fechaN = new DateTime();
                fechaN = DateTime.Parse(dtpFecha.Text);

                string rutaReporteResumen = System.Windows.Forms.Application.StartupPath + "\\ENROL\\ReporteCasos\\" + DateTime.Now.ToString("dd/MM/yyyy").Replace("/", "") + ".xml";

                if (!File.Exists(rutaReporteResumen))
                {
                    XDocument document = new XDocument(new XDeclaration("1.0", "utf-8", null));

                    //Creamos el nodo raiz y lo añadimos al documento
                    XElement nodoRaiz = new XElement("Casos");
                    document.Add(nodoRaiz);
                    document.Save(rutaReporteResumen);
                    iCorrelativo = 1;
                }

                XDocument xmlDoc = XDocument.Load(rutaReporteResumen);

                if (iCorrelativo == 0)
                    iCorrelativo = int.Parse(xmlDoc.Elements("Casos").Elements("Caso").Last().Element("Correlativo").Value) + 1;
                
                //xmlDoc.Elements("Casos")
                //.Last().Add(new XElement("Caso", new XElement("Correlativo", iCorrelativo.ToString()), new XElement("Hora", horaCaptura), new XElement("NoLibreta", lblNoLibreta.Text), new XElement("Nombres", lbl_nombres_info.Text), new XElement("Apellidos", lbl_apellidos_info.Text), new XElement("FechaNacimiento", fechaN.ToString("dd/MM/yyyy")), new XElement("Usuario", lbl_usuario.Text), new XElement("NombreUsuario", lblNombreUsuario.Text), new XElement("SedeCaptura", sedeEstacion.CIUDAD + ", " + sedeEstacion.PAIS), new XElement("FechaCaptura", fechaCaptura), new XElement("EstacionCaptura", Environment.MachineName + "/" + funciones.ObtenerBios())));
                //xmlDoc.Save(rutaReporteResumen);

                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "GenerarDetalleReporte(). " + ex.Message;
            }
            return dsResultado;
        }

        public DataSet EncriptarArchivo(string rutaArchivoPlano)
        {
            string linea = "";
            DataSet dsResultado = ArmarDsResultado();
            try
            {
                #region
                /*
                 * string archivoEncriptado = Path.Combine(Application.StartupPath, "ENROL", "XMLs") + "\\" + Path.GetFileNameWithoutExtension(rutaArchivoPlano) + "_" + lbl_usuario.Text + ".txt";
               string XmlKeysFile = Path.Combine(Properties.Settings.Default.DRIVE_LETTER, "DGMGT", "XMLKeys", "XMLKeys.xml");
               string llavePublica = Path.Combine(Properties.Settings.Default.DRIVE_LETTER, "DGMGT", "XMLKeys", "pasaportes_PublicKey.asc");



               DataSet dsXmlKey = new DataSet();
               dsXmlKey.ReadXml(XmlKeysFile);

               string archivoEncriptado = Path.Combine(Application.StartupPath, "ENROL", "XMLs") + "\\" + Path.GetFileNameWithoutExtension(rutaArchivoPlano) + "_" + lbl_usuario.Text + ".txt";


               UtilityCore.Cryptography.Cryptography Cryp = new UtilityCore.Cryptography.Cryptography();
               Cryp.Encryption(llavePublica, dsXmlKey.Tables[0].Rows[0]["PASSWORD"].ToString(), archivoEncriptado, rutaArchivoPlano);

               File.WriteAllText(archivoEncriptado + ".Done", string.Empty);
               File.Delete(rutaArchivoPlano);*/
                #endregion
                DataSet dsParamEncrip = new DataSet();
                dsParamEncrip.Tables.Add(new DataTable());
                dsParamEncrip.Tables[0].Columns.Add("rutaArchivoPlano", typeof(string));
                dsParamEncrip.Tables[0].Columns.Add("archivoEncriptado", typeof(string));
                dsParamEncrip.Tables[0].Columns.Add("rutaArchivo", typeof(string));
                dsParamEncrip.Tables[0].Columns.Add("publicKey", typeof(string));

                DataRow drParamEncript = dsParamEncrip.Tables[0].NewRow();

                linea = "1";
                string archivoEncriptado =   Path.GetFileNameWithoutExtension(rutaArchivoPlano) + "_" + Environment.MachineName + "_" + lbl_usuario.Text + "_" + Properties.Settings.Default.SUFIJO_ENCRIPTACION;
                linea = "";
                string rutaArchivo = Path.Combine(System.Windows.Forms.Application.StartupPath, "ENROL", "XMLs") + "\\";

                linea = "2";
                string publicKey = "";
                if(Properties.Settings.Default.SUFIJO_ENCRIPTACION.Equals("U"))
                    publicKey = Path.Combine(Properties.Settings.Default.DRIVE_LETTER, "DGMGT", "XMLKeys", lbl_usuario.Text + "_publi.xml");
                else if(Properties.Settings.Default.SUFIJO_ENCRIPTACION.Equals("M"))
                    publicKey = Path.Combine(Properties.Settings.Default.DRIVE_LETTER, "DGMGT", "XMLKeys", Environment.MachineName + "_publi.xml");

                drParamEncript["rutaArchivoPlano"] = rutaArchivoPlano;
                drParamEncript["archivoEncriptado"] = archivoEncriptado;
                drParamEncript["rutaArchivo"] = rutaArchivo;
                drParamEncript["publicKey"] = publicKey;
                dsParamEncrip.Tables[0].Rows.Add(drParamEncript);

                string strDs = dsParamEncrip.GetXml();

                //linea = "3";
                //string keypub = new StreamReader(publicKey).ReadToEnd();
                ////linea = "4";
                ////string info = File.ReadAllText(rutaArchivoPlano);

                //linea = "5";
                //byte[] encryp = Yoyo.Encrypt(keypub, File.ReadAllBytes(rutaArchivoPlano));//Encoding.ASCII.GetBytes(info));
                //linea = "6";
                //Yoyo.saveBynaryFile(encryp, archivoEncriptado, ".txt", rutaAchivo);

                //linea = "7";
                //string archivoDone = Path.Combine(rutaAchivo, archivoEncriptado + ".txt.Done");
                //linea = "8";
                //File.WriteAllText(archivoDone, string.Empty);
                //linea = "9";
                //File.Delete(rutaArchivoPlano);

                //linea = "10";
                //System.GC.Collect();
                //System.GC.WaitForPendingFinalizers();

                linea = "11";
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                dsResultado.Tables[0].Rows[0]["DATOS"] = null;
            }
            catch (Exception ex)
            {
                try { File.Delete(rutaArchivoPlano); } catch { }; 

                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "EncriptarArchivo(). " + ex.Message + " Inner: " + ex.InnerException + ". Línea: " + linea;
                dsResultado.Tables[0].Rows[0]["DATOS"] = null;
            }

            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();

            return dsResultado;
        }

        public async Task<bool> ValidarCampos()
        {
            DataSet dsPasaporte = ValidarPasaporte();
            //DataSet dsRostro = ValidarFotografia();
            //DataSet dsHuellas = new DataSet();
            //DataSet dsProbatorios = await ValidarProbatorios();

            txtMensaje.Text = string.Empty;

            pbxDatosPersonales.Image = bool.Parse(dsPasaporte.Tables[0].Rows[0]["RESULTADO"].ToString()) ? pbxCheck.Image : pbxWarning.Image;
            txtMensaje.Text += dsPasaporte.Tables[0].Rows[0]["MSG_ERROR"].ToString();

            if (!rbLibretaPass.Checked && !rbCUI.Checked) {
                if (pbxDocumento.Image != pbxCheck.Image)
                    txtMensaje.Text += "Escanee un documento de identificación. ";
            }

            if (!Settings.Default.ENROLAR_ROSTRO)
                pbxFotografia.Image = pbxCheck.Image;

            if (!rbLibretaPass.Checked && !rbCUI.Checked)
            {
                if (pbxFotografia.Image != pbxCheck.Image)
                    txtMensaje.Text += "Operación de ROSTRO pendiente. ";
            }

            if (!Settings.Default.ENROLAR_HUELLAS)
                pbxCompararHuellas.Image = pbxCheck.Image;
            
            //if(!chkPersonaSinHuellas.Checked)
            //    if(pbxCompararHuellas.Image == pbxLoad.Image)
            //        txtMensaje.Text += "Operación de HUELLAS pendiente. ";

            
            if(cmbTipoDocumento.SelectedValue.Equals("P") && cmbPaisEmisor.SelectedValue.Equals("GTM"))
                    if (pbxCONS.Image != pbxCheck.Image)
                        txtMensaje.Text = "Operación de consulta de pasaporte pendiente. ";

            if(pbxDeclaracion.Image == pbxLoad.Image)
                txtMensaje.Text = "Operación de consulta DRJV pendiente. ";
            
            if (pbxAlertas.Image == pbxLoad.Image)
                txtMensaje.Text = "Operación de consulta ALERTAS pendiente. ";
            
            if (pbxMovimientoSIOM.Image == pbxLoad.Image)
                txtMensaje.Text = "Operación de consulta SIOM pendiente. ";

            bool b1 = pbxDatosPersonales.Image == pbxCheck.Image;
            bool b2 = pbxFotografia.Image == pbxCheck.Image;            
            bool b3 = pbxCompararHuellas.Image == pbxCheck.Image;

            //if (b1 && b2 && b3)
            //    return true;
            //else
            //    return false;

            return txtMensaje.Text.Equals(string.Empty);
        }


        private DataSet ValidarPasaporte()
        {
            DataSet dsResultado = ArmarDsResultado();
            string msgError = string.Empty;

            try
            {
                if(!chkMRZ.Checked && rbLibretaPass.Checked && rbLibretaPass.Checked)
                    if(txtMrz.Text.Equals(string.Empty)) msgError += "¡Código MRZ no encontrado!. ";

                if (cmbTipoDocumento.SelectedIndex < 0)
                    msgError += "Seleccione un tipo de documento. ";

                if (cmbPaisEmisor.SelectedIndex < 0)
                    msgError += "Seleccione un país emisor. ";

                txtNumeroDocumento.Text = txtNumeroDocumento.Text.Trim();

                Regex regEx = new Regex(Settings.Default.REGEX_NUMERO_DOCUMENTO);
                if (!rbLibretaPass.Checked && !rbCUI.Checked) {
                    if (!regEx.IsMatch(txtNumeroDocumento.Text))
                        msgError += "Ingrese un número de documento válido. ";
                }
                     
                //if(pbxDocumento.Image == pbxWarning.Image) msgError += "¡Imagen de pasaporte no encontrada!. ";
                
                //if(Settings.Default.ESCANEAR_UV)
                //    if (pbxPasaporteEscaneadoUV.Image == pbxWarning.Image) msgError += "¡Imagen UV de pasaporte no encontrada!. ";
                
                //if (pbxCONS.Image == pbxWarning.Image) msgError += "¡Consulta de pasaporte no realizada!. ";
                //if (pbxCONS.Image == pbxLoad.Image) msgError += "¡Consulta de pasaporte en proceso!. ";
                        
                //if(pbxFotografia.Image != pbxCheck.Image)
                //    msgError += "No se encontró fotografía. ";


                if (msgError.Equals(string.Empty) == false)
                        throw new Exception(msgError);

                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                //tab_principal.TabPages["tabPasaporte"].ImageKey = "check.bmp";

            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "ValidarPasaporte(). " + ex.Message;
                //tab_principal.TabPages["tabPasaporte"].ImageKey = "warning.bmp";
            }

            return dsResultado;
        }

        private void btnActualizarCamaras_Click(object sender, EventArgs e)
        {
            try
            {
               ListarCamaras(CameraListBox, true);
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "btnActualizarCamaras_Click(). " + ex.Message;
                MessageBox.Show("btnActualizarCamaras_Click(). " + ex.Message);
            }

        }

        private void enrollment_FormClosing(object sender, FormClosingEventArgs e)
        {
            _session.DeviceStateChanged -= Session_DeviceStateChanged;
            _session.ProcessStateChanged -= Session_ProcessStateChanged;
            _session.ImageAvailable -= Session_ImageAvailable;
            _session.FieldAvailable -= Session_FieldAvailable;
            _session.ValidationResultAvailable -= Session_ValidationResultAvailable;
            _session.ReferenceDocumentAvailable -= Session_ReferenceDocumentAvailable;
            _session.DocumentFinished -= Session_DocumentFinished;

            //_formVirtualDevice.Hide();
            //_formVirtualDevice.Dispose();
            //_formVirtualDevice = null;
            _session.Dispose();

            if (salir != DialogResult.Yes)
            {
                e.Cancel = true;

                //_regulaReader.Disconnect();
                //UnsubscribeEvents();
                System.Windows.Forms.Application.Exit();
            }
         
        }
        String nom_escaner;
        int limiteEscaneos = 32;

        private void btn_escanear_Click(object sender, EventArgs e)
        {
            try
            {
                //nom_escaner = TwainLib.TwainOperations.GetScanSource();
                nom_escaner = Settings.Default.scan;

                if (nom_escaner.Length > 0)
                {
                    if (Settings.Default.CONFIRMA_CIUDADANO) limiteEscaneos--;
                    var nombreArchivo = TwainOperations.ScanImages(".jpg", true, nom_escaner);
                    int j = (nombreArchivo.Count > limiteEscaneos) ? limiteEscaneos : nombreArchivo.Count;

                    for (int i = 0; i < j; i++)
                    //foreach (var Itm in nombreArchivo)
                    {
                        if (tamanio <= limiteEscaneos)
                        {
                            tamanio += 1;
                            VerProbatorios(nombreArchivo[i], tamanio);
                        }
                        else
                            MessageBox.Show("Límite de escaneos alcanzados (" + limiteEscaneos + "). ");

                    }
                }
                else
                    btnSeleccionarEscaner_Click(sender, e);

                ValidarProbatorios();

            }
            catch (Exception ex)
            {
                txtMensaje.Text = "btn_escanear_Click(). " + ex.Message;
                MessageBox.Show("btn_escanear_Click(). " + ex.Message);
            }
        }

        private void VerProbatorios(String src, int contar)
        {
            try
            {
                int i = contar;
                /********/
                Label lblProbatorio = new Label();
                lblProbatorio.Name = "lblProbatorio_" + i.ToString();
                //ComprimirImagen(src, src, 65);

                string pathThumbnail = src.Split('.')[0] + "_thumbnail." + src.Split('.')[1];
                System.Drawing.Image.FromFile(src).GetThumbnailImage(ancho, alto, null, IntPtr.Zero).Save(pathThumbnail, ImageFormat.Png);

                System.Drawing.Image img = LoadBitmapUnlocked(pathThumbnail);//CreateNonIndexedImage(pathThumbnail);//(Image)(System.Drawing.Bitmap.FromFile(pathThumbnail).Clone());
                 
                try { File.Delete(pathThumbnail);} catch { }


                lblProbatorio.Image = (System.Drawing.Image)(img.Clone());
                img.Dispose();

                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();

                lblProbatorio.Image.Tag = src;
                lblProbatorio.Size = new System.Drawing.Size(ancho, alto);
                lblProbatorio.Location = new System.Drawing.Point(x, splitContainer1.Panel2.AutoScrollPosition.Y + y);
                lblProbatorio.BorderStyle = BorderStyle.FixedSingle;
                lblProbatorio.ContextMenuStrip = cmsProbatorios;
                splitContainer1.Panel2.Controls.Add(lblProbatorio);

                i++;

                x += ancho + 10;

                if (x > (splitContainer1.Panel2.Width - ancho))
                {
                    y += alto + 10;
                    x = 10;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("VerProbatorios(). " + ex.Message);
            }
        }

        //public static Image CreateNonIndexedImage(string path)
        //{
        //    using (var sourceImage = Image.FromFile(path))
        //    {
        //        var targetImage = new Bitmap(sourceImage.Width, sourceImage.Height,
        //          PixelFormat.Format32bppArgb);
        //        using (var canvas = Graphics.FromImage(targetImage))
        //        {
        //            canvas.DrawImageUnscaled(sourceImage, 0, 0);
        //        }
        //        return targetImage;
        //    }
        //}

        //private void ComprimirImagen(string inputFile, string ouputfile, long compression)
        //{
        //    try
        //    {
        //        using (var image = LoadBitmapUnlocked(inputFile))
        //        {

        //            EncoderParameters eps = new EncoderParameters(1);
        //            eps.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, compression);

        //            string mimetype = "image/jpeg";//GetMimeType(new System.IO.FileInfo(inputFile).Extension);
        //            ImageCodecInfo ici = GetEncoderInfo(mimetype);

        //            image.Save(ouputfile, ici, eps);
        //            //image.Save(inputFile, ici, eps);
        //            image.Dispose();
        //            GC.Collect();
        //            GC.WaitForPendingFinalizers();
        //        }

        //        GC.Collect();
        //        GC.WaitForPendingFinalizers();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("ComprimirImagen(). " + ex.Message);
        //    }
        //}

        //private Bitmap LoadBitmapUnlocked(string file_name)
        //{
        //    using (Bitmap bm = new Bitmap(file_name))
        //    {
        //        return new Bitmap(bm);
        //    }
        //}

        //static string GetMimeType(string ext)
        //{
        //    try
        //    {
        //        //    CodecName FilenameExtension FormatDescription MimeType 
        //        //    .BMP;*.DIB;*.RLE BMP ==> image/bmp 
        //        //    .JPG;*.JPEG;*.JPE;*.JFIF JPEG ==> image/jpeg 
        //        //    *.GIF GIF ==> image/gif 
        //        //    *.TIF;*.TIFF TIFF ==> image/tiff 
        //        //    *.PNG PNG ==> image/png 
        //        switch (ext.ToLower())
        //        {
        //            case ".bmp":
        //            case ".dib":
        //            case ".rle":
        //                return "image/bmp";

        //            case ".jpg":
        //            case ".jpeg":
        //            case ".jpe":
        //            case ".fif":
        //                return "image/jpeg";

        //            case "gif":
        //                return "image/gif";
        //            case ".tif":
        //            case ".tiff":
        //                return "image/tiff";
        //            case "png":
        //                return "image/png";
        //            default:
        //                return "image/jpeg";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("GetMimeType(). " + ex.Message);
        //    }
        //}

        //static ImageCodecInfo GetEncoderInfo(string mimeType)
        //{
        //    try
        //    {
        //        ImageCodecInfo[] encoders;
        //        encoders = ImageCodecInfo.GetImageEncoders();

        //        ImageCodecInfo encoder = (from enc in encoders
        //                                  where enc.MimeType == mimeType
        //                                  select enc).First();
        //        return encoder;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("GetEncoderInfo(). " + ex.Message);
        //    }
        //}

        private void probatorios_image_base64(string src)
        {
            try
            {
                array = new string[tamanio];
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    System.Drawing.Image.FromFile(src).Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                    byte[] imageBytes = ms.ToArray();

                    // Convert byte[] to Base64 String
                    string base64String = Convert.ToBase64String(imageBytes);

                    array[tamanio - 1] = base64String;
                    listaProbatorios.Add(array[tamanio - 1]);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("probatorios_image_base64(). " + ex.Message);
            }

        }

        private void CmbIngresoManual(ComboBox cmb, bool activar, string nuevoItem)
        {
            try
            {
                if (activar)
                {
                    cmb.SelectedIndex = -1;
                    cmb.DataSource = null;
                    cmb.DisplayMember = null;
                    cmb.ValueMember = null;
                    cmb.Items.Clear();
                    cmb.DropDownStyle = ComboBoxStyle.DropDown;
                    cmb.Enabled = false;

                    DataTable dt = new DataTable();
                    dt.Columns.Add("VALOR", typeof(string));
                    dt.Columns.Add("CODIGO", typeof(string));
                    DataRow dr = dt.NewRow();
                    dr["VALOR"] = nuevoItem;
                    dr["CODIGO"] = "-1";

                    dt.Rows.Add(dr);
                    cmb.DataSource = dt;
                    cmb.DisplayMember = "VALOR";
                    cmb.ValueMember = "CODIGO";
                    cmb.SelectedIndex = 0;
                }
                else
                {
                    cmb.DropDownStyle = ComboBoxStyle.DropDown;
                    cmb.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("CmbIngresoManual(). " + ex.Message);
            }
        }

        //private Task<int> Alertas(DPI dpi, PictureBox pbx, string tipoValidacion)
        //{
        //    return Task.Run(() =>
        //    {
        //        DataSet dsResultado = ArmarDsResultado();

        //        try
        //        {
        //            //pbx.Image = pbxLoad.Image;
        //            dsResultado = ConsultaArraigosxNombres(dpi.PRIMER_NOMBRE, dpi.SEGUNDO_NOMBRE, dpi.PRIMER_APELLIDO, dpi.SEGUNDO_APELLIDO);
        //            if (bool.Parse(dsResultado.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
        //                throw new Exception("Error al consultar ARRAIGOS por NOMBRES: " + dsResultado.Tables[0].Rows[0]["MSG_ERROR"].ToString());

        //            DataTable dtArraigos = new DataTable();
        //            Arraigos arraigo = (Arraigos)(dsResultado.Tables[0].Rows[0]["DATOS"]);

        //            if (arraigo.informacionArraigos.Tables[0] != null && arraigo.informacionArraigos.Tables[0].Rows.Count > 0)
        //            {
        //                DataRow[] dr = arraigo.informacionArraigos.Tables[0].Select(" status IN (1, 4) ");
        //                if (dr.Length > 0)
        //                    dtArraigos = arraigo.informacionArraigos.Tables[0].Select(" status IN (1, 4) ").CopyToDataTable();
        //            }

        //            dsResultado = ConsultaAlertasxNombres(dpi.PRIMER_NOMBRE, dpi.SEGUNDO_NOMBRE, dpi.PRIMER_APELLIDO, dpi.SEGUNDO_APELLIDO);
        //            if (bool.Parse(dsResultado.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
        //                throw new Exception("Error al consultar ALERTAS por NOMBRES: " + dsResultado.Tables[0].Rows[0]["MSG_ERROR"].ToString());

        //            DataTable dtAlertas = new DataTable();
        //            Alertas alerta = (Alertas)(dsResultado.Tables[0].Rows[0]["DATOS"]);

        //            if (alerta.informacionAlerta.Tables[0] != null && alerta.informacionAlerta.Tables[0].Rows.Count > 0)
        //            {
        //                DataRow[] dr = arraigo.informacionArraigos.Tables[0].Select(" alerta = 1 ");
        //                if (dr.Length > 0)
        //                    dtAlertas = arraigo.informacionArraigos.Tables[0].Select(" alerta = 1 ").CopyToDataTable();
        //            }

        //            pbx.Image = ((dtArraigos != null && dtArraigos.Rows.Count > 0) || dtAlertas.Rows.Count > 0) ? pbxWarning.Image : pbxCheck.Image;

        //            if (tipoValidacion.ToUpper().Equals("TITULAR"))
        //                visorAlertas = new VisorAlertas(dpiTitular, dtArraigos, dtAlertas, string.Empty);
        //            else if (tipoValidacion.ToUpper().Equals("PADRE"))
        //                visorAlertasPadre = new VisorAlertas(dpiTitular, dtArraigos, dtAlertas, string.Empty);
        //            else if (tipoValidacion.ToUpper().Equals("MADRE"))
        //                visorAlertasMadre = new VisorAlertas(dpiTitular, dtArraigos, dtAlertas, string.Empty);
        //        }
        //        catch (Exception ex)
        //        {
        //            pbx.Image = pbxWarning.Image;
        //            if (tipoValidacion.ToUpper().Equals("TITULAR"))
        //                visorAlertas = new VisorAlertas(dpiTitular, new DataTable(), new DataTable(), ex.Message);
        //            else if (tipoValidacion.ToUpper().Equals("PADRE"))
        //                visorAlertasPadre = new VisorAlertas(dpiTitular, new DataTable(), new DataTable(), ex.Message);
        //            else if (tipoValidacion.ToUpper().Equals("MADRE"))
        //                visorAlertasMadre = new VisorAlertas(dpiTitular, new DataTable(), new DataTable(), ex.Message);
        //        }
        //        return 0;
        //    });
        //}

        public Task<DPI> LeerDPIAsync()
        {
            return Task.Run(() =>
            {
                DPI dpi = new DPI();
                try
                {
                    string[] lectores = new string[10];
                    int TotalLectores = 0;
                    string Err;
                    string NombreLector = "";
                    string Valor = "";
                    fotoDPI = null;

                    DGMReader APIReader = new DGMReader();

                    APIReader.apConnectReader();

                    string _apIsReaderConnected = APIReader.apIsReaderConnected();
                    string _apConnectReader = APIReader.apConnectReader();

                    if (_apIsReaderConnected.Equals("00") == false)
                        throw new Exception("Error de lectura de DPI, _apIsReaderConnected = " + _apIsReaderConnected);

                    if (_apConnectReader.Equals("00") == false)
                        throw new Exception("Error de lectura de DPI, _apConnectReader = " + _apConnectReader);

                    //if (!(_apIsReaderConnected != "00") || !(_apConnectReader != "00"))
                    {
                        //Consultar Lectores disponibles
                        Err = APIReader.apGetReaderNames(ref lectores, ref TotalLectores);

                        NombreLector = lectores[0].ToString();

                        //Determinar si hay una tarjeta insertada en el lector

                        Err = APIReader.apIsCardInserted(NombreLector);

                        //Conectarse con el lector para leer los datos de la tarjeta            
                        Err = APIReader.apConnectReader();

                        //Devuelve en valor el número de CUI impreso y grabado en el chip
                        Err = APIReader.apGetCUI(ref Valor);
                        dpi.CUI = Valor.Trim();

                        //Devuelve en valor el nombre del departamento de vecindad
                        Err = APIReader.apGetDepartmentVecindad(ref Valor);
                        dpi.DEPARTAMENTO_VECINDAD = Valor.Trim();

                        //Devuelve en valor el nombre del municipio de vecindad
                        Err = APIReader.apGetMunicipalityVecindad(ref Valor);
                        dpi.MUNICIPIO_VECINDAD = Valor.Trim();

                        //Devuelve en valor la fecha emisión en el formato como se ve impreso en el DPI
                        Err = APIReader.apGetIssueDate(ref Valor);
                        dpi.FECHA_EMISION = Valor.Trim();

                        //Devuelve en valor la fecha de vencimiento en el formato como se ve impreso en el DPI
                        Err = APIReader.apGetExpireDate(ref Valor);
                        dpi.FECHA_EXPIRA = Valor.Trim();

                        //Devuelve en la variable serie el valor del número de serie del DPI
                        Err = APIReader.apGetCardSerialNumber(ref Valor);
                        dpi.SERIE_NUMERO = Valor.Trim();

                        //Devuelve en valor de la cedula de vecindad en el formato como está impreso en el DPI
                        Err = APIReader.apGetCedula(ref Valor);
                        dpi.CEDULA_VECINDAD = Valor.Trim();

                        //‘Devuelve en valor el nombre del departamento de Extensión de cédula
                        Err = APIReader.apGetDepartmentCedula(ref Valor);
                        dpi.DEPARTAMENTO_CEDULA = Valor.Trim();

                        //Devuelve en valor el nombre del municipio de Extensión de cédula
                        Err = APIReader.apGetMunicipalityCedula(ref Valor);
                        dpi.MUNICIPIO_CEDULA = Valor.Trim();

                        //Devuelve en valor el primer nombre del ciudadano
                        Err = APIReader.apGetFirstName(ref Valor);
                        dpi.PRIMER_NOMBRE = Valor.Trim();

                        //Devuelve en valor el segundo nombre del ciudadano
                        Err = APIReader.apGetMiddleName(ref Valor);
                        dpi.SEGUNDO_NOMBRE = Valor.Trim();

                        //Devuelve en valor el tercer nombre del ciudadano
                        Err = APIReader.apGetThirdName(ref Valor);
                        dpi.TERCER_NOMBRE = Valor.Trim();

                        //Devuelve en valor el apellido nombre del ciudadano
                        Err = APIReader.apGetFirstLastName(ref Valor);
                        dpi.PRIMER_APELLIDO = Valor.Trim();

                        //Devuelve en valor el segundo apellido del ciudadano
                        Err = APIReader.apGetSecondLastName(ref Valor);
                        dpi.SEGUNDO_APELLIDO = Valor.Trim();

                        //Devuelve en valor el apellido de casada del ciudadano
                        Err = APIReader.apGetMarriedName(ref Valor);
                        dpi.CASADA_APELLIDO = Valor.Trim();

                        //Devuelve en valor la fecha de nacimiento del ciudadano en el formato impreso en el DPI
                        Err = APIReader.apGetBirthDate(ref Valor);
                        dpi.FECHA_NACIMIENTO = Valor.Trim();

                        //Devuelve en valor el nombre del país de nacimiento del ciudadano 
                        Err = APIReader.apGetCountryBirth(ref Valor);
                        dpi.PAIS_NACIMIENTO = Valor.Trim();
                        MessageBox.Show("PAIS DE NACIMIENTO DEL DPI: "+dpi.PAIS_NACIMIENTO);
                        //Devuelve en valor el nombre del Departamento de nacimiento del ciudadano 
                        Err = APIReader.apGetDepartmentBirth(ref Valor);
                        dpi.DEPARTAMENTO_NACIMIENTO = Valor.Trim();
                        MessageBox.Show("DEPARTAMENTO DE NACIMIENTO DEL DPI: " + dpi.DEPARTAMENTO_NACIMIENTO);

                        //Devuelve en valor el nombre del Municipio de nacimiento del ciudadano 
                        Err = APIReader.apGetMunicipalityBirth(ref Valor);
                        dpi.MUNICIPIO_NACIMIENTO = Valor.Trim();
                        MessageBox.Show("MUNICIPIO DE NACIMIENTO DEL DPI: " + dpi.MUNICIPIO_NACIMIENTO);

                        //Devuelve en valor el nombre del genero del ciudadano como está impreso en el DPI 
                        Err = APIReader.apGetSex(ref Valor);
                        dpi.SEXO = Valor.Trim();

                        //Devuelve en valor el nombre de la ocupacion del ciudadano como está grabado en el DPI 
                        Err = APIReader.apGetOccupation(ref Valor);
                        dpi.OCUPACION = Valor.Trim();

                        //Devuelve en valor el folio de asiento de nacimiento del ciudadano como está impreso en el DPI
                        Err = APIReader.apGetFolio(ref Valor);
                        dpi.FOLIO = Valor.Trim();

                        //Devuelve en valor el libro de asiento de nacimiento del ciudadano como está impreso en el DPI
                        Err = APIReader.apGetLibro(ref Valor);
                        dpi.LIBRO = Valor.Trim();

                        //Devuelve en valor la partida de asiento de nacimiento del ciudadano como está impreso en el DPI
                        Err = APIReader.apGetPartida(ref Valor);
                        dpi.PARTIDA = Valor.Trim();

                        //Devuelve en valor el estatus marital del ciudadano 
                        Err = APIReader.apGetMaritalStatus(ref Valor);
                        dpi.ESTADO_CIVIL = Valor.Trim();

                        //Devuelve en valor el MRZ del DPI
                        Err = APIReader.apGetMRZ(ref Valor);
                        dpi.MRZ = Valor.Trim();

                        //Devuelve en valor la nacionalidad del ciudadano
                        Err = APIReader.apGetNationality(ref Valor);
                        dpi.NACIONALIDAD = Valor.Trim();

                        //Devuelve en valor el país de nacimiento del ciudadano
                        Err = APIReader.apGetCountryBirth(ref Valor);
                        dpi.PAIS_NACIMIENTO = Valor.Trim();

                        //Devuelve en valor el país de nacimiento del ciudadano
                        Err = APIReader.apGetEtnia(ref Valor);
                        dpi.ETNIA = Valor.Trim();

                        //Devuelve en la variable imagen los bytes correspondientes a la fotografía del ciudadano ‘almacenada en el DPI       
                        Err = APIReader.apGetFacialImage(ref fotoDPI);
                        if (fotoDPI == null)
                        {
                            fotoDPI = pbxUsuario.Image;
                            MessageBox.Show("La fotografía del DPI es null. ");
                        }
                        else
                        {
                            dpi.IMAGE = fotoDPI;

                            using (MemoryStream ms = new MemoryStream())
                            {
                                fotoDPI.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                byte[] imageBytes = ms.ToArray();

                                // Convert byte[] to Base64 String
                                string base64String = Convert.ToBase64String(imageBytes);
                                dpi.FOTOGRAFIA_BASE_64 = base64String;
                            }
                        }                        

                        dpi.INFORMACION_DPI_LEIDA = true;
                        dpi.MENSAJE_ERROR = string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    //throw new Exception("LeerDPI(). " + ex.Message);
                    dpi.INFORMACION_DPI_LEIDA = false;
                    dpi.MENSAJE_ERROR = "LeerDPI(). " + ex.Message;
                }
                return dpi;
            }
            );
        }       

        public Task<DataSet> MOC_DPI(string cui)
        {
            return Task.Run(() =>
            {
                DataSet ds = ArmarDsResultado();
                ds.Tables[0].Rows[0]["RESULTADO"] = false;

                try
                {
                    DGMReader APIReader = new DGMReader();

                    bool MOC_DPI = false;
                    int Remaining = -1;
                    APIReader.apConnectReader();

                    string _apIsReaderConnected = APIReader.apIsReaderConnected();
                    string _apConnectReader = APIReader.apConnectReader();

                    if (_apIsReaderConnected.Equals("00") == false)
                        throw new Exception("Error de lectura de DPI, _apIsReaderConnected = " + _apIsReaderConnected);

                    if (_apConnectReader.Equals("00") == false)
                        throw new Exception("Error de lectura de DPI, _apConnectReader = " + _apConnectReader);

                    string Err;
                    string Valor = "";

                    Err = APIReader.apGetCUI(ref Valor);
                    if (!cui.Trim().Equals(Valor.Trim()))
                        throw new Exception("El CUI del DPI leído (" + "" + "), no coincide con el de la operación de MOCH (" + Valor.Trim() + ").");

                    string respuesta = APIReader.apMatchOnCard(10, ref MOC_DPI, ref Remaining);

                    if (MOC_DPI)
                        ds.Tables[0].Rows[0]["RESULTADO"] = true;
                    else
                        ds.Tables[0].Rows[0]["RESULTADO"] = false;

                    ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;

                    //DGMReader APIReader = new DGMReader();
                    //bool Result = false;
                    //int Remaining = -1;
                    //APIReader.apConnectReader();

                    //string _apIsReaderConnected = APIReader.apIsReaderConnected();
                    //string _apConnectReader = APIReader.apConnectReader();

                    //if (!(_apIsReaderConnected != "00") || !(_apConnectReader != "00"))
                    //{
                    //    string respuesta = APIReader.apMatchOnCard(10, ref Result, ref Remaining);

                    //    if (Result)
                    //        ds.Tables[0].Rows[0]["RESULTADO"] = true; //rtbInformacion.Text = "Las huellas coinciden";
                    //    else
                    //        ds.Tables[0].Rows[0]["RESULTADO"] = false; //rtbInformacion.Text = "Las huellas no coinciden";
                    //}
                    //else
                    //    throw new Exception(" Error en la comparacion. _apIsReaderConnected: " + _apIsReaderConnected + ", _apConnectReader: " + _apConnectReader);

                    //ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                }
                catch (Exception ex)
                {
                    ds.Tables[0].Rows[0]["RESULTADO"] = false;
                    ds.Tables[0].Rows[0]["MSG_ERROR"] = "MOC_DPI(). " + ex.Message;
                }
                return ds;
            }
            );
        }
        
        public void NuevaInstanciaEnrollment()
        {
            try
            {
                System.Windows.Forms.Application.Restart();   
            }
            catch (Exception ex)
            {
                throw new Exception(MethodBase.GetCurrentMethod().Name + "(). " + ex.Message);
            }
        }

        public async void NuevoIngreso()
        {
            try
            {

                nfvLeftThumb.Finger = nfvRightThumb.Finger = null;
                nfvLeftIndex.Finger = nfvLeftMiddle.Finger = nfvLeftRing.Finger = nfvLeftLittle.Finger = null;
                nfvRightIndex.Finger = nfvRightMiddle.Finger = nfvRightRing.Finger = nfvRightLittle.Finger = null;
                nfv442.Finger = null;

                nfvMI.Finger = nfvMD.Finger = nfvPU.Finger = null;

                rbnManoIzquierda.Checked = rbnManoDerecha.Checked = rbnPulgares.Checked = false;

                idMovimiento = Guid.NewGuid();
                lblNuevo.Text = (int.Parse(lblNuevo.Text) + 1).ToString();
                this.lblRostroComprimido.Text = "0";

                this.lblRostroSegmentado.Text = "-1";
                this.pbxCoincideRostro.BackColor = Color.White;

                Settings.Default.SEGMENTAR_ROSTRO = false;
                txtEdad.Text = String.Empty;

                lblIdMovimientoSIOM.Text = "0";
                lblAcuerdoPais.Text = "-1";
                pbxMovimientoSIOM.Image = pbxStart.Image;
                pbxAlertas.Image = pbxStart.Image;
                pbxAlertas.Tag = "";
                pbxMovimientoSIOM.Tag = "";
                lblIdBusqueda.Text = "-1";
                lblIdTipoVisa.Text = lblNumeroVisa.Text = string.Empty;

                //SE CAMBIO PARA SER INVOCADO DESDE EL FORMULARIO DE VUELOS
                //if (sedeDataEquipo.idtipodelegacion == 1)
                //{
                //    dsVuelo = GetVuelos();
                //    if (bool.Parse(dsVuelo.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                //        throw new Exception(dsVuelo.Tables[0].Rows[0]["MSG_ERROR"].ToString());
                //}

                btnDJRV.BackColor = Color.White;
                pbxDeclaracion.Image = pbxStart.Image;
                btnDJRV.Text = "DJRV: 0/0/-1";
                chkEnviarRS.Text = "Enviar a RS/0";

                lblParentesco.Text = "0";

                chkMNA.Checked = false;
                chkMNA.Enabled = true;

                formularioLimpio = true;
                if(!clicAgregarPersona)
                    movimiento = new Movimiento();

                lblGrupo.Text = "Grupo (" + movimiento.adultos.Count + "A/" + movimiento.menores.Count + "M)";

                lblEscanerPasaporte.Text = Settings.Default.ESCANER_PASAPORTES;
                txtMrz.Enabled = Settings.Default.ESCANER_PASAPORTES.ToUpper().Equals("MRZ") ? true : false;
                //cmbSexo.SelectedValue = "1";
                scanDataModel = new ScanDataModel();

                chkPersonaSinHuellas.Enabled = true;
                chkPersonaSinHuellas.Checked = false;

                //INDICE_ESCANEO = 0;
                esVerificacionGuardarVisible = false;

                lblIdSolicitud.Text = lblNoCaso.Text = txtNoDocumento.Text = string.Empty;

                rbnAlCourier.Checked = rbnAlPadreoMadre.Checked = rbnAlRepresentante.Checked = rbnAlTitular.Checked = false;
                rbnAlPadreoMadre.Visible = true;
                rbnAlTitular.Visible = true;

                lbl_id_pais_sede.Text = string.Empty;

                btnGuardar.Visible = true;

                panel1.Visible = panel4.Visible = false;

                //lblEnvioBD.Text = "Sin archivos pendientes...";

                grpTipoEntrega.Enabled = true;
                if(lblNuevo.Text.Equals("0"))
                    pbxConectarEscanerP_Click(new object(), new EventArgs());
                
                pbxDocumentoEnEscaner.Image = pbxWarning.Image;
                pbxEscanearPasaporte.Image = pbxWarning.Image;

                chkEscanAutomatico.Checked = true;
                chkEscanAutomatico_Click(new object(), new EventArgs());
                pbxCompararHuellas.Image = pbxWarning.Image;

                rbnd1.Checked = rbnd2.Checked = rbnd2.Checked = rbnd4.Checked = rbnd5.Checked = rbnd6.Checked = rbnd7.Checked = rbnd8.Checked = rbnd9.Checked = rbnd10.Checked = false;
                nFVHuellaAlmacenada.Finger = nFVHuellaMovimiento.Finger = nFVH1.Finger = nFVH2.Finger = nFVPos1.Finger = nFVPos2.Finger = null;
                
                pbxDocumento.Image = pbxWarning.Image;

                if (Settings.Default.ENROLAR_DOCUMENTO == false)
                    pbxDocumento.Image = pbxCheck.Image;

                pbxPasaporteEscaneadoUV.Image = pbxWarning.Image;

                pbxAnverso.Image = ENROLLMENT_V3.Properties.Resources.anverso;
                pbxReverso.Image = ENROLLMENT_V3.Properties.Resources.reverso;

                pbxAnversoUV.Image = null;
                pbxReversoUV.Image = null;

                pbxAnversoUV.Visible = pbxReversoUV.Visible = Settings.Default.IMAGEN_UV_VISIBLE;

                pbxFotografia.Image = Settings.Default.ENROLAR_ROSTRO ? pbxWarning.Image : pbxCheck.Image;
                pbxCompararHuellas.Image = Settings.Default.ENROLAR_HUELLAS ? pbxWarning.Image : pbxCheck.Image;

                lstHuellas.Items.Clear();

                pbxDatosPersonales.Image = pbxWarning.Image;

                dsCatalogos = new DataSet();

                CargarComboBoxICAO(cmbPaisOrigen, true, "Pais");
                CargarComboBoxICAO(cmbPaisDestino, true, "Pais");

                CargarComboBoxICAO(cmbPaisResidencia, true, "Pais");
                CargarComboBoxICAO(cmbPaisNacimiento, true, "Pais");

                CargarComboBoxJson(cmbTipoDocumento, true, "TipoDocumento");
                CargarComboBox(cmbTipoPasaporte, true, "TipoPasaporte");
                CargarComboBoxICAO(cmbPaisEmisor, true, "Pais");
                CargarComboBoxICAO(cmbSexo, true, "Genero");
                CargarComboBoxICAO(cmbNacionalidad, true, "Pais");

                CargarComboBox(cmbCategoriaMigratoria, true, "CategoriaMigratoria");
                CargarComboBox(cmbCaracteristica, true, "Caracteristica");

                CargarComboBox(cmbMotivoViaje, true, "MotivoViaje");
                CargarComboBox(cmbProfesion, true, "Profesion");

                cmbTipoDocumento.DropDownStyle = ComboBoxStyle.DropDownList;
                cmbPaisEmisor.DropDownStyle = ComboBoxStyle.DropDownList;
                cmbSexo.DropDownStyle = ComboBoxStyle.DropDownList;
                cmbNacionalidad.DropDownStyle = ComboBoxStyle.DropDownList;

                cmbTipoDocumento.Enabled = cmbPaisEmisor.Enabled = cmbSexo.Enabled = cmbNacionalidad.Enabled = false;

                txtNumeroDocumento.Text = txtPrimerApellido.Text = txtSegundoApellido.Text = txtPrimerNombre.Text = txtSegundoNombre.Text = txtObservaciones.Text = string.Empty;
                txtNumeroDocumento.ReadOnly = false;
                txtPrimerApellido.ReadOnly = txtSegundoApellido.ReadOnly = txtPrimerNombre.ReadOnly = txtSegundoNombre.ReadOnly = true;

                dtpFechaNacimiento.Enabled = dtpFechaVencimiento.Enabled = true;
                dtpFechaNacimiento.Value = dtpFechaVencimiento.Value = DateTime.Today;

                chkFechaVencimiento.Checked = chkFechaVencimiento.Enabled = false;

                //((Control)tab_principal.Controls.Find("tabProbatorios", false)[0]).Visible = false;
                try { tab_principal.TabPages.Remove(tab_principal.TabPages[1]); } catch { };

                tabHuellas.TabPages[0].ImageKey = "warning.bmp";
                chkEnviarRS.Checked = false;

                chkPersonaSinHuellas.Checked = false;
                chkPersonaSinHuellas.Enabled = true;

                Escanner442 = false;

                //lblEnvioBD.Text = string.Empty;
                lbl_sede.Text = "DELEGACIÓN: " + sedeDataEquipo.nombredelegacion.ToUpper() + " / " + equipoData.nombre_tipo_flujo;
                hrsPasaporte = minPasaporte = segPasaporte = 0;

                pbxCheck.Image.Tag = "Check";
                pbxWarning.Image.Tag = "Warning";
                pbxLoad.Image.Tag = "Loading";
                pbxUsuario.Image.Tag = "FotoDefault";

                pbxCheckColor.Image.Tag = "Check";
                pbxWarningColor.Image.Tag = "Warning";
                pbxLoadColor.Image.Tag = "Loading";

                intentosMOCTitular = intentosMOCPadre = intentosMOCMadre = 1;

                parametrizacion = new PARAMETRIZACION();                

                pbxRostroIcao.Image = /*picb_usuario.Image =*/ pbxUsuario.Image;
                pbxRostroIcao.SizeMode = PictureBoxSizeMode.Zoom;

                System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmEnrolamiento));
                pbxFotoExterna.Image = ((System.Drawing.Image)(resources.GetObject("pbxFotoExterna.Image")));

                pbxDPI.Image = pbxWarning.Image;
                pbxMOCH.Image = pbxWarning.Image;
                pbxCONS.Image = pbxStart.Image;

                chkMRZ.Checked = false;
                chkMRZ_MouseClick(null, new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));
                txtMrz.Text = string.Empty;                

                //DATOS PARA MENORES
                pbxFotoDPIPadre.Image = pbxFotoDPIMadre.Image = pbxUsuario.Image;
                pbxDPIPadre.Image = pbxWarning.Image;
                pbxMOCHPadre.Image = pbxWarning.Image;

                pbxDPIMadre.Image = pbxWarning.Image;
                pbxMOCHMadre.Image = pbxWarning.Image;            
                
                lblCuiPadre.Text = lblCuiMadre.Text = "CUI";               

                ActivarControlesRostro(true);

                _subject442.Fingers.Clear();                
                
                pbxHitSibio.Image = pbxWarning.Image;                

                Settings.Default.CITA_ID = string.Empty;
                if (Settings.Default.CONSULTAR_CITA) txtMrz.Select();                

                pbxDPI.Enabled = pbxMOCH.Enabled = false;
                btnLeerDPIPadre.Enabled = false;
                btnLeerDPIMadre.Enabled = false;
                pbxDPIPadre.Enabled = pbxMOCHPadre.Enabled = pbxDPIMadre.Enabled = pbxMOCHMadre.Enabled = false;

                foto = no_caso = tipo_pasaporte = nombres = apellidos = apellido_casada = direccion = tel_casa = tel_trabajo = tel_celular = correo = pais = sexo = estado_civil = nacionalidad = fecha_nacimiento = string.Empty;
                depto_nacimiento = muni_nacimiento = pais_nacimiento = identificacion = depto_emision = municipio_emision = color_ojos = color_tez = color_cabello = estatura = padre = madre = sede_entrega = string.Empty;
                partida_nacimiento = libro = folio = acta = pasaporte_autorizado = identificacion_padre = identificacion_madre = autorizado_dgm = usuario = estacion = lugar_fecha = cui_menor = tipo_entrega = string.Empty;
                direccion_entrega1 = direccion_entrega2 = direccion_entrega3 = string.Empty;

                //btnCapturarHuellas442.Enabled = false;
                //btnActivarHuellas11.Enabled = false;
                //btnNuevaCapHuellas442.Enabled = false;

                //ELIMINANDO ARCHIVOS TEMPORALES
                if(Settings.Default.ELIMINAR_TEMPORALES)
                {
                    string carpetaArchivos = Path.Combine(System.Windows.Forms.Application.StartupPath, "ENROL", "json") + "\\";
                    string[] files = Directory.GetFiles(carpetaArchivos, "*.jpeg");

                    if (files.Length > 0)
                    {
                        tmrEnvioBD.Enabled = false;
                        lblEnvioBD.Text = "Eliminando archivos temporales (" + files.Length + ")...";

                        foreach (string file in files)
                            try { File.Delete(file); } catch (Exception) { throw; }

                        lblEnvioBD.Text = "Archivos eliminados exitosamente..";
                        tmrEnvioBD.Enabled = true;
                    }
                }

                //if (Settings.Default.ENROLAR_HUELLAS)
                //{
                //    ListarEscanersHuellas(cmbEscanerHuellas, true, Settings.Default.FILTRAR_ESCANER_HUELLAS, PARAMETRIZACION.TipoEscanerHuellas.Unidactilar);

                //    rbnd2.Checked = false;
                //    rbnd6.Checked = rbnd7.Checked = rbnd8.Checked = rbnd9.Checked = rbnd10.Checked = false;

                //}

                if (Settings.Default.ENROLAR_ROSTRO)
                {
                    //CAMARAS
                    if (Settings.Default.CONTROLADOR_WEBCAM.Equals("AFORGE") && videoCaptureDevice == null)
                    {
                        facesView.Visible = false;
                        pbxVideo.Visible = true;

                        facesView.Controls.Remove(btnCapturarRostro);
                        facesView.Controls.Remove(grpCamaras);

                        pbxVideo.Controls.Add(btnCapturarRostro);
                        pbxVideo.Controls.Add(grpCamaras);

                        grpFotografia.Controls.Remove(facesView);
                        grpFotografia.Controls.Add(pbxVideo);
                        pbxVideo.Location = new Point(10, 20);

                        filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                        if (filterInfoCollection.Count <= 0)
                            MessageBox.Show("No se encontraron cámaras disponibles.");

                        CameraListBox.Items.Clear();

                        if (filterInfoCollection.Count > 0)
                        {
                            int index = -1;
                            String monikerString = string.Empty;
                            if (filterInfoCollection.Count > 0)
                            {
                                for (int i = 0; i < filterInfoCollection.Count; i++)
                                {
                                    CameraListBox.Items.Add(filterInfoCollection[i].Name);

                                    if (Settings.Default.FILTRAR_CAMARA)
                                        if (monikerString.Equals(string.Empty))
                                            if (filterInfoCollection[i].Name.Equals(Settings.Default.FILTRO_CAMARA))
                                            {
                                                index = i;
                                                monikerString = filterInfoCollection[i].MonikerString;

                                                camara = new Dispositivo();
                                                camara.make = filterInfoCollection[i].Name;
                                                camara.model = "N/A";
                                                camara.serialnumber = monikerString;
                                            }

                                }
                            }

                            if (Settings.Default.FILTRAR_CAMARA)
                            {
                                if (index == -1)
                                    MessageBox.Show("Cámara: " + Settings.Default.FILTRO_CAMARA + " no encontrada. ");

                                if (index > -1)
                                {
                                    CameraListBox.SelectedIndex = index;

                                    if (videoCaptureDevice != null)
                                        if (videoCaptureDevice.IsRunning)
                                            videoCaptureDevice.Stop();

                                    videoCaptureDevice = new VideoCaptureDevice(monikerString);
                                    videoCaptureDevice.NewFrame += CapturarImagen;
                                    videoCaptureDevice.Start();
                                }
                            }
                            else
                            {
                                if (CameraListBox.Items.Count > 0)
                                {
                                    CameraListBox.SelectedIndex = 0;

                                    if (videoCaptureDevice != null)
                                        if (videoCaptureDevice.IsRunning)
                                            videoCaptureDevice.Stop();

                                    camara = new Dispositivo();
                                    camara.make = filterInfoCollection[0].Name;
                                    camara.model = "N/A";
                                    camara.serialnumber = filterInfoCollection[0].MonikerString;

                                    videoCaptureDevice = new VideoCaptureDevice(filterInfoCollection[0].MonikerString);
                                    videoCaptureDevice.NewFrame += CapturarImagen;
                                    videoCaptureDevice.Start();
                                }
                            }
                        }
                    }

                    if (Settings.Default.CONTROLADOR_WEBCAM.Equals("NEURO"))
                    {

                        //await Iniciar_Cliente_Fotos(new Object(), new EventArgs());

                        this.facesView.Visible = true;
                        this.pbxVideo.Visible = false;

                        ListarCamaras(CameraListBox, true);

                        if (_biometricFaceClient.CurrentBiometric == null)
                        {
                            DataSet dsActivarCamara = await funciones.ActivarCamara(facesView, _biometricFaceClient);
                            if (bool.Parse(dsActivarCamara.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                                throw new Exception(dsActivarCamara.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                        }
                    }
                }

                btnCapturarRostro.Visible = false;
                grpCamaras.Visible = false;

                tab_principal.SelectedIndex = 0;
                tabAnversoReverso.SelectedIndex = 0;
                txtMrz.Select();
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "NuevoIngreso(). " + ex.Message;
                MessageBox.Show("NuevoIngreso(). " + ex.Message);
            }
        }

        private void CapturarImagen(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                lock (LvLock)
                {
                    Evf_Bmp?.Dispose();
                    Evf_Bmp = new Bitmap(eventArgs.Frame);
                }
                pbxVideo.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show("CapturarImagen(). " + ex.Message);
            }
        }

        private void btnSiguiente_Click(object sender, EventArgs e)
        {
            try
            {
                tab_principal.SelectedIndex = (tab_principal.SelectedIndex + 1 < tab_principal.TabCount) ? tab_principal.SelectedIndex + 1 : tab_principal.SelectedIndex;
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "btnSiguiente_Click(). " + ex.Message;
                MessageBox.Show("btnSiguiente_Click(). " + ex.Message);
            }
        }

        private void btnAnterior_Click(object sender, EventArgs e)
        {
            try
            {
                tab_principal.SelectedIndex = (tab_principal.SelectedIndex - 1 >= 0) ? tab_principal.SelectedIndex - 1 : tab_principal.SelectedIndex;
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "btnAnterior_Click(). " + ex.Message;
                MessageBox.Show("btnAnterior_Click(). " + ex.Message);
            }
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = MessageBox.Show("¿Está seguro que desea SALIR?", "Salir", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                    this.Close();
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "btnSalir_Click(). " + ex.Message;
                MessageBox.Show("btnSalir_Click(). " + ex.Message);
            }
        }
        

        internal static string foto, no_caso, tipo_pasaporte, nombres, apellidos, apellido_casada, direccion, tel_casa, tel_trabajo, tel_celular, correo, pais, sexo, estado_civil, nacionalidad, fecha_nacimiento,
            depto_nacimiento, muni_nacimiento, pais_nacimiento, identificacion, depto_emision, municipio_emision, color_ojos, color_tez, color_cabello, estatura, padre, madre, sede_entrega,
            partida_nacimiento, libro, folio, acta, pasaporte_autorizado, identificacion_padre, identificacion_madre, autorizado_dgm, usuario, estacion, lugar_fecha, cui_menor, tipo_entrega,
            direccion_entrega1, direccion_entrega2, direccion_entrega3;

        private async void btnLeerDPIPadre_Click(object sender, EventArgs e)
        {
            try
            {
                pbxDPIPadre.Image = pbxWarning.Image;
                pbxMOCHPadre.Image = pbxWarning.Image;

                DialogResult result = MessageBox.Show("¡Inserte la tarjeta en el lector y espere a que se realice la lectura!", "Lectura DPI", MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK)
                {
                    panel_inferior.Enabled = picb_cerrar.Enabled = false;

                    pbxDPIPadre.Image = pbxLoad.Image;

                    dpiPadre = await LeerDPIAsync();

                    if (dpiPadre.INFORMACION_DPI_LEIDA == false)
                        throw new Exception(dpiPadre.MENSAJE_ERROR);

                    if (dpiPadre.SEXO.Contains("MASCULINO") == false && dpiPadre.SEXO.Equals("M") == false)
                        throw new Exception("¡El género del padre debe ser MASCULINO!");
                    

                    dpiPadre.PARENTESCO = "Padre";
                    lblCuiPadre.Text = dpiPadre.CUI;
                    
                    pbxFotoDPIPadre.Image = dpiPadre.IMAGE;
                    pbxFotoDPIPadre.Image.Tag = "FotoPadre";

                    MessageBox.Show("¡Información leída correctamente!", "Lectura DPI", MessageBoxButtons.OK);

                    pbxDPIPadre.Image = pbxCheck.Image;

                    for (int intentosMOCTitular = 1; intentosMOCTitular <= parametrizacion.INTENTOS_MOC_TITULAR; intentosMOCTitular++)
                    {
                        pbxMOCHPadre.Image = pbxWarning.Image;

                        if (intentosMOCTitular == 1)
                            MessageBox.Show("¡Inserte la tarjeta en el lector y coloque el dedo cuando la luz del sensor se encienda! (" + intentosMOCTitular + "/" + parametrizacion.INTENTOS_MOC_TITULAR.ToString() + ")", "Match on Card", MessageBoxButtons.OK);
                        else
                            MessageBox.Show("¡Coloque el dedo cuando la luz del sensor se encienda! (" + intentosMOCTitular + "/" + parametrizacion.INTENTOS_MOC_TITULAR.ToString() + "), No retire el DPI", "Match on Card", MessageBoxButtons.OK);
                   
                        pbxMOCHPadre.Image = pbxLoad.Image;

                        _biometricFingerClient.Cancel();

                        DataSet dsMOC = await MOC_DPI(lblCuiPadre.Text);
                        bool MOC = bool.Parse(dsMOC.Tables[0].Rows[0]["RESULTADO"].ToString());
                        string msgError = dsMOC.Tables[0].Rows[0]["MSG_ERROR"].ToString();

                        if (MOC == false && msgError.Equals(string.Empty) == false)
                            throw new Exception(dsMOC.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                        if (MOC)
                        {
                            MessageBox.Show("¡MOC exitoso! (" + intentosMOCTitular + "/" + parametrizacion.INTENTOS_MOC_TITULAR + ")", "Match on Card", MessageBoxButtons.OK);
                            intentosMOCTitular = parametrizacion.INTENTOS_MOC_TITULAR + 1;

                            this.Enabled = tab_principal.Enabled = true;

                            pbxMOCHPadre.Image = pbxCheck.Image;
                        }
                        else
                        {
                            if (intentosMOCTitular < parametrizacion.INTENTOS_MOC_TITULAR)
                                MessageBox.Show("¡Las huellas no coinciden, se realizará un nuevo intento, ¡NO retire el DPI!", "Match on Card", MessageBoxButtons.OK);
                            else
                            {
                                MessageBox.Show("¡Límite de intentos MOC alcanzado (" + intentosMOCTitular + "/" + parametrizacion.INTENTOS_MOC_TITULAR + ")", "Match on Card", MessageBoxButtons.OK);
                                this.Enabled = tab_principal.Enabled = true;
                            }

                            pbxMOCHPadre.Image = pbxWarning.Image;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "btnLeerDPIPadre_Click(). " + ex.Message;
                MessageBox.Show("btnLeerDPIPadre_Click(). " + ex.Message);

                pbxDPIPadre.Image = pbxWarning.Image;
                pbxMOCHPadre.Image = pbxWarning.Image;
            }
            panel_inferior.Enabled = picb_cerrar.Enabled = true;
        }

        private void pbxFotoDPIPadre_Click(object sender, EventArgs e)
        {
        }

        private async void btnLeerDPIMadre_Click(object sender, EventArgs e)
        {
            try
            {
                pbxDPIMadre.Image = pbxWarning.Image;
                pbxMOCHMadre.Image = pbxWarning.Image;

                DialogResult result = MessageBox.Show("¡Inserte la tarjeta en el lector y espere a que se realice la lectura!", "Lectura DPI", MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK)
                {
                    panel_inferior.Enabled = picb_cerrar.Enabled = false;

                    pbxDPIMadre.Image = pbxLoad.Image;

                    dpiMadre = await LeerDPIAsync();

                    if (dpiMadre.INFORMACION_DPI_LEIDA == false)
                        throw new Exception(dpiMadre.MENSAJE_ERROR);

                    if (dpiMadre.SEXO.Contains("FEMENINO") == false && dpiMadre.SEXO.Equals("F") == false)
                        throw new Exception("¡El género del madre debe ser FEMENINO!");
                    
                    pbxFotoDPIMadre.Image = dpiMadre.IMAGE;
                    pbxFotoDPIMadre.Image.Tag = "FotoMadre";

                    MessageBox.Show("¡Información leída correctamente!", "Lectura DPI", MessageBoxButtons.OK);

                    pbxDPIMadre.Image = pbxCheck.Image;

                    for (int intentosMOCTitular = 1; intentosMOCTitular <= parametrizacion.INTENTOS_MOC_TITULAR; intentosMOCTitular++)
                    {
                        pbxMOCHMadre.Image = pbxWarning.Image;

                        if (intentosMOCTitular == 1)
                            MessageBox.Show("¡Inserte la tarjeta en el lector y coloque el dedo cuando la luz del sensor se encienda! (" + intentosMOCTitular + "/" + parametrizacion.INTENTOS_MOC_TITULAR.ToString() + ")", "Match on Card", MessageBoxButtons.OK);
                        else
                            MessageBox.Show("¡Coloque el dedo cuando la luz del sensor se encienda! (" + intentosMOCTitular + "/" + parametrizacion.INTENTOS_MOC_TITULAR.ToString() + "), No retire el DPI", "Match on Card", MessageBoxButtons.OK);

                        pbxMOCHMadre.Image = pbxLoad.Image;

                        _biometricFingerClient.Cancel();

                        DataSet dsMOC = await MOC_DPI(lblCuiMadre.Text);
                        bool MOC = bool.Parse(dsMOC.Tables[0].Rows[0]["RESULTADO"].ToString());
                        string msgError = dsMOC.Tables[0].Rows[0]["MSG_ERROR"].ToString();

                        if (MOC == false && msgError.Equals(string.Empty) == false)
                            throw new Exception(dsMOC.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                        if (MOC)
                        {
                            MessageBox.Show("¡MOC exitoso! (" + intentosMOCTitular + "/" + parametrizacion.INTENTOS_MOC_TITULAR + ")", "Match on Card", MessageBoxButtons.OK);
                            intentosMOCTitular = parametrizacion.INTENTOS_MOC_TITULAR + 1;

                            this.Enabled = tab_principal.Enabled = true;

                            pbxMOCHMadre.Image = pbxCheck.Image;
                        }
                        else
                        {
                            if (intentosMOCTitular < parametrizacion.INTENTOS_MOC_TITULAR)
                                MessageBox.Show("¡Las huellas no coinciden, se realizará un nuevo intento, ¡NO retire el DPI!", "Match on Card", MessageBoxButtons.OK);
                            else
                            {
                                MessageBox.Show("¡Límite de intentos MOC alcanzado (" + intentosMOCTitular + "/" + parametrizacion.INTENTOS_MOC_TITULAR + ")", "Match on Card", MessageBoxButtons.OK);
                                this.Enabled = tab_principal.Enabled = true;
                            }

                            pbxMOCHMadre.Image = pbxWarning.Image;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "btnLeerDPIMadre_Click(). " + ex.Message;
                MessageBox.Show("btnLeerDPIMadre_Click(). " + ex.Message);

                pbxDPIMadre.Image = pbxWarning.Image;
                pbxMOCHMadre.Image = pbxWarning.Image;
            }
            panel_inferior.Enabled = picb_cerrar.Enabled = true;
        }

        private void pbxFotoDPIMadre_Click(object sender, EventArgs e)
        {            
        }

        private async void pbxDPI_Click(object sender, EventArgs e)
        {
            try
            {
                if (Settings.Default.CONSULTAR_CITA) throw new Exception("Está función está desabilitada. ");

                if (pbxMOCH.Image.Tag.Equals("Loading")) throw new Exception("¡Espere que finalice la lectura del DPI!");

                if (pbxDPI.Image.Tag.Equals("Loading")) MessageBox.Show("¡La operación de lectura ya se encuentra en proceso!");
                else
                {
                    DialogResult result = MessageBox.Show("¡Inserte la tarjeta en el lector y espere a que se realice la lectura!", "Lectura DPI", MessageBoxButtons.OKCancel);
                    if (result == DialogResult.OK)
                    {
                        pbxDPI.Image = pbxWarning.Image;
                        pbxMOCH.Image = pbxWarning.Image;

                        pbxDPI.Image = pbxLoad.Image;
                        dpiTitular = await LeerDPIAsync();

                        if (dpiTitular.INFORMACION_DPI_LEIDA == false)
                            throw new Exception(dpiTitular.MENSAJE_ERROR);

                        //lbl_nombres_info.Text = dpiTitular.PRIMER_NOMBRE.Trim() + " " + dpiTitular.SEGUNDO_NOMBRE.Trim();
                        //lbl_apellidos_info.Text = dpiTitular.PRIMER_APELLIDO + " " + dpiTitular.SEGUNDO_APELLIDO;                            
                    }
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "pbxDPI_Click(). " + ex.Message;
                MessageBox.Show("pbxDPI_Click(). " + ex.Message);
                pbxDPI.Image = pbxWarning.Image;
            }
            //panel_inferior.Enabled = picb_cerrar.Enabled = true;
            //tabHuellas.Enabled = true;
        }

        private async void pbxMOCH_Click(object sender, EventArgs e)
        {
            try
            {
                if(!Settings.Default.CONSULTAR_CITA)
                {
                    if (pbxDPI.Image.Tag.Equals("Warning"))
                        throw new Exception("¡Primero lea la información del DPI!");

                    if (pbxDPI.Image.Tag.Equals("Loading"))
                        throw new Exception("¡Espere que finalice la lectura del DPI!");
                }
                
                if (pbxMOCH.Image.Tag.Equals("Loading"))
                    MessageBox.Show("¡La operación de MOC ya se encuentra en proceso!");
                else
                {
                    if (intentosMOCTitular >= (parametrizacion.INTENTOS_MOC_TITULAR + 1))
                        throw new Exception("¡Límite de intentos máximo (" + parametrizacion.INTENTOS_MOC_TITULAR + ")alcanzado!");

                    DialogResult result = MessageBox.Show("¡Inserte la tarjeta en el lector y espere a que se realice la lectura!", "Lectura DPI", MessageBoxButtons.OKCancel);
                    if (result == DialogResult.OK)
                    {
                        pbxMOCH.Image = pbxWarning.Image;

                        //panel_inferior.Enabled = picb_cerrar.Enabled = false;                                     

                        _biometricFingerClient.Force();

                        MessageBox.Show("¡Coloque el dedo en el sensor y espere hasta que la validación finalice! (" + (intentosMOCTitular) + "/" + parametrizacion.INTENTOS_MOC_TITULAR.ToString() + "), No retire el DPI", "Match on Card", MessageBoxButtons.OK);

                        pbxMOCH.Image = pbxLoad.Image;
                        
                        DataSet dsMOC = await MOC_DPI("");

                        bool MOC = bool.Parse(dsMOC.Tables[0].Rows[0]["RESULTADO"].ToString());
                        string msgError = dsMOC.Tables[0].Rows[0]["MSG_ERROR"].ToString();


                        if (MOC == false && msgError.Equals(string.Empty) == false)
                            throw new Exception(dsMOC.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                        if (MOC)
                        {
                            MessageBox.Show("¡MOC exitoso! (" + (intentosMOCTitular) + "/" + parametrizacion.INTENTOS_MOC_TITULAR + ")", "Match on Card", MessageBoxButtons.OK);
                            pbxMOCH.Image = pbxCheck.Image;
                            intentosMOCTitular--;
                        }
                        else
                        {
                            MessageBox.Show("¡Las huellas no coinciden! (" + (intentosMOCTitular) + "/" + parametrizacion.INTENTOS_MOC_TITULAR + ")", "Match on Card", MessageBoxButtons.OK);
                            pbxMOCH.Image = pbxWarning.Image;
                        }
                        intentosMOCTitular++;
                    }
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "pbxMOCH_Click(). " + ex.Message;
                MessageBox.Show("pbxMOCH_Click(). " + ex.Message);
                pbxMOCH.Image = pbxWarning.Image;
            }
            //panel_inferior.Enabled = picb_cerrar.Enabled = true;
        }

        private async void pbxDPIPadre_Click(object sender, EventArgs e)
        {
            try
            {
                if (pbxMOCHMadre.Image.Tag.Equals("Loading") || pbxDPIMadre.Image.Tag.Equals("Loading"))
                    throw new Exception("¡Espere que finalice la lectura del DPI de la madre!");

                if (pbxMOCHPadre.Image.Tag.Equals("Loading"))
                    throw new Exception("¡Espere que finalice la lectura del DPI!");

                if (pbxDPIPadre.Image.Tag.Equals("Loading"))
                    MessageBox.Show("¡La operación de lectura ya se encuentra en proceso!");
                else
                {
                    DialogResult result = MessageBox.Show("¡Inserte la tarjeta en el lector y espere a que se realice la lectura!", "Lectura DPI", MessageBoxButtons.OKCancel);
                    if (result == DialogResult.OK)
                    {
                        pbxDPIPadre.Image = pbxWarning.Image;
                        pbxDPIPadre.Image = pbxLoad.Image;

                        dpiPadre = await LeerDPIAsync();

                        if (dpiPadre.INFORMACION_DPI_LEIDA == false)
                            throw new Exception(dpiPadre.MENSAJE_ERROR);

                        if (dpiPadre.SEXO.Contains("MASCULINO") == false && dpiPadre.SEXO.Equals("M") == false)
                            throw new Exception("¡El género del padre debe ser MASCULINO!");

                        if (dpiPadre.IMAGE != null) pbxFotoDPIPadre.Image = dpiPadre.IMAGE;
                        pbxFotoDPIPadre.Image.Tag = "FotoPadre";

                        MessageBox.Show("¡Información leída correctamente!", "Lectura DPI", MessageBoxButtons.OK);
                        pbxDPIPadre.Image = pbxCheck.Image;
                    }
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "pbxDPIPadre_Click(). " + ex.Message;
                MessageBox.Show("pbxDPIPadre_Click(). " + ex.Message);
                pbxDPIPadre.Image = pbxWarning.Image;
            }
        }

        private async void pbxMOCHPadre_Click(object sender, EventArgs e)
        {
            try
            {
                if (pbxMOCHMadre.Image.Tag.Equals("Loading") || pbxDPIMadre.Image.Tag.Equals("Loading"))
                    throw new Exception("¡Espere que finalice la lectura del DPI de la madre!");

                if (pbxDPIPadre.Image.Tag.Equals("Warning"))
                    throw new Exception("¡Primero lea la información del DPI!");

                if (pbxDPIPadre.Image.Tag.Equals("Loading"))
                    throw new Exception("¡Espere que finalice la lectura del DPI!");

                if (pbxMOCHPadre.Image.Tag.Equals("Loading"))
                    MessageBox.Show("¡La operación de MOC ya se encuentra en proceso!");
                else
                {
                    if (intentosMOCPadre >= (parametrizacion.INTENTOS_MOC_PADRE + 1))
                        throw new Exception("¡Límite de intentos máximo (" + parametrizacion.INTENTOS_MOC_PADRE + ")alcanzado!");

                    DialogResult result = MessageBox.Show("¡Inserte la tarjeta en el lector y espere a que se realice la lectura!", "Lectura DPI", MessageBoxButtons.OKCancel);
                    if (result == DialogResult.OK)
                    {
                        pbxMOCHPadre.Image = pbxWarning.Image;

                        MessageBox.Show("¡Coloque el dedo en el sensor y espere hasta que la validación finalice! (" + intentosMOCPadre + "/" + parametrizacion.INTENTOS_MOC_PADRE.ToString() + "), No retire el DPI", "Match on Card", MessageBoxButtons.OK);

                        pbxMOCHPadre.Image = pbxLoad.Image;

                        _biometricFingerClient.Force();

                        DataSet dsMOC = await MOC_DPI(lblCuiPadre.Text.Trim());
                        bool MOC = bool.Parse(dsMOC.Tables[0].Rows[0]["RESULTADO"].ToString());
                        string msgError = dsMOC.Tables[0].Rows[0]["MSG_ERROR"].ToString();

                        if (MOC == false && msgError.Equals(string.Empty) == false)
                            throw new Exception(dsMOC.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                        if (MOC)
                        {
                            MessageBox.Show("¡MOC exitoso! (" + intentosMOCPadre + "/" + parametrizacion.INTENTOS_MOC_PADRE + ")", "Match on Card", MessageBoxButtons.OK);
                            intentosMOCPadre--;
                            pbxMOCHPadre.Image = pbxCheck.Image;
                        }
                        else
                        {
                            MessageBox.Show("¡Las huellas no coinciden! (" + (intentosMOCPadre) + "/" + parametrizacion.INTENTOS_MOC_PADRE + ")", "Match on Card", MessageBoxButtons.OK);
                            pbxMOCHPadre.Image = pbxWarning.Image;
                        }
                        intentosMOCPadre++;
                    }
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "pbxMOCHPadre_Click(). " + ex.Message;
                MessageBox.Show("pbxMOCHPadre_Click(). " + ex.Message);
                pbxMOCHPadre.Image = pbxWarning.Image;
            }
        }

        private async void pbxDPIMadre_Click(object sender, EventArgs e)
        {
            try
            {
                if (pbxMOCHPadre.Image.Tag.Equals("Loading") || pbxDPIPadre.Image.Tag.Equals("Loading"))
                    throw new Exception("¡Espere que finalice la lectura del DPI de la padre!");

                if (pbxMOCHMadre.Image.Tag.Equals("Loading"))
                    throw new Exception("¡Espere que finalice la lectura del DPI!");

                if (pbxDPIMadre.Image.Tag.Equals("Loading"))
                    MessageBox.Show("¡La operación de lectura ya se encuentra en proceso!");
                else
                {
                    DialogResult result = MessageBox.Show("¡Inserte la tarjeta en el lector y espere a que se realice la lectura!", "Lectura DPI", MessageBoxButtons.OKCancel);
                    if (result == DialogResult.OK)
                    {
                        pbxDPIMadre.Image = pbxWarning.Image;
                        pbxDPIMadre.Image = pbxLoad.Image;

                        dpiMadre = await LeerDPIAsync();

                        if (dpiMadre.INFORMACION_DPI_LEIDA == false)
                            throw new Exception(dpiMadre.MENSAJE_ERROR);

                        if (dpiMadre.SEXO.Contains("FEMENINO") == false && dpiMadre.SEXO.Equals("F") == false)
                            throw new Exception("¡El género de la madre debe ser FEMENINO!");                       

                        if(dpiMadre.IMAGE!=null) pbxFotoDPIMadre.Image = dpiMadre.IMAGE;
                        pbxFotoDPIMadre.Image.Tag = "FotoMadre";

                        MessageBox.Show("¡Información leída correctamente!", "Lectura DPI", MessageBoxButtons.OK);
                        pbxDPIMadre.Image = pbxCheck.Image;
                    }
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "pbxDPIMadre_Click(). " + ex.Message;
                MessageBox.Show("pbxDPIMadre_Click(). " + ex.Message);
                pbxDPIMadre.Image = pbxWarning.Image;
            }
        }

        private async void pbxMOCHMadre_Click(object sender, EventArgs e)
        {
            try
            {
                if (pbxMOCHPadre.Image.Tag.Equals("Loading") || pbxDPIPadre.Image.Tag.Equals("Loading"))
                    throw new Exception("¡Espere que finalice la lectura del DPI de la padre!");

                if (pbxDPIMadre.Image.Tag.Equals("Warning"))
                    throw new Exception("¡Primero lea la información del DPI!");

                if (pbxDPIMadre.Image.Tag.Equals("Loading"))
                    throw new Exception("¡Espere que finalice la lectura del DPI!");

                if (pbxMOCHMadre.Image.Tag.Equals("Loading"))
                    MessageBox.Show("¡La operación de MOC ya se encuentra en proceso!");
                else
                {
                    if (intentosMOCMadre >= (parametrizacion.INTENTOS_MOC_MADRE + 1))
                        throw new Exception("¡Límite de intentos máximo (" + parametrizacion.INTENTOS_MOC_MADRE + ")alcanzado!");

                    DialogResult result = MessageBox.Show("¡Inserte la tarjeta en el lector y espere a que se realice la lectura!", "Lectura DPI", MessageBoxButtons.OKCancel);
                    if (result == DialogResult.OK)
                    {
                        pbxMOCHMadre.Image = pbxWarning.Image;

                        MessageBox.Show("¡Coloque el dedo en el sensor y espere hasta que la validación finalice! (" + intentosMOCMadre + "/" + parametrizacion.INTENTOS_MOC_MADRE.ToString() + "), No retire el DPI", "Match on Card", MessageBoxButtons.OK);

                        pbxMOCHMadre.Image = pbxLoad.Image;

                        _biometricFingerClient.Force();

                        DataSet dsMOC = await MOC_DPI(lblCuiMadre.Text);
                        bool MOC = bool.Parse(dsMOC.Tables[0].Rows[0]["RESULTADO"].ToString());
                        string msgError = dsMOC.Tables[0].Rows[0]["MSG_ERROR"].ToString();

                        if (MOC == false && msgError.Equals(string.Empty) == false)
                            throw new Exception(dsMOC.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                        if (MOC)
                        {
                            MessageBox.Show("¡MOC exitoso! (" + intentosMOCMadre + "/" + parametrizacion.INTENTOS_MOC_MADRE + ")", "Match on Card", MessageBoxButtons.OK);
                            intentosMOCMadre--;
                            pbxMOCHMadre.Image = pbxCheck.Image;
                        }
                        else
                        {
                            MessageBox.Show("¡Las huellas no coinciden! (" + (intentosMOCMadre) + "/" + parametrizacion.INTENTOS_MOC_MADRE + ")", "Match on Card", MessageBoxButtons.OK);
                            pbxMOCHMadre.Image = pbxWarning.Image;
                        }
                        intentosMOCMadre++;
                    }
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "pbxMOCHMadre_Click(). " + ex.Message;
                MessageBox.Show("pbxMOCHMadre_Click(). " + ex.Message);
                pbxMOCHMadre.Image = pbxWarning.Image;
            }
        }        

        private void btnResumen_Click(object sender, EventArgs e)
        {            
        }

        private void btnDetalle_Click(object sender, EventArgs e)
        {            
        }

        private void pbxAlertas_Click(object sender, EventArgs e)
        {
            try
            {
                if (Settings.Default.CONSULTAR_ALERTAS == false)
                    MessageBox.Show("Opcion desactivada, consulte al administrador.");

                //if (pbxAlertas.Image == pbxStart.Image || pbxAlertas.Image == pbxLoad.Image)
                //    throw new Exception("");

                if (pbxAlertas.Image == pbxError.Image)
                {
                    string error = pbxAlertas.Tag.ToString();
                    MessageBox.Show(error);
                }
                Task.Run(() =>
                {
                    ConsultarAlertas();
                });

            }
            catch (Exception ex)
            {
                txtMensaje.Text = "pbxAlertas_Click(). " + ex.Message;
                MessageBox.Show("pbxAlertas_Click(). " + ex.Message);
            }
        }

        private void Label31_DoubleClick(object sender, EventArgs e)
        {
            MessageBox.Show("A partir de la versión 27 se tienen los siguientes cambios: Parametrización del servidor FTP, modalidad de llave en disco. Utilización de utiliría copilada en .NET Core (YA NO JEJE). En la versión 28 (22/08/2019) se agregó: Actualización del MM SDK a versión 11, parametrización para Match de dactilar y facial en servidor remoto, se agregó emisión para caso especial DIPLOMATICO Art. 98 Cod. Migración. En la versión 29, se amplió la cantidad de memmoria que puede manejar la aplicación de 32 bits y se cambió el control IcaoWarningView a una PictureBox. En la versión 30 se parametrizó el nombre de la carpeta FTP. En la versión 32, se lee el QR de la cita, se corrigió error en reporte para pasaporte DIPLOMÁTICO MENOR.");
        }
        
        private void Pic_logo_dgm_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                //ProcesarRostrosNeuroStart();
            }
            catch (Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": " + ex.Message);
            }
        }
       
        private void LblEncriptar_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (lblEncriptar.Text.Equals("") == false)
                {
                    string rutaXML = Path.Combine(System.Windows.Forms.Application.StartupPath, "ENROL", "XMLs", "SftpData", "CasoPasaporte_" + "" + ".xml");

                    string horaCaptura = DateTime.Now.ToString("HH:mm:ss");
                    string fechaCaptura = DateTime.Now.ToString("dd/MM/yyyy");

                    StringReader strReader = new StringReader(lblEncriptar.Text);
                    DataSet dsParamEncript = new DataSet();
                    dsParamEncript.ReadXml(strReader);

                    if (dsParamEncript == null)
                        throw new Exception("¡El dataset es nulo!");

                    if (dsParamEncript.Tables.Count == 0)
                        throw new Exception("¡No existen tablas!");

                    if (dsParamEncript.Tables[0].Rows.Count == 0)
                        throw new Exception("¡No existen filas!");

                    if (bool.Parse(dsParamEncript.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                        throw new Exception(dsParamEncript.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                    //if (bool.Parse(dsEncriptarArchivo.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                    //    throw new Exception("¡Error al encriptar el archivo, contacte al administrador del sistema!. " + dsEncriptarArchivo.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                    string rutaArchivoEncriptado = Path.Combine(System.Windows.Forms.Application.StartupPath, "ENROL", "XMLs") + "\\" + Path.GetFileNameWithoutExtension(rutaXML) + "_" + Environment.MachineName + "_" + lbl_usuario.Text + "_" + Properties.Settings.Default.SUFIJO_ENCRIPTACION + ".txt";                    

                    tmrPasaporte.Enabled = false;

                    int iCorrelativo = 0;
                    try
                    {
                        string rutaReporteResumen = System.Windows.Forms.Application.StartupPath + "\\ENROL\\ReporteCasos\\" + DateTime.Now.ToString("dd/MM/yyyy").Replace("/", "") + ".xml";

                        if (!File.Exists(rutaReporteResumen))
                        {
                            XDocument document = new XDocument(new XDeclaration("1.0", "utf-8", null));

                            //Creamos el nodo raiz y lo añadimos al documento
                            XElement nodoRaiz = new XElement("Casos");
                            document.Add(nodoRaiz);
                            document.Save(rutaReporteResumen);
                            iCorrelativo = 1;
                        }

                        XDocument xmlDoc = XDocument.Load(rutaReporteResumen);

                        if (iCorrelativo == 0)
                            iCorrelativo = int.Parse(xmlDoc.Elements("Casos").Elements("Caso").Last().Element("Correlativo").Value) + 1;

                        //xmlDoc.Elements("Casos").Last().Add(new XElement("Caso", new XElement("Correlativo", iCorrelativo.ToString()), new XElement("Hora", horaCaptura), new XElement("NoCaso", txtNoCaso.Text), new XElement("Nombres", txtPrimerNombre.Text + " " + txtSegundoNombre.Text + " " + txtTercerNombre.Text), new XElement("Apellidos", txtPrimerApellido.Text + " " + txtSegundoApellido.Text), new XElement("FechaNacimiento", fechaN.ToString("dd/MM/yyyy")), new XElement("Usuario", lbl_usuario.Text), new XElement("NombreUsuario", lblNombreUsuario.Text), new XElement("SedeCaptura", sedeEstacion.CIUDAD + ", " + sedeEstacion.PAIS), new XElement("FechaCaptura", fechaCaptura), new XElement("EstacionCaptura", Environment.MachineName)));
                        xmlDoc.Save(rutaReporteResumen);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("¡Error a bitacorizar! " + ex.Message);
                    }                                       

                    //ProcesarRostrosNeuroStart();

                    MessageBox.Show("Almacenado con éxito! ");
                    txtMensaje.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "LblEncriptar_TextChanged(). " + ex.Message;
                MessageBox.Show("LblEncriptar_TextChanged(). " + ex.Message);
            }
            lblEncriptar.Text = string.Empty;
        }

        //private Task<int> EnviarArchivosFTP()
        //{
        //    return Task.Run(() =>
        //    {
        //        //DataSet dsResultado = ArmarDsResultado();
        //        //try
        //        //{
        //        //    string FtpKeysFile = Path.Combine(Properties.Settings.Default.DRIVE_LETTER, "DGMGT", "FTPKeys", "FTPKeys.xml");

        //        //    DataSet dsFTP = new DataSet();
        //        //    dsFTP.ReadXml(FtpKeysFile);

        //        //    FtpCore ftpCore = new FtpCore(dsFTP.Tables[0].Rows[0]["HOST"].ToString(), dsFTP.Tables[0].Rows[0]["USER"].ToString(), dsFTP.Tables[0].Rows[0]["PASSWORD"].ToString()) { uploadPath = dsFTP.Tables[0].Rows[0]["FOLDER"].ToString() };
        //        //    ftpCore.InitCliet();
        //        //    ftpCore.uploadFile(rutaArchivoEncriptado);
        //        //    ftpCore.uploadFile(rutaArchivoEncriptado + ".done");
        //        //    File.Move(rutaArchivoEncriptado, Path.Combine(Application.StartupPath, "ENROL", "XMLs", "Done") + "\\" + Path.GetFileNameWithoutExtension(rutaArchivoEncriptado) + ".txt");
        //        //    File.Delete(rutaArchivoEncriptado + ".done");

        //        //    dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
        //        //    dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
        //        //    dsResultado.Tables[0].Rows[0]["DATOS"] = null;
        //        //}
        //        //catch (Exception ex)
        //        //{
        //        //    dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
        //        //    dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "EnvioFTPArchivo(). " + ex.Message;
        //        //    dsResultado.Tables[0].Rows[0]["DATOS"] = null;
        //        //}
        //        return 0;
        //    });

        //    //DataSet dsEnviarArchivo = new DataSet();
        //    //if (Properties.Settings.Default.ENVIAR_FTP)
        //    //{
        //    //    //xml.crearXml(rutaXML, pasaporte_xml);

        //    //    dsEnviarArchivo = funciones.EnvioFTPArchivo(rutaArchivoEncriptado);

        //    //    if (bool.Parse(dsEnviarArchivo.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
        //    //        MessageBox.Show("¡Error al enviar el archivo, intente después con la opción correspondiente!. " + dsEnviarArchivo.Tables[0].Rows[0]["MSG_ERROR"].ToString());
        //    //}
        //}
        
        private void LiveViewPicBox_DoubleClick(object sender, EventArgs e)
        {           
        }


        private void btnConfirmaCiudadano_Click(object sender, EventArgs e)
        {
        }

        private void BtnActualizarListaEscanerHuellas_Click(object sender, EventArgs e)
        {           
        }

        private void btnCita_Click(object sender, EventArgs e)
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("btnCita_Click(). " + ex.Message);
            }
        }

        private void txtCita_KeyPress(object sender, KeyPressEventArgs e)
        {
            if((char) e.KeyChar == 13)
            {
                //ScanDataModel scanDataModel = new ScanDataModel();
                //scanDataModel.MrzString = txtMrz.Text;
                //ProcessScan(scanDataModel);
            }
        }
            
        private void picb_logo_Click(object sender, EventArgs e)
        {
            MessageBox.Show(txtMensaje.Text);
        }

        private async void pbxBiometria_Click(object sender, EventArgs e)
        {          
        }

        private Task<DataSet> ConectarEscanerP()
        {
            return Task.Run(() =>
            {
                DataSet dsResultado = ArmarDsResultado();                
                try
                {
                    if (!Api.IsDevicePlugged()) throw new Exception("Dispositivo no encontrado, revise la conexión del puerto USB y la corriente del escáner");
                    if (!Api.IsDeviceConnected())
                    {
                        Api.ConnectToDevice();
                        setScanSettings();
                    }
                    dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
                    dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                    dsResultado.Tables[0].Rows[0]["DATOS"] = null;
                }
                catch (Exception ex)
                {
                    dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                    dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "ConectarEscanerP(). " + ex.Message;
                    dsResultado.Tables[0].Rows[0]["DATOS"] = null;
                }
                return dsResultado;
            });
        }

        void setScanSettings()
        {
            ScanSettings settings = new ScanSettings();
            {
                ScanLightFlags flags = ScanLightFlags.None;
                flags |= ScanLightFlags.Use;
                flags |= ScanLightFlags.AmbientLightElimination;
                settings.Infrared = flags;                                
            }   
            {
                ScanLightFlags flags = ScanLightFlags.None;
                flags |= ScanLightFlags.Use;
                flags |= ScanLightFlags.AmbientLightElimination;
                settings.Visible = flags;
            }
            {
                ScanLightFlags flags = ScanLightFlags.None;
                flags |= ScanLightFlags.Use;
                flags |= ScanLightFlags.AmbientLightElimination;
                settings.Ultraviolet = flags;
            }

            settings.Resolution = ScanResolution.Default;
            Api.SetScanSettings(settings);
        }

        private async void btnCapturarRostro_MouseClick(object sender, MouseEventArgs e)
        {
            formularioLimpio = false;
            btnCapturarRostro.Enabled = false;

            pbxFotografia.Image = pbxWarning.Image;
            pbxFotografia.Image = pbxLoad.Image;
            lblRostroSegmentado.Text = "0";
            
            try
            {
                //TOMA DE FOTOGRAFIA
                byte[] byteArray = null;

                if (Settings.Default.CONTROLADOR_WEBCAM.Equals("NEURO"))
                {
                    if (_biometricFaceClient.FaceCaptureDevice == null) throw new Exception("¡Seleccione una cámara (1)!");
                    if (CameraListBox.Items.Count == 0) throw new Exception("¡Seleccione una cámara (2)!");
                    byteArray = facesView.Face.Image.Save().ToArray();

                    pbxRostroIcao.Image = funciones.ImageFromByteArray(byteArray);

                }

                if (Settings.Default.CONTROLADOR_WEBCAM.Equals("AFORGE"))
                {
                    if (CameraListBox.Items.Count == 0) throw new Exception("¡Seleccione una cámara (2)!");

                    //MemoryStream msRostro = new MemoryStream();
                    //((Image)pbxVideo.Image.Clone()).Save(msRostro, ImageFormat.Png);
                    //byteArray = msRostro.ToArray();

                    int width = pbxVideo.Size.Width;
                    int height = pbxVideo.Size.Height;

                    Bitmap bm = new Bitmap(width, height);
                    pbxVideo.DrawToBitmap(bm, new Rectangle(0, 0, width, height));
                    pbxRostroIcao.Image = (System.Drawing.Image)bm;
                }
               
                pbxRostroIcao.SizeMode = PictureBoxSizeMode.Zoom;

                //DataSet dsProcesarRostro = await ProcesarRostro();

                //pbxFotografia.Image = pbxCheck.Image;
                //if (bool.Parse(dsProcesarRostro.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                //    MessageBox.Show(dsProcesarRostro.Tables[0].Rows[0]["MSG_ERROR"].ToString());
                //else
                //    ValidarPasaporte();

                //pbxFotografia.Image = pbxCheck.Image;

                if (sender != null)
                {
                    DataSet dsProcesarRostro = await ProcesarRostro();

                    pbxFotografia.Image = pbxCheck.Image;
                    if (bool.Parse(dsProcesarRostro.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                        MessageBox.Show(dsProcesarRostro.Tables[0].Rows[0]["MSG_ERROR"].ToString());
                    else
                        ValidarPasaporte();
                }
                else
                    pbxFotografia.Image = pbxCheck.Image;
            }
            catch (Exception ex)
            {
                pbxFotografia.Image = pbxWarning.Image;
                txtMensaje.Text = "btnCapturarRostro_MouseClick(). " + ex.Message;
                MessageBox.Show("btnCapturarRostro_MouseClick(). " + ex.Message);
            }

            btnCapturarRostro.Select();
            btnCapturarRostro.Enabled = true;
        }

        private async Task<DataSet> ProcesarRostro()
        {
            DataSet dsResultado = funciones.GetDsResultado();
            
            try
            {
                lblRostroSegmentado.Text = "0";

                byte[] byteArray = funciones.ByteArrayFromImage(pbxRostroIcao.Image, ImageFormat.Png);
                
                //SEGMENTACION DE ROSTRO
                int edad = 0;
                if (txtEdad.Text == String.Empty || int.TryParse(txtEdad.Text, out edad) == false) edad = 0;

                Settings.Default.SEGMENTAR_ROSTRO = false;
                if (edad >= 18)
                    Settings.Default.SEGMENTAR_ROSTRO = true;

                if (Settings.Default.SEGMENTAR_ROSTRO)
                {
                    pbxCoincideRostro.BackColor = Color.White;
                    NFace face = new NFace { Image = NImage.FromMemory(byteArray) };
                    DataSet dsRostroSegmentado = await funciones.SegmentarRostro(face, _biometricFaceClient);

                    if (bool.Parse(dsRostroSegmentado.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                        throw new Exception(dsRostroSegmentado.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                    this.lblRostroSegmentado.Text = "1";
                    pbxRostroIcao.Image = (Bitmap)dsRostroSegmentado.Tables[0].Rows[0]["DATOS"];
                    pbxRostroIcao.SizeMode = PictureBoxSizeMode.StretchImage;


                    NFace rostroA = new NFace() { Image = NImage.FromMemory(funciones.ByteArrayFromImage(this.pbxFotoExterna.Image, ImageFormat.Png)) };
                    NFace rostroB = new NFace() { Image = NImage.FromMemory(funciones.ByteArrayFromImage(this.pbxRostroIcao.Image, ImageFormat.Png)) };

                    DataSet dsCompararRostro = await funciones.RostrosIguales(rostroA, rostroB, this._biometricFaceClient);

                    bool coincideRostro = bool.Parse(dsCompararRostro.Tables[0].Rows[0]["RESULTADO"].ToString());
                    pbxCoincideRostro.BackColor = coincideRostro ? Color.Green : Color.Red;

                }

                //if(lblRostroComprimido.Text.Equals("1"))
                //{
                //    //INICIO - COMPRESIÓN DE IMÁGENES
                //    string rutaImagenSinComprimir = Path.Combine(Application.StartupPath, "ENROL", "json") + "\\FOTO_NORMAL.jpeg";
                //    string rutaImagenComprimida = Path.Combine(Application.StartupPath, "ENROL", "json") + "\\FOTO_COMPRIMIDA.jpeg";

                //    try { if (File.Exists(rutaImagenSinComprimir)) File.Delete(rutaImagenSinComprimir); } catch (Exception ex) { throw new Exception("Error al eliminar imagen del pasaporte. Detalles: " + ex.Message); }

                //    Bitmap bm = new Bitmap(pbxRostroIcao.Image);
                //    bm.Save(rutaImagenSinComprimir, ImageFormat.Jpeg);

                //    var ms = new MemoryStream();
                //    bm.Save(ms, ImageFormat.Jpeg);
                //    Image image = Image.FromStream(ms);

                //    DataSet dsComprimirImagen = funciones.ComprimirImagen(image, ".jpeg", 25);
                //    if (bool.Parse(dsComprimirImagen.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                //        throw new Exception(dsComprimirImagen.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                //    Image imageCompresa = ((Image)dsComprimirImagen.Tables[0].Rows[0]["DATOS"]);
                //    pbxRostroIcao.Image = null;
                //    pbxRostroIcao.Image = imageCompresa;

                //    try { if (File.Exists(rutaImagenComprimida)) File.Delete(rutaImagenComprimida); } catch (Exception ex) { throw new Exception("Error al eliminar imagen del pasaporte. Detalles: " + ex.Message); }
                //        ((Image)imageCompresa.Clone()).Save(rutaImagenComprimida, ImageFormat.Jpeg);

                //    lblRostroComprimido.Text = "1";
                    
                //    //FIN - COMPRESIÓN DE IMÁGENES
                //}

                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
                dsResultado.Tables[0].Rows[0]["DATOS"] = null;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
            }
            catch (Exception ex)
            {
                pbxFotografia.Image = pbxWarning.Image;
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "ProcesarRostro(). " + ex.Message;
            }

            return dsResultado;
        }

        private void btnResumen_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                string rutaReporteResumen = System.Windows.Forms.Application.StartupPath + "\\ENROL\\ReporteCasos\\" + dtpFecha.Value.ToString("dd/MM/yyyy").Replace("/", "") + ".xml";

                if (!File.Exists(rutaReporteResumen))
                    throw new Exception("¡No existen casos! " + rutaReporteResumen);

                VisorReportes visorReportes = new VisorReportes();

                DataTable dt = new DataTable();

                DataSet dsEncabezado = new DataSet();
                dsEncabezado.Tables.Add();
                dsEncabezado.Tables[0].Columns.Add("nombre_institucion", typeof(string));
                dsEncabezado.Tables[0].Columns.Add("sede", typeof(string));
                dsEncabezado.Tables[0].Columns.Add("usuario", typeof(string));
                dsEncabezado.Tables[0].Columns.Add("fecha", typeof(string));
                dsEncabezado.Tables[0].Columns.Add("hora_primer_caso", typeof(string));
                dsEncabezado.Tables[0].Columns.Add("numero_primer_caso", typeof(string));
                dsEncabezado.Tables[0].Columns.Add("hora_ultimo_caso", typeof(string));
                dsEncabezado.Tables[0].Columns.Add("numero_ultimo_caso", typeof(string));
                dsEncabezado.Tables[0].Columns.Add("total", typeof(string));
                dsEncabezado.Tables[0].Columns.Add("horas", typeof(string));
                dsEncabezado.Tables[0].Columns.Add("promedio", typeof(string));
                dsEncabezado.Tables[0].Columns.Add("nombreusuario", typeof(string));
                dsEncabezado.Tables[0].Columns.Add("estacioncaptura", typeof(string));

                DataRow dr = dsEncabezado.Tables[0].NewRow();
                dr["nombre_institucion"] = Settings.Default.NOMBRE_INSTITUCION;
                dr["sede"] = sedeEstacion.CIUDAD + ", " + sedeEstacion.PAIS;
                dr["nombreusuario"] = lblNombreUsuario.Text;
                dr["fecha"] = dtpFecha.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                dr["estacioncaptura"] = Environment.MachineName;

                DataSet dsTemp = new DataSet();
                DataSet dsDetalle = new DataSet();
                dsTemp.ReadXml(rutaReporteResumen);
                try
                {
                    dsDetalle.Tables.Add(dsTemp.Tables[0].Select(" usuario = '" + lbl_usuario.Text + "' AND sedecaptura = '" + sedeEstacion.CIUDAD + ", " + sedeEstacion.PAIS + "' AND fechacaptura = '" + dtpFecha.Value.ToString("dd/MM/yyyy") + "'").CopyToDataTable());
                    //dsDetalle.Tables.Add(dsTemp.Tables[0].Copy());
                }
                catch
                {
                    throw new Exception("¡No se encontraron registros! " + " usuario = '" + lbl_usuario.Text + "' AND sedecaptura = '" + sedeEstacion.CIUDAD + ", " + sedeEstacion.PAIS + "' AND fechacaptura = '" + dtpFecha.Value.ToString("dd/MM/yyyy") + "'");
                }

                dr["hora_primer_caso"] = dsDetalle.Tables[0].Rows[0]["hora"].ToString();
                dr["numero_primer_caso"] = dsDetalle.Tables[0].Rows[0]["nolibreta"].ToString();
                dr["hora_ultimo_caso"] = dsDetalle.Tables[0].Rows[dsDetalle.Tables[0].Rows.Count - 1]["hora"].ToString();
                dr["numero_ultimo_caso"] = dsDetalle.Tables[0].Rows[dsDetalle.Tables[0].Rows.Count - 1]["nolibreta"].ToString();
                dr["total"] = dsDetalle.Tables[0].Rows.Count;

                string s = dsDetalle.Tables[0].Rows[0]["fechacaptura"].ToString() + " " + dsDetalle.Tables[0].Rows[0]["hora"].ToString();
                DateTime hIni = DateTime.ParseExact(s, "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                s = dsDetalle.Tables[0].Rows[dsDetalle.Tables[0].Rows.Count - 1]["fechacaptura"].ToString() + " " + dsDetalle.Tables[0].Rows[dsDetalle.Tables[0].Rows.Count - 1]["hora"].ToString();
                DateTime hFin = DateTime.ParseExact(s, "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                TimeSpan timeSpan = hFin - hIni;
                double totalHours = timeSpan.TotalHours;
                if (totalHours < 1)
                    totalHours = 1;

                dr["horas"] = Math.Round(totalHours, 2).ToString();

                double promedio = double.Parse(dsDetalle.Tables[0].Rows.Count.ToString()) / Math.Round(totalHours, 2);
                dr["promedio"] = Math.Round(promedio, 2).ToString();

                dsEncabezado.Tables[0].Rows.Add(dr);

                ReportDataSource RD = new ReportDataSource();
                RD.Value = dsEncabezado.Tables[0];
                RD.Name = "Encabezado";

                var myType = typeof(FrmEnrolamiento);
                var nameSpace = myType.Namespace;

                visorReportes.reportViewer1.LocalReport.DataSources.Clear();
                visorReportes.reportViewer1.LocalReport.DataSources.Add(RD);
                visorReportes.reportViewer1.LocalReport.ReportEmbeddedResource = nameSpace.ToString() + ".Reportes.rpt_resumen.rdlc";
                //visorReportes.reportViewer1.LocalReport.ReportPath = Environment.CurrentDirectory + "\\Reportes\\rpt_detalle.rdlc";
                visorReportes.reportViewer1.SetDisplayMode(DisplayMode.PrintLayout);
                visorReportes.reportViewer1.LocalReport.Refresh();

                visorReportes.Activate();
                visorReportes.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "btnResumen_MouseClick()");
                txtMensaje.Text = "btnResumen_MouseClick(). " + ex.Message;
            }
        }

        private void btnDetalle_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                string rutaReporteResumen = System.Windows.Forms.Application.StartupPath + "\\ENROL\\ReporteCasos\\" + dtpFecha.Value.ToString("dd/MM/yyyy").Replace("/", "") + ".xml";

                if (!File.Exists(rutaReporteResumen))
                    throw new Exception("¡No existen casos! " + rutaReporteResumen);

                VisorReportes visorReportes = new VisorReportes();

                DataTable dt = new DataTable();

                DataSet dsEncabezado = new DataSet();
                dsEncabezado.Tables.Add();
                dsEncabezado.Tables[0].Columns.Add("nombre_institucion", typeof(string));
                dsEncabezado.Tables[0].Columns.Add("sede", typeof(string));
                dsEncabezado.Tables[0].Columns.Add("nombreusuario", typeof(string));
                dsEncabezado.Tables[0].Columns.Add("fecha", typeof(string));
                dsEncabezado.Tables[0].Columns.Add("estacioncaptura", typeof(string));

                DataRow dr = dsEncabezado.Tables[0].NewRow();
                dr["nombre_institucion"] = Settings.Default.NOMBRE_INSTITUCION;
                dr["sede"] = sedeEstacion.CIUDAD + ", " + sedeEstacion.PAIS;
                dr["nombreusuario"] = lblNombreUsuario.Text;
                dr["fecha"] = dtpFecha.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                dr["estacioncaptura"] = Environment.MachineName;

                dsEncabezado.Tables[0].Rows.Add(dr);

                ReportDataSource RD = new ReportDataSource();
                RD.Value = dsEncabezado.Tables[0];
                RD.Name = "Encabezado";

                dsEncabezado = new DataSet();
                DataSet dsTemp = new DataSet();
                dsTemp.ReadXml(rutaReporteResumen);
                try
                {
                    dsEncabezado.Tables.Add(dsTemp.Tables[0].Select(" usuario = '" + lbl_usuario.Text + "' AND sedecaptura = '" + sedeEstacion.CIUDAD + ", " + sedeEstacion.PAIS + "' AND fechacaptura = '" + dtpFecha.Value.ToString("dd/MM/yyyy") + "'").CopyToDataTable());
                    //dsEncabezado.Tables.Add(dsTemp.Tables[0].Copy());
                }
                catch
                {
                    throw new Exception("¡No se encontraron registros! " + " usuario = '" + lbl_usuario.Text + "' AND sedecaptura = '" + sedeEstacion.CIUDAD + ", " + sedeEstacion.PAIS + "' AND fechacaptura = '" + dtpFecha.Value.ToString("dd/MM/yyyy") + "'");
                }

                var myType = typeof(FrmEnrolamiento);
                var nameSpace = myType.Namespace;

                ReportDataSource RD2 = new ReportDataSource();
                RD2.Value = dsEncabezado.Tables[0];
                RD2.Name = "Detalles";
                visorReportes.reportViewer1.LocalReport.DataSources.Clear();
                visorReportes.reportViewer1.LocalReport.DataSources.Add(RD);
                visorReportes.reportViewer1.LocalReport.DataSources.Add(RD2);
                visorReportes.reportViewer1.LocalReport.ReportEmbeddedResource = nameSpace.ToString() + ".Reportes.rpt_detalle.rdlc";
                //visorReportes.reportViewer1.LocalReport.ReportPath = Environment.CurrentDirectory + "\\Reportes\\rpt_detalle.rdlc";
                visorReportes.reportViewer1.SetDisplayMode(DisplayMode.PrintLayout);
                visorReportes.reportViewer1.LocalReport.Refresh();

                visorReportes.Activate();
                visorReportes.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "btnDetalle_MouseClick()");
                txtMensaje.Text = "btnDetalle_MouseClick(). " + ex.Message;
            }
        }

        private void pbxCONS_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                if (pbxCONS.Image == pbxLoad.Image)
                    throw new Exception("Esperar que finalice el proceso actual. ");

                if (txtNoDocumento.Text.Trim().Equals(string.Empty))
                    throw new Exception("Sin número de libreta para consultar. ");

                if(pbxDocumento.Image != pbxCheck.Image)
                    throw new Exception("Sin imagen escaneada de pasaporte. ");

                ConsultarPasaporte();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "pbxCONS_MouseClick()");
                txtMensaje.Text = "pbxCONS_MouseClick(). " + ex.Message;
            }
        }

        void buzzerStart()
        {
            //if (!checkBoxBuzzer.Checked) return;

            BuzzerSettings settings = new BuzzerSettings();
            settings.Duration = 50;
            settings.HighTime = 5;
            settings.LowTime = 5;
            settings.Volume = 255;

            Api.SetBuzzer(settings);
            Api.UseBuzzer();
        }

        private async void btnGuardar_MouseClick(object sender, MouseEventArgs e)
        {
            if (handled)
            {
                handled = false;
                return;
            }
            btnCapturarRostro.Enabled = false;
            try
            {
                DataTable dtPersonas = new DataTable();
                //tab_principal.TabPages["tabPasaporte"].ImageKey = "warning.bmp";

                DataRow drVuelo = null;
                string idVuelo = string.Empty;
                string vuelo = string.Empty;
                string paisDestino = string.Empty;
                string ciudadDestino = string.Empty;

                this.Enabled = false;
                bool camposValidos = false;

                if (formularioLimpio && clicAgregarPersona)
                    camposValidos = true;
                else
                {
                    camposValidos = await ValidarCampos();
                    if (!camposValidos)
                        picb_logo_Click(sender, e);
                    else
                    {
                        FrmParentesco frmParentesco = new FrmParentesco();

                        if (int.Parse(txtEdad.Text) >= 18 && movimiento.menores.Count > 0)
                        {
                            frmParentesco.ShowDialog();

                            if (frmParentesco.parentesco == 0)
                            {
                                this.Enabled = true;
                                return;
                            }   
                        }

                        lblParentesco.Text = "" + frmParentesco.parentesco;

                        DataSet dsAgregarPersona = AgregarPersona();
                        if (bool.Parse(dsAgregarPersona.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                            throw new Exception(dsAgregarPersona.Tables[0].Rows[0]["MSG_ERROR"].ToString());
                    }
                }

                if (movimiento.adultos.Count == 0 && movimiento.menores.Count > 0)
                {
                    if (!chkMNA.Checked)
                    {
                        //if (movimiento.menores.Count == 1)
                        //    movimiento.menores.Clear();

                        throw new Exception("No se encontraron adultos para el(los) menor(es) existentes en la colección. Puede utilizar la opción MNA para Menor(es) No Acompañado(s)");
                    }
                }

                if (camposValidos)  
                {
                    lblGrupo.Text = "Grupo (" + movimiento.adultos.Count + "A/" + movimiento.menores.Count + "M)";

                    if (Settings.Default.VALIDAR_USUARIO_GUARDAR)
                    {
                        DataSet dsPersonaToDataTable = funciones.PersonaToDataTable(movimiento);
                        if (bool.Parse(dsPersonaToDataTable.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                            throw new Exception(dsPersonaToDataTable.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                        dtPersonas = (DataTable)dsPersonaToDataTable.Tables[0].Rows[0]["DATOS"];

                        //if (sedeDataEquipo.idtipodelegacion == 1)
                        //{
                        //    dsVuelo = GetVuelos();
                        //    if (bool.Parse(dsVuelo.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                        //        throw new Exception(dsVuelo.Tables[0].Rows[0]["MSG_ERROR"].ToString());
                        //}

                        esVerificacionGuardarVisible = true;

                        if (sedeDataEquipo.idtipodelegacion == 1)
                        {
                            VerificacionGuardar vGuardar = new VerificacionGuardar(loginData, _biometricFingerClient, dtPersonas, this.equipoData, this.sedeDataEquipo);
                            vGuardar.lblUsuario.Text = lbl_usuario.Text;
                            vGuardar.lblNombre.Text = lblNombreUsuario.Text;
                            vGuardar.TopMost = true;
                            vGuardar.ShowDialog();

                            esVerificacionGuardarVisible = false;

                            if (!vGuardar.VerificacionValida)
                                throw new Exception("¡Error al seleccionar un vuelo!");

                            this.Focus();
                            idVuelo = vGuardar.vuelo.Split('/')[0].Trim();

                            DataTable dtVuelos = (DataTable) dsVuelo.Tables[0].Rows[0]["DATOS"];

                            if (vGuardar.dsVuelos.Tables.Count > 0)
                            {
                                dtVuelos = vGuardar.dsVuelos.Tables[0];
                                if (dtVuelos.Select("idvuelo = " + idVuelo).Length < 1)
                                    throw new Exception("No se encontró información para vueloid: " + idVuelo);
                            }

                            drVuelo = dtVuelos.Select("idvuelo = " + idVuelo)[0];
                            //vuelo = vGuardar.vuelo.Split('/')[1].Trim();
                            //paisDestino = vGuardar.paisDestino;
                            //ciudadDestino = vGuardar.ciudadDestino;
                        }
                        else
                        {
                            VerificacionGuardarTerrestre vGuardar = new VerificacionGuardarTerrestre(loginData, _biometricFingerClient, dtPersonas);
                            vGuardar.lblUsuario.Text = lbl_usuario.Text;
                            vGuardar.lblNombre.Text = lblNombreUsuario.Text;
                            vGuardar.TopMost = true;
                            vGuardar.ShowDialog();

                            esVerificacionGuardarVisible = false;

                            this.Focus();
                        }
                    }

                    DialogResult result = MessageBox.Show("¿Está seguro que desea GUARDAR?", "Salir", MessageBoxButtons.YesNo);
                    if (result != DialogResult.Yes)
                    {
                        this.Enabled = true;
                        return;
                    }

                    movimiento.Id = idMovimiento;
                    Viaje viaje = new Viaje();

                    //PENDIENTE
                    viaje.idmotivoviaje = 1;//int.Parse(cmbMotivoViaje.SelectedValue.ToString());
                    viaje.nombremotivoviaje = cmbMotivoViaje.GetItemText(cmbTipoDocumento.SelectedItem).Substring(0, 1);
                    
                    if (lbl_sede.Text.Split('/')[1].Trim().Equals("S"))
                    {
                        viaje.direccionresidencia = string.Empty;
                        viaje.direcionimprevista = string.Empty;
                        viaje.tiempo = 0;
                    }

                    //PENDIENTE
                    if (lbl_sede.Text.Split('/')[1].Trim().Equals("E"))
                    {
                        viaje.direccionresidencia = string.Empty;
                        viaje.direcionimprevista = string.Empty;
                        viaje.tiempo = 0;
                    }

                    viaje.idtipoflujo = 0;
                    viaje.idtipotransporte = 1;

                    string tFlujo = lbl_sede.Text.Split('/')[1].Substring(1, 1);
                    if (tFlujo == "E")
                        viaje.idtipoflujo = 1;

                    if (tFlujo == "S")
                        viaje.idtipoflujo = 2;

                    movimiento.viaje = null;
                    if (sedeDataEquipo.idtipodelegacion == 1)
                    {
                        viaje.idciudadorigen = int.Parse(drVuelo["idciudadorigen"].ToString());
                        viaje.idpaisorigen = int.Parse(drVuelo["idpaisorigen"].ToString());
                        viaje.idciudaddestino = int.Parse(drVuelo["idciudaddestino"].ToString());
                        viaje.idpaisdestino = int.Parse(drVuelo["idpaisdestino"].ToString());

                        viaje.icaopaisorigen = drVuelo["icaopaisorigen"].ToString();
                        viaje.icaopaisdestino = drVuelo["icaopaisdestino"].ToString();

                        viaje.iddetalletransporte = int.Parse(idVuelo);
                        viaje.idtransporte = int.Parse(drVuelo["idtrasporte"].ToString());
                        viaje.idtipoclasificacionstatus = 1;
                        movimiento.viaje = viaje;
                    }
                    else
                    {
                        if (tFlujo == "E")
                        {
                            viaje.idciudadorigen = 60;
                            viaje.idpaisorigen = 49;
                            viaje.idciudaddestino = 20;
                            viaje.idpaisdestino = 67;

                            viaje.icaopaisorigen = "SLV";
                            viaje.icaopaisdestino = "GTM";

                            viaje.idtipotransporte = 2;
                            viaje.iddetalletransporte = null;
                            viaje.idtransporte = 2;
                            viaje.idtipoclasificacionstatus = 1;
                            movimiento.viaje = viaje;
                        }
                        else if (tFlujo == "S")
                        {
                            viaje.idciudadorigen = 20;
                            viaje.idpaisorigen = 67;
                            viaje.idciudaddestino = 60;
                            viaje.idpaisdestino = 49;

                            viaje.icaopaisorigen = "GTM";
                            viaje.icaopaisdestino = "SLV";

                            viaje.idtipotransporte = 2;
                            viaje.iddetalletransporte = null;
                            viaje.idtransporte = 2;
                            viaje.idtipoclasificacionstatus = 1;
                            movimiento.viaje = viaje;
                        }
                    }
                    
                    movimiento.usuarioingresa = lbl_usuario.Text;
                    movimiento.biosestacion = loginData.biosestacion;

                    movimiento.makeScanner = movimiento.modelScanner = movimiento.serialScanner = null;

                    if(dspEscanerHuellas != null)
                        movimiento.makeScanner = dspEscanerHuellas.make == null ? null : dspEscanerHuellas.make;
                    
                    if(dspEscanerHuellas != null)
                        movimiento.modelScanner = dspEscanerHuellas.model == null ? null: dspEscanerHuellas.model;
                    
                    if(dspEscanerHuellas != null)
                        movimiento.serialScanner = dspEscanerHuellas.serialnumber == null ? null: dspEscanerHuellas.model;

                    movimiento.makeCamera = camara.make;
                    movimiento.modelCamera = camara.model;
                    movimiento.serialCamera = camara.serialnumber.Length > 10 ? camara.serialnumber.Substring(0, 10) : camara.serialnumber;

                    DataSet dsDirectorioLlaves = funciones.ExisteDirectorioLlaves(lbl_usuario.Text, Environment.MachineName);

                    if (bool.Parse(dsDirectorioLlaves.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                        throw new Exception(dsDirectorioLlaves.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                    JsonResponseMovimiento respuesta = new JsonResponseMovimiento();

                    if (Settings.Default.CONSUMIR_API_INS_MOV)
                    {
                        FrmLoader frmLoader = new FrmLoader();
                        frmLoader.TopMost = true;
                        frmLoader.Show();

                        movimiento.fechaini = DateTime.Now;
                        DataSet dsMovimiento = await InsertarMovimiento(movimiento);

                        frmLoader.Hide();

                        movimiento.fechafin = DateTime.Now;
                        TimeSpan timeSpan = movimiento.fechafin - movimiento.fechaini;
                        movimiento.minutos = 0;
                        movimiento.segundos = timeSpan.TotalSeconds;



                        if (bool.Parse(dsMovimiento.Tables[0].Rows[0]["RESULTADO"].ToString()))
                        {
                            respuesta = (JsonResponseMovimiento)dsMovimiento.Tables[0].Rows[0]["DATOS"];
                            movimiento.response = respuesta;
                        }
                        else
                            movimiento.response_error = dsMovimiento.Tables[0].Rows[0]["MSG_ERROR"].ToString();

                        if (Settings.Default.GENERAR_ARCHIVO_JSON)
                        {
                            try
                            {
                                string archivoJson = "";
                                if(bool.Parse(dsMovimiento.Tables[0].Rows[0]["RESULTADO"].ToString()))
                                    archivoJson = Path.Combine(System.Windows.Forms.Application.StartupPath, "ENROL", "json", "Done") + "\\" + movimiento.Id + ".json";
                                else
                                    archivoJson = Path.Combine(System.Windows.Forms.Application.StartupPath, "ENROL", "json", "Done", "Error") + "\\" + movimiento.Id + ".json";

                                using (StreamWriter file = File.CreateText(archivoJson))
                                {
                                    JsonSerializer serializer = new JsonSerializer();
                                    serializer.Serialize(file, movimiento);
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Error al generar archivos temporales. Detalles: " + ex.Message);
                            }
                        }

                        if (bool.Parse(dsMovimiento.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                            throw new Exception(dsMovimiento.Tables[0].Rows[0]["MSG_ERROR"].ToString());
                    }

                    if (Settings.Default.IMPRIMIR_TICKET)
                        ImprimirTicket(new Enrolamiento());

                    lblEscaneos.Text = lblEscaneos.Text.Split('/')[0].Trim() + "/" + (int.Parse(lblEscaneos.Text.Split('/')[1].Trim()) + 1).ToString();

                    FrmRespuesta frmRespuesta = new FrmRespuesta(movimiento, dtPersonas);
                    frmRespuesta.ShowDialog();
                    //MessageBox.Show("Almacenado con éxito -> " + movimiento.Id);

                    clicAgregarPersona = false;
                    movimiento = new Movimiento();
                    this.btnNuevo_MouseClick(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "btnGuardar_MouseClick(). Mensaje: " + ex.Message + ". StackTrace: " + ex.StackTrace;
                MessageBox.Show("btnGuardar_MouseClick(). Mensaje: " + ex.Message);
            }

            this.TopMost = true;
            this.TopMost = false;

            this.Enabled = true;
            btnCapturarRostro.Enabled = true;
        }

        private void panel_ver_izq_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnNuevo_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                this.Enabled = false;

                if (movimiento.adultos.Count > 0 || movimiento.menores.Count > 0)
                {
                    DialogResult result = MessageBox.Show("Se ha detectado un grupo de personas sin guardar ¿Está seguro que desea CONTINUAR y borrar el grupo de personas?", "Grupos", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                        clicAgregarPersona = false;
                }

                NuevoIngreso();
                this.Enabled = true;
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "btnNuevo_Click(). " + ex.Message;
                MessageBox.Show("btnNuevo_Click(). " + ex.Message);
            }
        }

        private void chkMRZ_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                chkMRZ.Text = chkMRZ.Checked ? "Ingreso manual" : "Código MRZ: ";
                if (chkMRZ.Checked)
                {
                    MessageBox.Show("Ahora puede ingresar los datos manualmente");

                    txtMrz.Text = string.Empty;

                    cmbTipoDocumento.Enabled = cmbPaisEmisor.Enabled = cmbSexo.Enabled = cmbNacionalidad.Enabled = true;

                    cmbTipoDocumento.DropDownStyle = ComboBoxStyle.DropDown;
                    cmbPaisEmisor.DropDownStyle = ComboBoxStyle.DropDown;
                    cmbSexo.DropDownStyle = ComboBoxStyle.DropDown;
                    cmbNacionalidad.DropDownStyle = ComboBoxStyle.DropDown;

                    txtNumeroDocumento.ReadOnly = txtPrimerApellido.ReadOnly = txtSegundoApellido.ReadOnly = txtPrimerNombre.ReadOnly = txtSegundoNombre.ReadOnly = false;

                    dtpFechaNacimiento.Enabled = dtpFechaVencimiento.Enabled = true;

                    chkFechaVencimiento.Enabled = true;

                    txtMrz.Enabled = true;
                    txtMrz.ReadOnly = false;
                }
                else
                {
                    chkFechaVencimiento.Checked = false;

                    if (sender != null)
                        MessageBox.Show("Ahora puede ingresar los datos únicamente escaneando un documento");
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "chkMRZ_MouseClick(). " + ex.Message;
                MessageBox.Show("chkMRZ_MouseClick(). " + ex.Message);
            }
        }

        private void btn_cerrar_info_MouseClick(object sender, MouseEventArgs e)
        {
            cerrarAplicacion();
        }

        void cerrarAplicacion()
        {
            try
            {
                salir = MessageBox.Show("¿Está seguro que quiere salir de Enrollment?", "Salir", MessageBoxButtons.YesNo);
                if (salir == DialogResult.Yes)
                {
                    funciones.CancelarOperacionBiometrica(_biometricFaceClient);
                    //funciones.CancelarOperacionBiometrica(_biometricFaceClientIcao);
                    funciones.CancelarOperacionBiometrica(_biometricFingerClient);

                    System.Windows.Forms.Application.Exit();
                    Environment.Exit(Environment.ExitCode);
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "btn_cerrar_info_MouseClick(). " + ex.Message;
                MessageBox.Show("btn_cerrar_info_MouseClick(). " + ex.Message);
            }
        }

        private void chkFechaVencimiento_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                chkFechaVencimiento.Text = chkFechaVencimiento.Checked ? "Fecha de vencimiento no indicada" : "Fecha de vencimiento";
                if (chkFechaVencimiento.Checked)
                {
                    dtpFechaVencimiento.Enabled = false;
                    dtpFechaVencimiento.Value = DateTime.Today;
                }
                    
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "chkFechaVencimiento_MouseClick(). " + ex.Message;
                MessageBox.Show("chkFechaVencimiento_MouseClick(). " + ex.Message);
            }
        }

        private async void rbnd_Click(object sender, EventArgs e)
        {
            try
            {
                pbxCompararHuellas.Image = pbxWarning.Image;
                RadioButton radioButton = (RadioButton)sender;
                if (radioButton.Checked)
                {
                    pbxCompararHuellas.Image = pbxLoad.Image;
                    int posicion = int.Parse(radioButton.Name.Substring(radioButton.Name.Length - 1));
                    if (posicion == 0) posicion = 10;

                    //GroupBox grpMano = posicion < 6 ? grpManoDerecha : grpManoIzquierda;
                    //grpMano.Enabled = false;
                    NFingerView nFingerView = posicion < 6 ? nFVH1 : nFVH2;

                    if (_biometricFingerClient.CurrentBiometric != null) _biometricFingerClient.Cancel();

                    DataSet dsEscanerHuella = await funciones.EscanearHuella((NFPosition)posicion, nFingerView, _biometricFingerClient);
                    if (bool.Parse(dsEscanerHuella.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                        throw new Exception(dsEscanerHuella.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                    if (nFingerView.Finger.Image != null)
                        pbxCompararHuellas.Image = pbxCheck.Image;

                    formularioLimpio = false;
                }
            }
            catch(Exception ex)
            {
                txtMensaje.Text = "rbnd_Click(). " + ex.Message;
                MessageBox.Show("rbnd_Click(). " + ex.Message);
            }
        }

        private int CentrarX(int anchoTexto)
        {
            return (Settings.Default.ANCHO_PAPEL_PPI - anchoTexto) / 2;
        }

        private void btnImprimir_MouseClick(object sender, MouseEventArgs e)
        {
            Font font = new Font("Times New Roman", 10);
            Brush brush = new SolidBrush(Color.Black);
            PointF pointF = new PointF(0, 0);

            PrintDocument p = new PrintDocument();
            p.PrintPage += delegate (object sender1, PrintPageEventArgs e1)
            {
                lblTexto.Text = "Instituto Guatemalteco de Migración";
                e1.Graphics.DrawString(lblTexto.Text, font, brush, CentrarX(lblTexto.Width), pointF.Y);

                lblTexto.Text = "TICKET DE INGRESO";
                e1.Graphics.DrawString(lblTexto.Text, font, brush, CentrarX(lblTexto.Width), pointF.Y += Settings.Default.ALTO_FUENTE);
                
            };
            try
            {
                p.Print();
            }
            catch (Exception ex)
            {
                throw new Exception("Exception Occured While Printing", ex);
            }
        }

        private System.Drawing.Image GenerarQR(string cadena)
        {
            var options = new QrCodeEncodingOptions
            {
                DisableECI = true,
                CharacterSet = "UTF-8",
                Width = 200,
                Height = 200,
            };
            BarcodeWriter writer = new BarcodeWriter();
            writer.Format = BarcodeFormat.QR_CODE;
            writer.Options = new EncodingOptions
            {
                Width = 200,
                Height = 200,
            };

            return new Bitmap(writer.Write(cadena));

        }

        private void ImprimirTicket(Enrolamiento enrolamiento)
        {
            try
            {
                Font font = new Font("Times New Roman", 8);
                Brush brush = new SolidBrush(Color.Black);
                PointF pointF = new PointF(0, 0);

                PrintDocument p = new PrintDocument();
                PrintController printController = new StandardPrintController();
                p.PrintController = printController;

                p.PrintPage += delegate (object sender1, PrintPageEventArgs e1)
                {
                    try
                    {
                        lblTexto.Text = "Instituto Guatemalteco de Migración";
                        e1.Graphics.DrawString(lblTexto.Text, font, brush, CentrarX(lblTexto.Width), pointF.Y);

                        lblTexto.Text = "Seguridad Interna";
                        e1.Graphics.DrawString(lblTexto.Text, font, brush, CentrarX(lblTexto.Width), pointF.Y += Settings.Default.ALTO_FUENTE);

                        lblTexto.Text = "TICKET DE INGRESO";
                        e1.Graphics.DrawString(lblTexto.Text, font, brush, CentrarX(lblTexto.Width), pointF.Y += Settings.Default.ALTO_FUENTE);

                        System.Drawing.Image image = GenerarQR(enrolamiento.Id.ToString());
                        e1.Graphics.DrawImage(image, CentrarX(image.Width), pointF.Y += 12);
                        
                        lblTexto.Text = "Identificación: " + enrolamiento.TipoDocumento + " - " + enrolamiento.NumeroDocumento;
                        e1.Graphics.DrawString(lblTexto.Text, font, brush, 20, pointF.Y += (image.Height));

                        //lblTexto.Text = "Nivel: " + enrolamiento.Gestion;
                        //e1.Graphics.DrawString(lblTexto.Text, font, brush, 20, pointF.Y += Settings.Default.ALTO_FUENTE);

                        lblTexto.Text = "Fecha: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                        e1.Graphics.DrawString(lblTexto.Text, font, brush, 20, pointF.Y += Settings.Default.ALTO_FUENTE);

                        lblTexto.Text = "Máquina: " + enrolamiento.Maquina;
                        e1.Graphics.DrawString(lblTexto.Text, font, brush, 20, pointF.Y += Settings.Default.ALTO_FUENTE);

                        lblTexto.Text = "Usuario: " + enrolamiento.Usuario;
                        e1.Graphics.DrawString(lblTexto.Text, font, brush, 20, pointF.Y += Settings.Default.ALTO_FUENTE);

                        lblTexto.Text = "Ticket: " + enrolamiento.Id.ToString();
                        e1.Graphics.DrawString(lblTexto.Text, font, brush, 20, pointF.Y += Settings.Default.ALTO_FUENTE);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al generar el ticket: " + ex);

                    }
                };

                p.Print();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al imprimir el ticket: " + ex);
            }
        }

        private void txtCriterio_KeyPress(object sender, KeyPressEventArgs e)
        {
            //DataTable dt = (DataTable)dgvTramites.DataSource;
            
        }

        private void chkPersonaSinHuellas_MouseClick(object sender, MouseEventArgs e)
        {
            if (_biometricFingerClient != null && _biometricFingerClient.CurrentBiometric != null) _biometricFingerClient.Cancel();

            pbxCompararHuellas.Image = pbxCheck.Image;
            nFVH1.Finger = nFVH2.Finger = null;
            chkPersonaSinHuellas.Enabled = false;
        }

        //---------------------------------------------------------------------------------------

        void buzzerReady()
        {
            //if (!checkBoxBuzzer.Checked) return;

            BuzzerSettings settings = new BuzzerSettings();
            settings.Duration = 100;
            settings.HighTime = 100;
            settings.LowTime = 0;
            settings.Volume = 255;
            Api.SetBuzzer(settings);
            Api.UseBuzzer();
        }

        private async void tmrEnvioBD_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!Settings.Default.ENVIAR_FTP) return;

                if (Properties.Settings.Default.TIPO_CONEXION_ENTREGA.Equals("TEMP"))
                {
                    lblEnvioBD.Text = "Iniciando envío...";
                    tmrEnvioBD.Stop();

                    string carpetaArchivos = Path.Combine(System.Windows.Forms.Application.StartupPath, "ENROL", "json") + "\\";

                    string[] files = Directory.GetFiles(carpetaArchivos, "*.done");

                    if (files.Length == 0) lblEnvioBD.Text = "Sin archivos pendientes...";

                    foreach (string file in files)
                    {
                        string rutaArchivoJson = carpetaArchivos + Path.GetFileNameWithoutExtension(file);
                        lblEnvioBD.Text = Path.GetFileNameWithoutExtension(file);

                        using (StreamReader streamReader = File.OpenText(rutaArchivoJson))
                        using (JsonTextReader reader = new JsonTextReader(streamReader))
                        {
                            Newtonsoft.Json.Linq.JObject o2 = new Newtonsoft.Json.Linq.JObject();
                            try
                            {
                                o2 = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.Linq.JToken.ReadFrom(reader);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Error al convertir a JSON (" + lblEnvioBD.Text + "), reportar con el administrador: " + ex.Message);
                                continue;
                            }

                            Movimiento movimiento = (Movimiento)JsonConvert.DeserializeObject(o2.ToString(), (typeof(Movimiento)));
                            streamReader.Close();

                            DataSet dsInsertarMovimiento = await InsertarMovimiento(movimiento);

                            if (bool.Parse(dsInsertarMovimiento.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                            {
                                MessageBox.Show("¡Error al guardar entrega " + lblEnvioBD.Text + "! reportar con el administrador el siguiente error: " + dsInsertarMovimiento.Tables[0].Rows[0]["MSG_ERROR"].ToString());
                                continue;
                            }

                            File.Move(rutaArchivoJson, Path.Combine(carpetaArchivos, "Done") + "\\" + lblEnvioBD.Text);
                            File.Delete(file);
                        }
                    }                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("tmrEnvioBD_Tick(). " + ex.Message);
            }
            tmrEnvioBD.Start();
        }

        private void txtMrz_TextChanged(object sender, EventArgs e)
        {
            //if(txtMrz.Text.Length > 0)
            //{
            //    if (txtMrz.Text.Substring(0, 1).Equals("I"))
            //    {
            //        if (txtMrz.Text.Length == 96)
            //        {
            //            ScanDataModel scanDataModel = new ScanDataModel();
            //            scanDataModel.MrzString = txtMrz.Text.Replace("\r\n", " ").Trim();
            //            ProcessScan(scanDataModel);
            //            txtMrz.Enabled = false;
            //        }
            //    }
            //    else if (txtMrz.Text.Substring(0, 1).Equals("P"))
            //    {
            //        if(chkMRZ.Checked)
            //        //if (txtMrz.Text.Length == 89)
            //        {
            //            //ScanDataModel scanDataModel = new ScanDataModel();
            //            scanDataModel.MrzString = txtMrz.Text.Replace("\r\n", " ").Trim();
            //            ProcessScan(scanDataModel);
            //            txtMrz.Enabled = false;
            //        }
            //    }
                
            //}
        }

        bool handled = false;


        private async void btnAgregar_MouseClick(object sender, MouseEventArgs e)
        {
            this.Enabled = false;
            try
            {
                clicAgregarPersona = true;

                bool camposValidos = false;
                camposValidos = await ValidarCampos();
                if (!camposValidos)
                {
                    picb_logo_Click(new object(), new EventArgs());
                    this.Enabled = true;
                    return;
                }

                FrmParentesco frmParentesco = new FrmParentesco();

                if(int.Parse(txtEdad.Text) >= 18)            
                {
                    frmParentesco.ShowDialog();

                    if (frmParentesco.parentesco == 0)
                    {
                        this.Enabled = true;
                        return;
                    }
                }

                lblParentesco.Text = "" + frmParentesco.parentesco;

                //DialogResult result = MessageBox.Show("¿Está seguro que desea AGREGAR?", "Salir", MessageBoxButtons.YesNo);
                //if (result != DialogResult.Yes)
                //{
                //this.Enabled = true;
                //return;
                //}

                DataSet dsAgregarPersona = AgregarPersona();
                if (bool.Parse(dsAgregarPersona.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                    throw new Exception(dsAgregarPersona.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                lblGrupo.Text = "Grupo (" + movimiento.adultos.Count + "A/" + movimiento.menores.Count + "M)";

                MessageBox.Show("¡AGREGADO con éxito!");
                this.Enabled = false;
                NuevoIngreso();
                this.Enabled = true;
                //this.btnNuevo_MouseClick(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));
            }
            catch (Exception ex)
            {
                MessageBox.Show("btnAgregar(). " + ex.Message);
            }
            this.Enabled = true;
        }

        private DataSet ExisteAdulto(Movimiento movimiento)
        {
            DataSet dsResultado = ArmarDsResultado();
            dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;

            try
            {
                if (movimiento.adultos.Count == 0)
                    throw new Exception();

                foreach (Persona persona in movimiento.adultos)
                    if (txtNumeroDocumento.Text.Equals(persona.numerodocumento))
                    {
                        dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
                        dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                        dsResultado.Tables[0].Rows[0]["DATOS"] = persona;
                        break;
                    }
            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "ExisteAdulto(). " + ex.Message;
            }
            return dsResultado;
        }

        private DataSet ExisteMenor(Movimiento movimiento)
        {
            DataSet dsResultado = ArmarDsResultado();
            dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;

            try
            {
                if (movimiento.menores.Count == 0)
                    throw new Exception();

                foreach (Persona persona in movimiento.menores)
                    if (txtNumeroDocumento.Text.Equals(persona.numerodocumento))
                    {
                        dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
                        dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                        dsResultado.Tables[0].Rows[0]["DATOS"] = persona;
                        break;
                    }
            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "ExisteMenor(). " + ex.Message;
            }
            return dsResultado;
        }

        private DataSet AgregarPersona()
        {
            DataSet dsResultado = ArmarDsResultado();
            try
            {
                if (chkMNA.Checked)
                    if (int.Parse(txtEdad.Text) >= 18)
                        throw new Exception("Persona mayor de edad (" + txtEdad.Text + ")no puede ser agregada como MNA.");

                //MAYOR DE EDAD
                if (int.Parse(txtEdad.Text) >= 18)
                {
                    Persona adulto = new Persona();

                    DataSet dsExiste = ExisteAdulto(movimiento);
                    if (bool.Parse(dsExiste.Tables[0].Rows[0]["RESULTADO"].ToString()))
                    {
                        adulto = (Persona)dsExiste.Tables[0].Rows[0]["DATOS"];
                        MessageBox.Show("Persona con número de documento " + adulto.numerodocumento + " ya existe en la colección de ADULTOS. Se procederé a ACTUALIZAR el registro.");
                        movimiento.adultos.Remove(adulto);
                    }

                    DataTable dtPais = (DataTable)dsPais.Tables[0].Rows[0]["DATOS"];
                    DataRow[] drSelect = dtPais.Select(" ICAO = '" + cmbNacionalidad.SelectedValue.ToString() + "'");
                    if (drSelect == null || drSelect.Length == 0) 
                        throw new Exception("NACIONALIDAD (" + cmbNacionalidad.SelectedValue.ToString() + ") no encontrado, escanee nuevamente, seleccione uno manualmente o contacte al administrador. ");

                    if (lbl_sede.Text.Split('/')[1].Trim().Substring(0, 1).Equals("S"))
                    {
                        if (cmbNacionalidad.SelectedValue.ToString().Equals("GTM"))
                        {
                            FrmEntrevista frmEntrevista = new FrmEntrevista(loginData);
                            frmEntrevista.TopMost = true;
                            frmEntrevista.ShowDialog();

                            this.Focus();

                            if (frmEntrevista.entrevista.idempresareclutadora == 0)
                                frmEntrevista.entrevista.idempresareclutadora = null;

                            adulto.entrevistavisatrabajo = frmEntrevista.entrevista;
                        }
                    }

                    adulto.idnacionalidad = int.Parse(drSelect[0]["Codigo"].ToString());
                    adulto.idtipovisa = int.Parse(lblIdTipoVisa.Text.Trim().Equals("") ? "0" : lblIdTipoVisa.Text);
                    adulto.numerovisa = lblNumeroVisa.Text;
                    adulto.idbusqueda = 0;

                    adulto.primernombre = txtPrimerNombre.Text.Trim();
                    adulto.segundonombre = txtSegundoNombre.Text.Trim();
                    adulto.tercernombre = string.Empty;
                    adulto.primerapellido = txtPrimerApellido.Text.Trim();
                    adulto.segundoapellido = txtSegundoApellido.Text.Trim();
                    adulto.apellidocasada = string.Empty;

                    adulto.nombrecompleto = string.Empty;
                    adulto.nombrecompleto += adulto.primerapellido.Trim() + ", ";
                    adulto.nombrecompleto += adulto.segundoapellido.Trim() + ", ";
                    if (!adulto.apellidocasada.Trim().Equals(string.Empty))
                        adulto.nombrecompleto += adulto.apellidocasada.Trim() + ", ";

                    adulto.nombrecompleto += adulto.primernombre.Trim() + ", ";
                    adulto.nombrecompleto += adulto.segundonombre.Trim();

                    if (!adulto.apellidocasada.Trim().Equals(string.Empty))
                        adulto.nombrecompleto += ", " + adulto.tercernombre.Trim();

                    adulto.edad = int.Parse(txtEdad.Text);
                    adulto.sexo = cmbSexo.SelectedValue.ToString();
                    adulto.fechanacimiento = dtpFechaNacimiento.Value.ToString("dd/MM/yyyy");
                    adulto.fechavencimientodoc = dtpFechaVencimiento.Value.ToString("dd/MM/yyyy");
                    adulto.documentovalido = 0;//ESTE TAG FUE INCLUÍDO EN EL TAG DE REVISIÓN SECUNDARIA

                    //adulto.idtipodocumento = int.Parse(cmbTipoDocumento.SelectedValue.ToString());
                    if (cmbTipoDocumento.SelectedValue.ToString().Equals("I") || cmbTipoDocumento.SelectedValue.ToString().Equals("ID"))
                        if (cmbPaisEmisor.SelectedValue.ToString().Equals("GTM"))
                        {
                            adulto.idtipodocumento = 9;
                            adulto.tipodocumento = cmbTipoDocumento.SelectedValue.ToString(); //"PAS";
                        }
                            
                    if (cmbTipoDocumento.SelectedValue.ToString().Equals("I") || cmbTipoDocumento.SelectedValue.ToString().Equals("ID"))
                        if (!cmbPaisEmisor.SelectedValue.ToString().Equals("GTM"))
                        {
                            adulto.idtipodocumento = 11;
                            adulto.tipodocumento = cmbTipoDocumento.SelectedValue.ToString(); //"PAS";
                        } 

                    if (cmbTipoDocumento.SelectedValue.ToString().Equals("P"))
                    {
                        adulto.idtipodocumento = 1;
                        adulto.tipodocumento = "PAS";
                    }
                        

                    //if (cmbTipoDocumento.SelectedValue.ToString().Equals("VCC"))
                    //    adulto.idtipodocumento = 4;

                    adulto.numerodocumento = txtNumeroDocumento.Text.Trim();
                    
                    //PENDIENTE
                    adulto.idcategoria = 1;// int.Parse(cmbCategoriaMigratoria.SelectedValue.ToString());
                    
                    adulto.icaopaisemision = cmbPaisEmisor.SelectedValue.ToString();// "GTM";
                    adulto.acuerdopais = lblAcuerdoPais.Text;

                    adulto.idparentesco = null;
                    if (int.Parse(lblParentesco.Text) > 0)
                        adulto.idparentesco = int.Parse(lblParentesco.Text);
                    
                    adulto.idprofesion = int.Parse(cmbProfesion.SelectedValue.ToString());
                    adulto.enviarsegundarevision = int.Parse(chkEnviarRS.Text.Split('/')[1]);
                    adulto.iddelegacion = equipoData.sede_id;
                    adulto.ipestacion = equipoData.ip;//"10.200.7.115";//funciones.GetIpAddress();
                    adulto.clavedelegacion = sedeDataEquipo.clave;
                    adulto.mna = null;

                    adulto.idciudad = 0;

                    adulto.declaracionjurada = btnDJRV.Text.Split('/')[1].Trim();
                    adulto.drjvtitular = int.Parse(btnDJRV.Text.Split('/')[2].Trim());
                    adulto.numerolibreta = txtNoDocumento.Text;

                    adulto.LadoAVB64 = null;

                    if(Settings.Default.ENROLAR_DOCUMENTO)
                        adulto.LadoAVB64 = funciones.Base64FromImage(pbxAnverso.Image, ImageFormat.Jpeg);

                    if (Settings.Default.ESCANEAR_UV)
                        if (pbxAnversoUV.Image != null)
                            adulto.LadoAUB64 = funciones.Base64FromImage(pbxAnversoUV.Image, ImageFormat.Jpeg);

                    adulto.LadoBVB64 = null;
                    if (funciones.Base64FromImage(pbxReverso.Image, ImageFormat.Jpeg) != funciones.Base64FromImage(ENROLLMENT_V3.Properties.Resources.reverso, ImageFormat.Jpeg))
                        adulto.LadoBVB64 = null;//funciones.Base64FromImage(pbxReverso.Image, ImageFormat.Jpeg);

                    if (Settings.Default.ESCANEAR_UV)
                        adulto.LadoBUB64 = null;// pbxReversoUV.Image == null ? null : funciones.Base64FromImage(pbxReversoUV.Image, ImageFormat.Jpeg);

                    adulto.RostroSegmentado = int.Parse(this.lblRostroSegmentado.Text);
                    adulto.RostroB64 = funciones.Base64FromImage(Settings.Default.ENROLAR_ROSTRO ? pbxRostroIcao.Image : new Bitmap(1, 1), ImageFormat.Jpeg);

                    //adulto.categoriaimagenhuella = null;
                    adulto.codigocolorimagen = "GRAY";
                    adulto.algoritmocompresionimagen = "JPEG";
                    adulto.calidadhuella = 5;

                    if (pbxCoincideRostro.BackColor == Color.White)
                        adulto.CoincideRostro = "-1";
                    else if (pbxCoincideRostro.BackColor == Color.Red)
                        adulto.CoincideRostro = "0";
                    else if (pbxCoincideRostro.BackColor == Color.Green)
                        adulto.CoincideRostro = "1";
                     
                    adulto.PersonaSinHuellas = chkPersonaSinHuellas.Checked ? 1 : 0;
                    adulto.CoincideHuella = 0;
                    adulto.PosicionH1 = -1;
                    adulto.H1B64 = adulto.H1Wsq = null;

                    adulto.PosicionH1 = adulto.PosicionH2 = adulto.PosicionH3 = adulto.PosicionH4 = adulto.PosicionH5 = -1;
                    adulto.H1B64 = adulto.H2B64 = adulto.H3B64 = adulto.H4B64 = adulto.H5B64 = null;
                    adulto.H1Wsq = adulto.H2Wsq = adulto.H3Wsq = adulto.H4Wsq = adulto.H5Wsq = null;
                    //adulto.H1Wsq_bdsp = adulto.H2Wsq_bdsp = adulto.H3Wsq_bdsp = adulto.H4Wsq_bdsp = adulto.H5Wsq_bdsp = null;


                    adulto.PosicionH6 = adulto.PosicionH7 = adulto.PosicionH8 = adulto.PosicionH9 = adulto.PosicionH10 = -1;
                    adulto.H6B64 = adulto.H7B64 = adulto.H8B64 = adulto.H9B64 = adulto.H10B64 = null;
                    adulto.H6Wsq = adulto.H7Wsq = adulto.H8Wsq = adulto.H9Wsq = adulto.H10Wsq = null;
                    //adulto.H6Wsq_bdsp = adulto.H7Wsq_bdsp = adulto.H8Wsq_bdsp = adulto.H9Wsq_bdsp = adulto.H10Wsq_bdsp = null;

                    adulto.PosicionH11 = -1;
                    adulto.PDP11Wsq = null;

                    adulto.PosicionH12 = -1;
                    adulto.PIP12Wsq = null;

                    adulto.PosicionH13 = -1;
                    adulto.DCD13Wsq = null;

                    adulto.PosicionH14 = -1;
                    adulto.ICD14Wsq = null;

                    adulto.PosicionH15 = -1;
                    adulto.AP15Wsq = null;

                    if (Settings.Default.ENROLAR_HUELLAS)
                    {
                        if (!chkPersonaSinHuellas.Checked)
                        {
                            //if (nFVHuellaMovimiento.Finger != null)
                            {
                                //if (Settings.Default.VERIFICAR_HUELLA_GTM)
                                
                                if(dspEscanerHuellas.type == "1")
                                {
                                    adulto.CoincideHuella = pbxCompararHuellas.Image == pbxCheck.Image ? 1 : 0;
                                    adulto.PosicionH1 = 0;// pbxCompararHuellas.Image == pbxCheck.Image ? (int)nFVHuellaAlmacenada.Finger.Position : 0;
                                    adulto.H1B64 = Convert.ToBase64String(nFVHuellaMovimiento.Finger.Image.Save(NImageFormat.Png).ToArray());
                                    adulto.H1Wsq = Convert.ToBase64String(nFVHuellaMovimiento.Finger.Image.Save(NImageFormat.Wsq).ToArray());
                                }
                                else if(dspEscanerHuellas.type == "442")
                                {
                                    //adulto.PosicionH1 = 0;
                                    //adulto.CoincideHuella = 0;
                                    //adulto.PosicionH1 = 0;
                                    //adulto.H1B64 = Convert.ToBase64String(nFVHuellaMovimiento.Finger.Image.Save(NImageFormat.Png).ToArray());
                                    //adulto.H1Wsq = Convert.ToBase64String(nFVHuellaMovimiento.Finger.Image.Save(NImageFormat.Wsq).ToArray());

                                    if(nfvMD.Finger != null)
                                    {
                                        adulto.PosicionH13 = 13;
                                        adulto.DCD13B64 = Convert.ToBase64String(nfvMD.Finger.Image.Save(NImageFormat.Png).ToArray());
                                        adulto.DCD13Wsq = Convert.ToBase64String(nfvMD.Finger.Image.Save(NImageFormat.Wsq).ToArray());
                                    }
                                        

                                    if (nfvMI.Finger != null)
                                    {
                                        adulto.PosicionH14 = 14;
                                        adulto.ICD14B64 = Convert.ToBase64String(nfvMI.Finger.Image.Save(NImageFormat.Png).ToArray());
                                        adulto.ICD14Wsq = Convert.ToBase64String(nfvMI.Finger.Image.Save(NImageFormat.Wsq).ToArray());
                                    }
                                        

                                    if (nfvPU.Finger != null)
                                    {
                                        adulto.PosicionH15 = 15;
                                        adulto.AP15B64 = Convert.ToBase64String(nfvPU.Finger.Image.Save(NImageFormat.Png).ToArray());
                                        adulto.AP15Wsq = Convert.ToBase64String(nfvPU.Finger.Image.Save(NImageFormat.Wsq).ToArray());
                                    }

                                    ////////////////////////////////////
                                    //MANO DERECHA DEDO 1 AL 5
                                    if(nfvRightThumb.Finger != null)
                                    {
                                        adulto.PosicionH1 = 1;
                                        adulto.H1B64 = Convert.ToBase64String(nfvRightThumb.Finger.Image.Save(NImageFormat.Png).ToArray());
                                        adulto.H1Wsq = Convert.ToBase64String(nfvRightThumb.Finger.Image.Save(NImageFormat.Wsq).ToArray());
                                        //adulto.H1Wsq_bdsp = Convert.ToBase64String(nfvRightThumb.Finger.Image.Save(NImageFormat.Wsq).ToArray());
                                    }

                                    if (nfvRightIndex.Finger != null)
                                    {
                                        adulto.PosicionH2 = 2;
                                        adulto.H2B64 = Convert.ToBase64String(nfvRightIndex.Finger.Image.Save(NImageFormat.Png).ToArray());
                                        adulto.H2Wsq = Convert.ToBase64String(nfvRightIndex.Finger.Image.Save(NImageFormat.Wsq).ToArray());
                                        //adulto.H2Wsq_bdsp = Convert.ToBase64String(nfvRightIndex.Finger.Image.Save(NImageFormat.Wsq).ToArray());
                                    }

                                    if (nfvRightMiddle.Finger != null)
                                    {
                                        adulto.PosicionH3 = 3;
                                        adulto.H3B64 = Convert.ToBase64String(nfvRightMiddle.Finger.Image.Save(NImageFormat.Png).ToArray());
                                        adulto.H3Wsq = Convert.ToBase64String(nfvRightMiddle.Finger.Image.Save(NImageFormat.Wsq).ToArray());
                                        //adulto.H3Wsq_bdsp = Convert.ToBase64String(nfvRightMiddle.Finger.Image.Save(NImageFormat.Wsq).ToArray());
                                    }

                                    if (nfvRightRing.Finger != null)
                                    {
                                        adulto.PosicionH4 = 4;
                                        adulto.H4B64 = Convert.ToBase64String(nfvRightRing.Finger.Image.Save(NImageFormat.Png).ToArray());
                                        adulto.H4Wsq = Convert.ToBase64String(nfvRightRing.Finger.Image.Save(NImageFormat.Wsq).ToArray());
                                        //adulto.H4Wsq_bdsp = Convert.ToBase64String(nfvRightRing.Finger.Image.Save(NImageFormat.Wsq).ToArray());
                                    }

                                    if (nfvRightLittle.Finger != null)
                                    {
                                        adulto.PosicionH5 = 5;
                                        adulto.H5B64 = Convert.ToBase64String(nfvRightLittle.Finger.Image.Save(NImageFormat.Png).ToArray());
                                        adulto.H5Wsq = Convert.ToBase64String(nfvRightLittle.Finger.Image.Save(NImageFormat.Wsq).ToArray());
                                        //adulto.H5Wsq_bdsp = Convert.ToBase64String(nfvRightLittle.Finger.Image.Save(NImageFormat.Wsq).ToArray());
                                    }

                                    //MANO IZQUIERDA DEDO 6 AL 10

                                    if (nfvLeftThumb.Finger != null)
                                    {
                                        adulto.PosicionH6 = 6;
                                        adulto.H6B64 = Convert.ToBase64String(nfvLeftThumb.Finger.Image.Save(NImageFormat.Png).ToArray());
                                        adulto.H6Wsq = Convert.ToBase64String(nfvLeftThumb.Finger.Image.Save(NImageFormat.Wsq).ToArray());
                                        //adulto.H6Wsq_bdsp = Convert.ToBase64String(nfvLeftThumb.Finger.Image.Save(NImageFormat.Wsq).ToArray());
                                    }

                                    if (nfvLeftIndex.Finger != null)
                                    {
                                        adulto.PosicionH7 = 7;
                                        adulto.H7B64 = Convert.ToBase64String(nfvLeftIndex.Finger.Image.Save(NImageFormat.Png).ToArray());
                                        adulto.H7Wsq = Convert.ToBase64String(nfvLeftIndex.Finger.Image.Save(NImageFormat.Wsq).ToArray());
                                        //adulto.H7Wsq_bdsp = Convert.ToBase64String(nfvLeftIndex.Finger.Image.Save(NImageFormat.Wsq).ToArray());
                                    }

                                    if (nfvLeftMiddle.Finger != null)
                                    {
                                        adulto.PosicionH8 = 8;
                                        adulto.H8B64 = Convert.ToBase64String(nfvLeftMiddle.Finger.Image.Save(NImageFormat.Png).ToArray());
                                        adulto.H8Wsq = Convert.ToBase64String(nfvLeftMiddle.Finger.Image.Save(NImageFormat.Wsq).ToArray());
                                        //adulto.H8Wsq_bdsp = Convert.ToBase64String(nfvLeftMiddle.Finger.Image.Save(NImageFormat.Wsq).ToArray());
                                    }

                                    if (nfvLeftRing.Finger != null)
                                    {
                                        adulto.PosicionH9 = 9;
                                        adulto.H9B64 = Convert.ToBase64String(nfvLeftRing.Finger.Image.Save(NImageFormat.Png).ToArray());
                                        adulto.H9Wsq = Convert.ToBase64String(nfvLeftRing.Finger.Image.Save(NImageFormat.Wsq).ToArray());
                                        //adulto.H9Wsq_bdsp = Convert.ToBase64String(nfvLeftRing.Finger.Image.Save(NImageFormat.Wsq).ToArray());
                                    }

                                    if (nfvLeftLittle.Finger != null)
                                    {
                                        adulto.PosicionH10 = 10;
                                        adulto.H10B64 = Convert.ToBase64String(nfvLeftLittle.Finger.Image.Save(NImageFormat.Png).ToArray());
                                        adulto.H10Wsq = Convert.ToBase64String(nfvLeftLittle.Finger.Image.Save(NImageFormat.Wsq).ToArray());
                                        //adulto.H10Wsq_bdsp = Convert.ToBase64String(nfvLeftLittle.Finger.Image.Save(NImageFormat.Wsq).ToArray());
                                    }

                                    ////////////////////////////////////
                                }
                            }
                        }
                    }

                    //adulto.PosicionH2 = -1;
                    //adulto.H2B64 = adulto.H2Wsq = null;

                    adulto.idbusqueda = int.Parse(lblIdBusqueda.Text);
                    adulto.validacionflujosiom = int.Parse(this.lblIdMovimientoSIOM.Text);

                    //PENDIENTE
                    //if (nFVH2.Finger != null)
                    //{
                    //    if (!chkPersonaSinHuellas.Checked)
                    //    {
                    //        adulto.PosicionH2 = ((int)nFVH2.Finger.Position);
                    //        adulto.H2B64 = Convert.ToBase64String(nFVH2.Finger.Image.Save(NImageFormat.Png).ToArray());
                    //        adulto.H2Wsq = Convert.ToBase64String(nFVH2.Finger.Image.Save(NImageFormat.Wsq).ToArray());
                    //    }
                    //}

                    string mrz = txtMrz.Text.Trim();
                    mrz = mrz.Replace("\r\n", " ");

                    List<string> mrzLines = mrz.Split(' ').OfType<string>().ToList();

                    if(mrzLines.Count == 1)
                    {
                        adulto.mrzL1 = mrzLines[0];
                        adulto.mrzLines = 1;
                    }

                    if (mrzLines.Count == 2)
                    {
                        adulto.mrzL1 = mrzLines[0];
                        adulto.mrzL2 = mrzLines[1];
                        adulto.mrzLines = 2;
                    }

                    if (mrzLines.Count == 3)
                    {
                        adulto.mrzL1 = mrzLines[0];
                        adulto.mrzL2 = mrzLines[1];
                        adulto.mrzL3 = mrzLines[2];
                        adulto.mrzLines = 3;
                    }

                    adulto.probatorios = new List<Probatorios>();
                    movimiento.adultos.Add(adulto);
                }

                //MENOR DE EDAD
                if (int.Parse(txtEdad.Text) < 18)
                {
                    Persona menor = new Persona();

                    DataSet dsExiste = ExisteMenor(movimiento);
                    if (bool.Parse(dsExiste.Tables[0].Rows[0]["RESULTADO"].ToString()))
                    {
                        menor = (Persona)dsExiste.Tables[0].Rows[0]["DATOS"];
                        MessageBox.Show("Persona con número de documento " + menor.numerodocumento + " ya existe en la colección de MENORES. Se procederé a ACTUALIZAR el registro.");
                        movimiento.menores.Remove(menor);
                    }

                    DataRow[] drSelect = dsCatalogos.Tables["Pais"].Select(" ICAO = '" + cmbNacionalidad.SelectedValue.ToString() + "'");
                    if (drSelect == null || drSelect.Length == 0)
                        throw new Exception("NACIONALIDAD (" + cmbNacionalidad.SelectedValue.ToString() + ") no encontrado, escanee nuevamente, seleccione uno manualmente o contacte al administrador. ");

                    menor.idnacionalidad = int.Parse(drSelect[0]["Codigo"].ToString());
                    menor.idtipovisa = int.Parse(lblIdTipoVisa.Text.Trim().Equals("") ? "0" : lblIdTipoVisa.Text);
                    menor.numerovisa = lblNumeroVisa.Text;
                    menor.idbusqueda = 0;

                    menor.primernombre = txtPrimerNombre.Text.Trim();
                    menor.segundonombre = txtSegundoNombre.Text.Trim();
                    menor.tercernombre = string.Empty;
                    menor.primerapellido = txtPrimerApellido.Text.Trim();
                    menor.segundoapellido = txtSegundoApellido.Text.Trim();
                    menor.apellidocasada = string.Empty;

                    menor.nombrecompleto = string.Empty;
                    menor.nombrecompleto += menor.primerapellido.Trim() + ", ";
                    menor.nombrecompleto += menor.segundoapellido.Trim() + ", ";
                    if (!menor.apellidocasada.Trim().Equals(string.Empty))
                        menor.nombrecompleto += menor.apellidocasada.Trim() + ", ";

                    menor.nombrecompleto += menor.primernombre.Trim() + ", ";
                    menor.nombrecompleto += menor.segundonombre.Trim();

                    if (!menor.apellidocasada.Trim().Equals(string.Empty))
                        menor.nombrecompleto += ", " + menor.tercernombre.Trim();

                    menor.edad = int.Parse(txtEdad.Text);
                    menor.sexo = cmbSexo.SelectedValue.ToString();
                    menor.fechanacimiento = dtpFechaNacimiento.Value.ToString("dd/MM/yyyy");
                    menor.fechavencimientodoc = dtpFechaVencimiento.Value.ToString("dd/MM/yyyy");
                    menor.documentovalido = 0;//ESTE TAG FUE INCLUÍDO EN EL TAG DE REVISIÓN SECUNDARIA

                    //adulto.idtipodocumento = int.Parse(cmbTipoDocumento.SelectedValue.ToString());
                    if (cmbTipoDocumento.SelectedValue.ToString().Equals("I") || cmbTipoDocumento.SelectedValue.ToString().Equals("ID"))
                        if (cmbPaisEmisor.SelectedValue.ToString().Equals("GTM"))
                        {
                            menor.idtipodocumento = 9;
                            menor.tipodocumento = cmbTipoDocumento.SelectedValue.ToString(); //"PAS";
                        }

                    if (cmbTipoDocumento.SelectedValue.ToString().Equals("I") || cmbTipoDocumento.SelectedValue.ToString().Equals("ID"))
                        if (!cmbPaisEmisor.SelectedValue.ToString().Equals("GTM"))
                        {
                            menor.idtipodocumento = 11;
                            menor.tipodocumento = cmbTipoDocumento.SelectedValue.ToString(); //"PAS";
                        }

                    if (cmbTipoDocumento.SelectedValue.ToString().Equals("P"))
                    {
                        menor.idtipodocumento = 1;
                        menor.tipodocumento = "PAS";
                    }

                    menor.numerodocumento = txtNumeroDocumento.Text.Trim();

                    //PENDIENTE
                    menor.idcategoria = 1;// int.Parse(cmbCategoriaMigratoria.SelectedValue.ToString());

                    menor.tipodocumento = cmbTipoDocumento.SelectedValue.ToString(); //"PAS";
                    menor.icaopaisemision = cmbPaisEmisor.SelectedValue.ToString();// "GTM";
                    menor.acuerdopais = lblAcuerdoPais.Text;

                    if (chkMNA.Checked)
                        menor.idparentesco = null;
                    else
                    {
                        menor.idparentesco = null;
                        if (int.Parse(lblParentesco.Text) > 0)
                            menor.idparentesco = int.Parse(lblParentesco.Text);
                    }
                    
                    menor.idprofesion = 0;
                    menor.enviarsegundarevision = int.Parse(chkEnviarRS.Text.Split('/')[1]);
                    menor.iddelegacion = equipoData.sede_id;
                    menor.ipestacion = equipoData.ip;//"10.200.7.115";//funciones.GetIpAddress();
                    menor.clavedelegacion = sedeDataEquipo.clave;
                    menor.mna = chkMNA.Checked ? 1 : 0;

                    menor.idciudad = 0;

                    menor.declaracionjurada = btnDJRV.Text.Split('/')[1].Trim();
                    menor.drjvtitular = int.Parse(btnDJRV.Text.Split('/')[2].Trim());
                    menor.numerolibreta = txtNoDocumento.Text;

                    menor.LadoAVB64 = null;

                    if (Settings.Default.ENROLAR_DOCUMENTO)
                        menor.LadoAVB64 = funciones.Base64FromImage(pbxAnverso.Image, ImageFormat.Jpeg);

                    if (Settings.Default.ESCANEAR_UV)
                        if (pbxAnversoUV.Image != null)
                            menor.LadoAUB64 = funciones.Base64FromImage(pbxAnversoUV.Image, ImageFormat.Jpeg);

                    menor.LadoBVB64 = null;
                    if (funciones.Base64FromImage(pbxReverso.Image, ImageFormat.Jpeg) != funciones.Base64FromImage(ENROLLMENT_V3.Properties.Resources.reverso, ImageFormat.Jpeg))
                        menor.LadoBVB64 = funciones.Base64FromImage(pbxReverso.Image, ImageFormat.Jpeg);

                    if (Settings.Default.ESCANEAR_UV)
                        menor.LadoBUB64 = pbxReversoUV.Image == null ? null : funciones.Base64FromImage(pbxReversoUV.Image, ImageFormat.Jpeg);

                    menor.RostroSegmentado = int.Parse(this.lblRostroSegmentado.Text);
                    menor.RostroB64 = funciones.Base64FromImage(Settings.Default.ENROLAR_ROSTRO ? pbxRostroIcao.Image : new Bitmap(1, 1), ImageFormat.Jpeg);
                    menor.CoincideRostro = "-1";

                    menor.PersonaSinHuellas = chkPersonaSinHuellas.Checked ? 1 : 0;
                    menor.CoincideHuella = 0;
                    menor.PosicionH1 = -1;
                    menor.H1B64 = menor.H1Wsq = null;

                    menor.PosicionH1 = menor.PosicionH2 = menor.PosicionH3 = menor.PosicionH4 = menor.PosicionH5 = -1;
                    menor.H1B64 = menor.H2B64 = menor.H3B64 = menor.H4B64 = menor.H5B64 = null;
                    menor.H1Wsq = menor.H2Wsq = menor.H3Wsq = menor.H4Wsq = menor.H5Wsq = null;
                    //menor.H1Wsq_bdsp = menor.H2Wsq_bdsp = menor.H3Wsq_bdsp = menor.H4Wsq_bdsp = menor.H5Wsq_bdsp = null;


                    menor.PosicionH6 = menor.PosicionH7 = menor.PosicionH8 = menor.PosicionH9 = menor.PosicionH10 = -1;
                    menor.H6B64 = menor.H7B64 = menor.H8B64 = menor.H9B64 = menor.H10B64 = null;
                    menor.H6Wsq = menor.H7Wsq = menor.H8Wsq = menor.H9Wsq = menor.H10Wsq = null;
                    //menor.H6Wsq_bdsp = menor.H7Wsq_bdsp = menor.H8Wsq_bdsp = menor.H9Wsq_bdsp = menor.H10Wsq_bdsp = null;

                    if (Settings.Default.ENROLAR_HUELLAS)
                    {
                        if (!chkPersonaSinHuellas.Checked)
                        {
                            if (nFVHuellaMovimiento.Finger != null)
                            {
                                if(Settings.Default.VERIFICAR_HUELLA_GTM)
                                {
                                    menor.CoincideHuella = pbxCompararHuellas.Image == pbxCheck.Image ? 1 : 0;
                                    menor.PosicionH1 = pbxCompararHuellas.Image == pbxCheck.Image ? (int)nFVHuellaAlmacenada.Finger.Position : 0;
                                    menor.H1B64 = Convert.ToBase64String(nFVHuellaMovimiento.Finger.Image.Save(NImageFormat.Png).ToArray());
                                    menor.H1Wsq = Convert.ToBase64String(nFVHuellaMovimiento.Finger.Image.Save(NImageFormat.Wsq).ToArray());
                                }
                                else
                                {
                                    menor.CoincideHuella = 0;
                                    menor.PosicionH1 = 0;
                                    menor.H1B64 = Convert.ToBase64String(nFVHuellaMovimiento.Finger.Image.Save(NImageFormat.Png).ToArray());
                                    menor.H1Wsq = Convert.ToBase64String(nFVHuellaMovimiento.Finger.Image.Save(NImageFormat.Wsq).ToArray());
                                }
                                
                            }
                        }

                        //PENDIENTE
                        //if (nFVH2.Finger != null)
                        //{
                        //    if (!chkPersonaSinHuellas.Checked)
                        //    {
                        //        menor.PosicionH2 = ((int)nFVH2.Finger.Position);
                        //        menor.H2B64 = Convert.ToBase64String(nFVH2.Finger.Image.Save(NImageFormat.Png).ToArray());
                        //        menor.H2Wsq = Convert.ToBase64String(nFVH2.Finger.Image.Save(NImageFormat.Wsq).ToArray());
                        //    }
                        //}
                    }

                    menor.PosicionH2 = -1;
                    menor.H2B64 = menor.H2Wsq = null;

                    menor.idbusqueda = int.Parse(lblIdBusqueda.Text);
                    menor.validacionflujosiom = int.Parse(this.lblIdMovimientoSIOM.Text);

                    string mrz = txtMrz.Text.Trim();
                    mrz = mrz.Replace("\r\n", " ");

                    List<string> mrzLines = mrz.Split(' ').OfType<string>().ToList();

                    if (mrzLines.Count == 1)
                    {
                        menor.mrzL1 = mrzLines[0];
                        menor.mrzLines = 1;
                    }

                    if (mrzLines.Count == 2)
                    {
                        menor.mrzL1 = mrzLines[1];
                        menor.mrzLines = 2;
                    }

                    if (mrzLines.Count == 3)
                    {
                        menor.mrzL1 = mrzLines[2];
                        menor.mrzLines = 3;
                    }

                    menor.probatorios = new List<Probatorios>() { new Probatorios {documentomadre=null,documentopadre=null } };
                    movimiento.menores.Add(menor);
                }

                lblEscaneos.Text = lblEscaneos.Text.Split('/')[0].Trim() + "/" + (int.Parse(lblEscaneos.Text.Split('/')[1].Trim()) + 1).ToString();

                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;

            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "AgregarPersona(). " + ex.Message;
            }
            return dsResultado;
        }

        private void chkMNA_CheckedChanged(object sender, EventArgs e)
        {
            if (chkMNA.Checked)
                chkMNA.Enabled = false;
        }

        private void chkEnviarRS_CheckedChanged(object sender, EventArgs e)
        {
            chkEnviarRS.Text = "Enviar a RS./0";
            if (chkEnviarRS.Checked)
            {
                FrmRevisionSecundaria frmRevisionSecundaria = new FrmRevisionSecundaria();
                frmRevisionSecundaria.ShowDialog();

                chkEnviarRS.Text = "Enviar a RS./" + frmRevisionSecundaria.motivoRS;

                if (frmRevisionSecundaria.motivoRS == 0)
                    chkEnviarRS.Checked = false;
            }
        }

        private async void btnIniciarOpenCV_Click(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                funciones.ActivarCamaraOpenCV(this.pbxVideo);
            });
            //Thread camera;
            //camera = new Thread(new ThreadStart(Metodo));// funciones.ActivarCamaraOpenCV));
            //camera.Start();
        }

        private void btnGuardar_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.KeyCode == System.Windows.Forms.Keys.Space)
            //{
            //    handled = true;
            //}
        }

        private void btnDJRV_Click(object sender, EventArgs e)
        {
            FrmDeclaracion frmDeclaracion = new FrmDeclaracion(declaracionData, Settings.Default.API_DECLARACION_BY_LIBRETA, loginData, Settings.Default.DECLARACION_TKN);
            frmDeclaracion.ShowDialog();
        }

        private void lblVuelos_DoubleClick(object sender, EventArgs e)
        {
            this.Enabled = false;
            try
            {
                MessageBox.Show("Iniciando actualización de catálogo de vuelos");
                
                dsVuelo = GetVuelos();
                if (bool.Parse(dsVuelo.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                    throw new Exception(dsVuelo.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                MessageBox.Show("¡Catálogo de vuelos actualizado correctamente!");
            }
            catch (Exception ex)
            {

                MessageBox.Show("lblVuelos_DoubleClick(). Error al actualizar catálogo de vuelos: " + ex.Message);
            }
            this.Enabled = true;
        }

        private bool EsCuiValido(string cui)
        {

            if (cui.Length != 13)
                return false;

            return true;
        }

        private void pbxVideo_Click(object sender, EventArgs e)
        {
            btnCapturarRostro_MouseClick(new object(), null); ;
        }

        private async void rbnManoIzquierda_Click(object sender, EventArgs e)
        {
            grpPlantillas.Enabled = false;
            try
            {
                if (!rbnManoIzquierda.Checked)
                    return;

                nfvLeftIndex.Finger = nfvLeftMiddle.Finger = nfvLeftRing.Finger = nfvLeftLittle.Finger = null;

                if (cmbEscanerHuellas.SelectedItem == null) throw new Exception("¡Seleccione un escáner de huellas! ");
                if (!Escanner442) throw new Exception("¡El escáner seleccionado no es 442! ");

                NFinger nfSeleccionado = new NFinger();
                _subject442 = new NSubject();
                _subject442.Fingers.Clear();
                _subject442.Fingers.Add(new NFinger { Position = _slaps[0] });

                nfSeleccionado = _subject442.Fingers[0];

                nfv442.Finger = null;
                nfv442.Finger = nfSeleccionado;
                //fSelector.SelectedPosition = nfSeleccionado.Position;

                _biometricFingerClient.FingerScanner = cmbEscanerHuellas.SelectedItem as NFScanner;

                _biometricFingerClient.FingersCheckForDuplicatesWhenCapturing = true;
                var biometricTask = _biometricFingerClient.CreateTask(NBiometricOperations.Capture | NBiometricOperations.CreateTemplate | NBiometricOperations.Segment | NBiometricOperations.AssessQuality, _subject442);
                biometricTask.Biometric = nfSeleccionado;

                var performedTask = await _biometricFingerClient.PerformTaskAsync(biometricTask);

                if (performedTask.Error != null)
                    throw new Exception("Error en la captura biométrica, error: " + performedTask.Error.ToString());

                if (performedTask.Status == NBiometricStatus.DuplicateFound)
                    throw new Exception("Existe(n) huella(s) repetida(s). ¡Intente nuevamente!");

                if (performedTask.Status == NBiometricStatus.WrongHand)
                    throw new Exception("Mano cambiada. ¡Intente nuevamente!");

                if (performedTask.Status != NBiometricStatus.Ok)
                    throw new Exception("Estatus de la captura biométrica no válido, estatus: " + performedTask.Status.ToString());

                nfvMI.Finger = nfv442.Finger;

                switch (nfSeleccionado.Position)
                {
                    case NFPosition.PlainLeftFourFingers:
                        for (int i = 0; i < _subject442.Fingers.Count; i++)
                        {
                            switch (_subject442.Fingers[i].Position)
                            {
                                case NFPosition.LeftIndex:
                                    nfvLeftIndex.Finger = _subject442.Fingers[i];
                                    break;
                                case NFPosition.LeftMiddle:
                                    nfvLeftMiddle.Finger = _subject442.Fingers[i];
                                    break;
                                case NFPosition.LeftRing:
                                    nfvLeftRing.Finger = _subject442.Fingers[i];
                                    break;
                                case NFPosition.LeftLittle:
                                    nfvLeftLittle.Finger = _subject442.Fingers[i];
                                    break;
                            }
                        }
                        break;
                }
                if (performedTask.Status == NBiometricStatus.Ok)
                {
                    rbnManoDerecha.Checked = true;
                    rbnManoDerecha_Click(sender,e);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("rbnManoIzquierda_Click(). " + ex.Message);
            }

            //ActivarControles442(true);
            grpPlantillas.Enabled = true;
        }

        private async void rbnManoDerecha_Click(object sender, EventArgs e)
        {
            grpPlantillas.Enabled = false;
            try
            {
                if (!rbnManoDerecha.Checked)
                    return;

                nfvRightIndex.Finger = nfvRightMiddle.Finger = nfvRightRing.Finger = nfvRightLittle.Finger = null;

                if (cmbEscanerHuellas.SelectedItem == null) throw new Exception("¡Seleccione un escáner de huellas! ");
                if (!Escanner442) throw new Exception("¡El escáner seleccionado no es 442! ");

                NFinger nfSeleccionado = new NFinger();
                _subject442 = new NSubject();
                _subject442.Fingers.Clear();
                _subject442.Fingers.Add(new NFinger { Position = _slaps[1] });

                nfSeleccionado = _subject442.Fingers[0];

                nfv442.Finger = null;
                nfv442.Finger = nfSeleccionado;
                //fSelector.SelectedPosition = nfSeleccionado.Position;

                _biometricFingerClient.FingerScanner = cmbEscanerHuellas.SelectedItem as NFScanner;

                _biometricFingerClient.FingersCheckForDuplicatesWhenCapturing = true;
                var biometricTask = _biometricFingerClient.CreateTask(NBiometricOperations.Capture | NBiometricOperations.CreateTemplate | NBiometricOperations.Segment | NBiometricOperations.AssessQuality, _subject442);
                biometricTask.Biometric = nfSeleccionado;

                var performedTask = await _biometricFingerClient.PerformTaskAsync(biometricTask);

                if (performedTask.Error != null)
                    throw new Exception("Error en la captura biométrica, error: " + performedTask.Error.ToString());

                if (performedTask.Status == NBiometricStatus.DuplicateFound)
                    throw new Exception("Existe(n) huella(s) repetida(s). ¡Intente nuevamente!");

                if (performedTask.Status == NBiometricStatus.WrongHand)
                    throw new Exception("Mano cambiada. ¡Intente nuevamente!");

                if (performedTask.Status != NBiometricStatus.Ok)
                    throw new Exception("Estatus de la captura biométrica no válido, estatus: " + performedTask.Status.ToString());

                nfvMD.Finger = nfv442.Finger;

                switch (nfSeleccionado.Position)
                {
                    case NFPosition.PlainRightFourFingers:
                        for (int i = 0; i < _subject442.Fingers.Count; i++)
                        {
                            switch (_subject442.Fingers[i].Position)
                            {
                                case NFPosition.RightIndex:
                                    nfvRightIndex.Finger = _subject442.Fingers[i];
                                    break;
                                case NFPosition.RightMiddle:
                                    nfvRightMiddle.Finger = _subject442.Fingers[i];
                                    break;
                                case NFPosition.RightRing:
                                    nfvRightRing.Finger = _subject442.Fingers[i];
                                    break;
                                case NFPosition.RightLittle:
                                    nfvRightLittle.Finger = _subject442.Fingers[i];
                                    break;
                            }
                        }
                        break;
                }
                if (performedTask.Status == NBiometricStatus.Ok)
                {
                    rbnPulgares.Checked = true;
                    rbnPulgares_Click(sender, e);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("rbnManoDerecha_Click(). " + ex.Message);
            }
            grpPlantillas.Enabled = true;
            //ActivarControles442(true);
        }

        private async void rbnPulgares_Click(object sender, EventArgs e)
        {
            grpPlantillas.Enabled = false;
            try
            {
                if (!rbnPulgares.Checked)
                    return;

                nfvRightThumb.Finger = nfvLeftThumb.Finger = null;
                
                if (cmbEscanerHuellas.SelectedItem == null) throw new Exception("¡Seleccione un escáner de huellas! ");
                if (!Escanner442) throw new Exception("¡El escáner seleccionado no es 442! ");

                NFinger nfSeleccionado = new NFinger();
                _subject442 = new NSubject();
                _subject442.Fingers.Clear();
                _subject442.Fingers.Add(new NFinger { Position = _slaps[2] });

                nfSeleccionado = _subject442.Fingers[0];

                nfv442.Finger = null;
                nfv442.Finger = nfSeleccionado;
                //fSelector.SelectedPosition = nfSeleccionado.Position;

                _biometricFingerClient.FingerScanner = cmbEscanerHuellas.SelectedItem as NFScanner;

                _biometricFingerClient.FingersCheckForDuplicatesWhenCapturing = true;
                var biometricTask = _biometricFingerClient.CreateTask(NBiometricOperations.Capture | NBiometricOperations.CreateTemplate | NBiometricOperations.Segment | NBiometricOperations.AssessQuality, _subject442);
                biometricTask.Biometric = nfSeleccionado;

                var performedTask = await _biometricFingerClient.PerformTaskAsync(biometricTask);

                if (performedTask.Error != null)
                    throw new Exception("Error en la captura biométrica, error: " + performedTask.Error.ToString());

                if (performedTask.Status == NBiometricStatus.DuplicateFound)
                    throw new Exception("Existe(n) huella(s) repetida(s). ¡Intente nuevamente!");

                if (performedTask.Status == NBiometricStatus.WrongHand)
                    throw new Exception("Mano cambiada. ¡Intente nuevamente!");

                if (performedTask.Status != NBiometricStatus.Ok)
                    throw new Exception("Estatus de la captura biométrica no válido, estatus: " + performedTask.Status.ToString());

                nfvPU.Finger = nfv442.Finger;

                switch (nfSeleccionado.Position)
                {
                    case NFPosition.PlainThumbs:
                        for (int i = 0; i < _subject442.Fingers.Count; i++)
                        {
                            switch (_subject442.Fingers[i].Position)
                            {
                                case NFPosition.RightThumb:
                                    nfvRightThumb.Finger = _subject442.Fingers[i];
                                    break;
                                case NFPosition.LeftThumb:
                                    nfvLeftThumb.Finger = _subject442.Fingers[i];
                                    break;
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("rbnPulgares_Click(). " + ex.Message);
            }
            grpPlantillas.Enabled = true;
            //ActivarControles442(true);
        }

        private void pbxAnverso_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                var dlg = new OpenFileDialog();
                if (dlg.ShowDialog() != DialogResult.OK)
                    return;

                ScanDataModel scanDataModel = new ScanDataModel();

                JObject o1 = JObject.Parse(File.ReadAllText(dlg.FileName));

                byte[] imageBytes = Convert.FromBase64String(o1.Value<string>("LadoAVB64"));
                MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);

                // Convert byte[] to Image
                ms.Write(imageBytes, 0, imageBytes.Length);
                System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);

                scanDataModel.Anverso = (Bitmap)image;

                scanDataModel.MrzString = o1.Value<string>("Mrz");
                scanDataModel.MrzString = scanDataModel.MrzString.Replace(" ", "\r\n");

                ProcessScan(scanDataModel);
            }
            catch (Exception ex)
            {
                MessageBox.Show("pbxAnverso_MouseClick(). " + ex.Message);
            }
        }

        //---------------------------------------------------------------------------------------

        private string onScanOcr()
        {
            string mrz = Api.GetOcrPc();
            if (mrz != null)
                return mrz.Replace("\r", "\r\n");

            return string.Empty;
        }

        private void pbxMovimientoSIOM_Click(object sender, EventArgs e)
        {
            try
            {
                if (Settings.Default.CONSULTAR_MOVIMIENTO_SIOM == false)
                    MessageBox.Show("Opcion desactivada, consulte al administrador.");
                else
                    consultaMovimientoSIOM();

                if (pbxMovimientoSIOM.Image == pbxError.Image)
                {
                    string error = pbxMovimientoSIOM.Tag.ToString();
                    MessageBox.Show(error);
                }

            }
            catch (Exception ex)
            {
                txtMensaje.Text = "pbxMovimientoSIOM_Click(). " + ex.Message;
                MessageBox.Show("pbxMovimientoSIOM_Click(). " + ex.Message);
            }
        }

        private void pbxVideo_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                lock (LvLock)
                {
                    if (Evf_Bmp != null)
                    {
                        LVBratio = LVBw / (float)LVBh;
                        LVration = Evf_Bmp.Width / (float)Evf_Bmp.Height;
                        if (LVBratio < LVration)
                        {
                            w = LVBw;
                            h = (int)(LVBw / LVration);
                        }
                        else
                        {
                            w = (int)(LVBh * LVration);
                            h = LVBh;

                            xi = (LVBw - w) / 2;
                        }
                        e.Graphics.DrawImage(Evf_Bmp, xi, 0, w, h);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("pbxVideo_Paint(). " + ex.Message);
            }
        }

        //---------------------------------------------------------------------------------------

        void updatePlugState(object o, PlugEventArgs args)
        {
            
            SampleNet.Tools.RunInGui(this, delegate()
            {
                if (args.State == PlugState.Plugged)
                {
                    pbxConectarEscanerP_Click(new object(), new EventArgs());                    
                }
                else
                {
                    pbxConectarEscanerP.Image = pbxWarning.Image;                 
                }
                //updateDeviceInfo();
            });

        }

        void onDocPresented(object sender, EventArgs args)
        {
            SampleNet.Tools.RunInGui(this, delegate ()
            {
                pbxDocumentoEnEscaner.Image = pbxCheck.Image;
                SampleNet.Tools.HandleApiExceptions(delegate ()
                {
                    if (!esVerificacionGuardarVisible)
                        if (chkEscanAutomatico.Checked)
                            pbxEscanearPasaporte_Click(null, args);
                });
            });
        }
        //---------------------------------------------------------------------------------------
        void onDocRemoved(object sender, EventArgs args)
        {
            SampleNet.Tools.RunInGui(this, delegate ()
            {
                pbxDocumentoEnEscaner.Image = pbxWarning.Image;
            });
        }

        private async void ProcessScan(ScanDataModel scan)
        {
            formularioLimpio = false;
            try
            {
                try
                {
                    System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmEnrolamiento));
                    pbxFotoExterna.Image = ((System.Drawing.Image)(resources.GetObject("pbxFotoExterna.Image")));

                    pbxEscanearPasaporte.Image = pbxWarning.Image;
                    pbxEscanearPasaporte.Image = pbxLoad.Image;
                    this.pbxDocumento.Image = pbxWarning.Image;
                    pbxPasaporteEscaneadoUV.Image = pbxWarning.Image;
                    pbxEscanearPasaporte.Image = pbxCheck.Image;

                    try
                    {
                        pbxCoincideRostro.BackColor = Color.White;

                        if(scan.Anverso != null)
                        {
                            byte[] byteArray = funciones.ByteArrayFromImage(scan.Anverso, ImageFormat.Png);
                            NFace face = new NFace { Image = NImage.FromMemory(byteArray) };
                            DataSet dsRostroSegmentado = await funciones.SegmentarRostro(face, _biometricFaceClient);

                            if (bool.Parse(dsRostroSegmentado.Tables[0].Rows[0]["RESULTADO"].ToString()))
                            {
                                pbxFotoExterna.Image = (Bitmap)dsRostroSegmentado.Tables[0].Rows[0]["DATOS"];
                                pbxFotoExterna.SizeMode = PictureBoxSizeMode.StretchImage;
                            }
                        }                        

                        if (pbxRostroIcao.Image == pbxUsuario.Image)
                        {
                            btnCapturarRostro_MouseClick(null, new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));
                            pbxFotografia.Image = pbxLoad.Image;
                        }


                        DataSet dsProcessImage = new DataSet();
                        if (scan.Anverso == null) scan.Anverso = new Bitmap(1, 1);
                        if (scan.Anverso != null)
                        {
                            dsProcessImage = funciones.ProcessDocumentImage(pbxAnverso, scan.Anverso);

                            if (bool.Parse(dsProcessImage.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                                throw new Exception(dsProcessImage.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                            pbxAnverso.Image = scan.Anverso;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al procesar fotografía VISIBLE ANVERSO: " + ex.Message);
                        pbxAnverso.Image = ENROLLMENT_V3.Properties.Resources.anverso;
                        this.pbxDocumento.Image = pbxWarning.Image;
                    }

                    try
                    {
                        DataSet dsProcessImage = new DataSet();
                        if (scan.Reverso == null) scan.Reverso = new Bitmap(1, 1);
                        if (scan.Reverso != null)
                        {
                            dsProcessImage = funciones.ProcessDocumentImage(pbxReverso, scan.Reverso);

                            if (bool.Parse(dsProcessImage.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                                throw new Exception(dsProcessImage.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                            pbxReverso.Image = scan.Reverso;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al procesar fotografía VISIBLE REVERSO: " + ex.Message);
                        pbxReverso.Image = ENROLLMENT_V3.Properties.Resources.anverso;
                        this.pbxDocumento.Image = pbxWarning.Image;
                    }

                    if (Settings.Default.ESCANEAR_UV)
                    {
                        try
                        {
                            DataSet dsProcessImage = new DataSet();
                            if (scan.AnversoUV == null) scan.AnversoUV = new Bitmap(1, 1);
                            if (scan.AnversoUV != null)
                            {
                                dsProcessImage = funciones.ProcessDocumentImage(pbxAnversoUV, scan.AnversoUV);

                                if (bool.Parse(dsProcessImage.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                                    throw new Exception(dsProcessImage.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                                pbxAnversoUV.Image = scan.AnversoUV;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error al procesar fotografía UV ANVERSO: " + ex.Message);
                            pbxAnversoUV.Image = ENROLLMENT_V3.Properties.Resources.anverso;
                            this.pbxDocumento.Image = pbxWarning.Image;
                        }

                        try
                        {
                            DataSet dsProcessImage = new DataSet();
                            if (scan.ReversoUV == null) scan.ReversoUV = new Bitmap(1, 1);
                            if (scan.ReversoUV != null)
                            {
                                dsProcessImage = funciones.ProcessDocumentImage(pbxReversoUV, scan.ReversoUV);

                                if (bool.Parse(dsProcessImage.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                                    throw new Exception(dsProcessImage.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                                pbxReversoUV.Image = scan.ReversoUV;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error al procesar fotografía UV REVERSO: " + ex.Message);
                            pbxReversoUV.Image = ENROLLMENT_V3.Properties.Resources.anverso;
                            this.pbxDocumento.Image = pbxWarning.Image;
                        }
                    }
                }
                catch (Exception ex)
                {
                    pbxDocumento.Image = pbxWarning.Image;
                    throw new Exception("Error al obtener imagen del pasaporte. Detalles: " + ex.Message + ", " + ex.StackTrace);
                }

                pbxDocumento.Image = pbxCheck.Image;

                try
                {
                    //LADO A - MRZ
                    //if (INDICE_ESCANEO <= 1)
                    {
                        //if (chkMRZ.Checked)
                        //    return;

                        if(chkMRZ.Checked == false)
                            txtMrz.Text = scan.MrzString;
                        if (txtMrz.Text.Equals(string.Empty)) throw new Exception("No se encontró un código MRZ válido para realizar la búsqueda. ");

                        //PASAPORTE ALEMAN
                        if (txtMrz.Text.StartsWith("P<D<<"))
                        {
                            txtMrz.Text = txtMrz.Text.Replace("P<D<<", "P<DEU");
                            txtMrz.Text = txtMrz.Text.Replace("D<<", "DEU");
                        }
                            

                        string mrz = txtMrz.Text.Trim();
                        mrz = mrz.Replace("\r\n", " ");

                        List<string> mrzLines = mrz.Split(' ').OfType<string>().ToList();

                        MrzData mrzData = MrzParser.Parse(mrzLines);

                        if (MrzParser.error != string.Empty)
                            throw new Exception(MrzParser.error);

                        txtNoDocumento.Text = mrzData.DocumentNumber;

                        //NACIONALIDAD
                        DataRow[] drSelect = dsCatalogos.Tables["Pais"].Select(" ICAO = '" + (mrzData.Nationality == null ? mrzData.Country : mrzData.Nationality) + "'");
                        if (drSelect == null || drSelect.Length == 0) MessageBox.Show("NACIONALIDAD (" + mrzData.Nationality.Split('<')[0].Trim() + ") no encontrado, escanee nuevamente, seleccione uno manualmente o contacte al administrador. ");
                        else cmbNacionalidad.SelectedIndex = cmbNacionalidad.FindStringExact(drSelect[0]["Nombre"].ToString());

                        drSelect = dsCatalogos.Tables["Pais"].Select(" ICAO = '" + (mrzData.Nationality == null ? mrzData.Country : mrzData.Nationality) + "'");
                        if (drSelect == null || drSelect.Length == 0) MessageBox.Show("PAIS DE RESIDENCIA (" + mrzData.Nationality.Split('<')[0].Trim() + ") no encontrado, escanee nuevamente, seleccione uno manualmente o contacte al administrador. ");
                        else cmbPaisResidencia.SelectedIndex = cmbPaisResidencia.FindStringExact(drSelect[0]["Nombre"].ToString());

                        drSelect = dsCatalogos.Tables["Pais"].Select(" ICAO = '" + (mrzData.Nationality == null ? mrzData.Country : mrzData.Nationality) + "'");
                        if (drSelect == null || drSelect.Length == 0) MessageBox.Show("PAIS DE NACIMIENTO (" + mrzData.Nationality.Split('<')[0].Trim() + ") no encontrado, escanee nuevamente, seleccione uno manualmente o contacte al administrador. ");
                        else cmbPaisNacimiento.SelectedIndex = cmbPaisNacimiento.FindStringExact(drSelect[0]["Nombre"].ToString());

                        drSelect = dsCatalogos.Tables["Pais"].Select(" ICAO = '" + mrzData.Country.Split('<')[0].Trim() + "'");
                        if (drSelect == null || drSelect.Length == 0) MessageBox.Show("PAÍS DE EMISIÓN (" + mrzData.Country.Split('<')[0].Trim() + ") no encontrado, escanee nuevamente, seleccione uno manualmente o contacte al administrador. ");
                        else
                        {
                            cmbPaisEmisor.SelectedIndex = cmbPaisEmisor.FindStringExact(drSelect[0]["Nombre"].ToString());
                            lblAcuerdoPais.Text = drSelect[0]["acuerdopais"].ToString();
                        }    

                        //DATOS DE IDENTIFICACIÓN
                        cmbTipoPasaporte.SelectedIndex = 0;
                        string tipoDocumento = mrzData.DocumentType.Split('<')[0].Substring(0, 1).Trim();

                        //if (tipoDocumento.Equals("I") && cmbPaisEmisor.SelectedValue.Equals("GTM"))
                        //    tipoDocumento = "DPI";

                        if (tipoDocumento.Equals("I"))
                            tipoDocumento = "ID";

                        drSelect = dsCatalogos.Tables["TipoDocumento"].Select(" CODIGO = '" + tipoDocumento + "'");
                        if (drSelect == null || drSelect.Length == 0) MessageBox.Show("TIPO DE DOCUMENTO (" + mrzData.DocumentType.Split('<')[0].Trim() + ") no encontrado, escanee nuevamente, seleccione uno manualmente o contacte al administrador. ");
                        else
                        {
                            cmbTipoDocumento.SelectedIndex = cmbTipoDocumento.FindStringExact(drSelect[0]["valor"].ToString());
                            if (drSelect[0]["CODIGO"].ToString().Equals("P"))
                            {
                                pbxDocumento.Image = pbxCheck.Image;
                                cmbTipoPasaporte.SelectedIndex = 1;

                                if(cmbPaisEmisor.SelectedValue.Equals("GTM"))
                                    txtNoDocumento.Text = mrzData.PersonalNumber.Split('<')[0];
                            }
                        }

                        txtNumeroDocumento.Text = mrzData.DocumentNumber.Split('<')[0].Trim();
                        if (txtNumeroDocumento.Text.Equals(string.Empty))
                            txtNumeroDocumento.Text = mrzData.DocumentNumber.Replace("<", "").Trim();

                        if ((cmbTipoDocumento.SelectedValue.Equals("I") || cmbTipoDocumento.SelectedValue.Equals("ID") || cmbTipoDocumento.SelectedValue.Equals("DPI")) && cmbPaisEmisor.SelectedValue.Equals("GTM"))
                            txtNumeroDocumento.Text += mrzData.OptionalData1.Substring(0, 4);

                        //ENROLAMIENTO DE HUELLAS
                        enrolarHuellas();

                        dtpFechaVencimiento.Value = mrzData.ExpiryDate;

                        //DATOS GENERALES
                        txtPrimerApellido.Text = mrzData.SurName.Split(' ')[0].Trim();
                        txtSegundoApellido.Text = mrzData.SurName.Split(' ').Length > 1 ? mrzData.SurName.Split(' ')[1].Trim() : string.Empty;
                        txtPrimerNombre.Text = mrzData.GivenName.Split(' ')[0].Trim();
                        txtSegundoNombre.Text = mrzData.GivenName.Split(' ').Length > 1 ? mrzData.GivenName.Split(' ')[1].Trim() : string.Empty;

                        drSelect = dsCatalogos.Tables["Genero"].Select(" ICAO = '" + mrzData.Sex.Trim() + "'");
                        if (drSelect == null || drSelect.Length == 0) MessageBox.Show("SEXO (" + mrzData.Sex.Trim() + ") no encontrado, escanee nuevamente, seleccione uno manualmente o contacte al administrador. ");
                        else cmbSexo.SelectedIndex = cmbSexo.FindStringExact(drSelect[0]["Nombre"].ToString());

                        dtpFechaNacimiento.Value = mrzData.BirthDate;

                        DataSet dsEdad = funciones.CalcularEdad(mrzData.BirthDate.ToString("dd/MM/yyyy"));
                        if (bool.Parse(dsEdad.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                            throw new Exception(dsEdad.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                        string datosEdad = dsEdad.Tables[0].Rows[0]["DATOS"].ToString();

                        if(datosEdad.Split('*').Length > 0)
                        {
                            txtEdad.Text = datosEdad.Split('*')[0];
                            CultureInfo provider = new CultureInfo("es-GT");
                            String fnacimiento = datosEdad.Split('*')[1].Split(' ')[0];
                            dtpFechaNacimiento.Value = DateTime.ParseExact(fnacimiento, "d/MM/yyyy", provider);
                        }
                        else
                            txtEdad.Text = dsEdad.Tables[0].Rows[0]["DATOS"].ToString();

                        chkMNA.Enabled = !(int.Parse(txtEdad.Text) >= 18);
                        if (int.Parse(txtEdad.Text) >= 18)
                            chkMNA.Checked = false;


                        if (cmbTipoDocumento.SelectedValue.Equals("P") && cmbPaisEmisor.SelectedValue.Equals("GTM"))
                        {
                            rbLibretaPass.Checked = true;
                            await Task.Run(() =>
                            {
                                ConsultarPasaporte();
                            });
                        }

                        if (Settings.Default.CONSULTAR_ALERTAS)
                        {
                            Task.Run(() =>
                            {
                                ConsultarAlertas();
                            });
                        }

                        if (cmbTipoDocumento.SelectedValue.Equals("I") || cmbTipoDocumento.SelectedValue.Equals("ID") || cmbTipoDocumento.SelectedValue.Equals("DPI"))
                        {
                            if (cmbPaisEmisor.SelectedValue.Equals("GTM"))
                            {
                                rbCUI.Checked = true;
                                txtNoDocumento.Text = txtNumeroDocumento.Text;
                                DataSet dsCui = new DataSet();
                                await Task.Run(() =>
                                {
                                    dsCui = ConsultarCUI(txtNumeroDocumento.Text.Trim());
                                });
                                 
                                if (bool.Parse(dsCui.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                                    throw new Exception(dsCui.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                                //DPI dpi = (DPI)dsCui.Tables[0].Rows[0]["DATOS"];
                                //txtPrimerNombre.Text = dpi.PRIMER_NOMBRE;
                                //txtSegundoNombre.Text = dpi.SEGUNDO_NOMBRE;
                                //txtPrimerApellido.Text = dpi.PRIMER_APELLIDO;
                                //txtSegundoApellido.Text = dpi.SEGUNDO_APELLIDO;

                                //drSelect = dsCatalogos.Tables["Genero"].Select(" ICAO = '" + dpi.SEXO + "'");
                                //if (drSelect == null || drSelect.Length == 0) MessageBox.Show("SEXO (" + mrzData.Sex.Trim() + ") no encontrado, escanee nuevamente, seleccione uno manualmente o contacte al administrador. ");
                                //else cmbSexo.SelectedIndex = cmbSexo.FindStringExact(drSelect[0]["Nombre"].ToString());

                                //dtpFechaNacimiento.Value = DateTime.ParseExact(dpi.FECHA_NACIMIENTO, "dd/MM/yyyy", CultureInfo.InvariantCulture) ;

                                //dsEdad = funciones.CalcularEdad(mrzData.BirthDate.ToString("dd/MM/yyyy"));
                                //if (bool.Parse(dsEdad.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                                //    throw new Exception(dsEdad.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                                //txtEdad.Text = dsEdad.Tables[0].Rows[0]["DATOS"].ToString();
                                //chkMNA.Enabled = !(int.Parse(txtEdad.Text) >= 18);
                                //if (int.Parse(txtEdad.Text) >= 18)
                                //    chkMNA.Checked = false;

                                //pbxFotoExterna.Image = dpi.IMAGE;
                             }
                        }

                        if (pbxFotografia.Image == pbxLoad.Image ||
                            (pbxFotografia.Image == pbxCheck.Image && pbxCoincideRostro.BackColor != Color.Green)
                            )
                        {
                            //pbxFotografia.Image = pbxLoad.Image;

                            DataSet dsProcesarRostro = await ProcesarRostro();

                            pbxFotografia.Image = pbxCheck.Image;
                            if (bool.Parse(dsProcesarRostro.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                                MessageBox.Show(dsProcesarRostro.Tables[0].Rows[0]["MSG_ERROR"].ToString());
                        }

                        if (loginData.SEDE_ID == 2)
                        //if (cmbTipoDocumento.SelectedValue.Equals("P") && !cmbPaisEmisor.SelectedValue.Equals("GTM"))
                        {
                            Task.Run(() =>
                            {
                                ConsultarDeclaracion();
                            });
                        }

                        if (!cmbPaisEmisor.SelectedValue.Equals("GTM"))
                        {
                            rbCUI.Checked = false;
                            rbLibretaPass.Checked = false;

                            string icaoPaisEmisor = cmbPaisEmisor.SelectedValue.ToString();
                            DataTable dtPais = (DataTable)dsPais.Tables[0].Rows[0]["DATOS"];
                            drSelect = dtPais.Select(" ICAO = '" + icaoPaisEmisor + "'");
                            if (drSelect == null || drSelect.Length == 0) 
                                MessageBox.Show("PAÍS DE EMISIÓN (" + mrzData.Country.Split('<')[0].Trim() + ") no encontrado, escanee nuevamente, seleccione uno manualmente o contacte al administrador. ");

                            string tFlujo = lbl_sede.Text.Split('/')[1].Substring(1, 1);
                            if (tFlujo == "S")
                            {
                                string categoriaPais = drSelect[0]["categoriavisa"].ToString();
                                categoriaPais = categoriaPais.ToUpper();
                                if (categoriaPais == "B" || categoriaPais == "C")
                                {
                                    dsTipoVisa = GetTipoVisa();
                                    if (bool.Parse(dsTipoVisa.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                                        throw new Exception(dsVuelo.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                                    FrmRequisitos frmRequisitos = new FrmRequisitos();
                                    frmRequisitos.ShowDialog();
                                    lblNumeroVisa.Text = frmRequisitos.numeroVisa;
                                    lblIdTipoVisa.Text = frmRequisitos.idTipoVisa + "";
                                }
                            }
                        }
                        
                        pbxDocumento.Image = pbxCheck.Image;

                        consultaMovimientoSIOM();
                        
                        ValidarCampos();
                    }
                }
                catch (Exception ex)
                {

                    pbxDocumento.Image = pbxWarning.Image;
                    throw new Exception("Error al procesar el MRZ pasaporte. Detalles: " + ex.Message);
                }

                pbxDocumento.Image = pbxCheck.Image;
            }
            catch (Exception ex)
            {
                MessageBox.Show("EscanearPasaporte(). " + ex.Message);
                txtMensaje.Text = "EscanearPasaporte(). " + ex.Message;
                btnGuardar.Visible = false;
            }
            btnGuardar.Visible = btnGuardar.Enabled = true;
        }
        private async void consultaMovimientoSIOM() {
            if (Settings.Default.CONSULTAR_MOVIMIENTO_SIOM && !cmbPaisEmisor.SelectedValue.Equals("GTM"))
            {
                MovimientoSIOMRequest movimientoSIOM = new MovimientoSIOMRequest();

                movimientoSIOM.primer_nombre = txtPrimerNombre.Text;
                movimientoSIOM.segundo_nombre = txtSegundoNombre.Text;
                movimientoSIOM.primer_apellido = txtPrimerApellido.Text;
                movimientoSIOM.segundo_apellido = txtSegundoApellido.Text;
                movimientoSIOM.fecha_nac = dtpFechaNacimiento.Value.ToString("yyyy-MM-dd");
                movimientoSIOM.icao = cmbPaisEmisor.SelectedValue.ToString();
                movimientoSIOM.tipoflujo = lbl_sede.Text.Split('/')[1].Substring(1, 1) == "E" ? "1" : "2";

                pbxMovimientoSIOM.Image = pbxStart.Image;
                pbxMovimientoSIOM.Image = pbxLoad.Image;
                pbxMovimientoSIOM.Tag = "";
                DataSet dsMovimientoSIOM = await ConsultarSIOM(movimientoSIOM);
                if (!bool.Parse(dsMovimientoSIOM.Tables[0].Rows[0]["RESULTADO"].ToString()))
                {
                    pbxMovimientoSIOM.Image = pbxError.Image;
                    pbxMovimientoSIOM.Tag = dsMovimientoSIOM.Tables[0].Rows[0]["MSG_ERROR"].ToString();//throw new Exception(dsMovimientoSIOM.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                }
                else
                {
                    MovimientoSIOMResponse movimientoSIOMResponse = (MovimientoSIOMResponse)dsMovimientoSIOM.Tables[0].Rows[0]["DATOS"];
                    lblIdMovimientoSIOM.Text = movimientoSIOMResponse.data.validacionflujosiom.ToString();
                    pbxMovimientoSIOM.Image = pbxCheck.Image;
                }
            }
        }
        private void enrolarHuellas() {
            if (Settings.Default.ENROLAR_HUELLAS)
            {
                if (cmbPaisEmisor.SelectedValue.Equals("GTM"))
                {
                    ListarEscanersHuellas(cmbEscanerHuellas, true, true, PARAMETRIZACION.TipoEscanerHuellas.Unidactilar);
                    tabHuellas.SelectedIndex = 0;
                }
                else
                {
                    ListarEscanersHuellas(cmbEscanerHuellas, true, true, PARAMETRIZACION.TipoEscanerHuellas.Multidactilar);

                    dspEscanerHuellas.type = "442";

                    //rbnManoIzquierda.Checked = true;
                    //rbnManoIzquierda_Click(null, new EventArgs());

                    //nfvLeftThumb.Finger = nfvRightThumb.Finger = null;
                    //nfvLeftIndex.Finger = nfvLeftMiddle.Finger = nfvLeftRing.Finger = nfvLeftLittle.Finger = null;
                    //nfvRightIndex.Finger = nfvRightMiddle.Finger = nfvRightRing.Finger = nfvRightLittle.Finger = null;
                    //nfv442.Finger = null;

                    //nfvMI.Finger = nfvMD.Finger = nfvPU.Finger = null;

                    //rbnManoIzquierda.Checked = rbnManoDerecha.Checked = rbnPulgares.Checked = true;

                    tabHuellas.SelectedIndex = 1;
                }
                rbnd2.Checked = false;
                rbnd6.Checked = rbnd7.Checked = rbnd8.Checked = rbnd9.Checked = rbnd10.Checked = false;

            }
        }

        private void txtNoDocumento_TextChanged(object sender, EventArgs e)
        {
            int cursorPosition = txtNoDocumento.SelectionStart;
            string originalText = txtNoDocumento.Text;
            string modifiedText = originalText.ToUpper().Replace("*", "");

            if (txtNoDocumento.Text != modifiedText)
            {
                txtNoDocumento.Text = modifiedText;
                txtNoDocumento.SelectionStart = modifiedText.Length;
            }
            else
            {
                txtNoDocumento.SelectionStart = cursorPosition;
            }
            txtNoDocumento.ScrollToCaret();
        }

        private async void txtNoDocumento_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == System.Windows.Forms.Keys.Multiply)
                {
                    if (rbCUI.Checked || rbLibretaPass.Checked)
                    {
                        if (pbxEscanearPasaporte.Image != pbxCheck.Image)
                            throw new Exception("Escanee un documento de identificación.");

                        if (pbxFotografia.Image != pbxCheck.Image)
                            throw new Exception("Operación de ROSTRO pendiente.");

                        if (rbCUI.Checked)
                        {
                            pbxCONS.Image = pbxWarning.Image;
                            txtNoDocumento.Text = txtNoDocumento.Text.Trim();
                            if (EsCuiValido(txtNoDocumento.Text))
                            {
                                DataSet dsCui = ConsultarCUI(txtNoDocumento.Text.Trim());

                                if (bool.Parse(dsCui.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                                    throw new Exception(dsCui.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                                txtMrz.Text = "SIN/MRZ - CUI INGRESADO MANUALMENTE";
                                //txtNoDocumento.Text = "CUI";
                                txtNumeroDocumento.Text = txtNoDocumento.Text;
                                cmbTipoDocumento.SelectedValue = "ID";

                                DataRow[] drSelect = dsCatalogos.Tables["Pais"].Select(" ICAO = 'GTM'");
                                lblAcuerdoPais.Text = drSelect[0]["acuerdopais"].ToString();

                                Task.Run(() =>
                                {
                                    ConsultarAlertas();
                                });

                                enrolarHuellas();
                            }
                        }
                        else if (rbLibretaPass.Checked)
                        {
                            await Task.Run(() =>
                            {
                                ConsultarPasaporte();
                            });

                            Task.Run(() =>
                            {
                                ConsultarAlertas();
                            });

                            txtMrz.Text = "SIN/MRZ - LIBRETA INGRESADA MANUALMENTE";
                            enrolarHuellas();
                        }
                    }
                    else
                    {
                        throw new Exception("Debe de seleccionar el tipo de documento a buscar");
                    }
                }
            }
            catch (Exception ex) {
                MessageBox.Show("txtNoDocumento_KeyDown(). " + ex.Message);
            }
        }

        private async Task EscanearPasaporte()
        {
            await Task.Run(() =>
            {
                try
                {
                    try
                    {
                        ScanDataModel scanDataModel = new ScanDataModel();
                        //if (INDICE_ESCANEO > 1) INDICE_ESCANEO = 0;

                        PictureBox pbxLado = INDICE_ESCANEO == 0 ? pbxAnverso : pbxReverso;
                        PictureBox pbxLadoUV = INDICE_ESCANEO == 0 ? pbxAnversoUV : pbxReversoUV;

                        //INDICE_ESCANEO++;

                        pbxEscanearPasaporte.Image = pbxWarning.Image;
                        pbxEscanearPasaporte.Image = pbxLoad.Image;
                        this.pbxDocumento.Image = pbxWarning.Image;
                        pbxPasaporteEscaneadoUV.Image = pbxWarning.Image;

                        buzzerStart();
                        Api.Scan();
                        buzzerReady();
                        pbxEscanearPasaporte.Image = pbxCheck.Image;

                        scanDataModel.Anverso = Api.GetImage(Desko.FullPage.LightSource.Visible, ImageClipping.Document);

                        if(Settings.Default.ESCANEAR_UV)
                            scanDataModel.AnversoUV = Api.GetImage(Desko.FullPage.LightSource.Ultraviolet, ImageClipping.Document);

                        scanDataModel.MrzString = onScanOcr();

                        ProcessScan(scanDataModel);
                        return;
                    }
                    catch (Exception ex)
                    {
                        pbxDocumento.Image = pbxWarning.Image;
                        throw new Exception("Error al obtener imagen del pasaporte. Detalles: " + ex.Message + ", " + ex.StackTrace);
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("EscanearPasaporte(). " + ex.Message);
                    txtMensaje.Text = "EscanearPasaporte(). " + ex.Message;
                    btnGuardar.Visible = false;
                }
            });
        }


        //private async Task EscanearPasaporte()
        //{
        //    await Task.Run(() =>
        //    {
        //        try
        //        {
        //            try
        //            {
        //                if (INDICE_ESCANEO > 1) INDICE_ESCANEO = 0;

        //                PictureBox pbxLado = INDICE_ESCANEO == 0 ? pbxAnverso : pbxReverso;
        //                PictureBox pbxLadoUV = INDICE_ESCANEO == 0 ? pbxAnversoUV : pbxReversoUV;

        //                INDICE_ESCANEO++;

        //                pbxEscanearPasaporte.Image = pbxWarning.Image;
        //                pbxEscanearPasaporte.Image = pbxLoad.Image;
        //                this.pbxDocumento.Image = pbxWarning.Image;
        //                pbxPasaporteEscaneadoUV.Image = pbxWarning.Image;

        //                buzzerStart();
        //                Api.Scan();
        //                buzzerReady();
        //                pbxEscanearPasaporte.Image = pbxCheck.Image;

        //                if (INDICE_ESCANEO > 1)
        //                    pbxDocumento.Image = pbxCheck.Image;

        //                try
        //                {

        //                    pbxLado.Image = Api.GetImage(LightSource.Visible, ImageClipping.Document);

        //                    string rutaImagenSinComprimir = Path.Combine(Application.StartupPath, "ENROL", "json") + "\\" + pbxLado.Name + "_" + DateTime.Now.ToString("ddMMyyyyhhmmss", CultureInfo.InvariantCulture) + ".jpeg";
        //                    string rutaImagenComprimida = Path.Combine(Application.StartupPath, "ENROL", "json") + "\\" + pbxLado.Name + "C_" + DateTime.Now.ToString("ddMMyyyyhhmmss", CultureInfo.InvariantCulture) + ".jpeg";

        //                    try { if (File.Exists(rutaImagenSinComprimir)) File.Delete(rutaImagenSinComprimir); } catch (Exception ex) { throw new Exception("Error al eliminar imagen del pasaporte. Detalles: " + ex.Message); }

        //                    Bitmap bm = new Bitmap((Image)pbxLado.Image.Clone());
        //                    bm.Save(rutaImagenSinComprimir, ImageFormat.Jpeg);

        //                    var ms = new MemoryStream();
        //                    bm.Save(ms, ImageFormat.Jpeg);
        //                    Image image = Image.FromStream(ms);

        //                    DataSet dsComprimirImagen = funciones.ComprimirImagen((Image)image.Clone(), ".jpeg", 25);
        //                    if (bool.Parse(dsComprimirImagen.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
        //                        throw new Exception(dsComprimirImagen.Tables[0].Rows[0]["MSG_ERROR"].ToString());

        //                    Image imageCompresa = ((Image)dsComprimirImagen.Tables[0].Rows[0]["DATOS"]);
        //                    pbxLado.Image = imageCompresa;

        //                    try { if (File.Exists(rutaImagenComprimida)) File.Delete(rutaImagenComprimida); } catch (Exception ex) { throw new Exception("Error al eliminar imagen del pasaporte. Detalles: " + ex.Message); }
        //                    ((Image)imageCompresa.Clone()).Save(rutaImagenComprimida, ImageFormat.Jpeg);

        //                }
        //                catch (Exception ex)
        //                {
        //                    MessageBox.Show("Error al generar fotografía visible: " + ex.Message);
        //                    pbxLado.Image = ENROLLMENT_V3.Properties.Resources.anverso;
        //                    this.pbxDocumento.Image = pbxWarning.Image;
        //                }

        //                if (Settings.Default.ESCANEAR_UV)
        //                {
        //                    try
        //                    {
        //                        pbxLadoUV.Image = Api.GetImage(LightSource.Ultraviolet, ImageClipping.Document);
        //                        pbxPasaporteEscaneadoUV.Image = pbxCheck.Image;

        //                        string rutaImagenSinComprimir = Path.Combine(Application.StartupPath, "ENROL", "json") + "\\" + pbxLadoUV.Name + "_" + DateTime.Now.ToString("ddMMyyyyhhmmss", CultureInfo.InvariantCulture) + ".jpeg";
        //                        string rutaImagenComprimida = Path.Combine(Application.StartupPath, "ENROL", "json") + "\\" + pbxLadoUV.Name + "C_" + DateTime.Now.ToString("ddMMyyyyhhmmss", CultureInfo.InvariantCulture) + ".jpeg";
        //                        try { if (File.Exists(rutaImagenSinComprimir)) File.Delete(rutaImagenSinComprimir); } catch (Exception ex) { throw new Exception("Error al eliminar imagen del pasaporte. Detalles: " + ex.Message); }

        //                        Bitmap bm = new Bitmap((Image)pbxLadoUV.Image.Clone());
        //                        bm.Save(rutaImagenSinComprimir, ImageFormat.Jpeg);

        //                        var ms = new MemoryStream();
        //                        bm.Save(ms, ImageFormat.Jpeg);
        //                        Image image = Image.FromStream(ms);

        //                        DataSet dsComprimirImagen = funciones.ComprimirImagen((Image)image.Clone(), ".jpeg", 25);
        //                        if (bool.Parse(dsComprimirImagen.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
        //                            throw new Exception(dsComprimirImagen.Tables[0].Rows[0]["MSG_ERROR"].ToString());

        //                        Image imageCompresa = ((Image)dsComprimirImagen.Tables[0].Rows[0]["DATOS"]);
        //                        pbxLadoUV.Image = imageCompresa;

        //                        try { if (File.Exists(rutaImagenComprimida)) File.Delete(rutaImagenComprimida); } catch (Exception ex) { throw new Exception("Error al eliminar imagen del pasaporte. Detalles: " + ex.Message); }
        //                        ((Image)imageCompresa.Clone()).Save(rutaImagenComprimida, ImageFormat.Jpeg);

        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        MessageBox.Show("Error al generar fotografía ultravioleta: " + ex.Message);
        //                        pbxLadoUV.Image = null;
        //                        pbxPasaporteEscaneadoUV.Image = pbxWarning.Image;
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                pbxDocumento.Image = pbxWarning.Image;
        //                throw new Exception("Error al obtener imagen del pasaporte. Detalles: " + ex.Message + ", " + ex.StackTrace);
        //            }

        //            try
        //            {
        //                //LADO A - MRZ
        //                if(INDICE_ESCANEO <= 1)
        //                {
        //                    if (chkMRZ.Checked) 
        //                        return;

        //                    txtMrz.Text = onScanOcr();
        //                    //if (txtMrz.Text.Equals(string.Empty)) throw new Exception("No se encontró un código MRZ válido para realizar la búsqueda. ");

        //                    if (Settings.Default.TEST)
        //                        txtMrz.Text = "P<GTMWYSS<ALVAREZ<<ROSA<AMELIA<<<<<<<<<<<<<<\r\n1814647643GTM8311195F2704232F7995585<<<<<<98\r\n";

        //                    if (!txtMrz.Text.Equals(string.Empty))
        //                    {
        //                        DataSet dsMrz = new DataSet();
        //                        dsMrz = funciones.ProcesarMRZ(txtMrz.Text);
        //                        if (!bool.Parse(dsMrz.Tables[0].Rows[0]["RESULTADO"].ToString()))
        //                            throw new Exception(dsMrz.Tables[0].Rows[0]["MSG_ERROR"].ToString());

        //                        MRZ mrz = (MRZ)dsMrz.Tables[0].Rows[0]["DATOS"];
        //                        lblNoLibreta.Text = mrz.Optional.Substring(0, 8);

        //                        if (mrz.DocCode.Split('<')[0].Trim().Equals("P"))
        //                            INDICE_ESCANEO = 0;

        //                        //if (mrz.DocIssuer.Equals("GTM"))
        //                        //    ConsultarPasaporte();

        //                        //if(!mrz.DocIssuer.Equals("GTM"))
        //                        {
        //                            DataRow[] drSelect = dsCatalogos.Tables["TipoDocumento"].Select(" codigo = '" + mrz.DocCode.Split('<')[0].Trim() + "'");
        //                            if (drSelect == null || drSelect.Length == 0) MessageBox.Show("TIPO DE DOCUMENTO (" + mrz.DocCode.Split('<')[0].Trim() + ") no encontrado, escanee nuevamente, seleccione uno manualmente o contacte al administrador. ");
        //                            else
        //                            {
        //                                cmbTipoDocumento.SelectedIndex = cmbTipoDocumento.FindStringExact(drSelect[0]["valor"].ToString());
        //                                if (drSelect[0]["codigo"].ToString().Equals("P"))
        //                                    pbxDocumento.Image = pbxCheck.Image;
        //                            }


        //                            drSelect = dsCatalogos.Tables["Pais"].Select(" ICAO = '" + mrz.DocIssuer.Split('<')[0].Trim() + "'");
        //                            if (drSelect == null || drSelect.Length == 0) MessageBox.Show("PAÍS DE EMISIÓN (" + mrz.DocIssuer.Split('<')[0].Trim() + ") no encontrado, escanee nuevamente, seleccione uno manualmente o contacte al administrador. ");
        //                            else cmbPaisEmisor.SelectedIndex = cmbPaisEmisor.FindStringExact(drSelect[0]["Nombre"].ToString());

        //                            txtNumeroDocumento.Text = mrz.DocNumber.Split('<')[0].Trim();
        //                            txtPrimerApellido.Text = mrz.HolderNamePrimary.Split('<')[0].Trim();
        //                            txtSegundoApellido.Text = mrz.HolderNamePrimary.Split('<').Length > 1 ? mrz.HolderNamePrimary.Split('<')[1].Trim() : string.Empty;
        //                            txtPrimerNombre.Text = mrz.HolderNameSecondary.Split('<')[0].Trim();
        //                            txtSegundoNombre.Text = mrz.HolderNameSecondary.Split('<').Length > 1 ? mrz.HolderNameSecondary.Split('<')[1].Trim() : string.Empty;

        //                            drSelect = dsCatalogos.Tables["Genero"].Select(" ICAO = '" + mrz.HolderSex.Trim() + "'");
        //                            if (drSelect == null || drSelect.Length == 0) MessageBox.Show("SEXO (" + mrz.HolderSex.Trim() + ") no encontrado, escanee nuevamente, seleccione uno manualmente o contacte al administrador. ");
        //                            else cmbSexo.SelectedIndex = cmbSexo.FindStringExact(drSelect[0]["Nombre"].ToString());

        //                            drSelect = dsCatalogos.Tables["Pais"].Select(" ICAO = '" + mrz.HolderNationality.Trim() + "'");
        //                            if (drSelect == null || drSelect.Length == 0) MessageBox.Show("NACIONALIDAD (" + mrz.HolderNationality.Split('<')[0].Trim() + ") no encontrado, escanee nuevamente, seleccione uno manualmente o contacte al administrador. ");
        //                            else cmbNacionalidad.SelectedIndex = cmbNacionalidad.FindStringExact(drSelect[0]["Nombre"].ToString());

        //                            string sFecha = "";
        //                            //FECHA NACIMIENTO
        //                            try
        //                            {
        //                                sFecha = mrz.HolderBirthDateDay.Trim() + "/" + mrz.HolderBirthDateMonth.Trim() + "/" + mrz.HolderBirthDateYear.Trim();
        //                                //CultureInfo provider = new CultureInfo("es-GT");
        //                                DateTime fecha = DateTime.ParseExact(sFecha, "dd/MM/yy", CultureInfo.InvariantCulture);

        //                                if (fecha > DateTime.Now)
        //                                    fecha = fecha.AddYears(-100);
        //                                dtpFechaNacimiento.Value = fecha;
        //                            }
        //                            catch { string errorCita = " La fecha de nacimiento incorrecta: " + sFecha + ". "; }

        //                            //FECHA DE VENCIMIENTO
        //                            try
        //                            {
        //                                sFecha = mrz.DocExpiryDateDay.Trim() + "/" + mrz.DocExpiryDateMonth.Trim() + "/" + mrz.DocExpiryDateYear.Trim();
        //                                //CultureInfo provider = new CultureInfo("es-GT");
        //                                DateTime fecha = DateTime.ParseExact(sFecha, "dd/MM/yy", CultureInfo.InvariantCulture);

        //                                if (fecha > DateTime.Now)
        //                                    fecha = fecha.AddYears(-100);
        //                                dtpFechaVencimiento.Value = fecha;
        //                            }
        //                            catch { string errorCita = " La fecha de nacimiento incorrecta: " + sFecha + ". "; }

        //                        }
        //                        pbxDocumento.Image = pbxCheck.Image;

        //                        ValidarCampos();
        //                        //if (!mrz.DocIssuer.Equals("GTM")) throw new Exception("País de emisión no válido (" + mrz.DocIssuer + ")");
        //                        //if (!mrz.DocCode.Substring(0, 1).ToUpper().Equals("P")) throw new Exception("Tipo de documento no válido (" + mrz.DocCode + ")");
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {

        //                pbxDocumento.Image = pbxWarning.Image;
        //                throw new Exception("Error al procesar el MRZ pasaporte. Detalles: " + ex.Message);
        //            }

        //            pbxDocumento.Image = pbxCheck.Image;
        //        }
        //        catch (Exception ex)
        //        {                    
        //            MessageBox.Show("EscanearPasaporte(). " + ex.Message);
        //            txtMensaje.Text = "EscanearPasaporte(). " + ex.Message;                    
        //            btnGuardar.Visible = false;
        //        }
        //    });            
        //}

        private DataSet ConsultarPasaporte()
        {
            MovimientoDiagnostico mv = new MovimientoDiagnostico();
            DateTime fechaIni = DateTime.Now;
            DateTime fechaFin = DateTime.Now;

            string jsonString = "";
            string body = "";

            if (rbLibretaPass.Checked == false) {
                if (cmbTipoDocumento.SelectedValue.Equals("P") && !cmbPaisEmisor.SelectedValue.Equals("GTM"))
                {
                    //pbxDocumento.Image = pbxCheck.Image;
                    pbxCONS.Image = pbxCheck.Image;
                    return null;
                }
            }

            if (!Settings.Default.CONSULTAR_PASAPORTE_GTM) {
                pbxCONS.Image = pbxCheck.Image;
                return null;
            }

            this.panel_inferior.Enabled = false;

            DataSet dsResultado = ArmarDsResultado();
            try
            {
                pbxCONS.Image = pbxWarning.Image;
                pbxCONS.Image = pbxLoad.Image;

                SolicitudEN pasaporte = new SolicitudEN();

                //ENVÍO EN VIVO CON CONEXIÓN DIRECTA A BASE DE DATOS SIN USO DE ARCHIVO TEMPORAL NI ENVÍO EN SEGUNDO PLANO
                if (Settings.Default.TIPO_CONSULTA_PASAPORTE.Equals("LOCAL"))
                {
                    jsonString = txtNoDocumento.Text;
                    if (!pasaporteEntregaLN.ConsultarLibreta(txtNoDocumento.Text))
                        throw new Exception(pasaporteEntregaLN.GetError());

                    pasaporte = pasaporteEntregaLN.GetDatosBiograficos();

                    pasaporteEntregaLN.ConsultarEstado(pasaporte.id);
                    string estadoSolicitud = pasaporteEntregaLN.GetEstado();
                    //if (!estadoSolicitud.Trim().Equals("PostImpresion"))
                    //    throw new Exception("Estado solicitud esperado: PostImpresion. Encontrado: " + estadoSolicitud);

                    if (!pasaporteEntregaLN.ConsultarFotografia(pasaporte.id))
                        throw new Exception(pasaporteEntregaLN.GetError());

                    pasaporte.foto = pasaporteEntregaLN.GetFotografia();

                    if (!pasaporteEntregaLN.ConsultarHuellas(pasaporte.id))
                        throw new Exception(pasaporteEntregaLN.GetError());

                    SolicitudEN huellas = pasaporteEntregaLN.GetHuellas();

                    pasaporte.huella_png1 = huellas.huella_png1;
                    pasaporte.huella_png2 = huellas.huella_png2;
                    pasaporte.huella_pos1 = huellas.huella_pos1;
                    pasaporte.huella_pos2 = huellas.huella_pos2;
                    pasaporte.huella_wsq1 = huellas.huella_wsq1;
                    pasaporte.huella_wsq2 = huellas.huella_wsq2;

                    lblNoCaso.Text = pasaporte.caso;

                    fechaFin = DateTime.Now;
                }

                //BIOGRAFIA
                if (Settings.Default.TIPO_CONSULTA_PASAPORTE.Equals("WS"))
                {
                    jsonString = txtNoDocumento.Text;
                    DataSet dsPasaporte = funciones.GetBiografiaByLibreta(txtNoDocumento.Text, loginData);
                    fechaFin = DateTime.Now;

                    if (bool.Parse(dsPasaporte.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                        throw new Exception(dsPasaporte.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                    ConsultaPasaporte consultaPasaporte = (ConsultaPasaporte)dsPasaporte.Tables[0].Rows[0]["DATOS"];

                    if (consultaPasaporte.codigo != 200) 
                        throw new Exception("Error en consulta de BIOGRAFIA. Detalles: " + consultaPasaporte.mensaje + ".");

                    pasaporte = consultaPasaporte.data;
                    body = JsonConvert.SerializeObject(consultaPasaporte.data);

                    if (pasaporte == null) throw new Exception("No se encontró ningún registro (BIOGRAFIA) para la libreta No. " + txtNoDocumento.Text);
                }

                txtPrimerNombre.Text = pasaporte.primer_nombre.Trim();
                txtSegundoNombre.Text = pasaporte.segundo_nombre.Trim();
                txtPrimerApellido.Text = pasaporte.primer_apellido.Trim();
                txtSegundoApellido.Text = pasaporte.segundo_apellido.Trim();

                DataRow[] drSelect = dsCatalogos.Tables["Genero"].Select(" ICAO = '" + pasaporte.sexo + "'");
                if (drSelect == null || drSelect.Length == 0) MessageBox.Show("SEXO (" + pasaporte.sexo + ") no encontrado, escanee nuevamente, seleccione uno manualmente o contacte al administrador. ");
                else cmbSexo.SelectedIndex = cmbSexo.FindStringExact(drSelect[0]["Nombre"].ToString());

                dtpFechaNacimiento.Value = DateTime.ParseExact(pasaporte.fecha_nacimiento, "d/MM/yyyy", CultureInfo.InvariantCulture);

                DataSet dsEdad = funciones.CalcularEdad(dtpFechaNacimiento.Value.ToString("dd/MM/yyyy"));
                if (bool.Parse(dsEdad.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                    throw new Exception(dsEdad.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                string datosEdad = dsEdad.Tables[0].Rows[0]["DATOS"].ToString();

                if (datosEdad.Split('*').Length > 0)
                {
                    txtEdad.Text = datosEdad.Split('*')[0];
                    CultureInfo provider = new CultureInfo("es-GT");
                    dtpFechaNacimiento.Value = DateTime.ParseExact(datosEdad.Split('*')[1].Split(' ')[0], "d/MM/yyyy", provider);
                }
                else
                    txtEdad.Text = dsEdad.Tables[0].Rows[0]["DATOS"].ToString();

                chkMNA.Enabled = !(int.Parse(txtEdad.Text) >= 18);
                if (int.Parse(txtEdad.Text) >= 18)
                    chkMNA.Checked = false;

                lblIdSolicitud.Text = pasaporte.id.ToString();

                //FOTOGRAFIA WS
                if (Settings.Default.TIPO_CONSULTA_PASAPORTE.Equals("WS"))
                {
                    SolicitudEN foto = new SolicitudEN();
                    DataSet dsPasaporte = funciones.GetFotoByLibreta(txtNoDocumento.Text, loginData);

                    if (bool.Parse(dsPasaporte.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                        throw new Exception(dsPasaporte.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                    ConsultaPasaporte consultaPasaporte = (ConsultaPasaporte)dsPasaporte.Tables[0].Rows[0]["DATOS"];

                    if (consultaPasaporte.codigo != 200) 
                        throw new Exception("Error en consulta de FOTOGRAFIA. Detalles: " + consultaPasaporte.mensaje + ".");

                    foto = consultaPasaporte.data;
                    if (pasaporte == null) 
                        throw new Exception("No se encontró ningún registro (FOTO) para la libreta No. " + txtNoDocumento.Text);

                    pasaporte.foto = foto.foto;
                }

                if (pasaporte.foto != null && !pasaporte.foto.ToString().Replace("data:image/png;base64,", "").Equals(string.Empty) && !pasaporte.foto.ToString().Replace("data:image/png;base64,", "").Equals(""))
                {
                    pbxFotoExterna.Image = funciones.ImageFromBase64String(pasaporte.foto.ToString().Replace("data:image/png;base64,", ""));
                    pbxFotoExterna.Image.Tag = "FotoTitular";
                }

                nFVPos1.Finger = null;
                nFVPos2.Finger = null;
                if (Settings.Default.VERIFICAR_HUELLA_GTM)
                {
                    //HUELLAS WS
                    if (Settings.Default.TIPO_CONSULTA_PASAPORTE.Equals("WS"))
                    {
                        SolicitudEN huellas = new SolicitudEN();
                        DataSet dsPasaporte = funciones.GetHuellasByLibreta(txtNoDocumento.Text, loginData);

                        if (bool.Parse(dsPasaporte.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                            throw new Exception(dsPasaporte.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                        ConsultaPasaporte consultaPasaporte = (ConsultaPasaporte)dsPasaporte.Tables[0].Rows[0]["DATOS"];

                        if (consultaPasaporte.codigo != 200)
                            throw new Exception("Error en consulta de FOTOGRAFIA. Detalles: " + consultaPasaporte.mensaje + ".");

                        huellas = consultaPasaporte.data;
                        if (pasaporte == null)
                            throw new Exception("No se encontró ningún registro (FOTO) para la libreta No. " + txtNoDocumento.Text);

                        pasaporte.huella_png1 = huellas.huella_png1;
                        pasaporte.huella_png2 = huellas.huella_png2;
                        pasaporte.huella_pos1 = huellas.huella_pos1;
                        pasaporte.huella_pos2 = huellas.huella_pos2;
                        pasaporte.huella_wsq1 = huellas.huella_wsq1;
                        pasaporte.huella_wsq2 = huellas.huella_wsq2;
                    }

                    lstHuellas.Items.Clear();

                    if (pasaporte.huella_pos1 != null) if (int.TryParse(pasaporte.huella_pos1, out int i)) CargarLstDedos(lstHuellas, int.Parse(pasaporte.huella_pos1));
                    if (pasaporte.huella_pos2 != null) if (int.TryParse(pasaporte.huella_pos2, out int i)) CargarLstDedos(lstHuellas, int.Parse(pasaporte.huella_pos2));

                    chkPersonaSinHuellas.Checked = false;
                    chkPersonaSinHuellas.Enabled = true;

                    if (lstHuellas.Items.Count == 0)
                    {
                        pbxCompararHuellas.Image = pbxCheck.Image;
                        pbxCONS.Image = pbxCheck.Image;

                        chkPersonaSinHuellas.Checked = true;
                        chkPersonaSinHuellas.Enabled = false;

                        btnGuardar.Visible = true;
                        MessageBox.Show("Persona sin huellas en base de datos.");

                        if (_biometricFingerClient.CurrentBiometric != null)
                            _biometricFingerClient.Cancel();

                        dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
                        dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                        dsResultado.Tables[0].Rows[0]["DATOS"] = null;

                        this.panel_inferior.Enabled = true;
                        return dsResultado;
                    }

                    DataSet dsFinger8b = funciones.StringBase64ToFinger8b(pasaporte.huella_wsq1 != null ? pasaporte.huella_wsq1 : pasaporte.huella_wsq2);
                    if (!bool.Parse(dsFinger8b.Tables[0].Rows[0]["RESULTADO"].ToString())) throw new Exception("Error al convertir una huella en Finger8b (1): " + dsFinger8b.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                    if (pasaporte.huella_wsq1 != null)
                    {
                        nFVPos1.Finger = new NFinger { Image = ((NImage)(dsFinger8b.Tables[0].Rows[0]["DATOS"])) };
                        nFVPos1.Finger.Position = (NFPosition)int.Parse(pasaporte.huella_pos1);
                    }

                    dsFinger8b = funciones.StringBase64ToFinger8b(pasaporte.huella_wsq2 != null ? pasaporte.huella_wsq2 : pasaporte.huella_wsq1);
                    if (!bool.Parse(dsFinger8b.Tables[0].Rows[0]["RESULTADO"].ToString())) throw new Exception("Error al convertir una huella en Finger8b (2): " + dsFinger8b.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                    if (pasaporte.huella_wsq2 != null)
                    {
                        nFVPos2.Finger = new NFinger { Image = ((NImage)(dsFinger8b.Tables[0].Rows[0]["DATOS"])) };
                        nFVPos2.Finger.Position = (NFPosition)int.Parse(pasaporte.huella_pos2);
                    }

                    CompararHuellas(false);
                }

                cmbNacionalidad.SelectedIndex = cmbNacionalidad.FindStringExact("GUATEMALA");
                cmbPaisResidencia.SelectedIndex = cmbPaisResidencia.FindStringExact("GUATEMALA");
                cmbPaisNacimiento.SelectedIndex = cmbPaisNacimiento.FindStringExact("GUATEMALA");
                cmbPaisEmisor.SelectedIndex = cmbPaisEmisor.FindStringExact("GUATEMALA");
                cmbTipoPasaporte.SelectedIndex = 1;
                //txtNumeroDocumento.Text = "176304223";


                pbxCONS.Image = pbxCheck.Image;
                btnGuardar.Visible = true;
                ValidarPasaporte();

                //ConsultarDeclaracion();

                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                dsResultado.Tables[0].Rows[0]["DATOS"] = null;
            }
            catch (Exception ex)
            {
                pbxCONS.Image = pbxWarning.Image;
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "ConsultarPasaporte(). " + ex.Message;
                MessageBox.Show("ConsultarPasaporte(). " + ex.Message);
            }

            this.panel_inferior.Enabled = true;

            if (Settings.Default.MODO_DIAGNÓSTICO)
            {
                mv.id_movimiento = idMovimiento.ToString();
                mv.tipo_documento = string.Empty;
                mv.numero_documento = string.Empty;
                mv.nombre = string.Empty;
                mv.fecha_nacimiento = string.Empty;
                mv.comando = "ConsultarPasaporte()";
                mv.segundos = 0;
                mv.fecha_ini = fechaIni;
                mv.fecha_fin = fechaFin;
                mv.request = jsonString;
                mv.response = body;

                ReportesDB reporte = new ReportesDB();
                reporte.InsertarRegistro(mv);
            }

            return dsResultado;
        }

        private void lbl_titulo_Click(object sender, EventArgs e)
        {

        }

        private void label22_Click(object sender, EventArgs e)
        {

        }

        private void pbxCONS_Click(object sender, EventArgs e)
        {

        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {

        }

        private DataSet ConsultarAlertas()
        {

            AlertasByPersonaRequest alertasByPersona = new AlertasByPersonaRequest();
            alertasByPersona.numerodocumento = txtNumeroDocumento.Text;
            alertasByPersona.tipodocumento = cmbTipoDocumento.SelectedValue.ToString();
            alertasByPersona.icaopaisemision = cmbPaisEmisor.SelectedValue.ToString();
            alertasByPersona.primernombre = txtPrimerNombre.Text;
            alertasByPersona.segundonombre = txtSegundoNombre.Text;
            alertasByPersona.tercernombre = string.Empty;
            alertasByPersona.primerapellido = txtPrimerApellido.Text;
            alertasByPersona.segundoapellido = txtSegundoApellido.Text;
            alertasByPersona.apellidocasada = string.Empty;
            alertasByPersona.usuarioingresa = loginData.USUARIO;

            lblIdBusqueda.Text = "-1";
            pbxAlertas.Image = pbxStart.Image;
            pbxAlertas.Image = pbxLoad.Image;
            pbxAlertas.Tag = "";

            MovimientoDiagnostico mv = new MovimientoDiagnostico();
            DateTime fechaIni = DateTime.Now;
            DateTime fechaFin = DateTime.Now;

            string jsonString = "";
            string body = "";

            DataSet dsResultado = funciones.GetDsResultado();
            try
            {
                jsonString = JsonConvert.SerializeObject(alertasByPersona);
                byte[] data = UTF8Encoding.UTF8.GetBytes(jsonString);

                HttpWebRequest request;
                string url = Settings.Default.API_REST_MIROS +  @Settings.Default.API_ALERTAS;
                request = WebRequest.Create(url) as HttpWebRequest;
                request.Headers.Add("Authorization", $"Bearer {loginData.JWT_TOKEN}");
                request.Timeout = 10 * 1000;
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Accept = "application/json";
                request.ContentLength = data.Length;

                Stream postStream = request.GetRequestStream();
                postStream.Write(data, 0, data.Length);

                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                StreamReader reader = new StreamReader(response.GetResponseStream());
                body = reader.ReadToEnd();

                fechaFin = DateTime.Now;

                AlertasResponse alertasResponse;
                alertasResponse = JsonConvert.DeserializeObject<AlertasResponse>(body);

                if (alertasResponse.codigo != 200)
                    throw new Exception("Ocurrió un error código " + alertasResponse.codigo + ": " + alertasResponse.mensaje);

                lblIdBusqueda.Text = alertasResponse.data[0].idbusqueda.ToString();
                pbxAlertas.Image = pbxCheck.Image;

                dsResultado.Tables[0].Rows[0]["DATOS"] = alertasResponse;
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;

            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "ConsultarAlertas(). " + ex.Message;

                pbxAlertas.Image = pbxError.Image;
                pbxAlertas.Tag = "ConsultarAlertas(). " + ex.Message;
            }

            if (Settings.Default.MODO_DIAGNÓSTICO)
            {
                mv.id_movimiento = idMovimiento.ToString();
                mv.tipo_documento = string.Empty;
                mv.numero_documento = string.Empty;
                mv.nombre = string.Empty;
                mv.fecha_nacimiento = string.Empty;
                mv.comando = "ConsultaAlertas()";
                mv.segundos = 0;
                mv.fecha_ini = fechaIni;
                mv.fecha_fin = fechaFin;
                mv.request = jsonString;
                mv.response = body;

                ReportesDB reporte = new ReportesDB();
                reporte.InsertarRegistro(mv);
            }

            return dsResultado;
        }

        private Task<DataSet> ConsultarSIOM(MovimientoSIOMRequest movimientoSIOM)
        {
            return Task.Run(() =>
            {
                MovimientoDiagnostico mv = new MovimientoDiagnostico();
                DateTime fechaIni = DateTime.Now;
                DateTime fechaFin = DateTime.Now;

                string jsonString = "";
                string body = "";

                DataSet dsResultado = funciones.GetDsResultado();
                try
                {
                    jsonString = JsonConvert.SerializeObject(movimientoSIOM);
                    byte[] data = UTF8Encoding.UTF8.GetBytes(jsonString);

                    HttpWebRequest request;
                    string url = Settings.Default.API_REST_MIROS + @Settings.Default.API_MOVIMIENTO_SIOM;
                    request = WebRequest.Create(url) as HttpWebRequest;
                    request.Headers.Add("Authorization", $"Bearer {loginData.JWT_TOKEN}");
                    request.Timeout = 10 * 1000;
                    request.Method = "POST";
                    request.ContentType = "application/json";
                    request.Accept = "application/json";
                    request.ContentLength = data.Length;

                    Stream postStream = request.GetRequestStream();
                    postStream.Write(data, 0, data.Length);

                    HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    body = reader.ReadToEnd();

                    fechaFin = DateTime.Now;

                    MovimientoSIOMResponse movimientoSIOMRespnose;
                    movimientoSIOMRespnose = JsonConvert.DeserializeObject<MovimientoSIOMResponse>(body);

                    if (movimientoSIOMRespnose.codigo != 200)
                        throw new Exception("Ocurrió un error código " + movimientoSIOMRespnose.codigo + ": " + movimientoSIOMRespnose.mensaje);

                    dsResultado.Tables[0].Rows[0]["DATOS"] = movimientoSIOMRespnose;
                    dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;

                }
                catch (Exception ex)
                {
                    dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                    dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "ConsultarSIOM(). " + ex.Message;
                }

                if (Settings.Default.MODO_DIAGNÓSTICO)
                {
                    mv.id_movimiento = idMovimiento.ToString();
                    mv.tipo_documento = string.Empty;
                    mv.numero_documento = string.Empty;
                    mv.nombre = string.Empty;
                    mv.fecha_nacimiento = string.Empty;
                    mv.comando = "ConsultarSIOM()";
                    mv.segundos = 0;
                    mv.fecha_ini = fechaIni;
                    mv.fecha_fin = fechaFin;
                    mv.request = jsonString;
                    mv.response = body;

                    ReportesDB reporte = new ReportesDB();
                    reporte.InsertarRegistro(mv);
                }

                return dsResultado;
            });
        }

        private DataSet ConsultarDeclaracion()
        {
            MovimientoDiagnostico mv = new MovimientoDiagnostico();
            DateTime fechaIni = DateTime.Now;
            DateTime fechaFin = DateTime.Now;

            string jsonString = "";
            string body = "";

            if (Settings.Default.CONSULTAR_DJRV == false)
                return null;

            DataSet dsResultado = ArmarDsResultado();
            try
            {
                btnDJRV.BackColor = Color.Red;
                pbxDeclaracion.Image = pbxWarning.Image;
                pbxDeclaracion.Image = pbxLoad.Image;

                SolicitudEN pasaporte = new SolicitudEN();

                string fechaNac = dtpFechaNacimiento.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                string tipoMov = lbl_sede.Text.Split('/')[1].Trim().Substring(0, 1);
                string nacionalidad = cmbNacionalidad.SelectedValue.ToString();
                string fechaMov = DateTime.Today.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);


                DeclaracionByLibretaRequest declaracionRequest = new DeclaracionByLibretaRequest();
                declaracionRequest.libreta = txtNoDocumento.Text;
                declaracionRequest.fechaNac = fechaNac;
                declaracionRequest.tipoMov = tipoMov;
                declaracionRequest.nacionalidad = nacionalidad;
                declaracionRequest.fechaMov = fechaMov;
                declaracionRequest.delegacion = sedeDataEquipo.iddelegacion;

                jsonString = JsonConvert.SerializeObject(declaracionRequest);
                byte[] data = UTF8Encoding.UTF8.GetBytes(jsonString);

                ServicePointManager.ServerCertificateValidationCallback = (s, cert, chain, ssl) => true;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@"" + Settings.Default.API_DECLARACION_BY_LIBRETA);
                request.Timeout = 10 * 1000;
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Accept = "application/json";
                request.Headers.Add("Authorization", $"Bearer {Settings.Default.DECLARACION_TKN}");

                Stream postStream = request.GetRequestStream();
                postStream.Write(data, 0, data.Length);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    fechaFin = DateTime.Now;
                    body = reader.ReadToEnd();
                    DeclaracionResponse declaracionResponse = JsonConvert.DeserializeObject<DeclaracionResponse>(body);

                    if (declaracionResponse.codigo != 200)
                        throw new Exception("Error al guardar la entrega. Código: " + declaracionResponse.codigo + ", Mensaje: " + declaracionResponse.mensaje);

                    if (declaracionResponse.data == null)
                        throw new Exception("Error de comunicación del API, intente guardar nuevamente y repórtelo al administrador");

                    declaracionData = declaracionResponse.data;

                    if (declaracionData.correlativo == null)
                        btnDJRV.Text = "DJRV: No encontrada" + "/" + declaracionData.id + "/-1";
                    else
                    {
                        btnDJRV.Text = declaracionData.correlativo + "/" + declaracionData.id + "/" + (declaracionData.migrante[0].titular ? "1" : "0");
                        btnDJRV.BackColor = Color.Green;
                    }

                    //btnDJRV.Text = "DJRV: " + (declaracionData.correlativo == null ? "No encontrada" : declaracionData.correlativo) + "/" + declaracionData.id;
                    pbxDeclaracion.Image = pbxCheck.Image;
                    //btnGuardar.Visible = true;

                    dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
                    dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                    dsResultado.Tables[0].Rows[0]["DATOS"] = null;
                }
            }
            catch (Exception ex)
            {
                pbxDeclaracion.Image = pbxWarning.Image;
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "ConsultarDeclaracion(). " + ex.Message;
                MessageBox.Show("ConsultarDeclaracion(). " + ex.Message);
            }

            if (Settings.Default.MODO_DIAGNÓSTICO)
            {
                mv.id_movimiento = idMovimiento.ToString();
                mv.tipo_documento = string.Empty;
                mv.numero_documento = string.Empty;
                mv.nombre = string.Empty;
                mv.fecha_nacimiento = string.Empty;
                mv.comando = "ConsultarDeclaracion()";
                mv.segundos = 0;
                mv.fecha_ini = fechaIni;
                mv.fecha_fin = fechaFin;
                mv.request = jsonString;
                mv.response = body;

                ReportesDB reporte = new ReportesDB();
                reporte.InsertarRegistro(mv);
            }

            return dsResultado;
        }

        private DataSet ConsultarCUI(string cui)
        {
            DataSet dsResultado = ArmarDsResultado();
            try
            {
                DataSet dsWsRenapCui = ConsultaInformacionxCUIWsRenap(cui);

                if (bool.Parse(dsWsRenapCui.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                    throw new Exception(dsWsRenapCui.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                CuiWsRenap cuiWsRenap = (CuiWsRenap)dsWsRenapCui.Tables[0].Rows[0]["DATOS"];
                DataWsRenap vDataWsRenap = (DataWsRenap)cuiWsRenap.data;

                if (vDataWsRenap == null)
                    throw new Exception("CUI no encontrado");

                dpiTitular = new DPI();
                dpiTitular.CUI = cui;
                dpiTitular.PRIMER_NOMBRE = vDataWsRenap.primer_nombre.ToUpper().Trim();
                dpiTitular.SEGUNDO_NOMBRE = (vDataWsRenap.segundo_nombre == null) ? string.Empty : vDataWsRenap.segundo_nombre.ToUpper().Trim();
                dpiTitular.PRIMER_APELLIDO = vDataWsRenap.primer_apellido.ToUpper().Trim(); ;
                dpiTitular.SEGUNDO_APELLIDO = (vDataWsRenap.segundo_apellido == null) ? "" : vDataWsRenap.segundo_apellido.ToUpper();

                CultureInfo provider = new CultureInfo("es-GT");
                dpiTitular.FECHA_NACIMIENTO = DateTime.ParseExact(vDataWsRenap.fecha_nacimiento, "dd/MM/yyyy", provider).ToString("dd/MM/yyyy");
                dpiTitular.SEXO = vDataWsRenap.genero.ToUpper().Trim();

                if (vDataWsRenap.foto != null && !vDataWsRenap.foto.ToString().Replace("data:image/png;base64,", "").Equals(string.Empty) && !vDataWsRenap.foto.ToString().Replace("data:image/png;base64,", "").Equals(""))
                {
                    dpiTitular.IMAGE = funciones.ImageFromBase64String(vDataWsRenap.foto.ToString().Replace("data:image/png;base64,", ""));
                    dpiTitular.IMAGE.Tag = vDataWsRenap.foto_renap ? "FotoTitular" : "FotoDefault";
                }
                else
                {
                    dpiTitular.IMAGE = pbxUsuario.Image;
                    dpiTitular.IMAGE.Tag = "FotoDefault";
                    MessageBox.Show("¡La consulta no devolvió fotografía!");
                }

                txtPrimerNombre.Text = vDataWsRenap.primer_nombre.ToUpper().Trim();
                txtSegundoNombre.Text = (vDataWsRenap.segundo_nombre == null) ? string.Empty : vDataWsRenap.segundo_nombre.ToUpper().Trim();
                txtPrimerApellido.Text = vDataWsRenap.primer_apellido.ToUpper().Trim(); ;
                txtSegundoApellido.Text = (vDataWsRenap.segundo_apellido == null) ? "" : vDataWsRenap.segundo_apellido.ToUpper();

                if (vDataWsRenap.foto != null && !vDataWsRenap.foto.ToString().Replace("data:image/png;base64,", "").Equals(string.Empty) && !vDataWsRenap.foto.ToString().Replace("data:image/png;base64,", "").Equals(""))
                {
                    pbxFotoExterna.Image = funciones.ImageFromBase64String(vDataWsRenap.foto.ToString().Replace("data:image/png;base64,", ""));
                    pbxFotoExterna.Image.Tag = vDataWsRenap.foto_renap ? "FotoTitular" : "FotoDefault";
                }
                else
                {
                    pbxFotoExterna.Image = pbxUsuario.Image;
                    pbxFotoExterna.Image.Tag = "FotoDefault";
                    MessageBox.Show("¡La consulta no devolvió fotografía!");
                }
                
                if (vDataWsRenap.genero.ToUpper().Equals("M"))
                    cmbSexo.SelectedValue = "M";
                else if (vDataWsRenap.genero.ToUpper().Equals("F"))
                    cmbSexo.SelectedValue = "F";
                else
                    CmbIngresoManual(cmbSexo, true, vDataWsRenap.genero + "");// " - D");

                //CultureInfo provider = new CultureInfo("es-GT");
                dtpFechaNacimiento.Value = DateTime.ParseExact(vDataWsRenap.fecha_nacimiento, "d/MM/yyyy", provider);

                DataSet dsEdad = funciones.CalcularEdad(dtpFechaNacimiento.Value.ToString("dd/MM/yyyy"));
                if (bool.Parse(dsEdad.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                    throw new Exception(dsEdad.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                string datosEdad = dsEdad.Tables[0].Rows[0]["DATOS"].ToString();

                if (datosEdad.Split('*').Length > 0)
                {
                    txtEdad.Text = datosEdad.Split('*')[0];
                    dtpFechaNacimiento.Value = DateTime.ParseExact(datosEdad.Split('*')[1].Split(' ')[0], "d/MM/yyyy", provider);
                }
                else
                    txtEdad.Text = dsEdad.Tables[0].Rows[0]["DATOS"].ToString();

                DataRow[] drSelect = dsCatalogos.Tables["Pais"].Select(" ICAO = 'GTM'");
                cmbNacionalidad.SelectedIndex = cmbNacionalidad.FindStringExact(drSelect[0]["Nombre"].ToString());
                
                cmbPaisResidencia.SelectedIndex = cmbPaisResidencia.FindStringExact(drSelect[0]["Nombre"].ToString());
                cmbPaisNacimiento.SelectedIndex = cmbPaisNacimiento.FindStringExact(drSelect[0]["Nombre"].ToString());
                cmbPaisEmisor.SelectedIndex = cmbPaisEmisor.FindStringExact(drSelect[0]["Nombre"].ToString());

                //drSelect = dsCatalogos.Tables["TipoDocumento"].Select(" CODIGO = 'DPI'");
                //cmbTipoDocumento.SelectedIndex = cmbTipoDocumento.FindStringExact(drSelect[0]["valor"].ToString());
                //cmbTipoPasaporte.SelectedIndex = 0;

                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                dsResultado.Tables[0].Rows[0]["DATOS"] = dpiTitular;

                pbxCONS.Image = pbxCheck.Image;

                //pbxValidacionWsRenap.Image = pbxCheckColor.Image;
            }
            catch (Exception ex)
            {
                pbxCONS.Image = pbxWarning.Image;
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "ConsultarCUI(). " + ex.Message;    
            }
            return dsResultado;
        }

        public DataSet ConsultaInformacionxCUIWsRenap(string cui)
        {
            DataSet dsResultado = ArmarDsResultado();
            try
            {
                CuiWsRenap clsCui;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@"" + Settings.Default.CONSULTAS_RENAP + "/renap_cui?cui=" + cui);

                var user = "migracion-pasaportes-enrollment-3.0";
                var password = "abc123";
                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{user}:{password}"));
                request.Headers.Add("Authorization", "Basic " + credentials);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    var json = reader.ReadToEnd();
                    clsCui = JsonConvert.DeserializeObject<CuiWsRenap>(json);
                }

                dsResultado.Tables[0].Rows[0]["DATOS"] = clsCui;
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;

            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "ConsultaInformacionxCUIWsRenap(). " + ex.Message;
            }

            return dsResultado;
        }

        private void CargarCmbDedos(ComboBox cmb, int p)
        {
            try
            {
                if (p > 0) cmb.Items.Add(p + " - " + funciones.IntToStringFinger(p));
                if (p == 0) cmb.Items.Add("--- SELECCIONAR ---");

            }
            catch (Exception ex)
            {

                throw new Exception("CargarCmbDedos(). " + ex.Message);
            }
        }
        
        private void CargarLstDedos(ListBox lst, int p)
        {
            try
            {
                if (p > 0) lst.Items.Add(p + " - " + funciones.IntToStringFinger(p));
            }
            catch (Exception ex)
            {
                throw new Exception("CargarLstDedos(). " + ex.Message);
            }
        }

        protected delegate void Operation();
        protected void handleExceptions(Operation op)
        {
            try
            {
                op();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async void pbxConectarEscanerP_Click(object sender, EventArgs e)
        {
            try
            {
                pbxConectarEscanerP.Image = pbxWarning.Image;
                pbxConectarEscanerP.Image = pbxLoad.Image;

                if (Settings.Default.ESCANER_PASAPORTES.Equals("Desko"))
                {
                    DataSet dsResultado = await ConectarEscanerP();
                    if (bool.Parse(dsResultado.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                        throw new Exception("Error al CONECTAR escáner de pasaporte: " + dsResultado.Tables[0].Rows[0]["MSG_ERROR"].ToString());


                    pbxConectarEscanerP.Image = pbxCheck.Image;
                }
                if (Settings.Default.ESCANER_PASAPORTES.Equals("Regula"))
                {
                    _regulaReader.Connect();
                    //if (!_regulaReader.Connected)
                    //{
                    //    _regulaReader.Connect();
                    //    _regulaReader.InBackground = false;
                    //    UnsubscribeEvents();
                    //    SubscribeEvents();
                    //}
                }

                if (Settings.Default.ESCANER_PASAPORTES.Equals("PentaScanner4x")) {
                    var s =  _runtime.IdAnalyzeReleaseDate;
                    var s1 = _runtime.EnumerateUsbDevices(Desko.IDA.DeviceType.PentaScanner4x).ToArray();
                    if (s1.Length > 0)
                    {
                        _session = _runtime.CreateSession(Desko.IDA.DeviceType.Undefined, s1[0]);
                        _session.DeviceStateChanged += Session_DeviceStateChanged;
                        _session.ProcessStateChanged += Session_ProcessStateChanged;
                        _session.ImageAvailable += Session_ImageAvailable;
                        _session.FieldAvailable += Session_FieldAvailable;
                        _session.ValidationResultAvailable += Session_ValidationResultAvailable;
                        _session.ReferenceDocumentAvailable += Session_ReferenceDocumentAvailable;
                        _session.DocumentFinished += Session_DocumentFinished;
                    }
                    else {
                        throw new Exception("No se encotro escaner de documentos conectado");
                    }
                }
            }
            catch (Exception ex)
            {
                pbxConectarEscanerP.Image = pbxWarning.Image;
                txtMensaje.Text = "pbxConectarEscanerP_Click(). " + ex.Message;
                MessageBox.Show("pbxConectarEscanerP_Click(). " + ex.Message);
            }
        }


        public static void GuiInvoke(Control control, MethodInvoker code)
        {
            try
            {
                if (control != null && control.InvokeRequired)
                {
                    control.Invoke(code);
                    return;
                }

                code.Invoke();
            }
            catch (ObjectDisposedException) { }
        }

        private void Session_DeviceStateChanged(object sender, DeviceStateChangedEventArgs e)
        {
            _deviceStateInfo = e.DeviceStateInfo;

            GuiInvoke(this, () =>
            {
                SuspendLayout();
                try
                {
                    bool connectionEstablished = false;
                    string conState = e.DeviceStateInfo.ConnectionState.ToString();

                    switch (e.DeviceStateInfo.ConnectionState)
                    {
                        case Desko.IDA.ConnectionState.Initializing:
                            pbxConectarEscanerP.Image = pbxLoad.Image;
                            //statusLabelDevice.BackColor = Color.Yellow;
                            break;
                        case Desko.IDA.ConnectionState.ConnectedAndReady:
                            pbxConectarEscanerP.Image = pbxCheck.Image;
                            //statusLabelDevice.BackColor = Color.LimeGreen;
                            connectionEstablished = true;
                            break;
                        case Desko.IDA.ConnectionState.FailedNotAvailable:
                        case Desko.IDA.ConnectionState.FailedNotCompatible:
                        case Desko.IDA.ConnectionState.FailedAlreadyConnected:
                        case Desko.IDA.ConnectionState.ConnectedButOpen:
                            //statusLabelDevice.BackColor = Color.Red;
                            pbxConectarEscanerP.Image = pbxWarning.Image;
                            break;
                    }

                    if (connectionEstablished) {
                        _session.EpassReader.Enabled = false;
                        _session.EpassReader.Portrait = false;
                        _session.EpassReader.Data = false;
                    }

                }
                finally
                {
                    ResumeLayout();
                }

            });
        }

        private void Session_ImageAvailable(object sender, ImageAvailableEventArgs e)
        {
            lock (_enqueuedImages)
            {
                _enqueuedImages.Enqueue(e.Image);
            }
        }

        private void Session_FieldAvailable(object sender, FieldAvailableEventArgs e)
        {
            lock (_enqueuedFields)
            {
                _enqueuedFields.Enqueue(e.Field);
            }
        }

        private void Session_ValidationResultAvailable(object sender, ValidationResultAvailableEventArgs e)
        {

            lock (_enqueuedValidations)
            {
                _enqueuedValidations.Enqueue(e.Validation);
            }

        }

        private void Session_ReferenceDocumentAvailable(object sender, ReferenceDocumentAvailableEventArgs e)
        {
            GuiInvoke(this, () =>
            {
                MessageBox.Show(e.ReferenceDocument.Name);
            });
        }

        private void Session_ProcessStateChanged(object sender, ProcessStateChangedEventArgs e)
        {
            if (e.ProcessStateInfo.ProcessState == ProcessState.DocumentPresent && chkEscanAutomatico.Checked)
            {
                _documentPresent = true;
                pbxDocumentoEnEscaner.Image = pbxLoad.Image;
            }
               
            else if (e.ProcessStateInfo.ProcessState == ProcessState.DocumentRemoved)
                _documentPresent = false;

            GuiInvoke(this, () =>
            {


                if (e.ProcessStateInfo.ProcessState == ProcessState.CapturingStarted)
                {
                    if (_waitingForFlip)
                    {
                        _waitingForFlip = false;
                    }
                }

                if (e.ProcessStateInfo.ProcessState != ProcessState.DocumentPresent && e.ProcessStateInfo.ProcessState != ProcessState.DocumentRemoved)
                {
                    string estadoCapturaLinea1 = GetProcessLine1(e.ProcessStateInfo.ProcessState);
                    string estadoCapturaLinea2 = GetProcessLine2(e.ProcessStateInfo.ProcessState);

                    if (estadoCapturaLinea2 == "Please flip the document") {
                        MessageBox.Show("Por favor voltee el documento");
                    }

                    var r = e.ProcessStateInfo.ProgressMaximum;
                    var a = e.ProcessStateInfo.ProgressValue;

                    var g =  e.ProcessStateInfo.ProgressType != ProgressType.None;

                    switch (e.ProcessStateInfo.ProgressType)
                    {
                        case ProgressType.Incremental:
                            break;
                        case ProgressType.OpenEnded:

                            break;
                    }

                }

                switch (e.ProcessStateInfo.ProcessState)
                {
                    case ProcessState.CapturingStarted:
                        _document = null;
                        break;
                    case ProcessState.ManualDocumentFlipRequired:
                        _waitingForFlip = false;
                        break;
                    case ProcessState.CapturingImages:
                    case ProcessState.CapturingRfid:
                    case ProcessState.CapturingBarcode:
                    case ProcessState.ProcessingBarcode:
                    case ProcessState.ProcessingVizOcr:
                        //flowPanelProcessImages.Visible = flowPanelProcessImages.Controls.Count > 0;
                        break;
                    case ProcessState.ChipCommunicationError:
                        //labelChipCommunicationFailed.Visible = true;
                        break;
                    case ProcessState.WaitingForDocument:
                    case ProcessState.WaitingForDevice:
                    case ProcessState.None:
                    case ProcessState.CapturingFinished:
                        if (_documentPresent)
                            pbxDocumentoEnEscaner.Image = pbxCheck.Image;
                        break;
                    case ProcessState.CapturingCanceled:
                    case ProcessState.CapturingCanceledDeviceLost:
                    case ProcessState.CapturingCanceledDocumentMoved:
                    case ProcessState.CapturingCanceledDeviceOpened:
                    case ProcessState.CapturingCanceledDocumentStuck:
                    case ProcessState.CapturingCanceledTwoMrzPagesScanned:
                    case ProcessState.DeviceIsOpen:
                    case ProcessState.UpdateStarted:
                    case ProcessState.UpdateInProgress:
                    case ProcessState.UpdateFinished:
                    case ProcessState.CapturingCanceledDocumentRemovedPrematurely:
                    case ProcessState.CapturingCanceledChipCommunicationFailed:
                    default:
                        // flowPanelProcessImages.Visible = false;
                        break;
                }
            });
        }

        private string GetProcessLine1(ProcessState processState)
        {
            switch (processState)
            {
                case ProcessState.WaitingForDevice:
                    return "Waiting for device";
                case ProcessState.WaitingForDocument:
                    switch (_deviceStateInfo?.DuplexSupport ?? DuplexSupport.Undefined)
                    {
                        case DuplexSupport.DuplexAutoFeed:
                            return "Please insert document";
                        case DuplexSupport.ManualFlip:
                        default:
                            return "Please apply document";
                    }
                case ProcessState.CapturingStarted:
                case ProcessState.CapturingImages:
                case ProcessState.CapturingRfid:
                case ProcessState.CapturingBarcode:
                case ProcessState.ProcessingVizOcr:
                case ProcessState.ProcessingBarcode:
                    return "Scan in progress";
                case ProcessState.CapturingCanceled:
                case ProcessState.CapturingCanceledDeviceLost:
                case ProcessState.CapturingCanceledDocumentMoved:
                case ProcessState.CapturingCanceledDeviceOpened:
                case ProcessState.CapturingCanceledDocumentStuck:
                case ProcessState.CapturingCanceledTwoMrzPagesScanned:
                case ProcessState.CapturingCanceledDocumentRemovedPrematurely:
                case ProcessState.CapturingCanceledChipCommunicationFailed:
                case ProcessState.ChipCommunicationError:
                    return "Chip communication error";
                case ProcessState.CapturingFinished:
                case ProcessState.FetchingReferenceDocuments:
                    return "Capturing finished";
                case ProcessState.ManualDocumentFlipRequired:
                    return "Detected document with second page";
                case ProcessState.DeviceIsOpen:
                    return "Device is open";
                case ProcessState.UpdateStarted:
                    return "Update started.";
                case ProcessState.UpdateInProgress:
                    return "Update in progress.";
                case ProcessState.UpdateFinished:
                    return "Update completed.";
                case ProcessState.UpdateFailed:
                    return "Update failed!";
                case ProcessState.MdlEngagementCanceled:
                    return "Engagement canceled";
                case ProcessState.MdlDataTransferCanceled:
                    return "Transfer canceled";
                case ProcessState.None:
                default:
                    return string.Empty;
            }
        }

        private string GetProcessLine2(ProcessState processState)
        {
            switch (processState)
            {
                case ProcessState.WaitingForDevice:
                    return "Please connect and turn on the device";
                case ProcessState.WaitingForDocument:
                    return "Scan will start automatically";
                case ProcessState.ManualDocumentFlipRequired:
                    return "Please flip the document";
                case ProcessState.CapturingStarted:
                case ProcessState.CapturingImages:
                case ProcessState.CapturingRfid:
                case ProcessState.CapturingBarcode:
                    switch (_deviceStateInfo?.DuplexSupport ?? DuplexSupport.Undefined)
                    {
                        case DuplexSupport.DuplexAutoFeed:
                            return "Processing document";
                        case DuplexSupport.ManualFlip:
                        default:
                            return "Please do not move or remove the document";
                    }
                case ProcessState.ProcessingBarcode:
                case ProcessState.ProcessingVizOcr:
                case ProcessState.CapturingFinished:
                case ProcessState.FetchingReferenceDocuments:
                    if (_documentPresent)
                        return "Document can be removed";
                    else
                        return String.Empty;
                case ProcessState.DeviceIsOpen:
                    return "Please close the device";
                case ProcessState.CapturingCanceledDeviceLost:
                    return "Lost device connection";
                case ProcessState.CapturingCanceledDocumentMoved:
                    return "Document was moved during capturing";
                case ProcessState.CapturingCanceledDeviceOpened:
                    return "Device was opened during capturing";
                case ProcessState.CapturingCanceledDocumentStuck:
                    return "Document is stuck";
                case ProcessState.CapturingCanceledTwoMrzPagesScanned:
                    return "A second MRZ was scanned.";
                case ProcessState.CapturingCanceledDocumentRemovedPrematurely:
                    return "Document removed too early.";
                case ProcessState.ChipCommunicationError:
                    return "Error in chip communication.";
                case ProcessState.CapturingCanceledChipCommunicationFailed:
                    return "Chip communication has failed.";
                case ProcessState.MdlDataTransferCanceled:
                    return "mDL capturing failed.";
                case ProcessState.CapturingCanceled:
                case ProcessState.None:
                default:
                    return string.Empty;
            }
        }

        private void Session_DocumentFinished(object sender, DocumentFinishedEventArgs e)
        {
            _document = e.Document;
            //scanDataModel sdModel = new scanDataModel;
            ScanDataModel sdModel = new ScanDataModel();
            Bitmap img1 = new Bitmap(100,100);
            Bitmap img2 = new Bitmap(100,100);
            int indice = 0;
            int indiceMRZ = 0;
            string mrz1 = "";

            GuiInvoke(this, () =>
            {
                try
                {
                    SuspendLayout();
                    //flowPanelProcessImages.Controls.Clear();

                    if (e.Document.Success)
                    {
                        pbxEscanearPasaporte.Image = pbxCheck.Image;
                    }
                    else
                    {
                        pbxEscanearPasaporte.Image = pbxWarning.Image;
                    }
                    lock (_enqueuedImages)
                    {
                        _enqueuedImages.Clear();
                    }

                    //dataGridImages.Rows.Clear();
                    foreach (var image in e.Document.Images)
                    {
                        if (indice == image.ScanIndex) {
                            if (image.Name == "SCAN_Document_Visible")
                            {
                                img1 = image.Bitmap;
                            }

                            if (image.Name == "SCAN_MRZ_VISIBLE") indiceMRZ = image.ScanIndex;
                        }
                        else {
                            if (image.Name == "SCAN_Document_Visible")
                            {
                                img2 = image.Bitmap;
                            }

                            if (image.Name == "SCAN_Mrz_Visible") indiceMRZ = image.ScanIndex;
                        }
                   }

                    lock (_enqueuedFields)
                    {
                        _enqueuedFields.Clear();
                    }

                    foreach (var field in e.Document.Fields)
                    {
                        if (field.Name == "MRZ_Mrz") {
                            mrz1 = field.Text.Replace("\r", "\r\n");
                        }

                    }

                    lock (_enqueuedValidations)
                    {
                        _enqueuedValidations.Clear();
                    }

                    bool haveTotalResult = false;
                    foreach (var validation in e.Document.Validations)
                    {
                        Color back = SystemColors.Control;
                        String text = "";
                        switch (validation.Result)
                        {
                            case ValidationResult.Failed:
                                back = Color.LightSalmon;
                                text = "Validation: failed.";
                                break;
                            case ValidationResult.Passed:
                                back = Color.LightGreen;
                                text = "Validation: passed.";
                                break;
                            case ValidationResult.Warning:
                                back = Color.Wheat;
                                text = "Validation: warning.";
                                break;
                            case ValidationResult.None:
                                back = SystemColors.Control;
                                text = "Validation: none.";
                                break;
                        }

                        if (validation.Name == "Total")
                        {
                            haveTotalResult = true;
                        }
                    }

                    if (indiceMRZ == 0)
                    {
                        sdModel.Anverso = img1;
                        sdModel.Reverso = img2;
                    }
                    else {
                        sdModel.Anverso = img2;
                        sdModel.Reverso = img1;
                    }

                    sdModel.MrzString = mrz1;

                    ProcessScan(sdModel);
                }
                finally
                {
                    ResumeLayout();
                }

            });
        }

        private async void pbxEscanearPasaporte_Click(object sender, EventArgs e)
        {
            try
            {
                if (pbxEscanearPasaporte.Image.Tag == pbxLoad.Image.Tag)
                {
                    MessageBox.Show("Espere que finalice la operación actual");
                    return;
                }

                if (chkEscanAutomatico.Checked && sender != null) 
                    throw new Exception("No se puede usar esta opción mientras la opción Escan Automático esta seleccionada");
                
                if (pbxCONS.Image == pbxLoad.Image) 
                     throw new Exception("¡Espere a que finalice la consulta actual!");

                //ESCANEO DE PRUEBAS SIN LECTURA FÍSICA DE PASAPORTE
                //{
                //    ScanDataModel scanDataModel = new ScanDataModel();
                //    string pasaporteB64 = "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";

                //    byte[] imageBytes = Convert.FromBase64String(pasaporteB64);
                //    MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);

                //    //Convert byte[] to Image
                //    ms.Write(imageBytes, 0, imageBytes.Length);
                //    Image image = Image.FromStream(ms, true);

                //    scanDataModel.Anverso = (Bitmap)image;
                //    scanDataModel.MrzString = "" +
                //    "P<USAEDWARDS<<SHARON<KAY<<<<<<<<<<<<<<<<<<<<\r\n" +
                //    "5601741507USA4807230F2706074282751376<001138";

                //    ProcessScan(scanDataModel);
                //    return;
                //}

                if (Settings.Default.ESCANER_PASAPORTES.Equals("Desko"))
                {
                    if (!Api.IsDevicePlugged()) throw new Exception("Dispositivo no encontrado, revise la conexión del puerto USB y la corriente del escáner");
                    if (!Api.IsDeviceConnected()) throw new Exception("Dispositivo no conectado, revise la conexión del puerto USB y la corriente del escáner");

                    await EscanearPasaporte();
                }

                if (Settings.Default.ESCANER_PASAPORTES.Equals("Regula"))
                {
                    pbxEscanearPasaporte.Image = pbxWarning.Image;
                    pbxEscanearPasaporte.Image = pbxLoad.Image;

                    pbxAnverso.Image = null;
                    //pbxUltravioleta.Image = null;

                    pbxDocumento.Image = pbxWarning.Image;
                    //pbxPasaporteEscaneadoUV.Image = pbxWarning.Image;

                    _isDataReady = false;
                    _regulaReader.GetImages();

                    await Task.Run(() =>
                    {
                        while (!_isDataReady)
                        {
                            Thread.Sleep(100);
                        }
                    });

                    pbxEscanearPasaporte.Image = pbxCheck.Image;
                }

                if (Settings.Default.ESCANER_PASAPORTES.Equals("PentaScanner4x")) {
                    pbxEscanearPasaporte.Image = pbxWarning.Image;
                    pbxEscanearPasaporte.Image = pbxLoad.Image;
                    _session.PerformProcessStateAction(ProcessStateActionFlags.TriggerCapturing);
                    pbxEscanearPasaporte.Image = pbxCheck.Image;
                }
            }
            catch (Exception ex)
            {
                pbxEscanearPasaporte.Image = pbxWarning.Image;
                MessageBox.Show("pbxEscanearPasaporte_Click" + ex.Message);
            }

        }

        private void chkEscanAutomatico_Click(object sender, EventArgs e)
        {
            //if (chkEscanAutomatico.Checked) pbxEscanearPasaporte.Enabled = false;
            //if (!chkEscanAutomatico.Checked) pbxEscanearPasaporte.Enabled = true;
        }

        private void label46_Click(object sender, EventArgs e)
        {
            if(MessageBox.Show(null, "", null, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    Api.DisconnectFromDevice();
                    
                }
                catch (Exception ex) { throw new Exception("Ocurrió un error al desconectar el dispositivo: " + ex.Message); };
            }            
        }

        private void cmbTipoPasaporte_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private async void cmbEscanerHuellas_SelectedIndexChanged(object sender, EventArgs e)
        {


            //if (cmbEscanerHuellas.SelectedIndex == 0)
            //{
            //    MessageBox.Show("no selecciono lector");
            //}

            NFScanner nfsScanner = cmbEscanerHuellas.SelectedItem as NFScanner;
            if(nfsScanner != null) { 
            dspEscanerHuellas = new Dispositivo();
            dspEscanerHuellas.make = nfsScanner.Make;
            dspEscanerHuellas.model = nfsScanner.Model;
            dspEscanerHuellas.serialnumber = nfsScanner.SerialNumber;

            _biometricFingerClient.FingerScanner = nfsScanner;

            //cmbEscanerHuellas.Enabled = false;
            pbxCompararHuellas.Image = pbxWarning.Image;
            //tab_principal.TabPages["tabPasaporte"].ImageKey = "warning.bmp";
            try
            {
                funciones.MostrarHuellaDesdeBytes(null, nFVH2);

                NFScanner nFScanner = cmbEscanerHuellas.SelectedItem as NFScanner;
                _biometricFingerClient.FingerScanner = nFScanner;

                DataSet dsEsEscanerHuellas442 = funciones.EsEscanerHuellas442(nFScanner);
                if (bool.Parse(dsEsEscanerHuellas442.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                    throw new Exception(dsEsEscanerHuellas442.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                Escanner442 = bool.Parse(dsEsEscanerHuellas442.Tables[0].Rows[0]["DATOS"].ToString());

                pbxCompararHuellas.Image = pbxLoad.Image;
                if(_biometricFingerClient.CurrentBiometric == null)
                {
                    DataSet dsEscanerHuella = new DataSet();

                    if(Escanner442)
                    {
                        dspEscanerHuellas.type = "442";

                        rbnManoIzquierda.Checked = true;
                        rbnManoIzquierda_Click(null, new EventArgs());
                    }
                    else
                    {
                        dspEscanerHuellas.type = "1";

                        dsEscanerHuella = await funciones.EscanearHuella(NFPosition.RightIndex, nFVHuellaMovimiento, _biometricFingerClient);
                        if (bool.Parse(dsEscanerHuella.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                            throw new Exception(dsEscanerHuella.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                        if (nFVHuellaMovimiento.Finger.Image != null)
                            pbxCompararHuellas.Image = pbxCheck.Image;

                        formularioLimpio = false;

                        if (Settings.Default.VERIFICAR_HUELLA_GTM)
                            CompararHuellas(false);
                        else
                        {
                            tabHuellas.TabPages[0].ImageKey = "check.bmp";
                            pbxCompararHuellas.Image = pbxCheck.Image;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                pbxCompararHuellas.Image = pbxWarning.Image;
                MessageBox.Show("cmbEscanerHuellas_SelectedIndexChanged(). " + ex.Message);
            }
            }
            //cmbEscanerHuellas.Enabled = true;
        }

        private async void CompararHuellas(bool mostrarError)
        {
            try
            {
                //if (pbxCompararHuellas.Image == pbxLoad.Image)
                //    return;

                //if ((NFinger)nFVHuellaAlmacenada.Finger == null)
                //    return;

                pbxCompararHuellas.Image = pbxWarning.Image;
                pbxCompararHuellas.Image = pbxLoad.Image;

                DataSet dsHuellasIguales = new DataSet();

                if (nFVPos1.Finger == null && nFVPos2.Finger == null)
                {
                    if(pbxCONS.Image == pbxCheck.Image)
                    {
                        chkPersonaSinHuellas.Checked = true;
                        return;
                    }
                }

                nFVHuellaAlmacenada.Finger = nFVPos1.Finger != null ? nFVPos1.Finger : nFVPos2.Finger;

                if (nFVHuellaMovimiento.Finger == null) return;

                dsHuellasIguales = await funciones.HuellasIguales((NFinger)nFVHuellaAlmacenada.Finger, (NFinger)nFVHuellaMovimiento.Finger, _biometricFingerClient);

                if (bool.Parse(dsHuellasIguales.Tables[0].Rows[0]["RESULTADO"].ToString()))
                    pbxCompararHuellas.Image = pbxCheck.Image;

                if (!bool.Parse(dsHuellasIguales.Tables[0].Rows[0]["RESULTADO"].ToString()))
                {
                    nFVHuellaAlmacenada.Finger = nFVPos2.Finger != null ? nFVPos2.Finger : nFVPos1.Finger;
                    dsHuellasIguales = await funciones.HuellasIguales((NFinger)nFVHuellaAlmacenada.Finger, (NFinger)nFVHuellaMovimiento.Finger, _biometricFingerClient);

                    if (bool.Parse(dsHuellasIguales.Tables[0].Rows[0]["RESULTADO"].ToString()))
                        pbxCompararHuellas.Image = pbxCheck.Image;
                }
                if (pbxCompararHuellas.Image.Tag != pbxCheck.Image.Tag)
                {
                    //cmbEscanerHuellas_SelectedIndexChanged(new object(), new EventArgs());
                    throw new Exception("Las huellas no coinciden. ");
                }
                ValidarPasaporte();
            }
            catch (Exception ex)
            {
                pbxCompararHuellas.Image = pbxWarning.Image;
                if(mostrarError) MessageBox.Show("CompararHuellasEntrega(). " + ex.Message);
            }
        }

        private void pbxCompararHuellas_Click(object sender, EventArgs e)
        {
            try
            {
                if (chkPersonaSinHuellas.Checked)
                    throw new Exception("Se ha seleccionado la opción para persona sin huellas");
                cmbEscanerHuellas_SelectedIndexChanged(sender, e);
                //CompararHuellasEntrega(true);
            }
            catch (Exception ex)
            {

                MessageBox.Show("pbxCompararHuellas_Click(). " + ex.Message);
            }
        }

        private void CameraListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                _biometricFaceClient.FaceCaptureDevice = CameraListBox.SelectedItem as NCamera;

            }
            catch (Exception ex)
            {
                pbxCompararHuellas.Image = pbxWarning.Image;
                MessageBox.Show("CameraListBox_SelectedIndexChanged(). " + ex.Message);
            }
        }

        //private void 
        
        public DataSet ConsultaArraigosxNombres(string primerNombre, string segundoNombre, string primerApellido, string segundoApellido)
        {
            DataSet dsResultado = ArmarDsResultado();
            //try
            //{
            //    Arraigos arraigo;

            //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@"" + Settings.Default.IP_SERVICIOS_WEB + "/arraigo_nombres_apellidos?primer_nombre=" + primerNombre + "&segundo_nombre=" + segundoNombre + "&primer_apellido=" + primerApellido + "&segundo_apellido=" + segundoApellido);

            //    var user = "migracion-pasaportes-enrollment-3.0";
            //    var password = "abc123";

            //    var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{user}:{password}"));
            //    request.Headers.Add("Authorization", "Basic " + credentials);

            //    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            //    using (Stream stream = response.GetResponseStream())
            //    using (StreamReader reader = new StreamReader(stream))
            //    {
            //        var json = reader.ReadToEnd();
            //        arraigo = JsonConvert.DeserializeObject<Arraigos>(json);
            //    }

            //    string s = JsonConvert.SerializeObject(arraigo.data);
            //    DataTable dt = (DataTable)JsonConvert.DeserializeObject(s, (typeof(DataTable)));

            //    arraigo.informacionArraigos = new DataSet();
            //    arraigo.informacionArraigos.Tables.Add(dt);

            //    dsResultado.Tables[0].Rows[0]["DATOS"] = arraigo;
            //    dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;

            //}
            //catch (Exception ex)
            //{
            //    dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
            //    dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "ConsultaArraigosxNombres(). " + ex.Message;
            //}
            return dsResultado;
        }

        public DataSet ConsultaAlertasxNombres(string primerNombre, string segundoNombre, string primerApellido, string segundoApellido)
        {
            DataSet dsResultado = ArmarDsResultado();
            //try
            //{
            //    Alertas alertas;

            //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@"" + Settings.Default.IP_SERVICIOS_WEB + "/alertas_nombres_apellidos?primer_nombre=" + primerNombre + "&segundo_nombre=" + segundoNombre + "&primer_apellido=" + primerApellido + "&segundo_apellido=" + segundoApellido);

            //    var user = "migracion-pasaportes-enrollment-3.0";
            //    var password = "abc123";

            //    var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{user}:{password}"));
            //    request.Headers.Add("Authorization", "Basic " + credentials);

            //    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            //    using (Stream stream = response.GetResponseStream())
            //    using (StreamReader reader = new StreamReader(stream))
            //    {
            //        var json = reader.ReadToEnd();
            //        alertas = JsonConvert.DeserializeObject<Alertas>(json);
            //    }
            //    string s = JsonConvert.SerializeObject(alertas.data);
            //    DataTable dt = (DataTable)JsonConvert.DeserializeObject(s, (typeof(DataTable)));

            //    alertas.informacionAlerta = new DataSet();
            //    alertas.informacionAlerta.Tables.Add(dt);

            //    dsResultado.Tables[0].Rows[0]["DATOS"] = alertas;
            //    dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;

            //}
            //catch (Exception ex)
            //{
            //    dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
            //    dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "ConsultaAlertaxNombres(). " + ex.Message;
            //}
            return dsResultado;
        }
    

        private Task<DataSet> ConsultarDocumentoPago(string tipoDocumento, string valor)
        {
            return Task.Run(() =>
            {
                DataSet dsResultado = ArmarDsResultado();
                try
                {

                    var user = "migracion-pasaportes-enrollment-3.0";
                    var password = "abc123";
                    var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{user}:{password}"));

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@"" + Settings.Default.IP_SERVICIOS_WEB + "/boleta_banrural_pasaportes?boleta=" + valor);

                    switch (tipoDocumento)
                    {
                        case "CUI":

                            break;

                        case "BOLETA":
                            request = (HttpWebRequest)WebRequest.Create(@"" + Settings.Default.IP_SERVICIOS_WEB + "/boleta_banrural_pasaportes?boleta=" + valor);

                            request.Headers.Add("Authorization", "Basic " + credentials);

                            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                            using (Stream stream = response.GetResponseStream())
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                var json = reader.ReadToEnd();
                                BOLETA vBoleta = JsonConvert.DeserializeObject<BOLETA>(json);
                                dsResultado.Tables[0].Rows[0]["DATOS_PAGO_PASAPORTE"] = vBoleta;
                            }

                            break;

                        case "TRANSACCION":
                            request = (HttpWebRequest)WebRequest.Create(@"" + Settings.Default.IP_SERVICIOS_WEB + "/transaccion_banrural_pasaportes?transaccion=" + valor);
                                
                            request.Headers.Add("Authorization", "Basic " + credentials);

                            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                            using (Stream stream = response.GetResponseStream())
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                var json = reader.ReadToEnd();
                                TRANSACCION vBoleta = JsonConvert.DeserializeObject<TRANSACCION>(json);
                                dsResultado.Tables[0].Rows[0]["DATOS_PAGO_PASAPORTE"] = vBoleta;
                            }
                            break;

                        case "RECIBO":

                            //20/08/2020 CAMBIO EN LA VALIDACIÓN DEL PAGO DE PASAPORTES POR TRANSICIÓN A INSTITUTO GUATEMALTECO DE MIGRACIÓN
                            //request = (HttpWebRequest)WebRequest.Create(@"" + Settings.Default.IP_SERVICIOS_WEB + "/recibo_banrural_pasaportes?recibo=" + valor);

                            string boleta = valor.Split('.')[0].ToString();
                            string recibo = valor.Split('.')[1].ToString();

                            //PUERTO DE DESARROLLO :8080
                            request = (HttpWebRequest)WebRequest.Create(@"" + Settings.Default.IP_SERVICIOS_WEB + ":8080/recibo_banrural_pasaportes_IGM?boleta="+boleta+"&recibo=" + recibo);

                            request.Headers.Add("Authorization", "Basic " + credentials);

                            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                            using (Stream stream = response.GetResponseStream())
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                var json = reader.ReadToEnd();
                                RECIBO vRecibo = JsonConvert.DeserializeObject<RECIBO>(json);
                                dsResultado.Tables[0].Rows[0]["DATOS_PAGO_PASAPORTE"] = vRecibo;
                            }
                            break;
                    }

                    dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;

                }
                catch (Exception ex)
                {
                    dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                    dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "ConsultarDocumentoPago(). " + ex.Message;
                }
                return dsResultado;
            });

        }       

        private void tmrHora_Tick(object sender, EventArgs e)
        {
            lblHora.Text = DateTime.Now.ToLongTimeString();
        }

        int hrsPasaporte, minPasaporte, segPasaporte = 0;
        private void tmrPasaporte_Tick(object sender, EventArgs e)
        {
            try
            {
                if (segPasaporte == 60)
                {
                    minPasaporte++;
                    segPasaporte = 0;
                }

                if (minPasaporte == 60)
                {
                    hrsPasaporte++;
                    minPasaporte = 0;
                }

                if (hrsPasaporte == 60 && minPasaporte == 00 && segPasaporte == 00)
                {
                    hrsPasaporte = 0;
                    //minPasaporte = 0;
                }

                lblTimerPasaporte.Text = hrsPasaporte.ToString().PadLeft(2, '0') + ":" +  minPasaporte.ToString().PadLeft(2, '0') + ":" + segPasaporte.ToString().PadLeft(2, '0');

                segPasaporte++;
            }
            catch (Exception ex)
            {
                MessageBox.Show("tmrPasaporte_Tick(). " + ex.Message);
                txtMensaje.Text = "tmrPasaporte_Tick(). " + ex.Message;
            }
        }

        private void txtEstatura_KeyPress(object sender, KeyPressEventArgs e)
        {           
        }

        private void tsmiGirarIzquierdaProbatorio_Click(object sender, EventArgs e)
        {
            try
            {
                // Try to cast the sender to a ToolStripItem
                ToolStripItem menuItem = sender as ToolStripItem;
                if (menuItem != null)
                {
                    // Retrieve the ContextMenuStrip that owns this ToolStripItem
                    ContextMenuStrip owner = menuItem.Owner as ContextMenuStrip;
                    if (owner != null)
                    {
                        // Get the control that is displaying this context menu
                        Control sourceControl = owner.SourceControl;

                        if (sourceControl != null)
                        {
                            if (sourceControl is Label)
                            {
                                Label labelTemp = (Label)(sourceControl);
                                labelTemp.Image.RotateFlip(RotateFlipType.Rotate90FlipXY);
                                labelTemp.Refresh();

                                System.Drawing.Image imgReal = LoadBitmapUnlocked(labelTemp.Image.Tag.ToString());//CreateNonIndexedImage(labelTemp.Image.Tag.ToString());//Image.FromFile(labelTemp.Image.Tag.ToString());
                                imgReal.RotateFlip(RotateFlipType.Rotate90FlipXY);

                                try { File.Delete(labelTemp.Image.Tag.ToString());} catch { }

                                imgReal.Save(labelTemp.Image.Tag.ToString(), System.Drawing.Imaging.ImageFormat.Jpeg);
                                imgReal.Dispose();

                                System.GC.Collect();
                                System.GC.WaitForPendingFinalizers();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("tsmiGirarIzquierdaProbatorio_Click(). " + ex.Message);
                txtMensaje.Text = "tsmiGirarIzquierdaProbatorio_Click(). " + ex.Message;
            }
        }

        private void tsmiGirarDerechaProbatorio_Click(object sender, EventArgs e)
        {
            try
            {
                // Try to cast the sender to a ToolStripItem
                ToolStripItem menuItem = sender as ToolStripItem;
                if (menuItem != null)
                {
                    // Retrieve the ContextMenuStrip that owns this ToolStripItem
                    ContextMenuStrip owner = menuItem.Owner as ContextMenuStrip;
                    if (owner != null)
                    {
                        // Get the control that is displaying this context menu
                        Control sourceControl = owner.SourceControl;

                        if (sourceControl != null)
                        {
                            if (sourceControl is Label)
                            {
                                Label labelTemp = (Label)(sourceControl);
                                labelTemp.Image.RotateFlip(RotateFlipType.Rotate270FlipXY);
                                labelTemp.Refresh();

                                System.Drawing.Image imgReal = LoadBitmapUnlocked(labelTemp.Image.Tag.ToString());//CreateNonIndexedImage(labelTemp.Image.Tag.ToString());//Image.FromFile(labelTemp.Image.Tag.ToString());
                                imgReal.RotateFlip(RotateFlipType.Rotate270FlipXY);

                                try { File.Delete(labelTemp.Image.Tag.ToString());} catch { }

                                imgReal.Save(labelTemp.Image.Tag.ToString(), System.Drawing.Imaging.ImageFormat.Jpeg);
                                imgReal.Dispose();

                                System.GC.Collect();
                                System.GC.WaitForPendingFinalizers();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("tsmiGirarDerechaProbatorio_Click(). " + ex.Message);
                txtMensaje.Text = "tsmiGirarDerechaProbatorio_Click(). " + ex.Message;
            }
        }       

        private void abrirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {                
                // Try to cast the sender to a ToolStripItem
                ToolStripItem menuItem = sender as ToolStripItem;
                if (menuItem != null)
                {
                    // Retrieve the ContextMenuStrip that owns this ToolStripItem
                    ContextMenuStrip owner = menuItem.Owner as ContextMenuStrip;
                    if (owner != null)
                    {
                        // Get the control that is displaying this context menu
                        Control sourceControl = owner.SourceControl;

                        if (sourceControl != null)
                        {
                            if (sourceControl is Label)
                            {
                                Label labelTemp = (Label)(sourceControl);
                                System.Drawing.Image imgTemp = LoadBitmapUnlocked(labelTemp.Image.Tag.ToString());//CreateNonIndexedImage(labelTemp.Image.Tag.ToString());

                                int anchoProbatorio, altoProbatorio = 0;
                                anchoProbatorio = imgTemp.Width;
                                altoProbatorio = imgTemp.Height;

                                //-1 = Horizontal, 0 = Cuadrado, 1 = Vertical
                                int orientacionProbatorio = 0;

                                if (anchoProbatorio > altoProbatorio)
                                    orientacionProbatorio = -1;
                                else if (altoProbatorio > anchoProbatorio)
                                    orientacionProbatorio = 1;
                                else
                                    orientacionProbatorio = 0;


                                System.Drawing.Rectangle pantalla = Screen.FromControl(this).Bounds;
                                int anchoPantalla, altoPantalla = 0;

                                anchoPantalla = pantalla.Width;
                                altoPantalla = pantalla.Height;

                                VisorProbatorio visor = new VisorProbatorio();
                                visor.Size = new System.Drawing.Size(anchoPantalla, altoPantalla);

                                float relacionAspecto = float.Parse(anchoProbatorio.ToString()) / float.Parse(altoProbatorio.ToString());
                                
                                if (orientacionProbatorio == 1)
                                {
                                    int anchoVisor = Convert.ToInt16(altoPantalla * relacionAspecto);
                                    int altoVisor = altoPantalla - 40;
                                    visor.Size = new System.Drawing.Size(anchoVisor, altoVisor);
                                }   
                                else
                                {
                                    int anchoVisor = Convert.ToInt16(altoPantalla / relacionAspecto);
                                    int altoVisor = altoPantalla - 40;
                                    visor.Size = new System.Drawing.Size(altoVisor, anchoVisor);
                                }
                                    
                                visor.pbxProbatorio.Image = imgTemp;
                                //imgTemp.Dispose();

                                System.GC.Collect();
                                System.GC.WaitForPendingFinalizers();
                                visor.ShowDialog();
                            }
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("abrirToolStripMenuItem_Click(). " + ex.Message);
                txtMensaje.Text = "abrirToolStripMenuItem_Click(). " + ex.Message;
            }
        }

        private void toolSMIEliminar_Click(object sender, EventArgs e)
        {
            try
            {
                // Try to cast the sender to a ToolStripItem
                ToolStripItem menuItem = sender as ToolStripItem;
                if (menuItem != null)
                {
                    // Retrieve the ContextMenuStrip that owns this ToolStripItem
                    ContextMenuStrip owner = menuItem.Owner as ContextMenuStrip;
                    if (owner != null)
                    {
                        // Get the control that is displaying this context menu
                        Control sourceControl = owner.SourceControl;

                        if (sourceControl != null)
                        {
                            Label lbl = (Label)sourceControl;

                            try{File.Delete(lbl.Image.Tag.ToString());}catch { }

                            string rutaThumbnail = lbl.Image.Tag.ToString().Split('.')[0] + "_thumbnail." + lbl.Image.Tag.ToString().Split('.')[1];

                            try { File.Delete(rutaThumbnail); } catch { }

                            lbl.Image.Dispose();
                            lbl.Dispose();                            

                            System.GC.Collect();
                            System.GC.WaitForPendingFinalizers();

                            ValidarProbatorios();

                            Control.ControlCollection controlCollection = splitContainer1.Panel2.Controls;

                            if (controlCollection.Count == 0)
                                x = y = 10;
                            else
                            {
                                int xx;
                                int yy;
                                int imagenes;

                                xx = yy = 10;
                                imagenes = 0;

                                foreach (Control control in controlCollection)
                                {
                                    if (control is Label)
                                    {
                                        imagenes++;

                                        Label labelTemp = (Label)(control);
                                        labelTemp.Location = new System.Drawing.Point(xx, splitContainer1.Panel2.AutoScrollPosition.Y + yy);

                                        xx += ancho + 10;

                                        if (xx > (splitContainer1.Panel2.Width - ancho))
                                        {
                                            yy += alto + 10;
                                            xx = 10;
                                        }
                                    }                                    
                                }
                                x = xx;
                                y = yy;
                                tamanio = imagenes;
                            }
                        }   
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("toolSMIEliminar_Click(). " + ex.Message);
                txtMensaje.Text = "toolSMIEliminar_Click(). " + ex.Message;
            }
        }

        private void btnSeleccionarEscaner_Click(object sender, EventArgs e)
        {
            //Settings.Default.scan = TwainLib.TwainOperations.GetScanSource();
            //Settings.Default.Save();
        }

        private void btnForzarDIzquierdo_Click(object sender, EventArgs e)
        {
        }        

        //public int contadorFotos = 0;
        //public bool FotoCumpleIcao = false;        

        private Bitmap LoadBitmapUnlocked(string file_name)
        {
            using (Bitmap bm = new Bitmap(file_name))
            {
                return new Bitmap(bm);                
            }
        }     
        
        private Color GetColorForConfidence(NIcaoWarnings warnings, NIcaoWarnings flag, byte confidence)
        {
            if ((warnings & flag) == flag)
            {
                //return confidence <= 100 ? WarningColor : IndeterminateColor;
                return confidence <= 100 ? Color.Red : Color.Orange;
            }
            //return NoWarningColor;
            return Color.Green;
        }

        private Color GetColorForFlags(NIcaoWarnings warnings, params NIcaoWarnings[] flags)
        {
            //return flags.Any(f => (f & warnings) == f) ? WarningColor : NoWarningColor;
            return flags.Any(f => (f & warnings) == f) ? Color.Red : Color.Green;
        }

        
      private async void tab_principal_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (tab_principal.SelectedTab.Name.Equals("tabProbatorios"))
                    btn_escanear.Focus();

                await ValidarCampos();
            }
            catch (Exception ex)
            {
            }
        }

        private DataSet ValidarFotografia()
        {
            //return Task.Run(() =>
            //{
            DataSet dsResultado = ArmarDsResultado();
            string msgError = string.Empty;

            try
            {
                 if (pbxRostroIcao.Image == null)
                    msgError += "No se encontró fotografía de entrega. ";                

                if (msgError.Equals(string.Empty) == false || msgError.Equals("") == false)
                    throw new Exception(msgError);

                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                tab_principal.TabPages["tabFotografia"].ImageKey = "check.bmp";

            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "ValidarFotografia(). " + ex.Message;
                tab_principal.TabPages["tabFotografia"].ImageKey = "warning.bmp";
            }

            return dsResultado;

            //});
        }

        private string DedosRenap(int valor)
        {

            //1, 6 = PULGAR
            //2, 7 = INDICE
            //3, 8 = MEDIO
            //4, 9 = ANULAR
            //5, 10 = MEÑIQUE
            switch (valor)
            {
                case 2:
                case 7:
                    return "Index";

                case 1:
                case 6:
                    return "Thumb";

                case 3:
                case 8:
                    return "Middle";

                case 4:
                case 9:
                    return "Ring";

                case 5:
                case 10:
                    return "Little";

                default:
                    return "";
            }


            //CLASIFICACIÓN PARA LECTOR DE UNA HUELLA

            //1 - Indice
            //2 - Pulgar
            //3 - Medio
            //4 - Anular
            //5 - Meñique
            //6 - Ninguno

            //switch (valor)
            //{
            //    case "1":
            //    case "Indice":
            //    case "1 - Indice":
            //        return "Index";
            //        break;

            //    case "2":
            //    case "Pulgar":
            //    case "2 - Pulgar":
            //        return "Thumb";
            //        break;

            //    case "3":
            //    case "Medio":
            //    case "3 - Medio":
            //        return "Middle";
            //        break;

            //    case "4":
            //    case "Anular":
            //    case "4 - Anular":
            //        return "Ring";
            //        break;

            //    case "5":
            //    case "Meñique":
            //    case "5 - Meñique":
            //        return "Little";
            //        break;

            //    default:
            //        return "";

            //}
        }

        public Task<DataSet> ConsultarHuellasSIBIO(string vCui, string vFingerLeft, string vLeftCod, string vFingerRight, string vRightCod)
        {
            return Task.Run(() =>
            {
                DataSet dsResultado = ArmarDsResultado();
                try
                {
                    SibioParametrosConsulta vParametrosSibio = new SibioParametrosConsulta();
                    vParametrosSibio.cui = vCui;
                    vParametrosSibio.fingerleft = vFingerLeft;
                    vParametrosSibio.leftcod = vLeftCod;
                    vParametrosSibio.fingerright = vFingerRight;
                    vParametrosSibio.rightcod = vRightCod;

                    string postString = JsonConvert.SerializeObject(vParametrosSibio);

                    byte[] data = UTF8Encoding.UTF8.GetBytes(postString);

                    HttpWebRequest request;
                    request = WebRequest.Create(@"" + Settings.Default.IP_SERVICIOS_WEB + "/renap_sibio") as HttpWebRequest;
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

                    Sibio vSibio = JsonConvert.DeserializeObject<Sibio>(body);
                    dsResultado.Tables[0].Rows[0]["DATOS_SIBIO"] = vSibio;

                    dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;

                }
                catch (Exception ex)
                {
                    dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                    dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "ConsultarHuellasSIBIO(). " + ex.Message;
                }
                return dsResultado;
            }
            );
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

        private Task<DataSet> ValidarProbatorios()
        {
            return Task.Run(() =>
            {
                DataSet dsResultado = ArmarDsResultado();
                string msgError = string.Empty;

                try
                {
                    int probatoriosValidos = 0;
                    Control.ControlCollection controlCollection = splitContainer1.Panel2.Controls;

                    int cantidadControles = controlCollection.Count;

                    for (int i = 0; i < cantidadControles; i++)
                    {
                        Control control = controlCollection[i];

                        if (control is Label)
                            probatoriosValidos++;

                        if (probatoriosValidos >= 3)
                            i = cantidadControles + 2;
                    }
                    

                    dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
                    dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                    tab_principal.TabPages["tabProbatorios"].ImageKey = "check.bmp";
                }
                catch (Exception ex)
                {
                    dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                    dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "ValidarProbatorios(). " + ex.Message;
                    tab_principal.TabPages["tabProbatorios"].ImageKey = "warning.bmp";
                }

                return dsResultado;
            });

        }

        private void btnImprimir_Click(object sender, EventArgs e)
        {           
        }        
    }
}

public class ImageSelectedEventHandlerArgs : EventArgs
{
    public System.Drawing.Image Image { get; set; }
}