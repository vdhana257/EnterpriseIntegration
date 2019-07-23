//-----------------------------------------------------------------------
// <copyright file="UnityManager.cs" company="Microsoft">
//     Microsoft Copyright.
// </copyright>
// <summary>Helper class for dependency injection using unity</summary>
//-----------------------------------------------------------------------

namespace Microsoft.IT.Aisap.UnityManager
{
    using Practices.Unity;

    /// <summary>
    /// Helper class for dependency injection using unity
    /// </summary>
    public static class UnityManager
    {
        /// <summary>
        /// Initializes static members of the <see cref="UnityManager"/> class
        /// </summary>
        static UnityManager()
        {
            Container = new UnityContainer();
        }

        /// <summary>
        /// Gets the unity container
        /// </summary>
        public static UnityContainer Container { get; private set; }
    }
}
