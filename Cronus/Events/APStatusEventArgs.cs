using Cronus.Enum;

namespace Cronus.Events
{
    /// <summary>
    /// AP status event args
    /// </summary>
    public class APStatusEventArgs : EventArgs
    {
        /// <summary>
        /// AP ID
        /// </summary>
        public string APID { get; private set; }
        /// <summary>
        /// Store code
        /// </summary>
        public string StoreCode { get; private set; }
        /// <summary>
        /// AP status
        /// </summary>
        public APStatus Status { get; private set; }
    }
}
