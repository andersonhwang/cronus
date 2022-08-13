using Cronus.Model;

namespace Cronus.API
{
    /// <summary>
    /// Request object: task
    /// </summary>
    public class BasicTask
    {
        /// <summary>
        /// Store code, default is empty.
        /// </summary>
        public string StoreCode { get; set; } = string.Empty;
        /// <summary>
        /// Task datas list
        /// </summary>
        public List<TaskData> TaskDatas { get; set; } = new List<TaskData>();
    }
}
