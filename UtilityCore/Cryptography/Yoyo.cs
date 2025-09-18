using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace UtilityCore.Cryptography
{
    public class Yoyo
    {
        RSACryptoServiceProvider RSA;
        public string _privateKey { get; }
        public  string _publicKey { get; }

        public Yoyo()
        {
            RSA = new RSACryptoServiceProvider(4096);
            _privateKey = RSA.ToXmlString(true);
            _publicKey = RSA.ToXmlString(false);
        }
        public byte[] publicByte()
        {
            return Encoding.ASCII.GetBytes(_publicKey);

        }
        public byte[] privateByte()
        {
            return Encoding.ASCII.GetBytes(_privateKey);

        }

        public void saveKeys(string name)
        {
            string pathDir = Directory.GetCurrentDirectory() + "\\keys\\";
            if (!Directory.Exists(pathDir))
                Directory.CreateDirectory(pathDir);

            FileStream publickey = new FileStream(Path.Combine(pathDir + name + "_publickey.xml"), FileMode.Create, FileAccess.Write);
            byte[] publicbytes = Encoding.ASCII.GetBytes(_publicKey);
            publickey.Write(publicbytes, 0, publicbytes.Length);
            publickey.Close();
            publickey.Dispose();


            FileStream pribkey = new FileStream(Path.Combine(pathDir + name + "_publickey.xml"), FileMode.Create, FileAccess.Write);
            byte[] pribbytes = Encoding.ASCII.GetBytes(_privateKey);
            pribkey.Write(pribbytes, 0, pribbytes.Length);
            pribkey.Close();
            pribkey.Dispose();

        }

        public static byte[] Encrypt(string _publicKey, byte[] input)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(4096);

            rsa.FromXmlString(_publicKey);
            // by default this will create a 128 bits AES (Rijndael) object
            SymmetricAlgorithm sa = SymmetricAlgorithm.Create();
            ICryptoTransform ct = sa.CreateEncryptor();
            byte[] encrypt = ct.TransformFinalBlock(input, 0, input.Length);

            RSAPKCS1KeyExchangeFormatter fmt = new RSAPKCS1KeyExchangeFormatter(rsa);
            byte[] keyex = fmt.CreateKeyExchange(sa.Key);

            // return the key exchange, the IV (public) and encrypted data
            byte[] result = new byte[keyex.Length + sa.IV.Length + encrypt.Length];
            Buffer.BlockCopy(keyex, 0, result, 0, keyex.Length);
            Buffer.BlockCopy(sa.IV, 0, result, keyex.Length, sa.IV.Length);
            Buffer.BlockCopy(encrypt, 0, result, keyex.Length + sa.IV.Length, encrypt.Length);
            return result;
        }


        public static byte[] Decrypt(string _privateKey, byte[] input)
        {

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(4096);

            rsa.FromXmlString(_privateKey);
            // by default this will create a 128 bits AES (Rijndael) object
            SymmetricAlgorithm sa = SymmetricAlgorithm.Create();

            byte[] keyex = new byte[rsa.KeySize >> 3];
            Buffer.BlockCopy(input, 0, keyex, 0, keyex.Length);

            RSAPKCS1KeyExchangeDeformatter def = new RSAPKCS1KeyExchangeDeformatter(rsa);
            byte[] key = def.DecryptKeyExchange(keyex);

            byte[] iv = new byte[sa.IV.Length];
            Buffer.BlockCopy(input, keyex.Length, iv, 0, iv.Length);

            ICryptoTransform ct = sa.CreateDecryptor(key, iv);
            byte[] decrypt = ct.TransformFinalBlock(input, keyex.Length + iv.Length, input.Length - (keyex.Length + iv.Length));
            return decrypt;
        }

        public static void saveBynaryFile(byte[] data, string name,string extencion,string dir )
        {
            string pathDir;
            if (string.IsNullOrEmpty(dir))
            {
                pathDir = Path.Combine(Directory.GetCurrentDirectory() + "\\archivosCifrados\\");
            }
            else
                pathDir = dir;
                 
            
            if (!Directory.Exists(pathDir))
                Directory.CreateDirectory(pathDir);

            FileStream publickey = new FileStream(Path.Combine(pathDir + name + extencion), FileMode.Create, FileAccess.Write);
            publickey.Write(data, 0, data.Length);
            publickey.Close();
            publickey.Dispose();

            //Stream publickey = File.OpenWrite(Path.Combine(pathDir + name + extencion));

            //publickey.Write(data, 0, data.Length);
            //publickey.Close();
            //publickey.Dispose();


        }



    }
}
