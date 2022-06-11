/****************************************************
 *          Cronus - ESL Gen3 Middleware            *
 **************************************************** 
 * File:    TaskResult.cs
 * Date:    06/11/2022
 * Author:  Huang Hai Peng
 * Summary: The task result object class.
 * For .NET Core 3.1
 * (C) Suzhou ETAG Electronic Technology Co., Ltd
****************************************************/

using System;
using System.Collections.Generic;
using Cronos.SDK.Entity;

namespace Cronus.Model
{
    /// <summary>
    /// Task result
    /// </summary>
    public class TaskResult : ICloneable
    {
        #region Properities
        /// <summary>
        /// Task ID, refer to Task.TaskID
        /// </summary>
        public Guid TaskID { get; private set; }
        /// <summary>
        /// Tag ID, refer to Tag.TagID
        /// </summary>
        public string TagID { get; private set; }
        /// <summary>
        /// AP route record, refer to AP.APID
        /// </summary>
        public List<string> RouteRecord { get; private set; } = new List<string>();
        /// <summary>
        /// Task result
        /// </summary>
        public Enum.TaskStatus Status { get; private set; } = Enum.TaskStatus.Init;
        /// <summary>
        /// Total send count
        /// </summary>
        public int SendCount { get; private set; } = 0;
        /// <summary>
        /// Frist send time
        /// </summary>
        public DateTime? FirstSendTime { get; private set; } = null;
        /// <summary>
        /// Last send time
        /// </summary>
        public DateTime? LastSendTime { get; private set; } = null;
        /// <summary>
        /// Last recieve time
        /// </summary>
        public DateTime? LastRecvTime { get; private set; } = null;
        /// <summary>
        /// Token(ACK)
        /// </summary>
        public int Token { get; private set; } = 0;
        /// <summary>
        /// Battery voltage, 31 means 3.1v.
        /// </summary>
        public float? Battery { get; private set; } = null;
        /// <summary>
        /// Temperature, 27 mean 27℃
        /// </summary>
        public int? Temperature { get; private set; } = null;
        /// <summary>
        /// RF power, -256 means -256 dBm
        /// </summary>
        public int? RfPower { get; private set; } = null;
        /// <summary>
        /// Firmware version
        /// </summary>
        public string Version { get; private set; } = string.Empty;
        /// <summary>
        /// Screen code
        /// </summary>
        public string Screen { get; private set; } = string.Empty;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="taskID"></param>
        /// <param name="tagID"></param>
        /// <param name="token"></param>
        public TaskResult(Guid taskID, string tagID, int token)
        {
            TaskID = taskID;
            TagID = tagID;
            Token = token;
        }

        /// <summary>
        /// Constructor from task data object
        /// </summary>
        /// <param name="task">Task data</param>
        public TaskResult(TaskData task)
        {
            TaskID = task.TaskID;
            TagID = task.TagID;
        }
        #endregion

        #region Publick Methods
        /// <summary>
        /// Clone
        /// </summary>
        /// <returns>New task result object</returns>
        public object Clone()
        {
            return new TaskResult(TaskID, TagID, Token);
        }
        #endregion

        #region Internal Methods
        /// <summary>
        /// Send
        /// </summary>
        internal void Send()
        {
            SendCount += 1;
            Status = Enum.TaskStatus.Sending;
            LastSendTime = DateTime.Now;
        }

        /// <summary>
        /// Set token
        /// </summary>
        /// <param name="token">Token</param>
        internal void SetToken(int token) => Token = token;

        /// <summary>
        /// Process result
        /// </summary>
        /// <param name="tag">Tag entity</param>
        /// <param name="result">Result entity</param>
        /// <returns>Need return</returns>
        internal bool ProcessResult(string ap, ResultEntity result)
        {
            bool needReturn = false;
            // Drop obsolete result
            if (Status == Enum.TaskStatus.Success && result.TagResult == Cronos.SDK.Enum.TagResult.Faild) return needReturn;
            // Only keep last 10 route records
            while (RouteRecord.Count > 10) RouteRecord.RemoveAt(0);

            RouteRecord.Add(ap);
            RfPower = result.RfPower;
            if (result.Battery == 0)
            { Battery = null; }
            else
            { Battery = result.Battery / 10F; }
            Temperature = result.Temperature;
            if (result.TagResult == Cronos.SDK.Enum.TagResult.Success)
            {
                if (Status != Enum.TaskStatus.Success) needReturn = true;
                Status = Enum.TaskStatus.Success;
            }
            else
            {
                Status = Enum.TaskStatus.Failed;
                if (SendCount >= 0xFF) needReturn = true;
            }

            LastRecvTime = DateTime.Now;
            return needReturn;
        }

        /// <summary>
        /// Drop current task
        /// </summary>
        /// <returns></returns>
        internal TaskResult Drop()
        {
            Status = Enum.TaskStatus.Drop;
            return this;
        }
        #endregion
    }
}
