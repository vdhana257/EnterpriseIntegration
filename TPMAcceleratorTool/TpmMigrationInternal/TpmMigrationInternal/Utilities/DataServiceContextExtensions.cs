//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System.Data.Services.Client;
    using System.Threading.Tasks;

    public static class DataServiceContextExtensions
    {
        public static async Task<DataServiceResponse> SaveChangesAsync(this DataServiceContext context, SaveChangesOptions saveChangesOptions)
        {
            return await Task.Factory.FromAsync<DataServiceResponse>(
                (callback, state) => context.BeginSaveChanges(saveChangesOptions, callback, state),
                context.EndSaveChanges, 
                null);
        }
    }
}
