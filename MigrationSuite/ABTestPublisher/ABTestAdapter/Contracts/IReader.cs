using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABTestAdapter.Contracts
{
    public interface IReader
    {
        List<JObject> GetJsonBlobs();
    }
}
