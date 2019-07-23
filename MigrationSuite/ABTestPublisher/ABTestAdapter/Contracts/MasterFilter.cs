using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABTestAdapter.Contracts
{
    /// <summary>
    /// Filter criteria to get Master data
    /// </summary>
    public class MasterFilter
    {
        /// <summary>
        /// Gets or sets the List of Partner IDs
        /// </summary>
        /// <value>The partner ids.</value>
        public IEnumerable<string> Partners { get; set; }

        /// <summary>
        /// Gets or sets List of Transaction IDs
        /// </summary>
        /// <value>The transaction ids.</value>
        public IEnumerable<string> Transactions { get; set; }

        /// <summary>
        /// Gets or sets the Start Date for the request
        /// </summary>
        /// <value>The start date.</value>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the End Date for the request
        /// </summary>
        /// <value>The end date.</value>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Gets or sets the Page Number of the filter request
        /// </summary>
        /// <value>The page number.</value>
        public int PageNumber { get; set; }

        /// <summary>
        /// Gets or sets the Business Value for the request
        /// </summary>
        /// <value> The search value </value>
        public string BusinessValue { get; set; }

        /// <summary>
        /// Gets or sets the permission for the request.
        /// </summary>
        /// <value> The permission. </value>
        public string Permission { get; set; }
    }
}
