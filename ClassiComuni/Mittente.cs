using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassiComuni
{
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
