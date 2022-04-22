using System;
using System.Text;
using System.Windows;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ChatSocket2
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Mittente _mittente;
        private Thread _threadRicezione;
        private Thread _threadRicezioneBroadcast;
        private Socket _socket;
        private Socket _socketBroadcast;
        private ObservableCollection<Mittente> _listaMittenti;
        private const int PORTA = 64000;
        private const int PORTA_BROADCAST = 60000;
        private string _nomeUtente = "Utente2";
        //DispatcherTimer dTimer = null;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                _listaMittenti = new ObservableCollection<Mittente>();
                lstBoxAgenda.ItemsSource = _listaMittenti;

                SetupSocketMittente();
                SetupSocketBroadcast();
                _mittente = new Mittente("Tu", (_socket.LocalEndPoint as IPEndPoint).Address.ToString(), PORTA);
                NotificaConnessione();
                SetupRicezione();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SetupSocketMittente()
        {
            try
            {
                //Con InterNetwork specifico che comunico ipv4 mentre con Dgram specifico che utilizzo il protocollo udp
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                //Imposto l'indirizzo ip del mittente (colui che invia messaggi)
                IPAddress local_address = IPAddress.Any;
                //La socket del mittente necessita dell'indirizzo ip che posso ricavare dal dispositivo e di una porta
                IPEndPoint local_endpoint = new IPEndPoint(local_address.MapToIPv4(), PORTA);
                //Associo la socket mittente ad un endpoint, tramite questa associazione ho la possibilità di inviare e ricevere dati
                _socket.Bind(local_endpoint);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void SetupSocketBroadcast()
        {
            try
            {
                //Con InterNetwork specifico che comunico ipv4 mentre con Dgram specifico che utilizzo il protocollo udp
                _socketBroadcast = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _socketBroadcast.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                //Imposto l'indirizzo ip del mittente (colui che invia messaggi)
                IPAddress local_address = IPAddress.Any;
                //La socket del mittente necessita dell'indirizzo ip che posso ricavare dal dispositivo e di una porta
                IPEndPoint local_endpoint = new IPEndPoint(local_address.MapToIPv4(), PORTA_BROADCAST);
                //Associo la socket mittente ad un endpoint, tramite questa associazione ho la possibilità di inviare e ricevere dati
                _socketBroadcast.Bind(local_endpoint);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void NotificaConnessione()
        {
            //Endpoint broadcast
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse("255.255.255.255"), PORTA_BROADCAST);
            //Creo il messaggio da inviare in broadcast
            string messaggioString = $"{_nomeUtente}|{(_socket.LocalEndPoint as IPEndPoint).Address}|{PORTA}|";
            //Converto il messaggio da inviare in byte
            byte[] messaggio = Encoding.UTF8.GetBytes(messaggioString);
            //Mando il messaggio al remote endpoint
            _socketBroadcast.SendTo(messaggio, endpoint);
        }
        private void SetupRicezione()
        {
            //Associo il thread al metodo di recezione e lo avvio
            _threadRicezione = new Thread(new ThreadStart(RicezioneMessaggi));
            _threadRicezioneBroadcast = new Thread(new ThreadStart(RicezioneMessaggiBroadcast));
            _threadRicezione.Start();
            _threadRicezioneBroadcast.Start();

            //dTimer = new DispatcherTimer();
            ////Imposto l'evento che devo eseguire ogni tot tempo
            //dTimer.Tick += new EventHandler(aggiornamento_dTimer);
            ////Imposto ogni quanto chiama l'event handler, in questo caso ogni 250 millisecondi
            //dTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);
            ////Avvio il timer che ogni 250 millisecondi interrompe l'invio e guarda se mi è arrivato qualcosa
            //dTimer.Start();
        }
        private void RicezioneMessaggi()
        {
            //Non so chi è il remoteEndPoint quindi imposto i valori di default
            EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                string messaggio = string.Empty;

                // Variabile per contare i byte ricevuti
                int nBytes = 0;

                if ((nBytes = _socket.Available) > 0) // Evita di bloccarsi sulla ReceiveFrom() in assenza di dati
                {
                    byte[] buffer = new byte[nBytes];

                    //Ricezione dei caratteri in attesa
                    nBytes = _socket.ReceiveFrom(buffer, ref remoteEndPoint);

                    // Decodifico ciò che ho ricevuto in stringa
                    messaggio = Encoding.UTF8.GetString(buffer, 0, nBytes);

                    // Recupero il mittente e mi memorizzo il suo indirizzo ip
                    string from = ((IPEndPoint)remoteEndPoint).Address.ToString();
                    // Recupero il mittente e mi memorizzo la sua porta
                    int port = ((IPEndPoint)remoteEndPoint).Port;

                    if (messaggio != string.Empty)
                    {
                        string[] messaggioSplit = messaggio.Split('|');
                        Mittente mit = new Mittente(messaggioSplit[0], from, port);
                        int indice = MittenteRegistrato(mit);
                        if (indice == -1)
                        {
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                _listaMittenti.Add(mit);
                            }));
                        }
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            _listaMittenti[MittenteRegistrato(mit)].ListaMessaggi.Add(new Messaggio(mit, messaggioSplit[1]));
                        }));
                    }
                }
                Thread.Sleep(100);
            }
        }
        private void RicezioneMessaggiBroadcast()
        {
            //Non so chi è il remoteEndPoint quindi imposto i valori di default
            EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                string messaggio = string.Empty;

                // Variabile per contare i byte ricevuti
                int nBytes = 0;

                if ((nBytes = _socketBroadcast.Available) > 0) // Evita di bloccarsi sulla ReceiveFrom() in assenza di dati
                {
                    byte[] buffer = new byte[nBytes];

                    //Ricezione dei caratteri in attesa
                    nBytes = _socketBroadcast.ReceiveFrom(buffer, ref remoteEndPoint);

                    // Decodifico ciò che ho ricevuto in stringa
                    messaggio = Encoding.UTF8.GetString(buffer, 0, nBytes);

                    // Recupero il mittente e mi memorizzo il suo indirizzo ip
                    string from = ((IPEndPoint)remoteEndPoint).Address.ToString();
                    // Recupero il mittente e mi memorizzo la sua porta
                    int port = ((IPEndPoint)remoteEndPoint).Port;

                    string[] messaggioSplit = messaggio.Split('|');
                    int porta;
                    if (int.TryParse(messaggioSplit[2], out porta))
                    {
                        Mittente mit = new Mittente(messaggioSplit[0], messaggioSplit[1], porta);
                        int indice = MittenteRegistrato(mit);
                        if (indice == -1)
                        {
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                _listaMittenti.Add(mit);
                            }));
                        }
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            _listaMittenti[MittenteRegistrato(mit)].ListaMessaggi.Add(new Messaggio(mit, messaggioSplit[3]));
                        }));

                    }
                }
                Thread.Sleep(100);
            }
        }
        private int MittenteRegistrato(Mittente mit)
        {
            for (int i = 0; i < _listaMittenti.Count; i++)
            {
                if (_listaMittenti[i].Equals(mit))
                {
                    return i;
                }
            }
            return -1;
        }

        private void btnInvia_Click(object sender, RoutedEventArgs e)
        {
            if (lstBoxAgenda.SelectedIndex != -1 && !string.IsNullOrWhiteSpace(txtBoxMessaggio.Text))
            {
                //Indirizzo ip del destinatario
                IPAddress remote_address = IPAddress.Parse((lstBoxAgenda.SelectedItem as Mittente).IndirizzoIP);
                //Socket destinatario
                IPEndPoint remote_endpoint = new IPEndPoint(remote_address, (lstBoxAgenda.SelectedItem as Mittente).Porta);
                //Creo il messaggio da inviare al remote endpoint
                string messaggioString = $"{_nomeUtente}|{txtBoxMessaggio.Text}";
                //Converto il messaggio in byte
                byte[] messaggio = Encoding.UTF8.GetBytes(messaggioString);
                //Mando il messaggio al remote endpoint
                _socket.SendTo(messaggio, remote_endpoint);
                _listaMittenti[MittenteRegistrato(lstBoxAgenda.SelectedItem as Mittente)].ListaMessaggi.Add(new Messaggio(_mittente, messaggioString.Split('|')[1]));
            }
            else
            {
                MessageBox.Show("Assicurati di aver selezionato un utente dall'agenda e di aver scritto qualcosa nel campo del messaggio");
            }

            ////Indirizzo ip del destinatario
            //IPAddress remote_address = IPAddress.Parse(txtBoxIndirizzoIP.Text);
            ////Socket destinatario
            //IPEndPoint remote_endpoint = new IPEndPoint(remote_address, int.Parse(txtBoxPorta.Text));
            ////Converto il messaggio in byte
            //byte[] messaggio = Encoding.UTF8.GetBytes(txtBoxMessaggio.Text);
            ////Mando il messaggio al remote endpoint
            //_socket.SendTo(messaggio, remote_endpoint);
        }

        private void lstBoxAgenda_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (lstBoxAgenda.SelectedIndex != -1)
            {
                lstBoxMessaggi.ItemsSource = _listaMittenti[MittenteRegistrato(lstBoxAgenda.SelectedItem as Mittente)].ListaMessaggi;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _threadRicezione.Abort();
            _threadRicezioneBroadcast.Abort();
        }

        //private void aggiornamento_dTimer(object sender, EventArgs e)
        //{
        //    //Variabile per contare i byte ricevuti
        //    int nBytes = 0;

        //    if ((nBytes = socket.Available) > 0)
        //    {
        //        //Ricezione dei caratteri in attesa
        //        byte[] buffer = new byte[nBytes];
        //        //Non so chi è il remoteEndPoint quindi do i valori di default
        //        EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

        //        nBytes = socket.ReceiveFrom(buffer, ref remoteEndPoint);
        //        //Recupero il mittente e mi meorizzo il suo indirizzo ip
        //        string from = ((IPEndPoint)remoteEndPoint).Address.ToString();
        //        //Ciò che ho ricevuto lo trasformo in stringa
        //        string messaggio = Encoding.UTF8.GetString(buffer, 0, nBytes);

        //        lstBoxMessaggi.Items.Add(from + ": " + messaggio);
        //    }
        //}
    }
}
