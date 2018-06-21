using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaxiSluzba.Models
{
    public class Komentar
    {
        public string opis { get; set; }
        public DateTime datumVreme { get; set; }
        public Korisnik korisnik { get; set; }
        public Voznja voznja { get; set; }
        public OcenaVoznje ocenaVoznje {get; set;}

        public Komentar() { }

        public Komentar(string opis, DateTime datumVreme, Korisnik korisnik, Voznja voznja, OcenaVoznje ocenaVoznje)
        {
            this.opis = opis;
            this.datumVreme = datumVreme;
            this.korisnik = korisnik;
            this.voznja = voznja;
            this.ocenaVoznje = ocenaVoznje;
        }
    }
}