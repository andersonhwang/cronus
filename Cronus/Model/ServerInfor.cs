/****************************************************
 *          Cronus - ESL Gen3 Middleware            *
 **************************************************** 
 * File:    ServerInfor.cs
 * Date:    05/19/2022
 * Author:  Huang Hai Peng
 * Summary: 
 *  This class is the server infor class of Cronus.
 * (C) Suzhou ETAG Electronic Technology Co., Ltd
****************************************************/

namespace Cronus.Model
{
    /// <summary>
    /// Server information
    /// </summary>
    public class ServerInfor
    {
        /// <summary>
        /// APs count
        /// </summary>
        public int APsCount { get; private set; }
        /// <summary>
        /// Tags count
        /// </summary>
        public int TagsCount { get; private set; }
        /// <summary>
        /// Total tasks count
        /// </summary>
        public int TotalTasksCount { get; private set; }
    }
}
