namespace Cronus.API
{
    /// <summary>
    /// Request object: LED task
    /// </summary>
    public class LedTask
    {
        /// <summary>
        /// Store code, default is empty.
        /// </summary>
        public string StoreCode { get; set; } = string.Empty;
        /// <summary>
        /// Tag ID list
        /// </summary>
        public List<string> TagIDList { get; set; } = new List<string>();
        /// <summary>
        /// Red color light
        /// </summary>
        public bool Red { get; set; } = false;
        /// <summary>
        /// Green color light
        /// </summary>
        public bool Green { get; set; } = false;
        /// <summary>
        /// Blue color light
        /// </summary>
        public bool Blue { get; set; } = false;
        /// <summary>
        /// Led light flashing times, default is 1 minute.
        /// </summary>
        public int Times { get; set; } = 60;
    }
}
