/****************************************************
 *          Cronus - ESL Gen3 Middleware            *
 **************************************************** 
 * File:    ServerInfor.cs
 * Date:    06/11/2022
 * Author:  Huang Hai Peng
 * Summary: 
 *  This class is the server infor class of Cronus.
 *  For .NET Core 3.1
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
