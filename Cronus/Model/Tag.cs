﻿using Cronos.SDK.Entity;
using Cronos.SDK.Enum;
using Cronus.Enum;

namespace Cronus.Model
{
    /// <summary>
    /// The Tag
    /// </summary>
    public class Tag
    {
        #region Properities
        /// <summary>
        /// PK, Tag ID
        /// </summary>
        public string TagID { get; set; }
        /// <summary>
        /// FK, refer to T_AP.AP_ID
        /// </summary>
        /// <remarks>FK, refer to T_AP.AP_ID</remarks>
        public string DefaultAP { get; set; }
        /// <summary>
        /// FK, refer to Task Token
        /// </summary>
        public string TaskID { get; set; }
        /// <summary>
        /// FK, refer to Shop.SHOP_CODE
        /// </summary>
        public string StoreCode { get; set; }
        /// <summary>
        /// Tag type
        /// </summary>
        public string TagType { get; set; }
        /// <summary>
        /// Tag status
        /// </summary>
        public TagStatus TagStatus { get; set; }
        /// <summary>
        /// Power value
        /// </summary>
        public int Battery { get; set; }
        /// <summary>
        /// Temperature
        /// </summary>
        public int Temperature { get; set; }
        /// <summary>
        /// RfPower 
        /// </summary>
        public int RfPower { get; set; }
        /// <summary>
        /// Last token
        /// </summary>
        public int Token { get; set; }
        /// <summary>
        /// Total send count
        /// </summary>
        public int TotalSend { get; set; }
        /// <summary>
        /// Error count (total)
        /// </summary>
        public int ErrorCount { get; set; }
        /// <summary>
        /// Error count (temporary)
        /// </summary>
        public int ErrorCountTemp { get; set; }
        /// <summary>
        /// Last send time
        /// </summary>
        public DateTime? LastSendTime { get; set; }
        /// <summary>
        /// Last back time
        /// </summary>
        public DateTime? LastRecvTime { get; set; }
        /// <summary>
        /// Template name
        /// </summary>
        /// <summary>
        /// Version
        /// </summary>
        public string Version { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="tagID">Tag ID</param>
        /// <param name="storeCode">Store code</param>
        internal Tag(string tagID, string storeCode)
        {
            TagID = tagID;
            StoreCode = storeCode;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Mark tag send
        /// </summary>
        internal void Send()
        {
            TotalSend += 1;
            TagStatus = TagStatus.Working;
            LastSendTime = DateTime.Now;
        }

        /// <summary>
        /// Update tag with result entity
        /// </summary>
        /// <param name="result">Result entity</param>
        internal void PorcessResult(string ap, ResultEntity result)
        {
            Token = result.Token;
            LastRecvTime = DateTime.Now;
            if(result.TagResult == TagResult.Success)
            {
                DefaultAP = ap;
                RfPower = result.RfPower;
                TagStatus = TagStatus.Idle;
                ErrorCountTemp = 0;
                Temperature = result.Temperature;
                Battery = result.Battery;
                if (Battery < 24) TagStatus = TagStatus.LowPower;
            }
            else
            {

                ErrorCount++;
                ErrorCountTemp++;
                TagStatus = TagStatus.Error;
                if (ErrorCountTemp > 0xFF) TagStatus = TagStatus.Lost;
            }
        }
        #endregion
    }
}
