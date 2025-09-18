using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using READERDEMO;
using System.Drawing;
using System.Threading;
using System.Xml;
using System.IO;
using System.Windows.Forms;

using ENROLLMENT_V3.Models;

namespace ENROLLMENT_V3.Regula
{
    class RegulaController : Form
    {
        public readonly RegulaReader _regulaReader = new RegulaReader();
        public event EventHandler OnDataReady;

        ScanDataModel scanDataModel;
        FrmEnrolamiento frmEnrolamiento;
        public RegulaController()
        {
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
            Bitmap bmp;
            XmlElement item;
            XmlNodeList noList;

            var o = new XmlDocument();

            ClearResults(); // cleaning previous results if any

            //MRZCheckStatus.Checked = false;
            //MRZCheckStatus.Enabled = true;
            //SecurityCheckStatus.Checked = false;
            //SecurityCheckStatus.Enabled = true;
            //TextComparisonCheckStatus.Checked = false;
            //TextComparisonCheckStatus.Enabled = true;
            //RFIDCheckStatus.Checked = false;
            //RFIDCheckStatus.Enabled = true;
            //BACCheckStatus.Checked = false;
            //BACCheckStatus.Enabled = true;
            //RFIDAACheckStatus.Checked = false;
            //RFIDAACheckStatus.Enabled = true;
            //RFIDCACheckStatus.Checked = false;
            //RFIDCACheckStatus.Enabled = true;
            //RFIDTACheckStatus.Checked = false;
            //RFIDTACheckStatus.Enabled = true;
            //RFIDPACheckStatus.Checked = false;
            //RFIDPACheckStatus.Enabled = true;

            // Getting page images

            int total = _regulaReader.IsReaderResultTypeAvailable((int)(eRPRM_ResultType.RPRM_ResultType_RawImage));
            if (total > 0)
            {
                scanDataModel = new ScanDataModel();
                //m_RegulaReader.ResultPhotoHeight = 128;
                for (var i = 0; i < total; i++)
                {
                    var str = new MemoryStream(_regulaReader.GetReaderFileImage(i));
                    bmp = new Bitmap(str);

                    if(eRPRM_Lights.GetName(typeof(eRPRM_Lights), _regulaReader.CheckReaderImageLight(i)).Equals("RPRM_Light_White_Full"))
                    {
                        scanDataModel.Anverso = bmp;                    
                    }

                    if (eRPRM_Lights.GetName(typeof(eRPRM_Lights), _regulaReader.CheckReaderImageLight(i)).Equals("RPRM_Light_UV"))
                    {
                        scanDataModel.Reverso = bmp;
                    }
                }
            }
            
            // Getting text fields
            total = _regulaReader.IsReaderResultTypeAvailable((int)eRPRM_ResultType.RPRM_ResultType_OCRLexicalAnalyze);

            if (total > 0)
            {
                o.LoadXml(_regulaReader.CheckReaderResultXML((int)eRPRM_ResultType.RPRM_ResultType_OCRLexicalAnalyze, 0, 0));

                noList = o.GetElementsByTagName("Document_Field_Analysis_Info");
                scanDataModel.MrzString = ((XmlElement)noList.Item(13)).InnerText;
                //for (int i = 0; i < noList.Count; i++)
                //{
                //    item = (XmlElement)noList.Item(i);

                //    int j = AnalyzeGrid.Rows.Add();
                //    // this is text field type equal to one of enum eVisualFieldType value 
                //    int fieldType = Convert.ToInt32(item.GetElementsByTagName("Type").Item(0).InnerText);

                //    // this is text field LCID value. For Latin text is equal to 0
                //    int lcid = Convert.ToInt32(item.GetElementsByTagName("LCID").Item(0)?.InnerText);
                //    string caption = eVisualFieldType.GetName(typeof(eVisualFieldType), fieldType);

                //    if (lcid > 0)
                //        caption += string.Format("({0})", lcid);

                //    AnalyzeGrid.Rows[j].Cells[0].Value = caption;
                //    AnalyzeGrid.Rows[j].Cells[1].Value = item.GetElementsByTagName("Field_MRZ").Item(0)?.InnerText;
                //    AnalyzeGrid.Rows[j].Cells[2].Value = item.GetElementsByTagName("Field_RFID").Item(0)?.InnerText;
                //    AnalyzeGrid.Rows[j].Cells[3].Value = item.GetElementsByTagName("Field_Visual").Item(0)?.InnerText;
                //    AnalyzeGrid.Rows[j].Cells[4].Value = item.GetElementsByTagName("Field_Barcode").Item(0)?.InnerText;

                //    for (int k = 1; k <= 4; k++)
                //    {
                //        switch ((eRPRM_FieldVerificationResult)Convert.ToInt32(item.GetElementsByTagName("Matrix" + k)
                //                    .Item(0)
                //                    ?.InnerText))
                //        {
                //            case eRPRM_FieldVerificationResult.RCF_Disabled:
                //                AnalyzeGrid.Rows[j].Cells[k].Style.ForeColor = Color.Black;
                //                break;
                //            case eRPRM_FieldVerificationResult.RCF_Verified:
                //                AnalyzeGrid.Rows[j].Cells[k].Style.ForeColor = Color.DarkGreen;
                //                break;
                //            case eRPRM_FieldVerificationResult.RCF_Not_Verified:
                //                AnalyzeGrid.Rows[j].Cells[k].Style.ForeColor = Color.Red;
                //                break;
                //            default:
                //                AnalyzeGrid.Rows[j].Cells[k].Style.ForeColor = Color.Black;
                //                break;
                //        }
                //    }

                //    for (int k = 5; k <= 10; k++)
                //    {
                //        switch ((eRPRM_FieldVerificationResult)Convert.ToInt32(item.GetElementsByTagName("Matrix" + k)
                //                    .Item(0)
                //                    ?.InnerText))
                //        {
                //            case eRPRM_FieldVerificationResult.RCF_Disabled:
                //                AnalyzeGrid.Rows[j].Cells[k].Value = null;
                //                break;
                //            case eRPRM_FieldVerificationResult.RCF_Compare_True:
                //                AnalyzeGrid.Rows[j].Cells[k].Value = true;
                //                break;
                //            case eRPRM_FieldVerificationResult.RCF_Compare_False:
                //                AnalyzeGrid.Rows[j].Cells[k].Value = false;
                //                break;
                //            default:
                //                AnalyzeGrid.Rows[j].Cells[k].Value = null;
                //                break;
                //        }
                //    }
                //}
            }

            //// Getting graphic fields
            //total = _regulaReader.IsReaderResultTypeAvailable((int)eRPRM_ResultType.RPRM_ResultType_Graphics);
            //if (total > 0)
            //{
            //    // here you may specify any graphic field type you require
            //    var graphicField = eGraphicFieldType.gf_Portrait;
            //    var graphic = _regulaReader.GetReaderGraphicsBitmapByFieldType((int)graphicField);
            //    if (!(graphic is System.DBNull))
            //    {
            //        MemoryStream str = new MemoryStream(graphic);
            //        bmp = new Bitmap(str);
            //        GraphicFieldsGrid.Rows.Add(eGraphicFieldType.GetName(typeof(eGraphicFieldType), (int)graphicField),
            //            bmp);
            //    }
            //}

            //// Getting graphic fields from RFID chip
            //{
            //    // here you may specify any graphic field type you require
            //    var graphicField = eGraphicFieldType.gf_Portrait;
            //    var graphic = _regulaReader.GetRFIDGraphicsBitmapByFieldType((int)graphicField);
            //    if (!(graphic is System.DBNull))
            //    {
            //        MemoryStream str = new MemoryStream(graphic);
            //        bmp = new Bitmap(str);
            //        GraphicFieldsGrid.Rows.Add(
            //            eGraphicFieldType.GetName(typeof(eGraphicFieldType), (int)graphicField) + "_RFID", bmp);
            //    }
            //}

            //// Getting authenticity results
            //int rowNo = 0;
            //total = _regulaReader.IsReaderResultTypeAvailable((int)eRPRM_ResultType.RPRM_ResultType_Authenticity);

            //if (total > 0)
            //{
            //    var xmLstr =
            //        _regulaReader.CheckReaderResultXML((int)eRPRM_ResultType.RPRM_ResultType_Authenticity, 0, 0);
            //    o.LoadXml(xmLstr);
            //    XmlNodeList nodeList;
            //    noList = o.GetElementsByTagName("Document_Authenticity");
            //    string s;
            //    for (int i = 0; i < noList.Count; i++)
            //    {
            //        item = (XmlElement)noList.Item(i);

            //        var subitem = (XmlElement)item.GetElementsByTagName("UV_Luminescence").Item(0);
            //        if (subitem != null)
            //        {
            //            s = subitem.GetElementsByTagName("Result").Item(0)?.InnerText;
            //            if (s != "")
            //            {
            //                rowNo = AuthenticityGrid.Rows.Add("UV_Luminescence",
            //                    eMRZCheckResult.GetName(typeof(eMRZCheckResult), Convert.ToInt32(s)));
            //                if (s == "1")
            //                {
            //                    AuthenticityGrid.Rows[rowNo].Cells[0].Style.ForeColor = Color.DarkGreen;
            //                }
            //                else AuthenticityGrid.Rows[rowNo].Cells[0].Style.ForeColor = Color.Red;
            //            }
            //        }

            //        subitem = (XmlElement)item.GetElementsByTagName("IR_B900").Item(0);
            //        if (subitem != null)
            //        {
            //            s = subitem.GetElementsByTagName("Result").Item(0)?.InnerText;
            //            if (s != "")
            //            {
            //                rowNo = AuthenticityGrid.Rows.Add("IR B900",
            //                    eMRZCheckResult.GetName(typeof(eMRZCheckResult), Convert.ToInt32(s)));
            //                if (s == "1")
            //                {
            //                    AuthenticityGrid.Rows[rowNo].Cells[0].Style.ForeColor = Color.DarkGreen;
            //                }
            //                else AuthenticityGrid.Rows[rowNo].Cells[0].Style.ForeColor = Color.Red;
            //            }
            //        }

            //        //----------------------------------------------------------------
            //        subitem = (XmlElement)item.GetElementsByTagName("IR_Visibility").Item(0);

            //        if (subitem != null)
            //        {
            //            s = subitem.GetElementsByTagName("Result").Item(0)?.InnerText;

            //            nodeList = subitem.GetElementsByTagName("OneElement");
            //            if (nodeList.Count > 0)
            //            {
            //                rowNo = AuthenticityGrid.Rows.Add("IR_Visibility",
            //                    eMRZCheckResult.GetName(typeof(eMRZCheckResult), Convert.ToInt32(s)));
            //                if (s == "1")
            //                {
            //                    AuthenticityGrid.Rows[rowNo].Cells[0].Style.ForeColor = Color.DarkGreen;
            //                }
            //                else AuthenticityGrid.Rows[rowNo].Cells[0].Style.ForeColor = Color.Red;

            //                int elementNo = 1;
            //                foreach (XmlElement node1 in nodeList)
            //                {
            //                    XmlNodeList oneElementList = node1.GetElementsByTagName("Image");

            //                    foreach (XmlNode element in oneElementList)
            //                    {
            //                        if (element.FirstChild is XmlCDataSection)
            //                        {
            //                            s = node1.GetElementsByTagName("Visibility").Item(0)?.InnerText;
            //                            AuthenticityGrid.Rows.Add(String.Format("Element #{0}", elementNo),
            //                                Convert.ToInt32(s) == 0 ? "Invisible" : "Visible");
            //                            XmlCDataSection cdataSection = element.FirstChild as XmlCDataSection;
            //                            MemoryStream str =
            //                                new MemoryStream(Convert.FromBase64String(cdataSection.Value));
            //                            bmp = new Bitmap(str);
            //                            AuthenticityGrid.Rows.Add("", "Image", bmp);
            //                        }
            //                    }

            //                    oneElementList = node1.GetElementsByTagName("EtalonImage");
            //                    foreach (XmlNode element in oneElementList)
            //                    {
            //                        if (element.FirstChild is XmlCDataSection)
            //                        {
            //                            XmlCDataSection cdataSection = element.FirstChild as XmlCDataSection;
            //                            MemoryStream str =
            //                                new MemoryStream(Convert.FromBase64String(cdataSection.Value));
            //                            bmp = new Bitmap(str);
            //                            AuthenticityGrid.Rows.Add("", "Etalon image", bmp);
            //                        }
            //                    }

            //                    elementNo++;
            //                }
            //            }
            //        }

            //        //----------------------------------------------------------------
            //        subitem = (XmlElement)item.GetElementsByTagName("ImagePattern").Item(0);
            //        if (subitem != null)
            //        {
            //            s = subitem.GetElementsByTagName("Result").Item(0)?.InnerText;
            //            nodeList = subitem.GetElementsByTagName("OneElement");
            //            if (nodeList.Count > 0)
            //            {
            //                rowNo = AuthenticityGrid.Rows.Add("ImagePattern",
            //                    eMRZCheckResult.GetName(typeof(eMRZCheckResult), Convert.ToInt32(s)));
            //                if (s == "1")
            //                {
            //                    AuthenticityGrid.Rows[rowNo].Cells[0].Style.ForeColor = Color.DarkGreen;
            //                }
            //                else AuthenticityGrid.Rows[rowNo].Cells[0].Style.ForeColor = Color.Red;

            //                int elementNo = 1;
            //                foreach (XmlElement node1 in nodeList)
            //                {
            //                    XmlNodeList oneElementList = node1.GetElementsByTagName("Image");

            //                    foreach (XmlNode element in oneElementList)
            //                    {
            //                        if (element.FirstChild is XmlCDataSection)
            //                        {
            //                            s = node1.GetElementsByTagName("PercentValue").Item(0).InnerText;
            //                            AuthenticityGrid.Rows.Add(String.Format("Element #{0}", elementNo),
            //                                String.Format("Similarity - {0} %", s));


            //                            XmlCDataSection cdataSection = element.FirstChild as XmlCDataSection;
            //                            MemoryStream str =
            //                                new MemoryStream(Convert.FromBase64String(cdataSection.Value));
            //                            bmp = new Bitmap(str);
            //                            AuthenticityGrid.Rows.Add("", "Image", bmp);
            //                        }
            //                    }

            //                    oneElementList = node1.GetElementsByTagName("EtalonImage");
            //                    foreach (XmlNode element in oneElementList)
            //                    {
            //                        if (element.FirstChild is XmlCDataSection)
            //                        {
            //                            XmlCDataSection cdataSection = element.FirstChild as XmlCDataSection;
            //                            MemoryStream str =
            //                                new MemoryStream(Convert.FromBase64String(cdataSection.Value));
            //                            bmp = new Bitmap(str);
            //                            AuthenticityGrid.Rows.Add("", "Etalon image", bmp);
            //                        }
            //                    }

            //                    elementNo++;
            //                }
            //            }
            //        }
            //    }
            //}

        }

    }
}
