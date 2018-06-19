using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaxiSluzba.Models
{
    public class Voznja
    {
        public string datumVreme { get; set; }
        public Lokacija lokacijaZaTaksi { get; set; }
        public TipAutomobila tipAutomobila { get; set; }
        public Musterija musterija { get; set; }
        public Lokacija odrediste { get; set; }
        public Dispecer dispecer { get; set; }
        public Vozac vozac { get; set; }
        public string iznos { get; set; }
        public string komentar { get; set; }
        public StatusVoznje statusVoznje { get; set; }

        public Voznja() { }

        public Voznja(string datumVreme, Lokacija lokacijaZaTaksi, TipAutomobila tipAutomobila, Musterija musterija)
        {
            this.datumVreme = datumVreme;
            this.lokacijaZaTaksi = lokacijaZaTaksi;
            this.tipAutomobila = tipAutomobila;
            this.musterija = musterija;
        }
    }
}