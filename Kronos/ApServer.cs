using Cronos.SDK.Enum;
using Cronos.SDK.Event;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Cronos.SDK
{
    /// <summary>
    /// Ap server
    /// </summary>
    internal sealed class ApServer
    {
        #region Properties
        static ILogger _log;
        static object _locker = new();
        static readonly ManualResetEvent ResetEvent = new(false);
        // Thread signal
        static Regex REG_EXP = new("[0-9A-F]{2},[01]{1},6,[0-9A-F]{16}V[0-9]{3}V[0-9]{3}.[0-9]{3}[0-9A-F]{8}");
        static int BUFFER_SIZE = 8096;                          // Buffer length
                                                                // Register regex express
        Socket _listener;                                       // Server TCP socket listener
        IPEndPoint _serverEndPoint;                             // Server end point
        internal Dictionary<string, ApClient> Clients;     // Ap clients list   

        /// <summary>
        /// Cosntructor, start server
        /// </summary>
        internal ApServer(ILogger log)
        {
            _log = log;
            Clients = new Dictionary<string, ApClient>();
        }

        /// <summary>
        /// Ap event handler
        /// </summary>
        internal EventHandler<ApEventArgs> ApEvent { get; set; }

        /// <summary>
        /// Result event handler
        /// </summary>
        internal EventHandler<ResultEventArgs> ResultEvent { get; set; }
        #endregion

        #region Methods
        /// Initialize server
        /// </summary>
        /// <param name="port">Server socket port to listener</param>
        /// <returns>Success or failed</returns>
        internal bool Init(int port)
        {
            try
            {
                _serverEndPoint = new IPEndPoint(IPAddress.Any, port);
                _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _listener.Bind(_serverEndPoint);
                _listener.Listen(64);

                Task.Run(() =>
                {
                    while (true && Clients.Count < 1024)
                    {
                        Thread.Sleep(20);
                        try
                        {
                            ResetEvent.Reset();
                            _listener.BeginAccept(
                                    new AsyncCallback(AcceptCallback),
                                    _listener);
                            ResetEvent.WaitOne();
                        }
                        catch (Exception ex)
                        {
                            _log.LogError(ex, "[SDK]ACPT_ERROR.");
                        }
                    }
                });
                return true;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "[SDK]INIT_ERROR.");
                return false;
            }
        }

        /// <summary>
        /// Accept callback
        /// </summary>
        /// <param name="result">IAsync result</param>
        void AcceptCallback(IAsyncResult result)
        {
            try
            {
                // Signal the main thread to continue.  
                ResetEvent.Set();

                // Get the socket that handles the client request.  
                Socket listener = (Socket)result.AsyncState;
                Socket handler = listener.EndAccept(result);

                // Create the state object.  
                ApClient client = new ApClient(handler, _log);
                handler.BeginReceive(client.Buffer, 0, BUFFER_SIZE, 0, new AsyncCallback(ConnectCallback), client);
            }
            catch (Exception ex)
            {
                ConnectCallback(null);
                _log.LogError(ex, "[SDK]Accept Error");
            }
        }

        /// <summary>
        /// Connect callbakc
        /// </summary>
        /// <param name="result">IAsyncResult</param>
        void ConnectCallback(IAsyncResult result)
        {
            lock (_locker)
            {
                if (result is null || result.AsyncState is null) return;

                ApClient client = (ApClient)result.AsyncState;
                Socket handler = client.Socket;
                try
                {

                    // Read data from the client socket.   
                    int bytesRead = handler.EndReceive(result);
                    string reg = string.Empty;
                    if (bytesRead > 0)
                    {
                        // There  might be more data, so store the data received so far.  
                        client.RecvData.Append(Encoding.ASCII.GetString(client.Buffer, 0, bytesRead));
                        reg = client.RecvData.ToString().TrimEnd('\0');
                        _log.LogDebug("[SDK]REG_DATA:" + reg);
                    }

                    // Check register data
                    if (!REG_EXP.IsMatch(reg.Trim()))
                    {
                        _log.LogError($"[SDK]INVALID_REG:{client.IP}:{client.Port} Data:{reg}");
                        client.Close("REG_ERROR");
                        return;
                    }

                    var id = reg[..2];
                    var data = reg.Substring(3, 1);
                    var shop = reg.Substring(7, 4);
                    var mac = reg.Substring(11, 12);
                    var hardware = reg.Substring(23, 4);
                    var firmware = reg.Substring(27, 8);
                    var stationType = ApType.Data;

                    // Duplicate check
                    string shopID = shop + id;
                    if (Clients.ContainsKey(shopID))
                    {
                        Clients[shopID].Close("DUP_ERROR");
                        _log.LogWarning(string.Format("[SDK]DUP_REG Store:{0}, ID:{1}, IP:{2}, Port:{3}", shop, id, client.IP, client.Port));
                    }

                    client.ResultEventHandler += ResultEvent;
                    client.ApEventHandler += ApEvent;
                    client.AddApHandler += AddApHandler;
                    client.RemoveApHandler += RemoveApHandler;
                    client.Active(shop, id, mac, hardware, firmware, stationType);
                    Clients.Add(shopID, client);

                    if (ApEvent is not null) ApEvent(this, new ApEventArgs(shop, id, client.IP, client.Port, mac,
                        data == "1" ? ApEventType.OnlineWithData : ApEventType.Online,
                        hardware, firmware, stationType));
                    _log.LogInformation("[SDK]AP_REG " + client.StoreInfor);
                }
                catch (Exception ex)
                {
                    client.Close("HANDLE_ERROR");
                    _log.LogError(ex, "[SDK]CONNECT_CALLBACK ERROR");
                }
            }
        }

        /// <summary>
        /// Remove station
        /// </summary>
        /// <param name="shop">Store code</param>
        /// <param name="id">Ap ID</param>
        /// <param name="pid">Anti-duplicate ID</param>
        void RemoveApHandler(string shop, string id)
        {
            lock (_locker)
            {
                string shopId = shop + id;
                if (Clients.ContainsKey(shopId))
                {
                    Clients[shopId].Dispose();
                    Clients[shopId] = null;
                    Clients.Remove(shopId);
                }
            }
        }

        /// <summary>
        /// Add station handler
        /// </summary>
        /// <param name="client"></param>
        void AddApHandler(ApClient client)
        {
            lock (_locker)
            {
                string shopId = $"{client.Store ?? string.Empty}{client.ID ?? string.Empty}";
                if (!Clients.ContainsKey(shopId))
                {
                    Clients.Add(shopId, client);
                }
            }
        }

        /// <summary>
        /// Send method
        /// </summary>
        /// <param name="id">Ap ID</param>
        /// <param name="data">Data to send</param>
        /// <returns>Result</returns>
        internal SdkResult Send(string id, byte[] data)
        {
            try
            {
                if (!Clients.ContainsKey(id)) return SdkResult.UnregisteredAp;

                ApClient station = Clients[id];
                return station == null
                    ? SdkResult.UnregisteredAp
                    : station.Status switch
                    {
                        ApStatus.Error => SdkResult.Fail,
                        ApStatus.Offline => SdkResult.ApOffline,
                        ApStatus.Working => SdkResult.ApBusy,
                        ApStatus.Idle => station.OnSend(data),
                        _ => SdkResult.Error,
                    };
            }
            catch (Exception ex)
            {
                _log.LogError(ex, string.Format("[SDK]Ap {0}, Send error.", id));
                return SdkResult.Fail;
            }
        }
        #endregion
    }
}
