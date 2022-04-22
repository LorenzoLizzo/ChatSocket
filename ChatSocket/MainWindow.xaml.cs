using System;
using System.Text;
using System.Windows;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ClassiComuni;
using System.Windows.Controls;
using System.Windows.Media;

namespace ChatSocket
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Mittente _mittente;
        private Thread _threadRicezione;
        private Socket _socket;
        private ObservableCollection<Mittente> _listaMittenti;
        private int PORTA;
        private const int PORTA_BROADCAST = 60000;
        private string _nomeUtente;
        //DispatcherTimer dTimer = null;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void btnEntra_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!txtBoxNomeUtente.Text.Contains("|"))
                {
                    if (int.Parse(txtBoxPorta.Text) >= 49152 && int.Parse(txtBoxPorta.Text) <= 65535)
                    {
                        Setup();
                    }
                    else
                    {
                        MessageBox.Show("Selezionare una tra le porte specificate e assicurarsi che la porta in questione sia libera");
                    }
                }
                else
                {
                    MessageBox.Show(@"Il nome utente non può contenere il carattere : '|'");
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void Setup()
        {
            _nomeUtente = txtBoxNomeUtente.Text;
            PORTA = int.Parse(txtBoxPorta.Text);
            gridPorta.Visibility = Visibility.Collapsed;
            gridChat.Visibility = Visibility.Visible;

            _listaMittenti = new ObservableCollection<Mittente>();
            lstBoxAgenda.ItemsSource = _listaMittenti;

            SetupSocket();
            _mittente = new Mittente("Tu", (_socket.LocalEndPoint as IPEndPoint).Address.ToString(), PORTA);
            NotificaConnessione();
            SetupRicezione();
        }
        private void SetupSocket()
        {
            try
            {
                //Con InterNetwork specifico che comunico ipv4 mentre con Dgram specifico che utilizzo il protocollo udp
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _socket.EnableBroadcast = true;
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
        private void NotificaConnessione()
        {
            //Endpoint broadcast
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Broadcast, PORTA_BROADCAST);
            //Creo il messaggio da inviare in broadcast
            string messaggioString = $"{_nomeUtente}|{OperazioniChatBroadcast.Entra}";
            //Converto il messaggio da inviare in byte
            byte[] messaggio = Encoding.UTF8.GetBytes(messaggioString);
            //Mando il messaggio al remote endpoint
            _socket.SendTo(messaggio, endpoint);
        }
        private void NotificaDisconnessione()
        {
            //Endpoint broadcast
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Broadcast, PORTA_BROADCAST);
            //Creo il messaggio da inviare in broadcast
            string messaggioString = $"{_nomeUtente}|{OperazioniChatBroadcast.Esci}";
            //Converto il messaggio da inviare in byte
            byte[] messaggio = Encoding.UTF8.GetBytes(messaggioString);
            //Mando il messaggio al remote endpoint
            _socket.SendTo(messaggio, endpoint);
        }
        private void SetupRicezione()
        {
            //Associo il thread al metodo di recezione e lo avvio
            _threadRicezione = new Thread(new ThreadStart(RicezioneMessaggi));
            _threadRicezione.Start();

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

                    string[] messaggioSplit = messaggio.Split('|');

                    if(port != PORTA_BROADCAST)
                    {
                        Mittente mit = new Mittente(messaggioSplit[0], from, port);
                        int indice = mit.MittenteRegistrato(_listaMittenti);
                        if (indice == -1)
                        {
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                _listaMittenti.Add(mit);
                            }));
                        }
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            _listaMittenti[mit.MittenteRegistrato(_listaMittenti)].ListaMessaggi.Add(new Messaggio(mit, messaggioSplit[1]));
                            if (lstBoxAgenda.SelectedItem == null || !(lstBoxAgenda.SelectedItem as Mittente).Equals(mit))
                            {
                                _listaMittenti[mit.MittenteRegistrato(_listaMittenti)].MessaggiNonVisualizzati++;
                                lstBoxAgenda.Items.Refresh();
                            }
                        }));
                    }
                    else
                    {
                        Mittente mit = new Mittente(messaggioSplit[0], messaggioSplit[2], int.Parse(messaggioSplit[3]));
                        if (messaggioSplit[1] == OperazioniChatBroadcast.Entra.ToString())
                        {
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                _listaMittenti.Add(mit);
                            }));
                        }
                        else if (messaggioSplit[1] == OperazioniChatBroadcast.InviaMessaggio.ToString())
                        {
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                _listaMittenti[mit.MittenteRegistrato(_listaMittenti)].ListaMessaggi.Add(new Messaggio(mit, messaggioSplit[4]));
                                if (lstBoxAgenda.SelectedItem == null || !(lstBoxAgenda.SelectedItem as Mittente).Equals(mit))
                                {
                                    _listaMittenti[mit.MittenteRegistrato(_listaMittenti)].MessaggiNonVisualizzati++;
                                    lstBoxAgenda.Items.Refresh();
                                }
                            }));
                        }
                        else if (messaggioSplit[1] == OperazioniChatBroadcast.Esci.ToString())
                        {
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                _listaMittenti.Remove(mit);
                            }));
                        }
                    }
                }
                Thread.Sleep(100);
            }
        }

        private void btnInvia_Click(object sender, RoutedEventArgs e)
        {
            if (lstBoxAgenda.SelectedIndex != -1 && !string.IsNullOrWhiteSpace(txtBoxMessaggio.Text))
            {
                if (!txtBoxMessaggio.Text.Contains("|"))
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
                    _listaMittenti[(lstBoxAgenda.SelectedItem as Mittente).MittenteRegistrato(_listaMittenti)].ListaMessaggi.Add(new Messaggio(_mittente, messaggioString.Split('|')[1]));
                    txtBoxMessaggio.Clear();
                }
                else
                {
                    MessageBox.Show(@"Il messaggio non può contenere il carattere : '|'");
                }
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
        private void btnInviaBroadcast_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtBoxMessaggio.Text))
            {
                if (!txtBoxMessaggio.Text.Contains("|"))
                {
                    //Socket destinatario
                    IPEndPoint remote_endpoint = new IPEndPoint(IPAddress.Broadcast, PORTA_BROADCAST);
                    //Creo il messaggio da inviare al remote endpoint
                    string messaggioString = $"{_nomeUtente}|{OperazioniChatBroadcast.InviaMessaggio}|{txtBoxMessaggio.Text}";
                    //Converto il messaggio in byte
                    byte[] messaggio = Encoding.UTF8.GetBytes(messaggioString);
                    //Mando il messaggio al remote endpoint
                    _socket.SendTo(messaggio, remote_endpoint);
                    foreach (Mittente mit in _listaMittenti)
                    {
                        mit.ListaMessaggi.Add(new Messaggio(_mittente, messaggioString.Split('|')[2]));
                    }
                    txtBoxMessaggio.Clear();
                }
                else
                {
                    MessageBox.Show(@"Il messaggio non può contenere il carattere : '|'");
                }
            }
            else
            {
                MessageBox.Show("Assicurati di aver scritto qualcosa nel campo del messaggio");
            }
        }

        private void lstBoxAgenda_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if(lstBoxAgenda.SelectedIndex != -1)
            {
                lstBoxMessaggi.ItemsSource = _listaMittenti[(lstBoxAgenda.SelectedItem as Mittente).MittenteRegistrato(_listaMittenti)].ListaMessaggi;
                _listaMittenti[(lstBoxAgenda.SelectedItem as Mittente).MittenteRegistrato(_listaMittenti)].MessaggiNonVisualizzati = 0;
                lstBoxAgenda.Items.Refresh();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_threadRicezione != null && _threadRicezione.IsAlive)
            {
                _threadRicezione.Abort();
                NotificaDisconnessione();
            }
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
