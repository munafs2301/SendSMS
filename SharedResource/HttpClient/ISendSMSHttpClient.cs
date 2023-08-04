using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedResource.HttpClient
{
    public interface ISendSMSHttpClient
    {
        Task<HttpResponseMessage> PostToExternalApi<T>(T body);
    }
}
