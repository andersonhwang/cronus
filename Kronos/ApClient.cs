using Cronos.SDK.Data;
using Cronos.SDK.Enum;
using Cronos.SDK.Event;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cronos.SDK
{
    /// <summary>
    /// Ap client
    /// </summary>
    internal sealed class ApClient : IDisposable
    {
        #region Properties
        /// <summary>
        /// The locker
        /// </summary>
        private object _locker = new object();

        /// <summary>
        /// The logger
        /// </summary>
        private ILogger _log;

        /// <summary>
        /// Store code
        /// </summary>
        public string Store { get; private set; }

        /// <summary>
        /// Ap ID
        /// </summary>
        public string ID { get; private set; }

        /// <summary>
        /// Store infor
        /// </summary>
        internal string StoreInfor { get { return $"Store:{Store}, ID:{ID}, IP:{IP}, Port:{Port}, MAC:{Mac}, Status:{Status}"; } }

        /// <summary>
        /// Ap IP address
        /// </summary>
        public IPAddress IP { get; private set; }

        /// <summary>
        /// TCP socket port
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// Ap MAC address
        /// </summary>
        public string Mac { get; private set; }

        /// <summary>
        /// Hardware version
        /// </summary>
        public string Hardware { get; private set; }

        /// <summary>
        /// Firmware  version
        /// </summary>
        public string Firmware { get; private set; }

        /// <summary>
        /// Ap status
        /// </summary>
        public ApStatus Status { get; set; } = ApStatus.Offline;

        /// <summary>
        /// Ap type
        /// </summary>
        public ApType Type { get; set; }

        /// <summary>
        /// Recieve buffer
        /// </summary>
        internal byte[] Buffer { get; private set; }

        /// <summary>
        /// Receive data
        /// </summary>
        internal StringBuilder RecvData { get; set; }

        /// <summary>
        /// Air time (Receive)
        /// </summary>
        private DateTime AirTime { get; set; }

        /// <summary>
        /// Line time (Send)
        /// </summary>
        private DateTime LineTime { get; set; }

        /// <summary>
        /// Expire timer
        /// </summary>
        private Timer Expire { get; set; }

        /// <summary>
        /// Ap client
        /// </summary>
        internal Socket Socket { get; set; }
        #endregion

        #region Delegate Events
        /// <summary>
        /// Result event handler
        /// </summary>
        internal event EventHandler<ResultEventArgs> ResultEventHandler
        {
            add { ResultEvent += value; }
            remove { ResultEvent -= value; }
        }

        /// <summary>
        /// Ap event handler
        /// </summary>
        internal event EventHandler<ApEventArgs> ApEventHandler
        {
            add { ApEvent += value; }
            remove { ApEvent -= value; }
        }

        /// <summary>
        /// Result event
        /// </summary>
        event EventHandler<ResultEventArgs> ResultEvent;

        /// <summary>
        /// Ap event
        /// </summary>
        event EventHandler<ApEventArgs> ApEvent;

        /// <summary>
        /// delegate remove station
        /// </summary>
        /// <param name="shop">Store code</param>
        /// <param name="id">Ap ID</param>
        /// <param name="pid">Process ID</param>
        internal delegate void RemoveApEvent(string shop, string id);

        /// <summary>
        /// delegate add station
        /// </summary>
        /// <param name="shop">Store code</param>
        /// <param name="id">Ap ID</param>
        internal delegate void AddApEvent(ApClient client);

        /// <summary>
        /// Remove station event handler
        /// </summary>
        internal RemoveApEvent RemoveApHandler;

        /// <summary>
        /// Add station event handler
        /// </summary>
        internal AddApEvent AddApHandler;
        #endregion

        #region Construtor
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="socket">Client socket</param>
        /// <param name="log">Logger</param>
        public ApClient(Socket socket, ILogger log)
        {
            _log = log;

            IPEndPoint point = socket.RemoteEndPoint as IPEndPoint;
            Socket = socket;
            IP = point.Address;
            Port = point.Port;
            Status = ApStatus.Offline;
            Buffer = new byte[8096];
            RecvData = new StringBuilder();
        }
        #endregion

        /// <summary>
        /// Active station client
        /// </summary>
        /// <param name="shop">Store code</param>
        /// <param name="id">Ap ID</param>
        /// <param name="mac">Mac address</param>
        /// <param name="hardware">Hardware version</param>
        /// <param name="firmware">Firmware version</param>
        /// <param name="type">Ap type</param>
        /// <param name="isWorking">Default status is working</param>
        public void Active(string shop, string id, string mac, string hardware, string firmware, ApType type, bool isWorking = false)
        {
            try
            {
                lock (_locker)
                {
                    Store = shop;
                    ID = id;
                    Mac = mac;
                    Type = type;
                    Hardware = hardware;
                    Firmware = firmware;
                    Status = isWorking ? ApStatus.Working : ApStatus.Idle;
                    Socket.Send(Encoding.ASCII.GetBytes(PB.D_R));
                    StartReceiving();
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"[SDK]ACTIVE_ERROR {Store}-{ID}");
                Close("ACTIVE_ERROR");
            }
        }

        /// <summary>
        /// Start receiving
        /// </summary>
        private void StartReceiving()
        {
            try
            {
                Socket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, ReceiveCallback, this);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"[SDK]RECV_ERROR {Store}-{ID}");
                Close("START_RECIEVE_ERROR");
            }
        }

        /// <summary>
        /// Receive callback
        /// </summary>
        /// <param name="result"></param>
        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                int length = Socket.EndReceive(result);
                if (length > 1)
                {
                    OnRecieve(length, Buffer);
                }
                else if (length == 1)
                {
                    if ((char)Buffer[0] == '$')
                    {
                        _log.LogInformation(string.Format("[SDK]Ap {0}-{1} reponsed: OK", Store, ID));
                        Expire?.Change(900 * 1000, Timeout.Infinite);
                    }
                    else
                        _log.LogInformation($"[SDK]Ap {Store}-{ID} recieved:{(char)Buffer[0]}.");
                }
                else
                {
                    Close("REMOTE_DISCONNECT");
                }

                StartReceiving();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, string.Format("[SDK]RECV_ERROR {0}-{1}", Store, ID));
                Close("RECIEVE_ERROR");
            }
        }

        /// <summary>
        /// Send data to station client
        /// </summary>
        /// <param name="data">Data to send</param>
        /// <param name="working">Keep working</param>
        /// <returns>The Result</returns>
        internal SdkResult OnSend(byte[] data)
        {
            try
            {
                lock (_locker)
                {
                    LineTime = DateTime.Now;
                    Status = ApStatus.Working;
                    Expire = new Timer(OnTimeout, null, 300 * 1000, Timeout.Infinite);

                    Socket.Send(data);
                    _log.LogInformation(string.Format("[SDK]Ap {0}-{1} send data:{2}", Store, ID, PB.BH(data)));
                    return SdkResult.OK;
                }
            }
            catch (Exception ex)
            {
                Close("SEND_ERROR");

                _log.LogError(ex, string.Format("[SDK]STATION_ERROR Ap {0}-{1}", Store, ID));
                return SdkResult.Fail;
            }
        }

        /// <summary>
        /// Recieve
        /// </summary>
        /// <param name="bufferSize">Buffer size</param>
        /// <param name="buffer">Buffer list</param>
        internal void OnRecieve(int bufferSize, byte[] buffer)
        {
            AirTime = DateTime.Now;

            // Merge into one array
            byte[] data = new byte[bufferSize];
            Array.Copy(buffer, 0, data, 0, bufferSize);

            // Reset timer
            Expire?.Change(Timeout.Infinite, Timeout.Infinite);

            string strData;
            if (data[0] == '$')
            {
                // Anti stuck data package
                byte[] dataTrim = new byte[bufferSize - 1];
                Array.Copy(data, 1, dataTrim, 0, bufferSize - 1);
                strData = Encoding.ASCII.GetString(dataTrim);
            }
            else
            {
                strData = Encoding.ASCII.GetString(data);
            }

            Socket.Send(PB.D_E);
            Status = ApStatus.Idle;

            if (RecvData.Length > 0 && (strData[0] == '<' || strData[0] == '-' || strData[0] == '>') && strData.Length >= 33)
            {
                RecvData.Clear();
            }
            RecvData.Append(strData);
            _log.LogInformation($"[SDK]Ap {Store}-{ID} recieved:{strData}.");

            if (RecvData[0] == '<' || RecvData[0] == '-' || RecvData[0] == '>')
            {
                var result = RecvData.ToString();
                var len = Convert.ToInt32(result.Substring(1, RecvData[0] == '-' ? 4 : 6), 16) + 5;
                if (result.Length >= len) // > is stick
                {
                    if (result[0] == '-' && ApEvent is not null)
                    {
                        Task.Run(() =>
                        {
                            ApEvent(this, new ApEventArgs(Store, ID, IP, Port, Mac, ApEventType.Heartbeat, Hardware, Firmware));
                            AddApHandler?.Invoke(this);
                            Status = ApStatus.Idle;
                        });
                    }
                    else if (ResultEvent is not null)
                    {
                        Task.Run(() =>
                        {
                            ResultEvent(this, RP.REA(Store, ID, result));
                        });
                    }
                    RecvData.Clear();
                }
            }
        }

        /// <summary>
        /// Timeout
        /// </summary>
        /// <param name="target">Target</param>
        internal void OnTimeout(object target)
        {
            try
            {
                if (Expire != null)
                {
                    _log.LogError($"[SDK]TIME_OUT {Store}-{ID} AirTime:{AirTime:yy-MM-dd HH:mm:ss:fff} LineTime:{LineTime:yy-MM-dd HH:mm:ss:fff}");
                    Expire.Change(Timeout.Infinite, Timeout.Infinite);
                    Expire.Dispose();
                    Expire = null;
                }

                //Close("TIMEOUT");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "[SDK]STATION_TIMEOUT");
            }
        }

        /// <summary>
        /// Ap close
        /// </summary>
        internal void Close(string reason)
        {
            try
            {
                lock (_locker)
                {
                    if (Socket != null)
                    {
                        if (Socket.Connected)
                        {
                            Socket.Shutdown(SocketShutdown.Both);
                            Socket.Close();
                        }

                        Socket.Dispose();
                        Socket = null;
                    }

                    Expire?.Change(Timeout.Infinite, Timeout.Infinite);
                    Expire = null;
                    if (ApEvent is not null)
                    {
                        _ = Task.Run(() =>
                          {
                              ApEvent(this, new ApEventArgs(Store, ID, IP, Port, Mac, ApEventType.Offline, Hardware, Firmware));
                          });
                    }
                    if (Status != ApStatus.Offline) RemoveApHandler?.Invoke(Store, ID);
                    _log.LogWarning($"[SDK]SERVER_CLOSE {Store}-{ID} {reason}");
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"[SDK]!!!SERVER_CLOSE!!! {Store}-{ID} {reason}");
            }
            finally
            {
                Status = ApStatus.Offline;
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
