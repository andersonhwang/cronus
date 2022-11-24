using Cronos.SDK.Entity;
using Cronos.SDK.Enum;
using Cronos.SDK.Event;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Cronos.SDK.Data
{
    internal static class RP
    {
        internal static ILogger l;
        /// <summary>
        /// Get result from data
        /// </summary>
        /// <param name="shop">Store code</param>
        /// <param name="ap">Ap ID</param>
        /// <param name="data">Data</param>
        /// <returns>Result event argument</returns>
        public static ResultEventArgs REA(string shop, string ap, string data)
        {
            var result = data[0] switch
            {
                '<' => new ResultEventArgs(ap, shop, ResultType.SendData),
                '>' => new ResultEventArgs(ap, shop, ResultType.Query),
                _ => new ResultEventArgs(ap, shop, ResultType.Error, ResultCode.E98_UNKNOWN_ERROR),
            };

            if (data.StartsWith("<00001CFFFFFFFF"))
            {
                result.ResultCode = data[18] switch
                {
                    '0' => ResultCode.E00_ARM_SYS_ERROR,
                    '1' => ResultCode.E01_TAG_OTA_RESP,
                    '2' => ResultCode.I02_AP_OTA_OK,
                    '3' => ResultCode.E03_DATA_CRC_INVALID,
                    '4' => ResultCode.E04_AP_OTA_CRC_INVALID,
                    '5' => ResultCode.E05_TAG_OTA_CRC_INVALID,
                    '6' => ResultCode.E06_AP_DATA_TIMEOUT,
                    '7' => ResultCode.E07_AP_RESET,
                    '8' => ResultCode.E08_AP_RECV_TIMEOUT,
                    '9' => ResultCode.E09_AP_NO_DATA,
                    'a' => ResultCode.E10_DATA_HEADER_ERROR,
                    'b' => ResultCode.E11_DATA_PARE_ERROR,
                    'c' => ResultCode.E12_DATA_OTA_ERROR,
                    _ => ResultCode.E98_UNKNOWN_ERROR,
                };
            }
            else
            {
                var lst = GetResultEntities(data[7..], data[0] == '<' ? 28 : 64);
                result.ResultList = lst.Item1;
                result.ResultCode = lst.code;
            }
            return result;
        }

        /// <summary>
        /// Get result entities
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="len">Data length</param>
        /// <returns>Result entities list</returns>
        private static (List<ResultEntity>, ResultCode code) GetResultEntities(string data, int len)
        {
            string temp = string.Empty;
            try
            {
                List<ResultEntity> lst = new List<ResultEntity>();
                // Result Data
                // X|Data Length|TagData(...)
                // X|XXXXXX     |...
                // 0|1-6        |5-...
                for (int i = 0, j = data.Length / len; i < j; i++)
                {
                    // Result Node Data: SendData(28)
                    // Tag ID      |RfPower        |Power     |Version     |Screen  |Token   |Temperature
                    // 0-11(12)    |12-13(2)    |14-15(2)  |16-18(3)    |19-21(3)|22-25(4)|26-27(2)
                    // Result Node Data: Query(64)
                    // Tag ID      |RfPower    |Power   |Version |Screen  |Token   |Group   |Index   |Temperature|Wakeup  |Scan    |Key               |F7EC8B00
                    // 0-11(12)    |12-13(2)|14-15(2)|16-18(3)|19-21(3)|22-25(4)|26-29(4)|30-31(2)|32-33(2)   |34-35(2)|36-37(2)|38-55(18)|56-63
                    var node = data.Substring(i * len, len);
                    var r = new ResultEntity
                    {
                        TagID = node[..12],
                        RfPower = R(node.Substring(12, 2)),
                        Token = TK(node.Substring(22, 4))
                    };

                    if (r.RfPower != -256)
                    {
                        r.Battery = P(node.Substring(14, 2));
                        r.Version = node.Substring(16, 3);
                        r.ScreenType = node.Substring(19, 3);
                        if (len == 64)
                        {
                            r.Q_Group = int.Parse(node.Substring(26, 4), NumberStyles.HexNumber);
                            r.Q_Index = int.Parse(node.Substring(30, 2), NumberStyles.HexNumber);
                            r.Temperature = T(int.Parse(node.Substring(32, 2), NumberStyles.HexNumber));
                            r.Q_WakeupTime = int.Parse(node.Substring(34, 2), NumberStyles.HexNumber);
                            r.Q_ScanTime = int.Parse(node.Substring(36, 2), NumberStyles.HexNumber);
                            r.Q_Key = node.Substring(38, 18);
                        }
                        else
                        {
                            r.Temperature = T(int.Parse(node[26..], NumberStyles.HexNumber));
                        }
                    }
                    lst.Add(r);
                }

                return (lst, ResultCode.I00_OK);
            }
            catch (Exception ex)
            {
                l.LogError(ex, "Invalid data:" + temp);
                return (new List<ResultEntity>(), ResultCode.E99_SDK_SYS_ERROR);
            }
        }

        #region Private Methods
        /// <summary>
        /// Token
        /// </summary>
        static readonly Func<string, int> TK = data => int.Parse(data, NumberStyles.HexNumber);

        /// <summary>
        /// /Temperature
        /// </summary>
        static readonly Func<int, int> T = t => t > 127 ? t - 256 : t;

        /// <summary>
        /// Get signal
        /// </summary>
        /// <param name="signal">RfPower</param>
        /// <returns>信号强度</returns>
        static readonly Func<string, int> R = t=> -(0XFF - Convert.ToInt32(t, 16) + 0x01);

        /// <summary>
        /// Get Power Value
        /// </summary>
        /// <param name="value">Power Value String</param>
        /// <returns>Power Value</returns>
        private static int P(string value)
        {
            var p = Convert.ToInt32(value, 16) ;
            if (p < 15 || p > 35) return 0;
            return p;
        }
        #endregion
    }
}
