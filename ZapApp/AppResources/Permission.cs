using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZapApp.AppResources
{
    public class Permission
    {
        public async Task<bool> VerificarPermissaoOnlineAsync()
        {
            try
            {
                using var client = new HttpClient();
                string url = "https://raw.githubusercontent.com/TViguini/Activate_ZapPMV/main/Activate_ZapPMV.txt";
                string conteudo = await client.GetStringAsync(url);

                return conteudo.Trim().ToLower() == "true";
            }
            catch
            {
                return false; 
            }
        }

    }
}
