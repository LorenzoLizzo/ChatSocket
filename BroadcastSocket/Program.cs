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
            RicezioneMessaggiBroadcast();
        }

        private static void SetupSocketBroadcast()
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

        private static void RicezioneMessaggiBroadcast()
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
                    Mittente mit = new Mittente(messaggioSplit[0], from, port);
                    if (messaggioSplit[1] == OperazioniChatBroadcast.Entra.ToString())
                    {
                        Invia(_listaMittenti.AsEnumerable(), messaggio, from, port);
                        _listaMittenti.Add(mit);
                    }
                    else if (messaggioSplit[1] == OperazioniChatBroadcast.InviaMessaggio.ToString())
                    {
                        Invia(_listaMittenti.Where(x => !x.Equals(mit)), messaggio, from, port, messaggioSplit[2]);
                    }
                    else if (messaggioSplit[1] == OperazioniChatBroadcast.Esci.ToString())
                    {
                        _listaMittenti.Remove(mit);
                        Invia(_listaMittenti, messaggio, from, port);
                    }
                }
                //Thread.Sleep(100);
            }
        }

        private static void Invia(IEnumerable<Mittente> lista, string messaggio, string from, int port, string messaggioBroadcast = "")
        {
            foreach (Mittente item in lista)
            {
                //Indirizzo ip del destinatario
                IPAddress remote_address = IPAddress.Parse(item.IndirizzoIP);
                //Socket destinatario
                IPEndPoint remote_endpoint = new IPEndPoint(remote_address, item.Porta);
                //Creo il messaggio da inviare al remote endpoint
                string messaggioString = $"{messaggio}|{from}|{port}|{messaggioBroadcast}";
                //Converto il messaggio in byte
                byte[] msg = Encoding.UTF8.GetBytes(messaggioString);
                //Mando il messaggio al remote endpoint
                _socketBroadcast.SendTo(msg, remote_endpoint);
            }
        }
    }
}
