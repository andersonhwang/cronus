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
    public class ServerInfor
    {
        public int APsCount { get; private set; }
        public int TagsCount { get; private set; }
        public int TotalTasksCount { get; private set; }
    }
}
