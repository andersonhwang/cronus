using Cronus.Model;

namespace Cronus.Events
{
    /// <summary>
    /// Task result event args
    /// </summary>
    public class TaskResultEventArgs : EventArgs
    {
        /// <summary>
        /// Task results list
        /// </summary>
        public List<TaskResult> TaskResults { get; private set; }
    }
}
