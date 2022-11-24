using Cronos.SDK.Enum;
using System;
using System.Net;

namespace Cronos.SDK.Event
{
    /// <summary>
    /// Ap event arguments
    /// </summary>
    public sealed class ApEventArgs : EventArgs
    {
        #region Constructors and Destructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="shop">Store code</param>
        /// <param name="id">Ap ID</param>
        /// <param name="ip">Ap IP</param>
        /// <param name="port">Port</param>
        /// <param name="mac">Mac</param>
        /// <param name="hardware">Hardware</param>
        /// <param name="firmware">Firmware</param>
        /// <param name="eventType">Online/Heartbeat/Offline</param>
        /// <param name="stationType">Ap type</param>
        /// <param name="mac">MAC address</param>
        internal ApEventArgs(string shop, string id, IPAddress ip, int port, string mac, ApEventType eventType, string hardware, string firmware, ApType stationType = ApType.Data)
        {
            StoreCode = shop;
            ApID = id;
            IP = ip;
            Port = port;
            Mac = mac;
            Hardware = hardware;
            Firmware = firmware;
            EventType = eventType;
            ApType = stationType;
        }

        #endregion

        #region Public Properties
        /// <summary>
        /// Store code
        /// </summary>
        public string StoreCode { get; private set; }
        /// <summary>
        /// Ap ID
        /// </summary>
        public string ApID { get; private set; }
        /// <summary>
        /// Ap IP
        /// </summary>
        public IPAddress IP { get; private set; }
        /// <summary>
        /// Socket port
        /// </summary>
        public int Port { get; private set; }
        /// <summary>
        /// Mac
        /// </summary>
        public string Mac { get; private set; }
        /// <summary>
        /// Ap event type: online/heartbeat/offline
        /// </summary>
        public ApEventType EventType { get; private set; }
        /// <summary>
        /// Ap type, data or monitor
        /// </summary>
        public ApType ApType { get; set; } = ApType.Data;
        /// <summary>
        /// Hardware version
        /// </summary>
        public string Hardware { get; private set; }
        /// <summary>
        /// Firmware version
        /// </summary>
        public string Firmware { get; private set; }
        #endregion
    }
}
