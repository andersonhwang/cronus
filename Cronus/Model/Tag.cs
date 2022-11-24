/****************************************************
 *          Cronus - ESL Gen3 Middleware            *
 **************************************************** 
 * File:    Tag.cs
 * Date:    05/19/2022
 * Author:  Huang Hai Peng
 * Summary: This class is the tag object class of Cronus.
 * (C) Suzhou ETAG Electronic Technology Co., Ltd
****************************************************/
using Cronos.SDK.Entity;
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
        public Guid TaskID { get; set; } = Guid.Empty;
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
        public float Battery { get; set; }
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
        public int TotalSend { get; set; } = 0;
        /// <summary>
        /// Error count (total)
        /// </summary>
        public int ErrorCount { get; set; } = 0;
        /// <summary>
        /// Error count (temporary)
        /// </summary>
        public int ErrorCountTemp { get; set; } = 0;
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
        /// <param name="random">Random token, default is null</param>
        public Tag(string tagID, string storeCode, Random random = null)
        {
            TagID = tagID;
            StoreCode = storeCode;
            if (random != null) Token = random.Next(0xFFFF);
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
            if(result.TagResult == Cronos.SDK.Enum.TagResult.Success)
            {
                DefaultAP = ap;
                RfPower = result.RfPower;
                TagStatus = TagStatus.Idle;
                ErrorCountTemp = 0;
                Temperature = result.Temperature;
                Battery = result.Battery / 10F;
                if (Battery < 2.4F) TagStatus = TagStatus.LowPower;
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
