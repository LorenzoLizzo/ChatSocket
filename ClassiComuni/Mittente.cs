using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassiComuni
{
    public enum OperazioniChatBroadcast { Entra, InviaMessaggio, Esci }
    public class Mittente : IEquatable<Mittente>
    {
        public Mittente(string nominativo, string indirizzoIP, int porta)
        {
            Nominativo = nominativo;
            IndirizzoIP = indirizzoIP;
            Porta = porta;
            ListaMessaggi = new ObservableCollection<Messaggio>();
        }

        public string Nominativo { get; private set; }
        public string IndirizzoIP { get; private set; }
        public int Porta { get; private set; }
        public ObservableCollection<Messaggio> ListaMessaggi { get; private set; }
        public int MittenteRegistrato(ObservableCollection<Mittente> listaMittenti)
        {
            for (int i = 0; i < listaMittenti.Count; i++)
            {
                if (listaMittenti[i].Equals(this))
                {
                    return i;
                }
            }
            return -1;
        }
        public override string ToString()
        {
            return Nominativo;
        }
        public bool Equals(Mittente other)
        {
            if(this.Nominativo == other.Nominativo && this.IndirizzoIP == other.IndirizzoIP && this.Porta == other.Porta)
            {
                return true;
            }
            return false;
        }
    }
}
