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
    public class BizTrackAuthRequest
    {
        /// <summary>
        /// Gets or sets ITAuthResourceUri
        /// </summary>
        public string ITAuthRbacResourceUri { get; set; }

        /// <summary>
        /// Gets or Sets ClientId credentials for getting ITAUTH bearer token
        /// </summary>
        public string ClientId { get; set; }   
        /// <summary>
        /// Gets or Sets ClientSecret credentials for getting ITAUTH bearer token 
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or Sets authentication uri  
        /// </summary>
        public string AuthenticationContextUri { get; set; }
        


    }
}
