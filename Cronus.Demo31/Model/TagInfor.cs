using Cronus.Enum;
using System;

namespace Cronus.Demo
{
    /// <summary>
    /// Tag infor
    /// </summary>
    internal class TagInfor : BaseInfor
    {
        string _tagID = string.Empty;
        /// <summary>
        /// Tag ID
        /// </summary>
        public string TagID { get { return _tagID; } set { _tagID = value; NotifyPropertyChanged("TagID"); } }
        string _tagType = string.Empty;
        /// <summary>
        /// Tag Type
        /// </summary>
        public string TagType { get { return _tagType; } set { _tagType = value; NotifyPropertyChanged("TagType"); } }
        /// <summary>
        /// Tag type display text
        /// </summary>
        public string TagTypeText { get { return EnumHelper.GetTagType(_tagType); } }
        Enum.TaskStatus _status = 0;
        /// <summary>
        /// Status
        /// </summary>
        public Enum.TaskStatus Status { get { return _status; } set { _status = value; NotifyPropertyChanged("StatusText"); } }
        /// <summary>
        /// Status display text
        /// </summary>
        public string StatusText => _status.ToString();
        float? _battery = 0.0F;
        /// <summary>
        /// Battery
        /// </summary>
        public float? Battery { get { return _battery; } set { _battery = value; NotifyPropertyChanged("Battery"); } }
        int? _rFPower = -256;
        /// <summary>
        /// RF Power
        /// </summary>
        public int? RFPower { get { return _rFPower; } set { _rFPower = value; NotifyPropertyChanged("RFPower"); } }
        int? _temperature = 0;
        /// <summary>
        /// Temperature
        /// </summary>
        public int? Temperature { get { return _temperature; } set { _temperature = value; NotifyPropertyChanged("Temperature"); } }
        int _token = 0;
        /// <summary>
        /// Token
        /// </summary>
        public int Token { get { return _token; } set { _token = value; NotifyPropertyChanged("Token"); } }
        DateTime? _lastSend = null;
        /// <summary>
        /// Last send time
        /// </summary>
        public DateTime? LastSend { get { return _lastSend; } set { _lastSend = value; NotifyPropertyChanged("LastSend"); } }
        DateTime? _lastReceive = null;
        /// <summary>
        /// Last receive time
        /// </summary>
        public DateTime? LastRecv { get { return _lastReceive; } set { _lastReceive = value; NotifyPropertyChanged("LastRecv"); } }
        string _lastAP = string.Empty;
        /// <summary>
        /// Last route AP
        /// </summary>
        public string LastAP { get { return _lastAP; } set { _lastAP = value; NotifyPropertyChanged("LastAP"); } }
        int _sendCount = 0;
        /// <summary>
        /// Send count
        /// </summary>
        public int SendCount { get { return _sendCount; } set { _sendCount = value; NotifyPropertyChanged("SendCount"); } }
    }
}
