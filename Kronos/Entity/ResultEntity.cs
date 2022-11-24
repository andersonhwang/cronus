using Cronos.SDK.Enum;

namespace Cronos.SDK.Entity
{
    /// <summary>
    /// Result entity
    /// </summary>
    public sealed class ResultEntity
    {
        #region Public Properties
        /// <summary>
        /// Tag ID
        /// </summary>
        public string TagID { get; set; }
        /// <summary>
        /// Tag status
        /// </summary>
        public TagResult TagResult => RfPower == 0 ? TagResult.Unknown : (RfPower == -256 ? TagResult.Faild : TagResult.Success);
        /// <summary>
        /// RF Signal intensity(dBm)
        /// </summary>
        public int RfPower { get; set; } = -256;
        /// <summary>
        /// Temperature(°C)
        /// </summary>
        public int Temperature { get; set; } = 0;
        /// <summary>
        /// Hardware version
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// Screen type
        /// </summary>
        public string ScreenType { get; set; }
        /// <summary>
        /// Power value(V), 0.1x10
        /// </summary>
        public int Battery { get; set; } = 0;
        /// <summary>
        /// Batch code
        /// </summary>
        public int Token { get; set; }
        #endregion

        #region Extend Properties
        /// <summary>
        /// [ExtendProperties]Group code
        /// </summary>
        public int? Q_Group { get; set; }
        /// <summary>
        /// [ExtendProperties]Index value
        /// </summary>
        public int? Q_Index { get; set; }
        /// <summary>
        /// [ExtendProperties]Wakeup time
        /// </summary>
        public int? Q_WakeupTime { get; set; }
        /// <summary>
        /// [ExtendProperties]Scan time
        /// </summary>
        public int? Q_ScanTime { get; set; }
        /// <summary>
        /// [ExtendProperties]Key
        /// </summary>
        public string Q_Key { get; set; }
        #endregion
    }
}
