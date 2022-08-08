//*************************************************************
//*************************************************************
// Autor: Egger Christopher 
// Company: apic GmbH
// Datum: 08.008.2022                                 
// Version: 1.0                                              
//                                                          
// Description:                                                         
// Check if an port is reachabel or not
// Changelog:
//                                                          
//*************************************************************
//*************************************************************


using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Sockets;
using System.Timers;
using System.Threading;
using System.Windows.Threading;
using System.Net;


namespace NetUtil.Checker
{
    /// <summary>
    /// Check port event arguments
    /// </summary>
    public class CheckPortEventArgs : EventArgs
    {
        public CheckPortEventArgs(bool isFirstResult, string host, int port, bool connected, TimeSpan elapsed)
        {
            IsFirstResult = isFirstResult;
            Host = host;
            Port = port;
            Connected = connected;
            Elapsed = elapsed;
        }

        public bool IsFirstResult { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public bool Connected { get; set; }
        public TimeSpan Elapsed { get; set; }
    }

    /// <summary>
    /// Check port
    /// </summary>
    public class CheckPort
    {

        #region Getter / Setters

        /// <summary>
        /// How many retries should we to
        /// </summary>
        public int Retries { get; set; }

        /// <summary>
        /// Host to check
        /// </summary>
        public string Host { get; private set; }

        /// <summary>
        /// Port to check
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// Is port chcker running
        /// </summary>
        public bool IsRunning { get; private set; }

        #endregion

        #region pirvate members

        /// <summary>
        /// How many retries we have done
        /// </summary>
        private int _retries = 0;

        /// <summary>
        /// Tcp client
        /// </summary>
        private TcpClient _tcpClient = null;    

        /// <summary>
        /// Execution timer
        /// </summary>
        private System.Timers.Timer _executionTimer = null;

        /// <summary>
        /// Asyncron threadsafe event handler
        /// </summary>
        AsyncOperation _asyncOp = null;

        #endregion

        #region event handler

        /// <summary>
        /// Check result event
        /// </summary>
        public event EventHandler<CheckPortEventArgs> NewCheckResult;

        #endregion

        #region C'tor

        /// <summary>
        /// C'tor
        /// </summary>
        public CheckPort()
        {
            //Asyncon event handler
            _asyncOp = AsyncOperationManager.CreateOperation(null);

            //Set default values
            Retries = 4;
            IsRunning = false;
            _retries = 0;
        }

        #endregion

        #region public functions

        /// <summary>
        /// Start checking
        /// </summary>
        public void StartCheck(string host, int port)
        {            
            if (_executionTimer == null)
            {  
                _executionTimer = new System.Timers.Timer(500);
                _executionTimer.Elapsed += ExecutionTimer_Elapsed;
            }

            if (IsRunning) return;

            Host = host;
            Port = port;
            _retries = 0;
            IsRunning = true;
            _executionTimer.Enabled = true;
        }

        /// <summary>
        /// Start checking
        /// </summary>
        public void Cancle()
        {
            if(IsRunning)
            {
                _executionTimer.Enabled = false;
                IsRunning = false;
            } 
        }

        #endregion

        #region private functions

        /// <summary>
        /// Send an check result
        /// </summary>
        private void CreateNewCheckResult(bool isFirstResult, string host, int port, bool connected, TimeSpan elapsed)
        {
            CheckPortEventArgs arg = new CheckPortEventArgs(isFirstResult, host, port, connected, elapsed);

            _asyncOp.Post(new SendOrPostCallback(delegate (object obj)
            {
                OnCreateNewCheckResult(arg);
            }), null); 
        }

        /// <summary>
        /// Send an check result event handler
        /// </summary>
        protected void OnCreateNewCheckResult(CheckPortEventArgs e)
        {
            EventHandler<CheckPortEventArgs> tmpHandler = NewCheckResult;
            tmpHandler?.Invoke(this, e);
        }

        #endregion

        #region events

        /// <summary>
        /// Execution timer tick event
        /// </summary>
        private void ExecutionTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _executionTimer.Enabled = false;
            if (!IsRunning) return;

            IPAddress ipAddress = IPAddress.Parse(Host);
            IPEndPoint endPoint = new IPEndPoint(ipAddress, Port);

            if (_tcpClient == null) _tcpClient = new TcpClient();

            bool connected = false;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            try
            {
                //  _tcpClient.Connect(endPoint);
                _tcpClient.ConnectAsync(Host, Port).Wait(2000);
                if (_tcpClient.Connected)
                {
                    _tcpClient.Close();
                    connected = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            _tcpClient = null;
            stopWatch.Stop();
            if (!IsRunning) return;
            CreateNewCheckResult(_retries == 0,Host, Port, connected, stopWatch.Elapsed);

            _retries += 1;
            if (_retries >= Retries)
            {
                IsRunning = false;
                _executionTimer.Enabled = false;
            }
            else
            {
                _executionTimer.Enabled = true;
           }
        }

        #endregion
    }
}
