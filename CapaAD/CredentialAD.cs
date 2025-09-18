using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CapaEN;

namespace CapaAD
{
    public class CredentialAD
    {
        private bool resultado;
        private string error;

        public CredentialAD()
        {
            resultado = false;
            error = string.Empty;
        }

        public CredentialEN GetTextNotification(string mode)
        {
            resultado = false;
            CredentialEN credentialEN = new CredentialEN();

            try
            {                
                if (mode.Equals("DESA"))
                {
                    credentialEN.user = "test_ws";
                    credentialEN.password = "desa2019";
                }

                if (mode.Equals("PROD"))
                {
                    credentialEN.user = "test_ws";
                    credentialEN.password = "desa2019";
                }

                credentialEN.resultado = true;
            }
            catch (Exception ex)
            {
                credentialEN.error = "CapaAD.Credetentials.GetTextNotification(). " + ex.Message;
            }
            
            return credentialEN;
        }
    }   
}
