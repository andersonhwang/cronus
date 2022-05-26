/****************************************************
 *          Cronus - ESL Gen3 Middleware            *
 **************************************************** 
 * File:    AP.cs
 * Date:    05/26/2022
 * Author:  Huang Hai Peng
 * Summary: This class is the AP object class of Cronus.
 * (C) Suzhou ETAG Electronic Technology Co., Ltd
****************************************************/
using Cronus.Enum;

namespace Cronus.Model
{
    /// <summary>
    /// AP entity
    /// </summary>
    public class AP
    {
        /// <summary>
        /// AP ID
        /// </summary>
        public string APID { get; internal set; }
        /// <summary>
        /// Store code
        /// </summary>
        public string StoreCode { get; internal set; }
        /// <summary>
        /// AP Status
        /// </summary>
        public APStatus APStatus { get; internal set; } = APStatus.Init;
        /// <summary>
        /// Current tasks count
        /// </summary>
        public int CurrentTasks { get; internal set; } = 0;
        /// <summary>
        /// Last online time
        /// </summary>
        public DateTime? LastOnlineTime { get; internal set; } = null;
        /// <summary>
        /// Last offline time
        /// </summary>
        public DateTime? LastOfflineTime { get; internal set; } = null;
        /// <summary>
        /// Last heartbeat time
        /// </summary>
        public DateTime? LastHeartbeatTime { get; internal set; } = null;
        /// <summary>
        /// Last send time
        /// </summary>
        public DateTime? LastSendTime { get; internal set; } = null;
        /// <summary>
        /// Last receive time
        /// </summary>
        public DateTime? LastReceiveTime { get; internal set; } = null;
    }
}
