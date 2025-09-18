using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace UtilityCore.Cryptography
{
    public class KeyDual
    {
        public RSACryptoServiceProvider RSA { get; set; }
        protected string _privateKey { get; set; }
        protected string _publicKey { get; set; }

        private static UnicodeEncoding _encoder = new UnicodeEncoding();

        public KeyDual()
        {
            RSA = new RSACryptoServiceProvider(4096);
           _privateKey = RSA.ToXmlString(true);
           _publicKey = RSA.ToXmlString(false);
        }
        public KeyDual(string prikey,string pubkey)
        {
            RSA = new RSACryptoServiceProvider(4096);

            if(string.IsNullOrEmpty(prikey))
                RSA.FromXmlString(pubkey);
            else
                RSA.FromXmlString(prikey);

            _privateKey = prikey;
            _publicKey = pubkey;
        }
        public string getPublickey()
        {
            return _publicKey;
        }
        public string getPrivatekey()
        {
            return _privateKey;
        }

        public byte[] getPublickeyByte()
        {
            return Encoding.ASCII.GetBytes(_publicKey);

        }
        public byte[] getPrivatekeyByte()
        {
            return Encoding.ASCII.GetBytes(_publicKey);

        }
        public void savePublickey(string name)
        {
            string pathDir = Directory.GetCurrentDirectory()+"\\keys\\";
            if (!Directory.Exists(pathDir))
                Directory.CreateDirectory(pathDir);

            FileStream publickey = new FileStream(Path.Combine(pathDir + name + "_publickey.xml"), FileMode.Create, FileAccess.Write);
            byte[] publicbytes = this.getPublickeyByte();
            publickey.Write(publicbytes, 0, publicbytes.Length);
            publickey.Close();
            publickey.Dispose();

        }
        public void savePrivatekey(string name)
        {
            string pathDir = Directory.GetCurrentDirectory() + "\\keys\\";

            if (!Directory.Exists(pathDir))
                Directory.CreateDirectory(pathDir);

            FileStream publickey = new FileStream(Path.Combine(pathDir + name + "_privatekey.xml"), FileMode.Create, FileAccess.Write);
            byte[] publicbytes = this.getPrivatekeyByte();
            publickey.Write(publicbytes, 0, publicbytes.Length);
            publickey.Close();
            publickey.Dispose();
        }

        public static void saveFile(byte[] File,string name)
        {
            string pathDir = Directory.GetCurrentDirectory() + "\\archivosCifrados\\";
            if (!Directory.Exists(pathDir))
                Directory.CreateDirectory(pathDir);

            FileStream publickey = new FileStream(Path.Combine(pathDir + name + "_file.text"), FileMode.Create, FileAccess.Write);
            publickey.Write(File, 0, File.Length);
            publickey.Close();
             publickey.Dispose();
        }





        public byte[] EncrypText(string text,string key)
        {
            RSA = new RSACryptoServiceProvider(1024);
            RSA.FromXmlString(key);
            text.Replace("\n", "");
            byte[] encryptedData =RSA.Encrypt(Encoding.ASCII.GetBytes(text), false);
            return encryptedData;
        }
        public byte[] DecryptText(string text)
        {
            RSA = new RSACryptoServiceProvider(1024);
            RSA.FromXmlString(_privateKey);
            byte[] DecryptedData = RSA.Decrypt(Convert.FromBase64String(text), false);
            return DecryptedData;
        }


        public static byte[] Encrypt(RSA rsa, byte[] input)
        {


            byte[] iv;
            byte[] encryptedSessionKey;
            byte[] encryptedMessage;
            ICryptoTransform ct;

            using (Aes aes = new AesCryptoServiceProvider())
            {
                 iv = aes.IV;

                // Encrypt the session key
                RSAPKCS1KeyExchangeFormatter keyFormatter = new RSAPKCS1KeyExchangeFormatter(rsa);
                encryptedSessionKey = keyFormatter.CreateKeyExchange(aes.Key, typeof(Aes));
                ct = aes.CreateEncryptor();
                // Encrypt the message
                using (MemoryStream ciphertext = new MemoryStream())
                using (CryptoStream cs = new CryptoStream(ciphertext,ct , CryptoStreamMode.Write))
                {
                    //byte[] plaintextMessage = Encoding.UTF8.GetBytes(secretMessage);
                    cs.Write(input, 0, input.Length);
                    cs.Close();

                    encryptedMessage = ciphertext.ToArray();
                }





            }



            // by default this will create a 128 bits AES (Rijndael) object
            SymmetricAlgorithm sa = SymmetricAlgorithm.Create();
            ct = sa.CreateEncryptor();
            byte[] encrypt = ct.TransformFinalBlock(input, 0, input.Length);

            RSAPKCS1KeyExchangeFormatter fmt = new RSAPKCS1KeyExchangeFormatter(rsa);
            byte[] keyex = fmt.CreateKeyExchange( sa.Key);


            // return the key exchange, the IV (public) and encrypted data
            byte[] result = new byte[keyex.Length + sa.IV.Length + encrypt.Length];
            Buffer.BlockCopy(keyex, 0, result, 0, keyex.Length);
            Buffer.BlockCopy(sa.IV, 0, result, keyex.Length+1, sa.IV.Length);
            Buffer.BlockCopy(encrypt, 0, result, keyex.Length + sa.IV.Length, encrypt.Length);
            result = new byte[keyex.Length + sa.IV.Length + encrypt.Length];
            Buffer.BlockCopy(keyex, 0, result, 0, keyex.Length);
            Buffer.BlockCopy(sa.IV, 0, result, keyex.Length, sa.IV.Length);
            Buffer.BlockCopy(encrypt, 0, result, keyex.Length + sa.IV.Length, encrypt.Length);

            return result;
        }

        public static byte[] Decrypt(RSA rsa, byte[] input)
        {
            // by default this will create a 128 bits AES (Rijndael) object
            SymmetricAlgorithm sa = SymmetricAlgorithm.Create();

            byte[] keyex = new byte[rsa.KeySize>>3 ];
            Buffer.BlockCopy(input, 0, keyex, 0, keyex.Length);

            RSAPKCS1KeyExchangeDeformatter def = new RSAPKCS1KeyExchangeDeformatter(rsa);
            //RSAPKCS1KeyExchangeFormatter def = new RSAPKCS1KeyExchangeFormatter(rsa);
            byte[] key = def.DecryptKeyExchange( keyex);

            byte[] iv = new byte[sa.IV.Length];
            Buffer.BlockCopy(input, keyex.Length, iv, 0, iv.Length);

            ICryptoTransform ct = sa.CreateDecryptor(key, iv);
            byte[] decrypt = ct.TransformFinalBlock(input, keyex.Length + iv.Length, input.Length - (keyex.Length + iv.Length));
            return decrypt;
        }


    }
}
