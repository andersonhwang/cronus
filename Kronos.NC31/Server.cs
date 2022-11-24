using Cronos.SDK.Data;
using Cronos.SDK.Entity;
using Cronos.SDK.Enum;
using Cronos.SDK.Event;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Cronos.SDK
{
    /// <summary>
    /// Root class: the Server.
    /// </summary>
    [ClassInterface(ClassInterfaceType.None)]
    public sealed class Server
    {
        #region Private Fileds
        internal static byte[] KEY = new byte[] { 0x0D, 0xFF, 0xFF, 0xFF, 0x00, 0xF6, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
        internal static int PORT = 1234;

        /// <summary>
        /// Logger
        /// </summary>
        private static ILogger _log;
        /// <summary>
        /// Locker
        /// </summary>
        private static readonly object _locker = new object();
        /// <summary>
        /// Instance
        /// </summary>
        private static volatile Server _instance;
        /// <summary>
        /// Ap server
        /// </summary>
        private static ApServer _server;
        #endregion

        #region Public Properties
        /// <summary>
        /// Instantiate a server
        /// </summary>
        public static Server Instance
        {
            get
            {
                if (_instance != null) { return _instance; }

                lock (_locker)
                {
                    if (_instance == null)
                    {
                        _instance = new Server();
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// Get the list of online stations
        /// </summary>
        /// <param name="storeCode">Store code</param>
        /// <returns>Online station list</returns>
        public List<string> GetApOnlineList(string storeCode)
        {
            return _server.Clients.Keys
                .Where(x => x.StartsWith(storeCode))
                .Select(x => x[4..])
                .ToList();
        }

        /// <summary>
        /// Get the list of online stations
        /// </summary>
        /// <param name="storeCode">Store code</param>
        /// <returns>Online station list</returns>
        public List<string> GetApIdleList(string storeCode)
        {
            return _server.Clients.Keys
                .Where(x => x.StartsWith(storeCode) && _server.Clients[x].Status == ApStatus.Idle)
                .Select(x => x[4..])
                .ToList();
        }

        /// <summary>
        /// Get the list of working stations
        /// </summary>
        /// <param name="storeCode">Store code</param>
        /// <returns>Working station list</returns>
        public List<string> GetApWorkingList(string storeCode)
        {
            return _server.Clients.Keys
                .Where(x => x.StartsWith(storeCode) && _server.Clients[x].Status == ApStatus.Working)
                .Select(x => x[4..])
                .ToList();
        }
        #endregion

        #region Public Events
        /// <summary>
        ///     Returns tags updated results
        /// </summary>
        public event EventHandler<ResultEventArgs> ResultEventHandler
        {
            add { _server.ResultEvent += value; }
            remove { _server.ResultEvent -= value; }
        }

        /// <summary>
        ///     Returns the base station online/offline events
        /// </summary>
        public event EventHandler<ApEventArgs> ApEventHandler
        {
            add { _server.ApEvent += value; }
            remove { _server.ApEvent -= value; }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Start SDK
        /// </summary>
        /// <param name="key">Access key</param>
        /// <param name="port">The socket default value is 1234</param>
        /// <returns>Result</returns>
        public SdkResult Start(ILogger log, int port = 1234)
        {
            try
            {
                _log = log; PB.lg = log; RP.l = log;
                _server = new ApServer(_log);

                KEY[1] = KEY[2] = KEY[3] =
                KEY[6] = KEY[7] = KEY[8] =
                KEY[9] = KEY[10] = KEY[11] =
                KEY[12] = KEY[13] = 0xFF;
                PORT = port;

                if (_server.Init(PORT))
                {
                    _log.LogInformation("[SDK]Cronos.SDK start successfully.");
                    return SdkResult.OK;
                }

                _log.LogError("[SDK]Cronos.SDK start failed.");
                return SdkResult.StartFailed;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "[SDK]Cronos.SDK start error.");
                return SdkResult.SDKError;
            }
        }

        /// <summary>
        /// Send data
        /// </summary>
        /// <param name="storeCode">Store Code</param>
        /// <param name="apID">Ap ID</param>
        /// <param name="tagEntity">Tag entity</param>
        /// <returns>The result</returns>
        public SdkResult Send(string storeCode, string apID, TagEntity tagEntity) =>
            Send(storeCode, apID, new List<TagEntity>() { tagEntity });

        /// <summary>
        /// Send data with group
        /// </summary>
        /// <param name="storeCode">Store code</param>
        /// <param name="apID">Ap ID</param>
        /// <param name="groupID">Group ID</param>
        /// <param name="tagEntities">Tag entities</param>
        /// <returns></returns>
        public SdkResult Send(string storeCode, string apID, List<TagEntity> tagEntities)
        {
            try
            {
                if (!Regex.IsMatch(apID, "^[0-9A-F]{6}$")) { return SdkResult.InvalidApID; }

                if (tagEntities.Count == 0) { return SdkResult.EmptyData; }
                if (tagEntities.Count > 216) { return SdkResult.Overload; }

                for (int i = 0, j = tagEntities.Count; i < j; i++)
                {
                    TagEntity tag = tagEntities[i];
                    tag.TagID = tag.TagID.Trim().ToUpper();

                    if (!Regex.IsMatch(tag.TagID, "^[0-9A-F]{12}$")) { return SdkResult.InvalidTagID; }
                    if (tag.Token < 0 || tag.Token > 65535) { return SdkResult.InvalidToken; }
                    if (tag.Times < -1 || tag.Times > 36000) { return SdkResult.InvalidFlashTimes; }
                }

                if (tagEntities.GroupBy(x => x.TagID).Where(x => x.Count() > 1).Count() > 0) { return SdkResult.DuplicateTagID; }

                var data = PB.D(tagEntities);
                var result = SendData(storeCode, apID, data);
                if (result == SdkResult.OK)
                {
                    tagEntities.ForEach(x => { x.XCount++; x.XTime = DateTime.Now; });
                }
                return result;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "[SDK]Send error");
                return SdkResult.SDKError;
            }
        }

        /// <summary>
        /// Shift tags to fast/normal mode
        /// </summary>
        /// <param name="storeCode">Store code</param>
        /// <param name="apID">Ap ID</param>
        /// <param name="token">Token</param>
        /// <param name="fast">Ture is fast, false is slowly</param>
        /// <returns></returns>
        public SdkResult Shift(string storeCode, string apID, int token, bool fast = true)
        {
            try
            {
                var data = PB.B(BC.S, token, fast ? 0 : 1);
                return _server.Send(storeCode + apID, data);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "[SDK]Shift error");
                return SdkResult.Error;
            }
        }

        /// <summary>
        /// Switch page to display
        /// </summary>
        /// <param name="storeCode">Store code</param>
        /// <param name="apID">Ap ID</param>
        /// <param name="token">Token</param>
        /// <param name="page">Page, from 0 to 7(#1 to #7)</param>
        /// <returns></returns>
        public SdkResult SwitchPage(string storeCode, string apID, int token, int page = 0)
        {
            try
            {
                var data = PB.B(BC.P, token, page);
                return _server.Send(storeCode + apID, data);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "[SDK]Broadcast error");
                return SdkResult.Error;
            }
        }

        /// <summary>
        /// Display barcode
        /// </summary>
        /// <param name="storeCode">Store code</param>
        /// <param name="apID">Ap ID</param>
        /// <returns>The result</returns>
        public SdkResult DisplayBarcode(string storeCode, string apID, int token)
        {
            try
            {
                var data = PB.B(BC.B, token);
                return _server.Send(storeCode + apID, data);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "[SDK]Broadcast error");
                return SdkResult.Error;
            }
        }

        /// <summary>
        /// Send data, X model
        /// </summary>
        /// <param name="storeCode">Store code</param>
        /// <param name="apID">Ap ID</param>
        /// <param name="tagEntities">Tag entity with X mode</param>
        /// <returns>Result</returns>
        public SdkResult SendDataX(string storeCode, string apID, List<TagEntityX> tagEntities)
            => SendData(storeCode, apID, PB.DX(tagEntities));

        /// <summary>
        /// Send data to station
        /// </summary>
        /// <param name="storeCode">Store code</param>
        /// <param name="apID">Ap ID</param>
        /// <param name="data">Data</param>
        /// <returns>The result</returns>
        public SdkResult SendData(string storeCode, string apID, byte[] data)
        {
            try
            {
                return _server.Send(storeCode + apID, data);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "[SDK]Send error");
                return SdkResult.SDKError;
            }
        }

        /// <summary>
        /// Check specific station if is online
        /// </summary>
        /// <param name="storeCode">Store code</param>
        /// <param name="apID">Ap ID</param>
        /// <returns>Is online or not</returns>
        public bool IsOnline(string storeCode, string apID)
        {
            var id = storeCode + apID;
            if (!_server.Clients.ContainsKey(id))
                return false;

            return _server.Clients[id].Status == ApStatus.Idle || _server.Clients[id].Status == ApStatus.Working;
        }

        /// <summary>
        /// Check specific station if is working
        /// </summary>
        /// <param name="storeCode">Store code</param>
        /// <param name="apID">Ap ID</param>
        /// <returns>Is working or not</returns>
        public bool IsWorking(string storeCode, string apID)
        {
            var id = storeCode + apID;
            return _server.Clients.ContainsKey(id)
                ? _server.Clients[id].Status == ApStatus.Working
                : false;
        }

        /// <summary>
        /// Get tag data
        /// </summary>
        /// <param name="tagEntities"></param>
        /// <returns></returns>
        public byte[] GetTagData(List<TagEntity> tagEntities)
        {
            var lst = new List<byte>();
            tagEntities.ForEach(x => { lst.AddRange(x.Data); });
            return lst.ToArray();
        }

        /// <summary>
        /// Get tag data
        /// </summary>
        /// <param name="tagID"></param>
        /// <param name="token"></param>
        /// <param name="bitmap"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="times"></param>
        /// <param name="pattern"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public byte[] GetTagData(
            string tagID, int token, Bitmap bitmap,
            bool r = false, bool g = false, bool b = false, int times = 0,
            Pattern pattern = Pattern.UpdateDisplay, PageIndex page = PageIndex.P0)
        {
            var tagEntity = new TagEntity
            {
                TagID = tagID,
                Token = token,
                Image = bitmap,
                R = r,
                G = g,
                B = b,
                Times = times,
                Pattern = pattern,
                PageIndex = page
            };
            return tagEntity.Data;
        }
        #endregion
    }
}