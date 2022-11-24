using Cronos.SDK.Enum;

namespace Cronos.SDK.Helper
{
    /// <summary>
    /// Enumerate helper
    /// </summary>
    public static class EnumHelper
    {
        /// <summary>
        /// Get ESL type by string value
        /// </summary>
        /// <param name="prefix">ESL type string value</param>
        /// <returns>Enum ESL type</returns>
        public static ESLType GetESLType(string prefix) => prefix.Trim().ToUpper() switch
        {
            "30" => ESLType.ET0154_30,
            "31" => ESLType.ET0154_31,
            "32" => ESLType.ET0154_32,
            "33" => ESLType.ET0154_33,
            "34" => ESLType.ET0154_34,
            "35" => ESLType.ET0154_35,
            "36" => ESLType.ET0213_36,
            "37" => ESLType.ET0213_37,
            "38" => ESLType.ET0213_38,
            "39" => ESLType.ET0213_39,
            "3A" => ESLType.ET0266_3A,
            "3B" => ESLType.ET0266_3B,
            "3C" => ESLType.ET0266_3C,
            "3D" => ESLType.ET0290_3D,
            "3E" => ESLType.ET0290_3E,
            "3F" => ESLType.ET0290_3F,
            "40" => ESLType.ET0420_40,
            "41" => ESLType.ET0420_41,
            "42" => ESLType.ET0420_42,
            "43" => ESLType.ET0420_43,
            "44" => ESLType.ET0750_44,
            "45" => ESLType.ET0750_45,
            "46" => ESLType.ET0750_46,
            "47" => ESLType.ET0750_47,
            "48" => ESLType.ET0750_48,
            "49" => ESLType.ET1160_49,
            "4A" => ESLType.ET1160_4A,
            "4B" => ESLType.ET1160_4B,
            "4C" => ESLType.ET0430_4C,
            "4D" => ESLType.ET0430_4D,
            "4E" => ESLType.ET0430_4E,
            "4F" => ESLType.ET0580_4F,
            "50" => ESLType.ET0580_50,
            "51" => ESLType.ET0580_51,
            "52" => ESLType.ET0700_52,
            "53" => ESLType.ET0730_53,
            "54" => ESLType.ET0290_54,
            "55" => ESLType.ET0345_55,
            "58" => ESLType.ET1250_58,
            "5B" => ESLType.ET0266_5B,
            _ => ESLType.ET0154_30,
        };

        /// <summary>
        /// Get ESL size by prefix
        /// </summary>
        /// <param name="type">ESL type</param>
        /// <returns>ESL size</returns>
        public static (int, int) GetESLSize(ESLType type)
        {
            return type switch
            {
                ESLType.ET0154_30 => new(152, 152),
                ESLType.ET0154_31 => new(152, 152),
                ESLType.ET0154_32 => new(152, 152),
                ESLType.ET0154_33 => new(200, 200),
                ESLType.ET0154_34 => new(200, 200),
                ESLType.ET0154_35 => new(200, 200),
                ESLType.ET0213_36 => new(250, 122),
                ESLType.ET0213_37 => new(250, 122),
                ESLType.ET0213_38 => new(250, 122),
                ESLType.ET0213_39 => new(250, 122),
                ESLType.ET0266_3A => new(296, 152),
                ESLType.ET0266_3B => new(296, 152),
                ESLType.ET0266_3C => new(296, 152),
                ESLType.ET0290_3D => new(296, 128),
                ESLType.ET0290_3E => new(296, 128),
                ESLType.ET0290_3F => new(296, 128),
                ESLType.ET0420_40 => new(400, 300),
                ESLType.ET0420_41 => new(400, 300),
                ESLType.ET0420_42 => new(400, 300),
                ESLType.ET0420_43 => new(400, 300),
                ESLType.ET0750_44 => new(800, 480),
                ESLType.ET0750_45 => new(800, 480),
                ESLType.ET0750_46 => new(800, 480),
                ESLType.ET0750_47 => new(800, 480),
                ESLType.ET0750_48 => new(800, 480),
                ESLType.ET1160_49 => new(960, 640),
                ESLType.ET1160_4A => new(960, 640),
                ESLType.ET1160_4B => new(960, 640),
                ESLType.ET0430_4C => new(522, 152),
                ESLType.ET0430_4D => new(522, 152),
                ESLType.ET0430_4E => new(522, 152),
                ESLType.ET0580_4F => new(648, 480),
                ESLType.ET0580_50 => new(648, 480),
                ESLType.ET0580_51 => new(648, 480),
                ESLType.ET0290_54 => new(296, 128),
                ESLType.ET0345_55 => new(384, 184),
                ESLType.ET1250_58 => new(1304, 984),
                ESLType.ET0266_5B => new(296, 152),
                _ => new(152, 152),
            };
        }

        /// <summary>
        /// Get ESL size by prefix
        /// </summary>
        /// <param name="prefix">ESL prefix</param>
        /// <returns>ESL size</returns>
        public static (int, int) GetESLSize(string prefix) => GetESLSize(GetESLType(prefix));
    }
}
