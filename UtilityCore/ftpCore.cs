using System;
using System.IO;
using System.Net;
using System.Text;

namespace UtilityCore
{
    public class FtpCore
    {

        #region vars
        public string path { set; get; } = string.Empty;

        private string url = string.Empty;
        private string user = string.Empty;
        private string pass = string.Empty;
        public string uploadPath = string.Empty;

        NetworkCredential Credentials;
        FtpWebRequest client;
        #endregion

        #region Setters Getters
        public void setUrl(string _url)
        {
            this.url = _url;
        }

        public void setUser(string _user)
        {
            this.user = _user;
        }

        public string getUser()
        {
            return this.user;
        }

        public void setPass(string _pass)
        {
            this.pass = _pass;
        }

        #endregion

        #region Constructors
        public FtpCore()
        {
            
        }

        public FtpCore(string _url, string _user, string _pass)
        {
            this.url = _url;
            this.user = _user;
            this.pass = _pass;
        }
        #endregion

        #region clientMethods
        public void InitCliet() => this.Credentials = new NetworkCredential(user, pass);
        //public void CloseClient() => this.clientc;
        #endregion

        #region upload Files

        public void uploadFile(string pathFile)
        {
            try
            {
                string name = Path.GetFileName(pathFile);
                string ftpURL = url + "/" + uploadPath + Path.GetFileName(pathFile);
                client = (FtpWebRequest)WebRequest.Create(ftpURL);// url + uploadPath + Path.GetFileName(pathFile));
                client.Method = WebRequestMethods.Ftp.UploadFile;
                // This example assumes the FTP site uses anonymous logon.
                client.Credentials = this.Credentials;

                byte[] fileContents = File.ReadAllBytes(pathFile);
              /*  Encoding en = new StreamReader(pathFile).CurrentEncoding;

                using (StreamReader sourceStream = new StreamReader(pathFile, en))
                {

                    fileContents = en.GetBytes(sourceStream.ReadToEnd());//  Encoding.ASCII.GetBytes(sourceStream.ReadToEnd());  //sourceStream.CurrentEncoding.GetBytes(File.ReadAllText(pathFile));  //Encoding.Default.GetBytes(sourceStream.ReadToEnd());
                }*/

                client.ContentLength = fileContents.Length;

                using (Stream requestStream = client.GetRequestStream())
                {
                    requestStream.Write(fileContents, 0, fileContents.Length);
                }

                using (FtpWebResponse response = (FtpWebResponse)client.GetResponse())
                {
                    Console.WriteLine($"Upload File Complete, status {response.StatusDescription}");
                }

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.ToString());
                throw;
            }
        }
        public void UploadFiles(string dirPaths)
        {
            try
            {
                string[] files = Directory.GetFiles(dirPaths, "*.*");

                foreach (string file in files)
                {
                        client = (FtpWebRequest)WebRequest.Create(url+ uploadPath + Path.GetFileName(file));
                        client.Method = WebRequestMethods.Ftp.UploadFile;
                    // This example assumes the FTP site uses anonymous logon.
                        client.Credentials = Credentials;

                        byte[] fileContents = File.ReadAllBytes(file);
                    /*  using (StreamReader sourceStream = new StreamReader(file))
                      {
                          fileContents = Encoding.ASCII.GetBytes(sourceStream.ReadToEnd());
                      }*/

                    client.ContentLength = fileContents.Length;

                        using (Stream requestStream = client.GetRequestStream())
                        {
                            requestStream.Write(fileContents, 0, fileContents.Length);
                        }

                        using (FtpWebResponse response = (FtpWebResponse)client.GetResponse())
                        {
                            Console.WriteLine($"Upload File Complete, status {response.StatusDescription}");
                        }
                }

            }
            catch (Exception)
            {

                throw;
            }

        }
        public bool createDir(string _uploadPath)
        {
            WebRequest request = WebRequest.Create(url+ uploadPath + _uploadPath);
            request.Method = WebRequestMethods.Ftp.MakeDirectory;
            request.Credentials = Credentials;
            using (var resp = (FtpWebResponse)request.GetResponse())
            {
                return resp.StatusCode.ToString() == "PathnameCreated"? true:false;
            }
        }
        

        #endregion

    }
}
