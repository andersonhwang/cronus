/****************************************************
 *          Cronus - ESL Gen3 Middleware            *
 **************************************************** 
 * File:    SendServer
 * Date:    06/11/2022
 * Author:  Huang Hai Peng
 * Summary: This class is the main class of Cronus.
 * For .NET Core 3.1
 * (C) Suzhou ETAG Electronic Technology Co., Ltd
****************************************************/
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.RegularExpressions;
using Cronos.SDK;
using Cronos.SDK.Entity;
using Cronos.SDK.Enum;
using Cronus.Enum;
using Cronus.Events;
using Cronus.Model;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Serilog;
using System.Drawing;
using System.IO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;
using System.Text;

namespace Cronus
{
    /// <summary>
    /// Send server
    /// </summary>
    public class SendServer
    {
        #region Properties
        readonly object _locker = new object();    // The locker
        readonly Regex RegTagID = new Regex("^[0-9A-F]{12}$");
        readonly Regex RegStoreCode = new Regex("^[0-9]{4}$");
        readonly Dictionary<string, Dictionary<string, AP>> DicAPs = new Dictionary<string, Dictionary<string, AP>>(); // AP dictionary
        readonly Dictionary<string, TagX> DicTagXs = new Dictionary<string, TagX>();                 // TagX dictionary
        readonly ConcurrentQueue<TaskResult> CoqTaskResults = new ConcurrentQueue<TaskResult>();        // Task results queue
        readonly ConcurrentQueue<APStatusEventArgs> CoqAPEvents = new ConcurrentQueue<APStatusEventArgs>();    // AP events queue
        static ILogger _logger;         // Logger
        static CronusConfig _config;    // Configure
        static SendServer _instance;    // Instance of send server

        /// <summary>
        /// The instance of SendServer
        /// </summary>
        public static SendServer Instance
        {
            get
            {
                if (_instance is null) _instance = new SendServer();
                return _instance;
            }
        }
        #endregion

        #region Events handler
        /// <summary>
        /// The task event handler
        /// </summary>
        internal EventHandler<TaskResultEventArgs>? TaskHandler { get; private set; }

        /// <summary>
        /// The AP event handler
        /// </summary>
        internal EventHandler<APStatusEventArgs>? APHandler { get; private set; }

        /// <summary>
        /// Task event handler
        /// </summary>
        public event EventHandler<TaskResultEventArgs> TaskEventHandler
        {
            add { TaskHandler += value; }
            remove { TaskHandler -= value; }
        }

        /// <summary>
        /// AP event handler
        /// </summary>
        public event EventHandler<APStatusEventArgs> APEventHandler
        {
            add { APHandler += value; }
            remove { APHandler -= value; }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Start send server
        /// </summary>
        /// <param name="config">Cronus configure</param>
        /// <param name="log">ILogger</param>
        /// <returns>Result</returns>
        public Result Start(CronusConfig config, ILogger log = null)
        {
            try
            {
                _config = config;
                _logger = log;
                var filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var version = Assembly.LoadFile(filePath + "/Cronus.dll").GetName().Version;
                if (_logger is null)
                {
                    _logger = LoggerFactory
                        .Create(builder => builder.AddSerilog())
                        .CreateLogger("Cronus");
                }
                _logger.LogInformation("[Cronus]Start send server...");
                var result = StartSDK(_config.APPort, log);
                _logger.LogInformation($"[Cronus]Start SDK {result}.");
                if (!result) return Result.Error;
                Task.Run(async () => { await TaskDispatcher(); });
                Task.Run(async () => { await TaskFeedback(); });
                Task.Run(async () => { await APFeedback(); });

                _logger.LogInformation($"[Cronus]Start OK.(Build:{version})");

                return Result.OK;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Cronus]Start_Error.");
                return Result.Error;
            }
        }

        /// <summary>
        /// Attach basic data to Cronus
        /// </summary>
        /// <param name="tags">Tags list</param>
        /// <returns>Attach result</returns>
        public Result BasicData(List<Tag> tags)
        {
            try
            {
                lock (_locker)
                {
                    if (tags is null) return Result.NullData;
                    tags.ForEach(x =>
                    {
                        if (!DicTagXs.ContainsKey(x.TagID))
                        {
                            DicTagXs.Add(x.TagID, new TagX(x));
                        }
                    });
                    return Result.OK;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Basic_Data_Error");
                return Result.Error;
            }
        }

        #region Task Import
        /// <summary>
        /// Push image to tag, single store mode
        /// </summary>
        /// <param name="tagID">Tag ID</param>
        /// <param name="image">Bitmap image</param>
        /// <returns>Push result</returns>
        public Result Push(string tagID, Bitmap image)
            => Push(_config.DefaultStoreCode, tagID, image);

        /// <summary>
        /// Push image to tag
        /// </summary>
        /// <param name="storeCode">Store code</param>
        /// <param name="tagID">Tag ID</param>
        /// <param name="image">Bitmap image</param>
        /// <returns>Push result</returns>
        public Result Push(string storeCode, string tagID, Bitmap image)
        {
            try
            {
                lock (_locker)
                {
                    if (!RegStoreCode.IsMatch(storeCode)) return Result.InvalidStoreCode;
                    if (!RegTagID.IsMatch(tagID)) return Result.InvalidTagID;
                    if (image == null) return Result.InvalidImage;

                    var aps = GetAPs(storeCode);
                    var task = new TaskData(tagID, image);
                    var tagx = GetTagX(tagID, storeCode);
                    var result = tagx.WriteA(task, aps);
                    if (result != null) CoqTaskResults.Enqueue(result);

                    return Result.OK;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Push_Error");
                return Result.Error;
            }
        }

        /// <summary>
        /// Push task list, single store mode
        /// </summary>
        /// <param name="tasks">Task list</param>
        /// <returns>The result</returns>
        public Result Push(List<TaskData> tasks)
            => Push(_config.DefaultStoreCode, tasks);

        /// <summary>
        /// Push task list
        /// </summary>
        /// <param name="storeCode">Store code</param>
        /// <param name="tasks">Tasks list</param>
        /// <returns>The result</returns>
        public Result Push(string storeCode, List<TaskData> tasks)
        {
            try
            {
                lock (_locker)
                {
                    if (!RegStoreCode.IsMatch(storeCode)) return Result.InvalidStoreCode;
                    var aps = GetAPs(storeCode);

                    bool allDone = true, anyDone = false;
                    foreach (TaskData task in tasks)
                    {
                        if (!RegTagID.IsMatch(task.TagID))
                        {
                            allDone = false;
                            continue;
                        }
                        //if (task.Bitmap is null)
                        //{
                        //    allDone = false;
                        //    continue;
                        //}
                        var tagx = GetTagX(task.TagID, storeCode);
                        var result = tagx.WriteA(task, aps);
                        if (result != null) CoqTaskResults.Enqueue(result);
                        anyDone = true;
                    }

                    if (allDone) return Result.OK;
                    if (!anyDone) return Result.NoTaskCreate;
                    return Result.NotAllTaskCreate;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Push_Error");
                return Result.Error;
            }
        }

        /// <summary>
        /// Flashing LED lights, single store mode
        /// </summary>
        /// <param name="r">Red color</param>
        /// <param name="g">Green color</param>
        /// <param name="b">Blue color</param>
        /// <param name="times">Flashing times</param>
        /// <param name="idList">ID list</param>
        /// <returns>Result</returns>
        public Result LED(bool r, bool g, bool b, int times, List<string> idList)
            => LED(_config.DefaultStoreCode, r, g, b, times, idList);

        /// <summary>
        /// Flashing LED lights
        /// </summary>
        /// <param name="storeCode">Store code</param>
        /// <param name="r">Red color</param>
        /// <param name="g">Green color</param>
        /// <param name="b">Blue color</param>
        /// <param name="times">Flashing times</param>
        /// <param name="idList">ID list</param>
        /// <returns>Result</returns>
        public Result LED(string storeCode, bool r, bool g, bool b, int times, List<string> idList)
        {
            if (idList is null || idList.Count == 0) return Result.NullData;

            var tasks = new List<TaskData>();
            idList.ForEach(x => { tasks.Add(new TaskData(x, r, g, b, times)); });
            return Push(storeCode, tasks);
        }

        /// <summary>
        /// Swich page to display
        /// </summary>
        /// <param name="id">Tag ID</param>
        /// <param name="page">Page index</param>
        /// <returns>The result</returns>
        public Result SwitchPage(string id, int page)
            => SwitchPage(new List<string> { id }, page);

        /// <summary>
        /// Swtich page to display
        /// </summary>
        /// <param name="idList">Tag ID list</param>
        /// <param name="page">Page index</param>
        /// <returns>The result</returns>
        public Result SwitchPage(List<string> idList, int page)
            => SwitchPage(_config.DefaultStoreCode, idList, page);

        /// <summary>
        /// Swith page to display with store code
        /// </summary>
        /// <param name="storeCode">Store code</param>
        /// <param name="id">Tag ID</param>
        /// <param name="page">Page index</param>
        /// <returns>The result</returns>
        public Result SwitchPage(string storeCode, string id, int page)
            => SwitchPage(storeCode, new List<string> { id }, page);

        /// <summary>
        /// Swith page to display with store code
        /// </summary>
        /// <param name="storeCode">Store code</param>
        /// <param name="idList">Tag ID list</param>
        /// <param name="page">Page index</param>
        /// <returns>The result</returns>
        public Result SwitchPage(string storeCode, List<string> idList, int page)
        {
            if (idList is null || idList.Count == 0) return Result.NullData;

            var tasks = new List<TaskData>();
            idList.ForEach(x => { tasks.Add(new TaskData(x, page)); });
            return Push(storeCode, tasks);
        }
        #endregion

        #region Broadcast
        /// <summary>
        /// Broadcast: Switch page
        /// </summary>
        /// <param name="storeCode">Store code</param>
        /// <param name="page">Page to dispaly, from 0 to 3/7</param>
        /// <remarks>Some types have 4 pages cache, some types have 8 pages cahce. Please read the product introduce document.</remarks>
        /// <returns>Result</returns>
        public Result SwitchPageAll(string storeCode, int page)
        {
            try
            {
                var check = CheckBroadcast(storeCode);
                if (check != Result.OK) return check;

                foreach (var ap in Server.Instance.GetApOnlineList(storeCode))
                {
                    var result = Server.Instance.SwitchPage(storeCode, ap, 0, page);
                    if (result == SdkResult.OK)
                    {
                        CoqAPEvents.Enqueue(new APStatusEventArgs(storeCode, ap, APStatus.Working));
                    }
                    _logger.LogInformation($"[Cronus]SwitchPage to page {page}, {storeCode}-{ap}:{result}.");
                }

                return Result.OK;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Cronus]Swicth_Page_Error");
                return Result.Error;
            }
        }

        /// <summary>
        /// Broadcast: Display the ID barcode of the tag on its screen.
        /// </summary>
        /// <param name="storeCode">Store code</param>
        /// <returns>Result</returns>
        public Result DisplayBarcodeAll(string storeCode)
        {
            try
            {
                var check = CheckBroadcast(storeCode);
                if (check != Result.OK) return check;

                foreach (var ap in Server.Instance.GetApOnlineList(storeCode))
                {
                    var result = Server.Instance.DisplayBarcode(storeCode, ap, 0); // Default token is 0.
                    if (result == SdkResult.OK)
                    {
                        CoqAPEvents.Enqueue(new APStatusEventArgs(storeCode, ap, APStatus.Working));
                    }
                    _logger.LogInformation($"[Cronus]Display barcode, {storeCode}-{ap}:{result}.");
                }

                return Result.OK;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Cronus]Swicth_Page_Error");
                return Result.Error;
            }
        }
        #endregion

        #region Query Status
        /// <summary>
        /// Get tags
        /// </summary>
        /// <param name="storeCode">Store code</param>
        /// <returns>Tag list</returns>
        public List<Tag> GetTags(string storeCode)
        {
            return string.IsNullOrEmpty(storeCode)
                ? DicTagXs.Values.Select(x => x.Tag).ToList()
                : DicTagXs.Values.Where(x => x.Tag.StoreCode == storeCode).Select(x => x.Tag).ToList();
        }

        /// <summary>
        /// Get AP list
        /// </summary>
        /// <param name="storeCode">Store code, empty means global</param>
        /// <returns>AP list</returns>
        public List<AP> GetAPList(string storeCode)
        {
            return DicAPs.ContainsKey(storeCode)
                ? DicAPs[storeCode].Values.ToList()
                : new List<AP>();
        }
        #endregion
        #endregion

        #region Private methods
        /// <summary>
        /// Thread: Task dispatcher
        /// </summary>
        /// <returns>The task</returns>
        async Task TaskDispatcher()
        {
            var pre = new Exception();
            int count = 0, noAP = 0, noWork = 0;
            while (true)
            {
                try
                {
                    await Task.Delay(2000);
                    // No AP connect or no tag load
                    if (DicAPs.Count == 0 || DicTagXs.Count == 0) continue;
                    // Prepare task collection to send
                    // Store code->AP ID->Tasks
                    var tasks = new Dictionary<string, Dictionary<string, List<TagEntityX>>>();
                    // No AP online
                    foreach (var store in DicAPs.Keys)
                    {
                        var aps = Server.Instance.GetApIdleList(store);
                        if (aps.Count == 0)
                        {
                            noAP++; if (noAP >= 0xFFFF)
                            {
                                noAP = 0;
                                Log.Error($"[Cronus]Longtime_No_Ap:{store}");
                            }
                            continue;
                        }
                        tasks.Add(store, new Dictionary<string, List<TagEntityX>>());
                        foreach (var ap in aps) tasks[store].Add(ap, new List<TagEntityX>());
                    }
                    noAP = 0;

                    lock (_locker)
                    {
                        // Last first
                        var tags = DicTagXs.Values
                            .Where(x => DicAPs.ContainsKey(x.StoreCode) && x.NeedWork(DicAPs[x.StoreCode].Keys.ToList()))
                            .OrderBy(x => x.LastSend);
                        if (tags.Count() == 0)
                        {
                            noWork++; if (noWork >= 0xFFFF)
                            {
                                noWork = 0;
                                Log.Error("[Cronus]Longtime_No_Work");
                            }
                            continue;
                        }
                        noWork = 0;

                        foreach (var tag in tags)
                        {
                            foreach (var ap in DicAPs[tag.StoreCode].Keys)
                            {
                                if (tag.SameWay(ap) && tasks[tag.StoreCode].Count < 0xD0)
                                {
                                    var now = tasks[tag.StoreCode][ap].Sum(x => x.Data.Length);
                                    if ((now + tag.TagData.Data.Length) > 0x80000) continue;
                                    tasks[tag.StoreCode][ap].Add(tag.TagData);
                                    tag.Transfer(ap);
                                }
                            }
                        }

                        foreach (var store in tasks.Keys)
                        {
                            foreach (var ap in tasks[store].Keys)
                            {
                                if (tasks[store][ap].Count == 0) continue;
                                var builder = new StringBuilder();
                                var result = Server.Instance.SendDataX(store, ap, tasks[store][ap]);
                                tasks[store][ap].ForEach(x => {
                                    foreach (var b in x.Data) builder.Append(b.ToString("X2"));
                                    builder.AppendLine();
                                });
                                Log.Information($"[Cronus]{store}-{ap} send {tasks[store][ap].Count}: {result}");
                                Log.Debug(builder.ToString());
                                if (result == SdkResult.OK)
                                {
                                    UpdateAP(store, ap, tasks[store][ap].Count);
                                    foreach (var task in tasks[store][ap])
                                    {
                                        CoqTaskResults.Enqueue(DicTagXs[task.TagID].B);
                                    }
                                }
                            }
                        }
                    }

                    var length = DicTagXs.Values.Count(x => x.NeedWork());
                    var nowSend = DicTagXs.Values.Count(x => x.IsWorking());

                    tasks.Clear();
                }
                catch (Exception ex)
                {
                    if (pre.Message != ex.Message)
                    {
                        Log.Error(ex, "LOOP_ERROR.");
                        count = 0;
                    }
                    else if (count > 0xFF)
                    {
                        Log.Error(ex, "LOOP_ERROR_LONG.");
                        count = 0;
                        continue;
                    }
                    count++;
                }
            }
        }

        /// <summary>
        /// Thread: Task feedback
        /// </summary>
        /// <returns>The task</returns>
        async Task TaskFeedback()
        {
            var pre = new Exception();
            int count = 0;
            while (true)
            {
                try
                {
                    if (CoqTaskResults.IsEmpty)
                    {
                        await Task.Delay(2000);
                        continue;
                    }

                    var e = new TaskResultEventArgs();
                    while (CoqTaskResults.TryDequeue(out var result)) e.TaskResults.Add(result);
                    TaskHandler?.Invoke(null, e);
                }
                catch (Exception ex)
                {
                    if (pre.Message != ex.Message)
                    {
                        Log.Error(ex, "LOOP_ERROR.");
                        count = 0;
                    }
                    else if (count > 999)
                    {
                        Log.Error(ex, "LOOP_ERROR_LONG.");
                        count = 0;
                        continue;
                    }
                    count++;
                }
            }
        }

        /// <summary>
        /// Thread: AP status feedback
        /// </summary>
        /// <returns>The task</returns>
        async Task APFeedback()
        {
            var pre = new Exception();
            int count = 0;
            while (true)
            {
                try
                {
                    if (CoqAPEvents.IsEmpty)
                    {
                        await Task.Delay(2000);
                        continue;
                    }

                    if (CoqAPEvents.TryDequeue(out var ap))
                    {
                        APHandler?.Invoke(null, ap);
                    }
                }
                catch (Exception ex)
                {
                    if (pre.Message != ex.Message)
                    {
                        Log.Error(ex, "LOOP_ERROR.");
                        count = 0;
                    }
                    else if (count > 999)
                    {
                        Log.Error(ex, "LOOP_ERROR_LONG.");
                        count = 0;
                        continue;
                    }
                    count++;
                }
            }
        }

        /// <summary>
        /// Get tagx by tag ID
        /// </summary>
        /// <param name="tagID">Tag ID</param>
        /// <param name="storeCode">Store code</param>
        /// <returns>Tagx</returns>
        TagX GetTagX(string tagID, string storeCode)
        {
            lock (_locker)
            {
                if (DicTagXs.ContainsKey(tagID)) return DicTagXs[tagID];
                DicTagXs.Add(tagID, new TagX(new Tag(tagID, storeCode)));
                if (DicTagXs[tagID].Tag.StoreCode != storeCode)
                    DicTagXs[tagID].Tag.StoreCode = storeCode; // Update the store code
                return DicTagXs[tagID];
            }
        }

        /// <summary>
        /// Broadcast realtime check: online, and idle
        /// </summary>
        /// <param name="storeCode">Store code</param>
        /// <returns>Check result</returns>
        Result CheckBroadcast(string storeCode)
        {
            if (Server.Instance.GetApOnlineList(storeCode).Count == 0) return Result.NoApOnline;
            if (Server.Instance.GetApWorkingList(storeCode).Count > 0) return Result.APBusying;
            return Result.OK;
        }
        #endregion

        #region AP
        /// <summary>
        /// Get AP ID list
        /// </summary>
        /// <param name="storeCode">Store code</param>
        /// <returns>AP ID list</returns>
        List<string> GetAPs(string storeCode) => Server.Instance.GetApOnlineList(storeCode);

        /// <summary>
        /// Get AP
        /// </summary>
        /// <param name="storeCode">Store code</param>
        /// <param name="apID">AP ID</param>
        /// <returns>The AP</returns>
        AP GetAP(string storeCode, string apID)
        {
            if (!DicAPs.ContainsKey(storeCode))
            {
                DicAPs.Add(storeCode, new Dictionary<string, AP>());
            }

            if (DicAPs[storeCode].ContainsKey(apID))
            {
                return DicAPs[storeCode][apID];
            }
            else
            {
                var ap = new AP { StoreCode = storeCode, APID = apID };
                DicAPs[storeCode].Add(apID, ap);
                return ap;
            }
        }

        /// <summary>
        /// Update AP working status
        /// </summary>
        /// <param name="storeCode">Store code</param>
        /// <param name="apID">AP ID</param>
        /// <param name="count">Current task count</param>
        void UpdateAP(string storeCode, string apID, int count)
        {
            var ap = GetAP(storeCode, apID);
            ap.APStatus = APStatus.Working;
            ap.CurrentTasks = count;
            ap.LastSendTime = DateTime.Now;
        }
        #endregion

        #region SDK
        /// <summary>
        /// Init send service
        /// </summary>
        /// <param name="port">AP Port</param>
        /// <param name="logger">Logger</param>
        /// <returns>Start SDK result</returns>
        bool StartSDK(int port, ILogger logger)
        {
            try
            {
                var result = Server.Instance.Start(logger, port);
                Server.Instance.ApEventHandler += Instance_ApEventHandler; ;
                Server.Instance.ResultEventHandler += Instance_ResultEventHandler;
                _logger.LogInformation("[Cronus]Send SDK:" + result);
                return result == SdkResult.OK;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Cronus]Start_SDK_Error");
                return false;
            }
        }

        /// <summary>
        /// Result event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Instance_ApEventHandler(object sender, Cronos.SDK.Event.ApEventArgs e)
        {
            try
            {
                lock (_locker)
                {
                    var ap = GetAP(e.StoreCode, e.ApID);
                    switch (e.EventType)
                    {
                        case ApEventType.Online:
                            ap.APStatus = APStatus.Online;
                            ap.LastOnlineTime = DateTime.Now;
                            break;
                        case ApEventType.OnlineWithData:
                            ap.APStatus = APStatus.Working;
                            ap.LastOnlineTime = DateTime.Now;
                            break;
                        case ApEventType.Offline:
                            ap.APStatus = APStatus.Offline;
                            ap.LastOfflineTime = DateTime.Now;
                            break;
                        case ApEventType.Heartbeat:
                            ap.APStatus = APStatus.Online;
                            ap.LastHeartbeatTime = DateTime.Now;
                            break;
                        default: break;
                    }
                    DicAPs[e.StoreCode][e.ApID] = ap;
                    CoqAPEvents.Enqueue(new APStatusEventArgs(ap.StoreCode, ap.APID, ap.APStatus));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Cronus]Result_Process_Error");
            }
        }

        /// <summary>
        /// Station event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Instance_ResultEventHandler(object sender, Cronos.SDK.Event.ResultEventArgs e)
        {
            try
            {
                lock (_locker)
                {
                    CoqAPEvents.Enqueue(new APStatusEventArgs(e.StoreCode, e.ApID, APStatus.Online));
                    foreach (var result in from result in e.ResultList
                                           where DicTagXs.ContainsKey(result.TagID)
                                           where DicTagXs[result.TagID].WriteB(e.ApID, result)
                                           select result)
                    {
                        CoqTaskResults.Enqueue(DicTagXs[result.TagID].B);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Cronus]Station_Process_Error");
            }
        }
        #endregion
    }
}