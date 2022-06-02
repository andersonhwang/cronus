/****************************************************
 *          Cronus - ESL Gen3 Middleware            *
 **************************************************** 
 * File:    APStatusEventArgs.cs
 * Date:    05/19/2022
 * Author:  Huang Hai Peng
 * Summary: 
 *  The ap status event args.
 * (C) Suzhou ETAG Electronic Technology Co., Ltd
****************************************************/
using Cronus.Enum;

namespace Cronus.Events
{
    /// <summary>
    /// AP status event args
    /// </summary>
    public class APStatusEventArgs : EventArgs
    {
        /// <summary>
        /// Store code
        /// </summary>
        public string StoreCode { get; private set; }
        /// <summary>
        /// AP ID
        /// </summary>
        public string APID { get; private set; }
        /// <summary>
        /// AP status
        /// </summary>
        public APStatus Status { get; private set; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="storeCode">Store code</param>
        /// <param name="ap">AP ID</param>
        /// <param name="status">AP Status</param>
        public APStatusEventArgs(string storeCode, string ap, APStatus status)
        {
            StoreCode = storeCode;
            APID = ap;
            Status = status;
        }
    }
}
