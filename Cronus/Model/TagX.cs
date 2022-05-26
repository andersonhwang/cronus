using Cronos.SDK;
using Cronos.SDK.Entity;
using Cronus.Enum;

namespace Cronus.Model
{
    /// <summary>
    /// Tag with A&B
    /// </summary>
    internal class TagX
    {
        /// <summary>
        /// The locker
        /// </summary>
        readonly object _locker = new();
        /// <summary>
        /// Tag ID
        /// </summary>
        internal string TagID => Tag.TagID;
        /// <summary>
        /// The tag
        /// </summary>
        internal Tag Tag { get; private set; }
        /// <summary>
        /// Tag data to execute
        /// </summary>
        internal TagEntityX TagData { get; private set; } = null;
        /// <summary>
        /// AP ID list, ready to send
        /// </summary>
        internal List<string> APs { get; private set; } = new();
        /// <summary>
        /// AP ID - Default
        /// </summary>
        internal string AP0 { get; private set; } = string.Empty;
        /// <summary>
        /// AP ID - Specific
        /// </summary>
        internal int Best { get; private set; } = -256;
        /// <summary>
        /// Task A token
        /// </summary>
        internal int AToken => A is null ? -1 : A.Token;
        /// <summary>
        /// Task B token
        /// </summary>
        internal int BToken => B is null ? -1 : B.Token;
        /// <summary>
        /// Task A
        /// </summary>
        internal TaskResult A { get; private set; } = null;
        /// <summary>
        /// Task B
        /// </summary>
        internal TaskResult B { get; private set; } = null;
        /// <summary>
        /// Last send time
        /// </summary>
        internal DateTime LastSend => B is null ? DateTime.Now : B.LastSendTime ?? DateTime.Now;
        /// <summary>
        /// Last receive time
        /// </summary>
        internal DateTime LastReceive => B is null ? DateTime.Now : B.LastRecvTime ?? DateTime.Now;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tag">Tag</param>
        internal TagX(Tag tag)
        {
            Tag = tag;
            AP0 = tag.DefaultAP;
        }

        /// <summary>
        /// Write A
        /// </summary>
        /// <param name="task">Tag task</param>
        /// <param name="aps">Optional AP list</param>
        /// <returns>Previous tag task</returns>
        internal TaskResult? WriteA(TaskData task, List<string> aps)
        {
            lock (_locker)
            {
                TaskResult previous = null;
                if (A != null && AToken != BToken)
                {
                    previous = A.Drop();
                }
                A = new TaskResult(task);
                APs.Clear();
                if (!string.IsNullOrEmpty(task.APID))   // Specific AP ID
                {
                    APs.Add(task.APID);
                }
                else if (!string.IsNullOrEmpty(AP0))    // Default AP ID
                {
                    APs.Add(AP0);
                }
                else
                {
                    aps.ForEach(ap => APs.Add(ap));     // All AP ID
                }
                TagData = new TagEntityX(TagID, 
                    Server.Instance.GetTagData(
                        task.TagID, GetToken(Tag.Token), task.Bitmap,
                        task.R, task.G, task.B, task.Times));

                return previous;
            }
        }

        /// <summary>
        /// Transfer to send
        /// </summary>
        /// <param name="ap">Execute AP ID</param>
        internal void Transfer(string ap)
        {
            lock(_locker)
            {
                B = (TaskResult)A.Clone();
                B.Send();
                APs.Remove(ap);
            }
        }

        /// <summary>
        /// Write B
        /// </summary>
        /// <param name="ap">AP ID</param>
        /// <param name="result">Result entity</param>
        /// <returns>Need return</returns>
        internal bool WriteB(string ap, ResultEntity result)
        {
            lock(_locker)
            {
                if (BToken != result.Token) return false;
                var needReturn = B?.ProcessResult(ap, result);      // Update task result
                Tag.PorcessResult(ap, result);     // Update tag
                return needReturn ?? false;
            }
        }

        /// <summary>
        /// Get token
        /// </summary>
        /// <param name="current">Current token, refer to Tag.Token</param>
        /// <returns>Next token</returns>
        internal int GetToken(int current)
        {
            int previous = A is null ? B is null ? current : B.Token : A.Token;
            current = (previous == current ? current : previous) + 1;
            return current > 0xFFFF ? 1 : current == -1 ? 1 : current + 1;
        }

        /// <summary>
        /// Need work
        /// </summary>
        /// <param name="aps">All AP</param>
        /// <returns>True means need</returns>
        internal bool NeedWork(List<string> aps)
        {
            if (TagData is null) return false;
            if (AToken == BToken && (BToken == -1 || B.Status == Enum.TaskStatus.Success)) return false;
            if (BToken != -1 && B.Status == Enum.TaskStatus.Sending && (DateTime.Now - (B.LastSendTime ?? DateTime.Now)).TotalMinutes < 5) return false;
            if (APs.Count == 0) APs.AddRange(aps);
            return true;
        }

        /// <summary>
        /// Need work
        /// </summary>
        /// <returns>Need</returns>
        internal bool NeedWork() => ((BToken != -1 && B.Status != Enum.TaskStatus.Success && B.SendCount < 256) || AToken != BToken);

        /// <summary>
        /// Is working
        /// </summary>
        /// <returns>Working</returns>
        internal bool IsWorking() => BToken != -1 && B.Status == Enum.TaskStatus.Sending && B.SendCount < 256;

        /// <summary>
        /// The same way
        /// </summary>
        /// <param name="ap">AP ID</param>
        /// <returns>Same</returns>
        internal bool SameWay(string ap) => APs.Contains(ap);
    }
}
