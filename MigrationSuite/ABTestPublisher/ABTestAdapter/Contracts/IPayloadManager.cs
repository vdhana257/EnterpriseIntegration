using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABTestAdapter.Contracts
{
    public interface IPayloadManager
    {
        Payload GetPayload(PayloadRequest request);
    }
}
