/****************************************************
 *          Cronus - ESL Gen3 Middleware            *
 **************************************************** 
 * File:    SendServer
 * Date:    05/19/2022
 * Author:  Huang Hai Peng
 * Summary: This class is the main class of Cronus.
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
using Serilog;
using SkiaSharp;

namespace Cronus
{
    /// <summary>
    /// Send server
    /// </summary>
    public class SendServer
    {
        #region Properties
        readonly object _locker = new();    // The locker
        readonly Regex RegTagID = new Regex("^[0-9A-F]{12}$");
        readonly Regex RegStoreCode = new Regex("^[0-9]{4}$");
        readonly Dictionary<string, AP> DicAPs = new();                 // AP dictionary
        readonly Dictionary<string, TagX> DicTagXs = new();             // TagX dictionary
        readonly ConcurrentQueue<TaskResult> CoqTaskResults = new();    // Task results queue
        static Microsoft.Extensions.Logging.ILogger _logger;            // Logger
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
        internal EventHandler<TaskResultEventArgs> TaskHandler { get; private set; }

        /// <summary>
        /// The AP event handler
        /// </summary>
        internal EventHandler<APStatusEventArgs> APHandler { get; private set; }

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
        public Result Start(CronusConfig config, Microsoft.Extensions.Logging.ILogger log = null)
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
            return Result.OK;
        }

        #region Task Import
        /// <summary>
        /// Push image to tag, single store mode
        /// </summary>
        /// <param name="tagID">Tag ID</param>
        /// <param name="image">SKImage</param>
        /// <returns>Push result</returns>
        public Result Push(string tagID, SKImage image)
            => Push(_config.DefaultStoreCode, tagID, image);

        /// <summary>
        /// Push image to tag
        /// </summary>
        /// <param name="storeCode">Store code</param>
        /// <param name="tagID">Tag ID</param>
        /// <param name="image">SKImage</param>
        /// <returns>Push result</returns>
        public Result Push(string storeCode, string tagID, SKImage image)
        {
            try
            {
                lock (_locker)
                {
                    if (!RegStoreCode.IsMatch(storeCode)) return Result.InvalidStoreCode;
                    if (!RegTagID.IsMatch(tagID)) return Result.InvalidTagID;
                    if (image == null) return Result.InvalidImage;

                    var aps = GetAPs(storeCode);
                    var task = new TaskData(tagID, SKBitmap.FromImage(image));
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
                        if (task.Bitmap is null)
                        {
                            allDone = false;
                            continue;
                        }
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
        /// Flashing LED lights
        /// </summary>
        /// <param name="r">Red color</param>
        /// <param name="g">Green color</param>
        /// <param name="b">Blue color</param>
        /// <param name="times">Flashing times</param>
        /// <param name="idList">ID list</param>
        /// <returns>Result</returns>
        public Result LED(bool r, bool g, bool b, int times, List<string> idList = null)
            => LED(_config.DefaultStoreCode, r, g, b, times, idList);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storeCode"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="times"></param>
        /// <param name="idList">ID list, null means global</param>
        /// <returns></returns>
        public Result LED(string storeCode, bool r, bool g, bool b, int times, List<string> idList = null)
        {
            var tasks = new List<TaskData>();
            idList.ForEach(x => { tasks.Add(new TaskData(x, r, g, b, times)); });
            return Push(storeCode, tasks);
        }

        /// <summary>
        /// Broadcast: Switch page
        /// </summary>
        /// <param name="storeCode"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public Result SwitchPage(string storeCode, int page)
        {
            try
            {
                if (Server.Instance.GetApOnlineList(storeCode).Count == 0) return Result.NoApOnline;

                foreach (var ap in Server.Instance.GetApOnlineList(storeCode))
                {
                    var result = Server.Instance.SwitchPage(ap, 0, page);
                    _logger.LogInformation($"[Cronus]SwitchPage to page {page}, {ap}:{result}.");
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
        public List<AP> GetAPList(string storeCode = "")
        {
            return string.IsNullOrEmpty(storeCode)
                ? DicAPs.Values.ToList()
                : DicAPs.Where(x => x.Key[..4] == storeCode).Select(x => x.Value).ToList();
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
            Exception pre = new();
            int count = 0, noAP = 0, noTag = 0, noWork = 0;
            while (true)
            {
                try
                {
                    await Task.Delay(2000);
                    // No tag load
                    if (DicTagXs.Count == 0) continue;
                    // No AP online
                    var aps = Server.Instance.GetApIdleList();
                    if (aps.Count == 0)
                    {
                        noAP++; if (noAP >= 1000)
                        {
                            noAP = 0;
                            Log.Error("[Cronus]Longtime_No_Ap.");
                        }
                        continue;
                    }

                    var tasks = new Dictionary<string, List<TagEntityX>>();
                    aps.ForEach(ap => tasks.Add(ap, new List<TagEntityX>()));
                    lock (_locker)
                    {
                        // Last first
                        var tags = DicTagXs.Values
                            .Where(x => x.NeedWork(aps))
                            .OrderBy(x => x.LastSend);

                        foreach (var tag in tags)
                        {
                            foreach (var ap in aps)
                            {
                                if (tag.SameWay(ap) && tasks[ap].Count < 208)
                                {
                                    var now = tasks[ap].Sum(x => x.Data.Length);
                                    if ((now + tag.TagData.Data.Length) > 524_288) continue;
                                    tasks[ap].Add(tag.TagData);
                                    tag.Transfer(ap);
                                }
                            }
                        }

                        foreach (var ap in aps)
                        {
                            if (tasks[ap].Count == 0) continue;
                            var result = Server.Instance.SendDataX(ap, tasks[ap]);
                            Log.Information($"[Cronus]{ap} send {tasks[ap].Count}: {result}");
                            if (result == SdkResult.OK) UpdateAP(ap, tasks[ap].Count);
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
        /// Thread: Task feedback
        /// </summary>
        /// <returns>The task</returns>
        async Task TaskFeedback()
        {
            Exception pre = new();
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
        #endregion

        #region AP
        /// <summary>
        /// Get AP ID list
        /// </summary>
        /// <param name="storeCode">Store code</param>
        /// <returns>AP ID list</returns>
        List<string> GetAPs(string storeCode) => Server.Instance.GetApOnlineList(storeCode);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="apID"></param>
        /// <returns></returns>
        AP GetAP(string apID)
        {
            if (DicAPs.ContainsKey(apID)) return DicAPs[apID];
            var ap = new AP { APID = apID };
            DicAPs.Add(apID, ap);
            return ap;
        }

        /// <summary>
        /// Update AP working status
        /// </summary>
        /// <param name="apID"></param>
        /// <param name="count"></param>
        void UpdateAP(string apID, int count)
        {
            var ap = GetAP(apID);
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
        public bool StartSDK(int port, Microsoft.Extensions.Logging.ILogger logger)
        {
            try
            {

                Server.Instance.ApEventHandler += Instance_ApEventHandler; ;
                Server.Instance.ResultEventHandler += Instance_ResultEventHandler;
                var result = Server.Instance.Start(logger, port);
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
                    var ap = GetAP(e.ApID);
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
                    DicAPs[e.ApID] = ap;
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