namespace Cronos.SDK.Enum
{
    /// <summary>
    /// ESL Type
    /// </summary>
    public enum ESLType
    {
        ET0154_30 = 0x30,
        ET0154_31 = 0x31,
        ET0154_32 = 0x32,
        ET0154_33 = 0x33,
        ET0154_34 = 0x34,
        ET0154_35 = 0x35,
        ET0213_36 = 0x36,
        ET0213_37 = 0x37,
        ET0213_38 = 0x38,
        ET0213_39 = 0x39,
        ET0266_3A = 0x3A,
        ET0266_3B = 0x3B,
        ET0266_3C = 0x3C,
        ET0290_3D = 0x3D,
        ET0290_3E = 0x3E,
        ET0290_3F = 0x3F,
        ET0420_40 = 0x40,
        ET0420_41 = 0x41,
        ET0420_42 = 0x42,
        ET0420_43 = 0x43,
        ET0750_44 = 0x44,
        ET0750_45 = 0x45,
        ET0750_46 = 0x46,
        ET0750_47 = 0x47,
        ET0750_48 = 0x48,
        ET1160_49 = 0x49,
        ET1160_4A = 0x4A,
        ET1160_4B = 0x4B,
        ET0430_4C = 0x4C,
        ET0430_4D = 0x4D,
        ET0430_4E = 0x4E,
        ET0580_4F = 0x4F,
        ET0580_50 = 0x50,
        ET0580_51 = 0x51,
        ET0700_52 = 0x52,
        ET0730_53 = 0x53,
        ET0290_54 = 0x54,
        ET0345_55 = 0x55,
        ET1250_58 = 0x58,
        ET0266_5B = 0x5B
    }

    /// <summary>
    /// Ap status
    /// </summary>
    public enum ApStatus
    {
        Offline,
        Idle,
        Working,
        Error
    }

    /// <summary>
    /// Tag status
    /// </summary>
    public enum TagResult
    {
        Success,
        Faild,
        Unknown,
    }

    /// <summary>
    /// Result code
    /// </summary>
    public enum SdkResult
    {
        OK,
        Error,
        Fail,
        InvalidStoreCode,
        InvalidApID,
        InvalidTagID,
        InvalidTagCount,
        InvalidData,
        InvalidImageType,
        InvalidToken,
        InvalidFlashTimes,
        InvalidGroup,
        InvalidIndex,
        InvalidKey,
        UnregisteredAp,
        DuplicateTagID,
        StartFailed,
        ApBusy,
        ApOffline,
        AccessDenied,
        Overload,
        EmptyData,
        SDKError
    }

    /// <summary>
    /// Result type
    /// </summary>
    public enum ResultType
    {
        /// <summary>
        /// Send data
        /// </summary>
        SendData,
        /// <summary>
        /// Query
        /// </summary>
        Query,
        /// <summary>
        /// Error
        /// </summary>
        Error,
    }

    /// <summary>
    /// Ap event type
    /// </summary>
    public enum ApEventType
    {
        /// <summary>
        /// Offline
        /// </summary>
        Offline = 0,
        /// <summary>
        /// Online
        /// </summary>
        Online = 1,
        /// <summary>
        /// Online with data
        /// </summary>
        OnlineWithData = 2,
        /// <summary>
        /// Heartbeat
        /// </summary>
        Heartbeat = 3,
    }

    /// <summary>
    /// Result code
    /// </summary>
    public enum ResultCode
    {
        I00_OK,
        I02_AP_OTA_OK,
        E00_ARM_SYS_ERROR,
        E01_TAG_OTA_RESP,
        E03_DATA_CRC_INVALID,
        E04_AP_OTA_CRC_INVALID,
        E05_TAG_OTA_CRC_INVALID,
        E06_AP_DATA_TIMEOUT,
        E07_AP_RESET,
        E08_AP_RECV_TIMEOUT,
        E09_AP_NO_DATA,
        E10_DATA_HEADER_ERROR,
        E11_DATA_PARE_ERROR,
        E12_DATA_OTA_ERROR,
        E98_UNKNOWN_ERROR,
        E99_SDK_SYS_ERROR,
    }

    /// <summary>
    /// Pattern
    /// </summary>
    public enum Pattern
    {
        /// <summary>
        /// Update and display
        /// </summary>
        UpdateDisplay,
        /// <summary>
        /// Update part no display
        /// </summary>
        UpdatePart,
        /// <summary>
        /// Update no display
        /// </summary>
        Update,
        /// <summary>
        /// Display
        /// </summary>
        Display,
        /// <summary>
        /// Display current tag information
        /// </summary>
        DisplayInfor,
        /// <summary>
        /// Clean current tag screen
        /// </summary>
        Query,
        /// <summary>
        /// No change, just check tag if exist
        /// </summary>
        Check,
        /// <summary>
        /// Sort Group-Index
        /// </summary>
        Sort,
        /// <summary>
        /// Reset key
        /// </summary>
        Key,
        /// <summary>
        /// Pre-OTA
        /// </summary>
        PreOTA,
        /// <summary>
        /// Only LED
        /// </summary>
        LED,
    }

    /// <summary>
    /// Broadcast code
    /// </summary>
    internal enum BC
    {
        S = 0x03,
        P = 0x04,
        B = 0x05,
    }

    /// <summary>
    /// Page index
    /// </summary>
    public enum PageIndex
    {
        /// <summary>
        /// Page 0
        /// </summary>
        P0,
        /// <summary>
        /// Page 1
        /// </summary>
        P1,
        /// <summary>
        /// Page 2
        /// </summary>
        P2,
        /// <summary>
        /// Page 3
        /// </summary>
        P3,
        /// <summary>
        /// Page 4
        /// </summary>
        P4,
        /// <summary>
        /// Page 5
        /// </summary>
        P5,
        /// <summary>
        /// Page 6
        /// </summary>
        P6,
        /// <summary>
        /// Page 7
        /// </summary>
        P7
    }

    /// <summary>
    /// Extent iamge type
    /// </summary>
    public enum ExImageType
    {
        FloydSteinbergDithering,
        BurksDithering,
        JarvisJudiceNinkeDithering,
        StuckiDithering,
        Sierra3Dithering,
        Sierra2Dithering,
        SierraLiteDithering,
        AtkinsonDithering,
        RandomDithering
    }

    /// <summary>
    /// Ap type
    /// </summary>
    public enum ApType
    {
        Data
    }
}
