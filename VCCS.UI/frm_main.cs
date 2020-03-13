using log4net;
using SIPSorcery.SIP;
using SIPSorcery.SIP.App;
using SIPSorcery.Sys;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using VCCS.UI.Media;
using VCCS.UI.Signalling;
using VCCS.UI.UserController;

namespace VCCS.UI
{
    public partial class frm_main : Form
    {
        private const int SIP_CLIENT_COUNT = 2;                             // The number of SIP clients (simultaneous calls) that the UI can handle.

        // Currently only supporting these mode(s) from local web cams. Extra work to convert other formats to bitmaps that can be displayed by WPF.

        private ILog logger = AppState.logger;

        private string m_sipUsername = SIPSoftPhoneState.SIPUsername;
        private string m_sipPassword = SIPSoftPhoneState.SIPPassword;
        private string m_sipServer = SIPSoftPhoneState.SIPServer;

        private SIPTransportManager _sipTransportManager;
        private SIPClient _sipClients;
        private SoftphoneSTUNClient _stunClient;                    // STUN client to periodically check the public IP address.
        private SIPRegistrationUserAgent _sipRegistrationClient;    // Can be used to register with an external SIP provider if incoming calls are required.

        private MediaManager _mediaManager;                         // The media (audio and video) manager.
        private MusicOnHold _musicOnHold;

        public frm_main()
        {
            InitializeComponent();
            ResetToCallStartState(null);
            // Do some UI initialization.

            _sipTransportManager = new SIPTransportManager();
            _sipTransportManager.IncomingCall += SIPCallIncoming;

            // If a STUN server hostname has been specified start the STUN client to lookup and periodically
            // update the public IP address of the host machine.
            if (!SIPSoftPhoneState.STUNServerHostname.IsNullOrBlank())
            {
                _stunClient = new SoftphoneSTUNClient(SIPSoftPhoneState.STUNServerHostname);
                _stunClient.PublicIPAddressDetected += (ip) =>
                {
                    SIPSoftPhoneState.PublicIPAddress = ip;
                };
                _stunClient.Run();
            }
        }

        private void panel5_Paint(object sender, PaintEventArgs e)
        {
        }

        private async void frm_main_Load(object sender, EventArgs e)
        {
            this.lb_time.Text = DateTime.Now.ToString("hh:mm:ss");
            System.Timers.Timer _clocktime = new System.Timers.Timer();
            _clocktime.Interval = 1000;
            _clocktime.Elapsed += Timer_Elapse;
            _clocktime.Start();

            _mediaManager = new MediaManager(this);

            _musicOnHold = new MusicOnHold();
            tbl_panel_hotline.Controls.Clear();
            for (var i = 0; i < 16; i++)
            {
                HotLine hotLine = new HotLine();
                hotLine.LblHotLine = i.ToString();
                hotLine.LblSDT = $"0{i}";
                var d = i / 4;
                var r = i % 4;
                if (d < 1)
                {
                    tbl_panel_hotline.Controls.Add(hotLine, i, 0);
                    hotLine.Dock = DockStyle.Fill;
                }
                if (d < 2 & d >= 1)
                {
                    r = r > 0 ? r : 0;
                    tbl_panel_hotline.Controls.Add(hotLine, r, 1);
                    hotLine.Dock = DockStyle.Fill;
                }
                if (d < 3 & d >= 2)
                {
                    r = r > 0 ? r : 0;
                    tbl_panel_hotline.Controls.Add(hotLine, r, 2);
                    hotLine.Dock = DockStyle.Fill;
                }
                if (d < 4 & d >= 3)
                {
                    r = r > 0 ? r : 0;
                    tbl_panel_hotline.Controls.Add(hotLine, r, 3);
                    hotLine.Dock = DockStyle.Fill;
                }
            }
            await Initialize();
        }

        private async Task Initialize()
        {
            await _sipTransportManager.InitialiseSIP();

            var mediaSessionFactory = new RTPMediaSessionManager(_mediaManager, _musicOnHold);
            var sipClient = new SIPClient(_sipTransportManager.SIPTransport, mediaSessionFactory);

            sipClient.CallAnswer += SIPCallAnswered;
            sipClient.CallEnded += ResetToCallStartState;
            sipClient.StatusMessage += (client, message) => { SetStatusText(lbl_status, message); };
            sipClient.RemotePutOnHold += RemotePutOnHold;
            sipClient.RemoteTookOffHold += RemoteTookOffHold;

            _sipClients = sipClient;

            string listeningEndPoints = null;

            foreach (var sipChannel in _sipTransportManager.SIPTransport.GetSIPChannels())
            {
                SIPEndPoint sipChannelEP = sipChannel.ListeningSIPEndPoint.CopyOf();
                sipChannelEP.ChannelID = null;
                listeningEndPoints += (listeningEndPoints == null) ? sipChannelEP.ToString() : $", {sipChannelEP}";
            }

            _sipRegistrationClient = new SIPRegistrationUserAgent(
            _sipTransportManager.SIPTransport,
            null,
            null,
            new SIPURI(m_sipUsername, m_sipServer, null, SIPSchemesEnum.sip, SIPProtocolsEnum.udp),
            m_sipUsername,
            m_sipPassword,
            null,
            m_sipServer,
            new SIPURI(m_sipUsername, IPAddress.Any.ToString(), null),
            180,
            null,
            null,
            (message) => { logger.Debug(message); });

            _sipRegistrationClient.Start();
        }

        #region Event

        private bool SIPCallIncoming(SIPRequest sipRequest)
        {
            SetStatusText(lbl_status, $"Incoming call from {sipRequest.Header.From.FriendlyDescription()}.");

            if (!_sipClients.IsCallActive)
            {
                _sipClients.Accept(sipRequest);

                this.Invoke(new MethodInvoker(delegate
                {
                    btn_end.Enabled = !btn_end.Enabled;
                }));

                return true;
            }

            return false;
        }

        private void SIPCallAnswered(SIPClient client)
        {
            _mediaManager.StartAudio();

            this.Invoke(new MethodInvoker(delegate
            {
                btn_call.Enabled = !btn_call.Enabled;
            }));
        }

        private void CallButton_Click(object sender, EventArgs e)
        {
            //if (client == _sipClients /*&& m_uriEntryTextBox.Text.IsNullOrBlank()*/)
            //{
            //    SetStatusText(lbl_status, "No call destination was specified.");
            //}

            string callDestination = null;

            SetStatusText(lbl_status, $"calling {callDestination}.");
            this.BeginInvoke(new MethodInvoker(delegate
            {
                btn_call.Enabled = !btn_call.Enabled;
                btn_end.Enabled = !btn_end.Enabled;
            }));

            // Put the first call on hold.
            if (_sipClients.IsCallActive)
            {
                _sipClients.PutOnHold();
                //m_holdButton.Visibility = Visibility.Collapsed;
                //m_offHoldButton.Visibility = Visibility.Visible;
            }

            // Start SIP call.
            Task.Run(() => _sipClients.Call(callDestination));
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            _sipClients.Cancel();
            ResetToCallStartState(_sipClients);
        }

        /// <summary>
        /// The button to hang up an outgoing call.
        /// </summary>
        private void ByeButton_Click(object sender, EventArgs e)
        {
            _sipClients.Hangup();

            ResetToCallStartState(_sipClients);
        }

        /// <summary>
        /// The button to answer an incoming call.
        /// </summary>
        private async void AnswerButton_Click(object sender, EventArgs e)
        {
            await AnswerCallAsync(_sipClients);
        }

        /// Answer an incoming call on the SipClient
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private async Task AnswerCallAsync(SIPClient client)
        {
            await client.Answer();

            _mediaManager.StartAudio();
            //m_answerButton.Visibility = Visibility.Collapsed;
            //m_rejectButton.Visibility = Visibility.Collapsed;
            //m_redirectButton.Visibility = Visibility.Collapsed;
            //m_byeButton.Visibility = Visibility.Visible;
            //m_transferButton.Visibility = Visibility.Visible;
            //m_holdButton.Visibility = Visibility.Visible;

            //m_call2ActionsGrid.IsEnabled = true;
        }

        private void RejectButton_Click(object sender, EventArgs e)
        {
            _sipClients.Reject();
            ResetToCallStartState(_sipClients);
        }

        /// <summary>
        /// The button to initiate an attended transfer request between the two in active calls.
        /// </summary>
        //private async void AttendedTransferButton_Click(object sender, System.Windows.RoutedEventArgs e)
        //{
        //    bool wasAccepted = await _sipClients[1].AttendedTransfer(_sipClients[0].Dialogue);

        //    if (!wasAccepted)
        //    {
        //        SetStatusText(m_signallingStatus, "The remote call party did not accept the transfer request.");
        //    }
        //}

        //private void RedirectButton_Click(object sender, System.Windows.RoutedEventArgs e)
        //{
        //    ResetToCallStartState(_sipClients);
        //}

        /// <summary>
        /// The button to send a blind transfer request to the remote call party.
        /// </summary>
        //private async void BlindTransferButton_Click(object sender, System.Windows.RoutedEventArgs e)
        //{
        //    var client = (sender == m_transferButton) ? _sipClients[0] : _sipClients[1];
        //    bool wasAccepted = await client.BlindTransfer(m_uriEntryTextBox.Text);

        //    if (wasAccepted)
        //    {
        //        //TODO: We need to the end the call

        //        ResetToCallStartState(client);
        //    }
        //    else
        //    {
        //        SetStatusText(m_signallingStatus, "The remote call party did not accept the transfer request.");
        //    }
        //}

        private void RemotePutOnHold(SIPClient sipClient)
        {
            // We can't put them on hold if they've already put us on hold.
            SetStatusText(lbl_status, "Put on hold by remote party.");

            //if (sipClient == _sipClients[0])
            //{
            //    Dispatcher.DoOnUIThread(() =>
            //    {
            //        m_holdButton.Visibility = Visibility.Collapsed;
            //    });
            //}
            //else if (sipClient == _sipClients[1])
            //{
            //    Dispatcher.DoOnUIThread(() =>
            //    {
            //        m_hold2Button.Visibility = Visibility.Collapsed;
            //    });
            //}
        }

        /// The remote call party has taken us off hold.
        /// </summary>
        private void RemoteTookOffHold(SIPClient sipClient)
        {
            SetStatusText(lbl_status, "Taken off hold by remote party.");

            //if (sipClient == _sipClients[0])
            //{
            //    Dispatcher.DoOnUIThread(() =>
            //    {
            //        m_holdButton.Visibility = Visibility.Visible;
            //    });
            //}
            //else if (sipClient == _sipClients[1])
            //{
            //    Dispatcher.DoOnUIThread(() =>
            //    {
            //        m_hold2Button.Visibility = Visibility.Visible;
            //    });
            //}
        }

        private void Timer_Elapse(object sender, ElapsedEventArgs e)
        {
            this.BeginInvoke(new MethodInvoker(delegate
            {
                this.lb_time.Text = DateTime.Now.ToString("hh:mm:ss");
            }));
        }

        private void ResetToCallStartState(SIPClient sipClient)
        {
            if (sipClient == null)
            {
                if (IsHandleCreated)
                {
                    this.Invoke((MethodInvoker)delegate ()

                    {
                        SetStatusText(lbl_status, "Ready");
                    });
                }
                else
                {
                    SetStatusText(lbl_status, "Ready");
                }
            }
        }

        private void SetStatusText(Label lbl, string text)
        {
            logger.Debug(text);
            lbl.Text = text;
        }

        #endregion Event

        private void frm_main_FormClosing(object sender, FormClosingEventArgs e)
        {
            _sipClients.Shutdown();

            _mediaManager.Close();
            _sipTransportManager.Shutdown();
            if (_stunClient != null)
            {
                _stunClient.Stop();
            }
        }
    }
}