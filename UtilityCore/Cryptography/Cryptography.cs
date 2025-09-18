using System;
using System.Data;
using System.IO;

namespace UtilityCore.Cryptography
{
    public class Cryptography 
    {

        DataSet dsResultado;
        public Cryptography()
        {

            dsResultado = ArmarDsResultado();
        }

        protected DataSet ArmarDsResultado()
        {
            DataSet ds = new DataSet();
            ds = new DataSet();
            ds.Tables.Add(new DataTable());
            ds.Tables[0].Columns.Add("RESULTADO", typeof(string));
            ds.Tables[0].Columns.Add("MSG_ERROR", typeof(string));

            DataRow dr = ds.Tables[0].NewRow();

            ds.Tables[0].Rows.Add(dr);

            return ds;
        }

        public DataSet KeyGeneration(string usuario, string contrasena, string nombreLlavePublica, string nombreLlavePrivada, string ruta)
        {
            #region PublicKey and Private Key Generation
            if (dsResultado == null)
                dsResultado = ArmarDsResultado();
            try
            {
                //PGPSnippet.KeyGeneration.KeysForPGPEncryptionDecryption.GenerateKey("usr_dgm_pasaportes", "5sr_dgm_p@s1p0rt2s", @"C:\Keys\");
                UtilityCore.Cryptography.KeyGeneration.GenerateKey(usuario, contrasena, ruta, nombreLlavePublica, nombreLlavePrivada);
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = "EXITO";
            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = "FRACASO";
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "KeyGeneration(). " + ex.Message;
            }

            return dsResultado;
            #endregion
        }

        public DataSet Encryption(string rutaLlavePublica, string rutaLlavePrivada, string contrasena, string rutaArchivoEncriptado, string rutaArchivoPlano)
        {
            #region PGP Encryption

            if (dsResultado == null)
                dsResultado = ArmarDsResultado();
            try
            {
                Keys encryptionKeys = new Keys(rutaLlavePublica, rutaLlavePrivada, contrasena);
                Encrypt encrypter = new Encrypt(encryptionKeys);
                using (Stream outputStream = File.Create(rutaArchivoEncriptado))
                {
                    encrypter.EncryptAndSign(outputStream, new FileInfo(rutaArchivoPlano));
                }
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = "EXITO";
            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = "FRACASO";
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "Encryption(). " + ex.Message;
            }

            return dsResultado;

            #endregion
        }

        public DataSet Encryption(string rutaLlavePublica, string contrasena, string rutaArchivoEncriptado, string rutaArchivoPlano)
        {
            #region PGP Encryption

            if (dsResultado == null)
                dsResultado = ArmarDsResultado();
            try
            {
                Keys encryptionKeys = new Keys(rutaLlavePublica,  contrasena);
                Encrypt encrypter = new Encrypt(encryptionKeys);
                using (Stream outputStream = File.Create(rutaArchivoEncriptado))
                {
                    encrypter.EncryptAndSign(outputStream, new FileInfo(rutaArchivoPlano));
                }
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = "EXITO";
            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = "FRACASO";
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "Encryption(). " + ex.Message;      
            }

            return dsResultado;

            #endregion
        }

        public DataSet Decryption(string rutaArchivoEncriptado, string rutaLlavePrivada, string contrasena, string rutaArchivoDesencriptado)
        {

            #region PGP Decryption

            if (dsResultado == null)
                dsResultado = ArmarDsResultado();
            try
            {
                //PGPDecrypt.Decrypt("C:\\Keys\\EncryptData.txt", @"C:\Keys\PGPPrivateKey.asc", "5sr_dgm_p@s1p0rt2s", "C:\\Keys\\OriginalText.txt");
             Descryption.Decrypt(rutaArchivoEncriptado, rutaLlavePrivada, contrasena, rutaArchivoDesencriptado);
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = "EXITO";
            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = "FRACASO";
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "Decryption(). " + ex.Message;
            }

            return dsResultado;

            #endregion
        }
    }
}
