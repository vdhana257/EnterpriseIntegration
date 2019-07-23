using ABTestAdapter.Contracts;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABTestAdapter.Helpers
{
    public static class AuthenticationHelper
    {
        /// <summary>
        /// Access TokenApplication
        /// </summary>
        /// <returns>The AAD token</returns>
        public static async Task<string> GetAccessTokenForBizTrackAsync(BizTrackAuthRequest authRequestParams)
        {
            
            AuthenticationResult result = null;

            AuthenticationContext authContext = new AuthenticationContext(authRequestParams.AuthenticationContextUri);
            ClientCredential clientCredential = new ClientCredential(authRequestParams.ClientId, authRequestParams.ClientSecret);

            try
            {
                result = await authContext.AcquireTokenAsync(authRequestParams.ITAuthRbacResourceUri, clientCredential);
            }
            catch (Exception)
            {
                throw;
            }
            
            return result.AccessToken;
        }
    }
}
