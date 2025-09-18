using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Reflection;

using ENROLLMENT_V3.Properties;
using System.Xml;

using CapaEN;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Xml.Serialization;

namespace ENROLLMENT_V3
{
    public partial class FrmEntrevista : Form
    {
        LoginData loginData;
        public EntrevistaVisaTrabajo entrevista;
        protected override bool ProcessCmdKey(ref Message msg, System.Windows.Forms.Keys keyData)
        {
            if (keyData == System.Windows.Forms.Keys.Enter)
                if (btnVerificar.Visible)
                    this.btnVerificar_MouseClick(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));

            return base.ProcessCmdKey(ref msg, keyData);
        }

        public FrmEntrevista(LoginData _loginData)
        {
            InitializeComponent();

            loginData = _loginData;
            entrevista = new EntrevistaVisaTrabajo();
            entrevista.idempresareclutadora = null;

            pbxSi.Visible = pbxNo.Visible = pbxNR.Visible = false;
        }
        private void btnSalir_Click(object sender, EventArgs e)
        {
            try
            {

                this.Close();
                this.Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name + "(). " + ex.Message);
            }
        }

        private void Ingreso_FormClosing(object sender, FormClosingEventArgs e)
        {   
        }

        private void Verificacion_Load(object sender, EventArgs e)
        {
            try
            {
                FUNCIONES funciones = new FUNCIONES();
                funciones.CargarLogo(pic_txt_dgm);


                lblAgencia.Visible = cmbAgencia.Visible = Settings.Default.ENTREVISTA_TRABAJO_EMPRESA;


            }
            catch (Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name + "(). " + ex.Message);
            }
        }

        private void btnVerificar_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {

                entrevista.visatrabajadortemporal = lblTrabajoTemporal.Text;

                if (this.lblTrabajoTemporal.Text.Equals(string.Empty))
                    throw new Exception("¡Selecione una respuesta!");

                if (lblTrabajoTemporal.Text.Equals("Sí") || lblTrabajoTemporal.Text.Equals("Si"))
                    if (cmbPais.Items.Count == 0 || cmbPais.SelectedIndex < 1)
                        throw new Exception("Seleccione un país. ");

                //if (lblTrabajoTemporal.Text.Equals("Sí") || lblTrabajoTemporal.Text.Equals("Si"))
                //    if (cmbAgencia.Items.Count == 0 || cmbAgencia.SelectedIndex < 1)
                //throw new Exception("Seleccione una agencia. ");

                entrevista.icaopaisdestinovisatrabajo = null;
                if(cmbPais.Items.Count > 0)
                    entrevista.icaopaisdestinovisatrabajo = cmbPais.SelectedValue.ToString();

                entrevista.idempresareclutadora = null;
                if(cmbAgencia.Items.Count > 0)
                    entrevista.idempresareclutadora = int.Parse(cmbAgencia.SelectedValue.ToString());
                
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name + "(). " + ex.Message);
            }
        }

        private void btnSi_MouseClick(object sender, MouseEventArgs e)
        {
            this.lblTrabajoTemporal.Text = "Si";
            pbxSi.Visible = true;
            pbxNo.Visible = false;
            pbxNR.Visible = false;

            CargarComboBoxICAO(cmbPais, true, "Pais");

            DataSet dsAgencias = GetAgencias();
            if (bool.Parse(dsAgencias.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                throw new Exception(dsAgencias.Tables[0].Rows[0]["MSG_ERROR"].ToString());

            CargarComboBox(cmbAgencia, true, "Empresas", "idempresa", "nombreempresa");
        }

        private void btnNo_MouseClick(object sender, MouseEventArgs e)
        {
            this.lblTrabajoTemporal.Text = "No";

            pbxSi.Visible = false;
            pbxNo.Visible = true;
            pbxNR.Visible = false;

            cmbPais.DataSource = null;
            cmbPais.Items.Clear();

            cmbAgencia.DataSource = null;
            cmbAgencia.Items.Clear();
        }

        private void btnNR_MouseClick(object sender, MouseEventArgs e)
        {
            this.lblTrabajoTemporal.Text = "NR";
            pbxSi.Visible = false;
            pbxNo.Visible = false;
            pbxNR.Visible = true;

            cmbPais.DataSource = null;
            cmbPais.Items.Clear();

            cmbAgencia.DataSource = null;
            cmbAgencia.Items.Clear();
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

                string ruta = Application.StartupPath + "\\Catalogos\\" + nombre + ".xml";
                DataSet ds = LeerXmlCatalogos(ruta, "Lista", "Nodo", "ICAO", "Nombre");

                cmb.DisplayMember = "VALOR";
                cmb.ValueMember = "CODIGO";
                cmb.DataSource = ds.Tables[0];

                if (cmb.Items.Count > 0 && SeleccionarIndice)
                    cmb.SelectedIndex = 0;
                else
                    cmb.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                throw new Exception("CargarComboBoxICAO(" + nombre + "). " + ex.Message);
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

                    if (i == 0)
                    {
                        dr["CODIGO"] = "0";
                        dr["VALOR"] = "SELECCIONE";
                        ds.Tables[0].Rows.Add(dr);

                        dr = ds.Tables[0].NewRow();
                        dr["CODIGO"] = nCodigo[0].InnerText;
                        dr["VALOR"] = nValor[0].InnerText;
                        ds.Tables[0].Rows.Add(dr);

                    }
                    else
                    {
                        dr["CODIGO"] = nCodigo[0].InnerText;
                        dr["VALOR"] = nValor[0].InnerText;
                        ds.Tables[0].Rows.Add(dr);
                    }

                    i++;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("LeerXmlCatalogos(). " + ex.Message);
            }

            return ds;
        }

        public DataSet GetAgencias()
        {
            DataSet dsResultado = ArmarDsResultado();
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@"http://172.16.80.8:5000/miros/empresareclutadora/getListEmpresa");
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
                            EmpresaResponse empresaResponse = JsonConvert.DeserializeObject<EmpresaResponse>(body);

                            if (empresaResponse.codigo != 200)
                                throw new Exception("Error al guardar la entrega. Código: " + empresaResponse.codigo + ", Mensaje: " + empresaResponse.mensaje);

                            if (empresaResponse.data.Length == 0)
                                throw new Exception("La colección está vacía. ");

                            if (empresaResponse.data[0] == null)
                                throw new Exception("Error de comunicación del API, intente guardar nuevamente y repórtelo al administrador");

                            string ruta = Application.StartupPath + @"\Catalogos\Empresas.xml";

                            ListaEmpresa lista = new ListaEmpresa();
                            lista.Nodo = new List<NodoEmpresa>();

                            foreach (EmpresaData empresaData in empresaResponse.data)
                            {
                                NodoEmpresa nodo = new NodoEmpresa();
                                nodo.idempresa = empresaData.idempresa;
                                nodo.nombreempresa = empresaData.nombreempresa;
                                lista.Nodo.Add(nodo);
                            }

                            var encoding = Encoding.GetEncoding("ISO-8859-1");
                            var serializer = new XmlSerializer(typeof(ListaEmpresa));
                            using (var writer = new StreamWriter(ruta, false, encoding))
                            {
                                serializer.Serialize(writer, lista);
                            }


                            dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
                            dsResultado.Tables[0].Rows[0]["DATOS"] = empresaResponse.data[0];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "GetAgencias(). " + ex.Message;
            }

            return dsResultado;
        }

        private void CargarComboBox(ComboBox cmb, bool SeleccionarIndice, string archivo, string llave, string nombre)
        {
            try
            {
                cmb.SelectedIndex = -1;
                cmb.DataSource = null;
                cmb.DisplayMember = null;
                cmb.ValueMember = null;
                cmb.Items.Clear();

                string ruta = Application.StartupPath + "\\Catalogos\\" + archivo + ".xml";
                DataSet ds = LeerXmlCatalogos(ruta, "Lista", "Nodo", llave, nombre);

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

            }
            catch (Exception ex)
            {
                throw new Exception("CargarComboBox(" + nombre + "). " + ex.Message);
            }
        }

        private DataSet ArmarDsResultado()
        {
            DataSet ds = new DataSet();
            ds.Tables.Add(new DataTable());
            ds.Tables[0].Columns.Add("RESULTADO", typeof(bool));
            ds.Tables[0].Columns.Add("MSG_ERROR", typeof(string));

            ds.Tables[0].Columns.Add("DATOS", typeof(object));

            DataRow dr = ds.Tables[0].NewRow();
            ds.Tables[0].Rows.Add(dr);

            return ds;
        }
    }
}
