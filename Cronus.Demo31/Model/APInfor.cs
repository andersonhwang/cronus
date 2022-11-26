using Cronus.Enum;
using System;

namespace Cronus.Demo
{
    /// <summary>
    /// AP Infor
    /// </summary>
    internal class APInfor : BaseInfor, IComparable<APInfor>
    {
        string _storeCode = string.Empty;
        /// <summary>
        /// Store Code
        /// </summary>
        public string StoreCode { get { return _storeCode; } set { _storeCode = value; NotifyPropertyChanged("StoreCode"); } }
        string _aPID = string.Empty;
        /// <summary>
        /// AP ID
        /// </summary>
        public string APID { get { return _aPID; } set { _aPID = value; NotifyPropertyChanged("APID"); } }
        APStatus _aPStatus = APStatus.Init;
        /// <summary>
        /// AP Status
        /// </summary>
        public APStatus APStatus { get { return _aPStatus; } set { _aPStatus = value; NotifyPropertyChanged("APStatus"); } }
        /// <summary>
        /// The ID
        /// </summary>
        public string ID { get { return StoreCode + "-" + APID; } }

        /// <summary>
        /// Compare AP infor object
        /// </summary>
        /// <param name="obj">AP Infor object</param>
        /// <returns>Compare result</returns>
        public int CompareTo(APInfor? obj)
        {
            if (obj == null) return 1;
            return ID.CompareTo(obj.ID);
        }
    }
}
