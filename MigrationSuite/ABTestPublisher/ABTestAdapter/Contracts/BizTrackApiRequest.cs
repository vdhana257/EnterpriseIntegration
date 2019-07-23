// ***********************************************************************
// Assembly         : 
// Author           : arvindch
// Created          : 08-30-2017
//
// Last Modified By : 
// Last Modified On : 08-30-2017
// ***********************************************************************
// <copyright file="TransactionInfo.cs" company="Microsoft Corporation">
//     Copyright ©  2016
// </copyright>
// <summary></summary>
// ***********************************************************************
namespace ABTestAdapter.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Model to encapsulate BizTrack authentication request params
    /// </summary>
    public class BizTrackApiRequest
    {
        public List<string> partners;


        public List<string> transactions;


        public MasterFilter masterFilter;

        public string upn = "arvindch@microsoft.com";

        public string baseUri = string.Empty;

        public string relativeUriMaster = string.Empty;

        public string relativeUriDetail = string.Empty;

        public BizTrackApiRequest()
        {
            partners = new List<string>();
            transactions = new List<string>();
            masterFilter = new MasterFilter();
            masterFilter.Partners = partners;
            masterFilter.Transactions = transactions;
            masterFilter.StartDate = DateTime.Parse("2000-01-01T00:00:00.000Z");
            masterFilter.EndDate = DateTime.Parse("2000-01-02T00:00:00.000Z");
            masterFilter.BusinessValue = "";
        }
    }
}
