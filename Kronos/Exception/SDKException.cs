using Cronos.SDK.Enum;
using System;

namespace Cronos.SDK
{
    /// <summary>
    /// SDK Exception
    /// </summary>
    [Serializable]
    public sealed class SDKException : ApplicationException
    {
        /// <summary>
        /// Result code
        /// </summary>
        public SdkResult ResultCode { get; private set; }

        /// <summary>
        /// Exception description
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="result">Result</param>
        /// <param name="message">Message</param>
        public SDKException(SdkResult result, string message)
        {
            ResultCode = result;
            HResult = (int)result;
            Description = message;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="result">Result</param>
        /// <param name="message">Message</param>
        /// <param name="arguments">Arguments of message</param>
        public SDKException(SdkResult result, string message, params object[] arguments)
        {
            ResultCode = result;
            HResult = (int)result;
            Description = string.Format(message, arguments);
        }
    }
}
