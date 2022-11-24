using Cronos.SDK.Entity;
using Cronos.SDK.Enum;
using System;
using System.Collections.Generic;

namespace Cronos.SDK.Event
{
    /// <summary>
    /// Result event arguments
    /// </summary>
    public sealed class ResultEventArgs : EventArgs
    {
        #region Constructors and Destructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Ap ID</param>
        /// <param name="shop">Store code</param>
        /// <param name="type">Result type</param>
        /// <param name="code">Error code</param>
        public ResultEventArgs(string id, string shop, ResultType type, ResultCode code = ResultCode.I00_OK)
        {
            ApID = id;
            StoreCode = shop;
            ResultType = type;
            ResultCode = code;
            ResultList = new List<ResultEntity>();
        }

        #endregion

        #region Public Properties
        /// <summary>
        /// Ap ID
        /// </summary>
        public string ApID { get; private set; }
        /// <summary>
        /// Store code
        /// </summary>
        public string StoreCode { get; private set; }

        /// <summary>
        /// Result entities list
        /// </summary>
        public IList<ResultEntity> ResultList { get; internal set; }

        /// <summary>
        /// Result type
        /// </summary>
        public ResultType ResultType { get; internal set; }

        /// <summary>
        /// Result code
        /// </summary>
        public ResultCode ResultCode { get; internal set; }
        #endregion
    }
}
