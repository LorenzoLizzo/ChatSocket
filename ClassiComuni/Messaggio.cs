using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassiComuni
{
    public class Messaggio
    {
        public Messaggio(Mittente mittente, string messaggio)
        {
            Mittente = mittente;
            MessaggioText = messaggio;
        }

        public Mittente Mittente { get; private set; }
        public string MessaggioText { get; private set; }

        public override string ToString()
        {
            return $"{Mittente.Nominativo}: {MessaggioText}";
        }
    }
}
