using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ClassiComuni;

namespace BroadcastSocket
{
    class Program
    {
        private static ObservableCollection<Mittente> _listaMittenti;
        private static Socket _socketBroadcast;
        private const int PORTA_BROADCAST = 60000;
        static void Main(string[] args)
        {
            _listaMittenti = new ObservableCollection<Mittente>();
            SetupSocketBroadcast();
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
                            //this.Dispatcher.BeginInvoke(new Action(() =>
                            //{
                            //    _listaMittenti.Add(mit);
                            //}));
                            _listaMittenti.Add(mit);
                        }
                        //this.Dispatcher.BeginInvoke(new Action(() =>
                        //{
                        //    _listaMittenti[MittenteRegistrato(mit)].ListaMessaggi.Add(new Messaggio(mit, messaggioSplit[3]));
                        //}));
                        _listaMittenti[MittenteRegistrato(mit)].ListaMessaggi.Add(new Messaggio(mit, messaggioSplit[3]));
                    }
                }
                //Thread.Sleep(100);
            }
        }
    }
}
