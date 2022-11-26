/****************************************************
 *          Cronus - ESL Gen3 Middleware            *
 **************************************************** 
 * File:    TaskData.cs
 * Date:    06/11/2022
 * Author:  Huang Hai Peng
 * Summary: The task data object class.
 * For .NET Core 3.1
 * (C) Suzhou ETAG Electronic Technology Co., Ltd
****************************************************/
using System;
using System.Drawing;
using Cronos.SDK.Enum;

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
        public Bitmap? Bitmap { get; private set; } = null;
        /// <summary>
        /// Pattern, default is update and display
        /// </summary>
        public Pattern Pattern { get; private set; } = Pattern.UpdateDisplay;
        /// <summary>
        /// Page index, default is the 1st page
        /// </summary>
        public PageIndex Page { get; private set; } = PageIndex.P0;
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
        /// Constructor for image
        /// </summary>
        /// <param name="tagID">Tag ID</param>
        /// <param name="bitmap">SKBitmap</param>
        /// <param name="pattern">Pattern</param>
        /// <param name="page">Page index</param>
        public TaskData(string tagID, Bitmap bitmap, Pattern pattern = Pattern.UpdateDisplay, PageIndex page = PageIndex.P0)
        {
            TagID = tagID;
            Bitmap = bitmap;
            Pattern = pattern;
            Page = page;
        }

        /// <summary>
        /// Constructor for image with specific ap ID
        /// </summary>
        /// <param name="apID">AP ID</param>
        /// <param name="tagID">Tag ID</param>
        /// <param name="bitmap">SKBitmap</param>
        /// <param name="pattern">Pattern</param>
        /// <param name="page">Page index</param>
        public TaskData(string apID, string tagID, Bitmap bitmap, Pattern pattern = Pattern.UpdateDisplay, PageIndex page = PageIndex.P0)
            : this(tagID, bitmap, pattern, page)
        {
            APID = apID;
        }

        /// <summary>
        /// Constructor for led
        /// </summary>
        /// <param name="tagID">Tag ID</param>
        /// <param name="r">Red light</param>
        /// <param name="g">Green light</param>
        /// <param name="b">Blue light</param>
        /// <param name="times">Flashing times</param>
        public TaskData(string tagID, bool r, bool g, bool b, int times)
        {
            if (times < 0) times = 0;
            else if (times > 36000) times = 36000;

            TagID = tagID;
            R = r;
            G = g;
            B = b;
            Times = times;
            Pattern = Pattern.LED;
        }

        /// <summary>
        /// Constructor for switch page
        /// </summary>
        /// <param name="tagID">Tag ID</param>
        /// <param name="page">Page index</param>
        public TaskData(string tagID, int page)
        {
            if (page < 0) page = 0;
            else if (page > 7) page = 7;

            TagID = tagID;
            Pattern = Pattern.Display;
            Page = (PageIndex)page;
        }
        #endregion
    }
}
