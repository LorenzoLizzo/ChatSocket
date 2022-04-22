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
                //Attivo il broadcast
                _socketBroadcast.EnableBroadcast = true;
                //Imposto l'indirizzo ip del mittente (colui che invia messaggi)
                IPAddress local_address = IPAddress.Any;
                //La socket del mittente necessita dell'indirizzo ip che posso ricavare dal dispositivo e di una porta
                IPEndPoint local_endpoint = new IPEndPoint(local_address.MapToIPv4(), PORTA_BROADCAST);
                //Associo la socket mittente ad un endpoint, tramite questa associazione ho la possibilità di inviare e ricevere dati
                _socketBroadcast.Bind(local_endpoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void RicezioneMessaggiBroadcast()
        {
            try
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
                        //Divido il messaggio 
                        string[] messaggioSplit = messaggio.Split('|');
                        Mittente mit = new Mittente(messaggioSplit[0], from, port);
                        if (messaggioSplit[1] == OperazioniChatBroadcast.Entra.ToString())
                        {
                            Invia(_listaMittenti.AsEnumerable(), messaggioSplit[0], OperazioniChatBroadcast.Entra, from, port);
                            GetUtentiConnessi(_listaMittenti.AsEnumerable(), from, port);

                            _listaMittenti.Add(mit);
                        }
                        else if (messaggioSplit[1] == OperazioniChatBroadcast.InviaMessaggio.ToString())
                        {
                            Invia(_listaMittenti.Where(x => !x.Equals(mit)), messaggioSplit[0], OperazioniChatBroadcast.InviaMessaggio, from, port, messaggioSplit[2]);
                        }
                        else if (messaggioSplit[1] == OperazioniChatBroadcast.Esci.ToString())
                        {
                            _listaMittenti.Remove(mit);
                            Invia(_listaMittenti, messaggioSplit[0], OperazioniChatBroadcast.Esci, from, port);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Console.Write(ex.Message);
            }  
        }

        private static void Invia(IEnumerable<Mittente> lista, string nominativo, OperazioniChatBroadcast operazione, string from, int port, string messaggioBroadcast = "")
        {
            try
            {
                foreach (Mittente item in lista)
                {
                    //Indirizzo ip del destinatario
                    IPAddress remote_address = IPAddress.Parse(item.IndirizzoIP);
                    //Socket destinatario
                    IPEndPoint remote_endpoint = new IPEndPoint(remote_address, item.Porta);
                    //Creo il messaggio da inviare al remote endpoint
                    string messaggioString = $"{nominativo}|{operazione}|{from}|{port}|{messaggioBroadcast}";
                    //Converto il messaggio in byte
                    byte[] msg = Encoding.UTF8.GetBytes(messaggioString);
                    //Mando il messaggio al remote endpoint
                    _socketBroadcast.SendTo(msg, remote_endpoint);
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }

        private static void GetUtentiConnessi(IEnumerable<Mittente> lista, string from, int port)
        {
            try
            {
                //Indirizzo ip del destinatario
                IPAddress remote_address = IPAddress.Parse(from);
                //Socket destinatario
                IPEndPoint remote_endpoint = new IPEndPoint(remote_address, port);
                foreach (Mittente item in lista)
                {
                    //Creo il messaggio da inviare al remote endpoint
                    string messaggioString = $"{item.Nominativo}|{OperazioniChatBroadcast.Entra}|{item.IndirizzoIP}|{item.Porta}";
                    //Converto il messaggio in byte
                    byte[] msg = Encoding.UTF8.GetBytes(messaggioString);
                    //Mando il messaggio al remote endpoint
                    _socketBroadcast.SendTo(msg, remote_endpoint);
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }
    }
}
