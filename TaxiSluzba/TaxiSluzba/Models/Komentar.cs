using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaxiSluzba.Models
{
    public class Komentar
    {
        public string opis { get; set; }
        public string datumVreme { get; set; }
        public string korisnik { get; set; }
        public string voznja { get; set; }
        public string ocenaVoznje {get; set;}

        public Komentar() { }

        public Komentar(string opis, string datumVreme, string korisnik, string voznja, string ocenaVoznje)
        {
            this.opis = opis;
            this.datumVreme = datumVreme;
            this.korisnik = korisnik;
            this.voznja = voznja;
            this.ocenaVoznje = ocenaVoznje;
        }
    }
}