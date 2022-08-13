namespace Cronus.API
{
    /// <summary>
    /// Request object: image task
    /// </summary>
    public class ImageTask
    {
        /// <summary>
        /// Store code, default is empty.
        /// </summary>
        public string StoreCode { get; set; } = string.Empty;
        /// <summary>
        /// Tag ID
        /// </summary>
        public string TagID { get; set; }
        /// <summary>
        /// SKImage data, Base-64
        /// </summary>
        public string ImageBase64 { get; set; }
    }
}
