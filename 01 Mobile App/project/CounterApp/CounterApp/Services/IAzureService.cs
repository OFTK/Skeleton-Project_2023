using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CounterApp.Services
{
    public interface IAzureService
    {
        bool IsLoggedIn();
        Task<bool> Authenticate();
        Task<string> GetFamilyDetailsFromServer();
        Task<string> UpdateBabyDetailsToServer(JObject update_request);
    }
}
