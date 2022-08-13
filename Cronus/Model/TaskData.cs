/****************************************************
 *          Cronus - ESL Gen3 Middleware            *
 **************************************************** 
 * File:    TaskData.cs
 * Date:    05/19/2022
 * Author:  Huang Hai Peng
 * Summary: The task data object class.
 * (C) Suzhou ETAG Electronic Technology Co., Ltd
****************************************************/
using SkiaSharp;

namespace Cronus.Model
{
    /// <summary>
    /// Task data object
    /// </summary>
    public class TaskData
    {
        #region Properties
        /// <summary>
        /// Task ID, refer to Task.TaskID
        /// </summary>
        public Guid TaskID { get; private set; } = Guid.Empty;
        /// <summary>
        /// Tag ID, refer to Tag.TagID
        /// </summary>
        public string TagID { get; private set; }
        /// <summary>
        /// AP ID, refer to AP.APID, Keep empty to use the default AP ID.
        /// </summary>
        public string APID { get; private set; } = string.Empty;
        /// <summary>
        /// Bitmap to send, only flashing LED light if is null.
        /// </summary>
        public SKBitmap? Bitmap { get; private set; } = null;
        /// <summary>
        /// Led light, red color
        /// </summary>
        public bool R { get; private set; } = false;
        /// <summary>
        /// Led light, green color
        /// </summary>
        public bool G { get; private set; } = false;
        /// <summary>
        /// Led light, blue color
        /// </summary>
        public bool B { get; private set; } = false;
        /// <summary>
        /// Led light flashing times
        /// </summary>
        public int Times { get; private set; } = 0;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tagID">Tag ID</param>
        /// <param name="bitmap">SKBitmap</param>
        public TaskData(string tagID, SKBitmap bitmap)
        {
            TagID = tagID;
            Bitmap = bitmap;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="apID">AP ID</param>
        /// <param name="tagID">Tag ID</param>
        /// <param name="bitmap">SKBitmap</param>
        public TaskData(string apID, string tagID, SKBitmap bitmap)
            : this(tagID, bitmap)
        {
            APID = apID;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tagID"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="times"></param>
        public TaskData(string tagID, bool r, bool g, bool b, int times)
        {
            if (times < 0) times = 0;
            else if (times > 36000) times = 36000;

            TagID = tagID;
            R = r;
            G = g;
            B = b;
            Times = times;
        }
        #endregion
    }
}
