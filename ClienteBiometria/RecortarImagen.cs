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
using System.Drawing;
using System.Drawing.Imaging;

namespace ENROLLMENT_V3
{
    public partial class RecortarImagen : Form
    {
        Rectangle rectRecorteFoto;
        bool isMouseDown = false;
        int factorReduccionFoto = 6;

        Image ImagenRedimension;

        private FrmEnrolamiento enrollmentForm = null;

        public RecortarImagen(FrmEnrolamiento _enrollment)
        {
            enrollmentForm = _enrollment;
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            //IMAGEN DISMINUIDA 6 VECES
            //pbxImagen.Image = Image.FromFile(@"C:\Users\USUARIO\Desktop\IMG_0001 - copia.JPG");

            int anchoFoto = (int) (pbxImagen.Image.Size.Width / factorReduccionFoto);
            int altoFoto = (int)(pbxImagen.Image.Size.Height / factorReduccionFoto);

            pbxImagen.Size = new Size(anchoFoto, altoFoto);

            System.Drawing.Rectangle pantalla = Screen.FromControl(this).Bounds;
            int anchoPantalla, altoPantalla = 0;

            anchoPantalla = pantalla.Width;
            altoPantalla = pantalla.Height;

            int coordenadaX = (anchoPantalla - anchoFoto) / 2;
            pbxImagen.Location = new Point(coordenadaX, pbxImagen.Location.Y);
            pbxImagen.SizeMode = PictureBoxSizeMode.StretchImage;

            rectRecorteFoto = new Rectangle((int)((anchoFoto - 240) / 2), (int) ((altoFoto - 320) / 2), 240, 320);

            btnListo.Enabled = false;
        }

        private void btnExplorarI_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog ofdImagen = new OpenFileDialog();
                ofdImagen.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";

                ofdImagen.FilterIndex = 1;
                ofdImagen.ShowDialog();

                if (ofdImagen.FileName.Equals("") || ofdImagen.FileName.Equals(string.Empty))
                    throw new Exception("¡Seleccione un archivo!");

                if (File.Exists(ofdImagen.FileName) == false)
                    throw new Exception("¡El archivo no existe!");
                
                txtArchivo.Text = ofdImagen.FileName;
                pbxImagen.Image = Image.FromFile(ofdImagen.FileName);
                pbxImagen.ImageLocation = ofdImagen.FileName;
            }
            catch (Exception ex)
            {
                MessageBox.Show("btnImagenEntrada_Click(). " + ex.Message);
                txtMensaje.Text += "btnImagenEntrada_Click(). " + ex.Message;
            }
        }

        private void pbxImagen_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                if(pbxImagen.Image != null)
                    e.Graphics.DrawRectangle(new Pen(Color.Red, 3), rectRecorteFoto);
            }
            catch (Exception ex)
            {
                MessageBox.Show("pbxImagen_Paint(). " + ex.Message);
                txtMensaje.Text += "pbxImagen_Paint(). " + ex.Message;
            }
        }

        private void pbxImagen_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                isMouseDown = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("pbxImagen_MouseDown(). " + ex.Message);
                txtMensaje.Text += "pbxImagen_MouseDown(). " + ex.Message;
            }
        }

        private void pbxImagen_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                isMouseDown = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("pbxImagen_MouseUp(). " + ex.Message);
                txtMensaje.Text += "pbxImagen_MouseUp(). " + ex.Message;
            }
        }

        private void pbxImagen_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (isMouseDown == true)
                {
                    rectRecorteFoto.Location = e.Location;
                    
                    if (rectRecorteFoto.Right > pbxImagen.Width)
                    {
                        rectRecorteFoto.X = pbxImagen.Width - rectRecorteFoto.Width;
                    }
                    if (rectRecorteFoto.Top < 0)
                    {
                        rectRecorteFoto.Y = 0;
                    }
                    if (rectRecorteFoto.Left < 0)
                    {
                        rectRecorteFoto.X = 0;
                    }
                    if (rectRecorteFoto.Bottom > pbxImagen.Height)
                    {
                        rectRecorteFoto.Y = pbxImagen.Height - rectRecorteFoto.Height;
                    }
                    Refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("pbxImagen_MouseMove(). " + ex.Message);
                txtMensaje.Text += "pbxImagen_MouseMove(). " + ex.Message;
            }
        }

        private void btnRecortar_Click(object sender, EventArgs e)
        {
            try
            {
                Rectangle cropRect = new Rectangle((rectRecorteFoto.X * factorReduccionFoto), (rectRecorteFoto.Y * factorReduccionFoto), (rectRecorteFoto.Width * factorReduccionFoto), (rectRecorteFoto.Height * factorReduccionFoto));                
                Bitmap ImagenOriginal = pbxImagen.Image.Clone() as Bitmap;
                Bitmap cropedImage = ImagenOriginal.Clone(cropRect, ImagenOriginal.PixelFormat);

                //cropedImage.Save("Cropped.jpeg", ImageFormat.Jpeg);

                //using (Graphics g = Graphics.FromImage(pbxImagen.Image))
                //{
                //    g.FillRectangle(new SolidBrush(Color.Blue), cropRect);
                //    pbxImagen.Refresh();
                //}

                ImagenRedimension = resizeImage(cropedImage as Image, new Size(480, 640));
                //ImagenRedimension.Save("Redimension.jpeg", ImageFormat.Jpeg);

                enrollmentForm.AsignarFotoRecortada(ImagenRedimension);

                btnListo.Enabled = true;

                MessageBox.Show("¡Recorte exitoso, haga clic en el botón ¡Listo! para cerrar!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("btnRecortar_Click(). " + ex.Message);
                txtMensaje.Text += "btnRecortar_Click(). " + ex.Message;
            }
        }

        private static Image resizeImage(Image imgToResize, Size size)
        {
            int sourceWidth = imgToResize.Width;
            int sourceHeight = imgToResize.Height;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)size.Width / (float)sourceWidth);
            nPercentH = ((float)size.Height / (float)sourceHeight);

            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            //Bitmap b = new Bitmap(destWidth, destHeight);
            Bitmap b = new Bitmap(size.Width, size.Height);

            Graphics g = Graphics.FromImage((Image)b);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            //g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);

            g.DrawImage(imgToResize, 0, 0, size.Width, size.Height);
            g.Dispose();

            return (Image)b;
        }

        private void btnMas_Click(object sender, EventArgs e)
        {
            try
            {
                int ancho = rectRecorteFoto.Size.Width + (int)(rectRecorteFoto.Size.Width * 0.1);
                int alto = rectRecorteFoto.Size.Height + (int)(rectRecorteFoto.Size.Height * 0.1);

                if (ancho > (pbxImagen.Width - rectRecorteFoto.X)|| alto > (pbxImagen.Height - rectRecorteFoto.Y))
                {
                    ancho = rectRecorteFoto.Size.Width;
                    alto = rectRecorteFoto.Size.Height;
                }
                rectRecorteFoto.Size = new Size(ancho, alto);

                pbxImagen.Refresh();

            }
            catch (Exception ex)
            {
                MessageBox.Show("btnMas_Click(). " + ex.Message);
                txtMensaje.Text += "btnMas_Click(). " + ex.Message;
            }
        }

        private void btnMenos_Click(object sender, EventArgs e)
        {
            try
            {
                int ancho = rectRecorteFoto.Size.Width - (int)(rectRecorteFoto.Size.Width * 0.1);
                int alto = rectRecorteFoto.Size.Height - (int)(rectRecorteFoto.Size.Height * 0.1);

                //if (rect.Right < 10 || rect.Bottom < 10)
                if (ancho < 10 || alto < 10)
                {
                    ancho = rectRecorteFoto.Size.Width;
                    alto = rectRecorteFoto.Size.Height;
                }
                rectRecorteFoto.Size = new Size(ancho, alto);

                pbxImagen.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("btnMas_Click(). " + ex.Message);
                txtMensaje.Text += "btnMas_Click(). " + ex.Message;
            }
        }

        private void btnListo_Click(object sender, EventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("btnListo_Click(). " + ex.Message);
                txtMensaje.Text += "btnListo_Click(). " + ex.Message;
            }
        }
    }
}
