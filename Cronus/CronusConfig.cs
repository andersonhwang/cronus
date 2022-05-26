namespace Cronus
{
    /// <summary>
    /// Cronus configure object
    /// </summary>
    public class CronusConfig
    {
        /// <summary>
        /// AP port, 1234
        /// </summary>
        public int APPort { get; set; } = 1234;
        /// <summary>
        /// Default store code, 0000
        /// </summary>
        public string DefaultStoreCode { get; set; } = "0000";
        /// <summary>
        /// Only one store, true, will ignore store code
        /// </summary>
        public bool OneStoreModel { get; set; } = true;
    }
}
