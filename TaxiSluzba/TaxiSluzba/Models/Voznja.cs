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

        public Voznja(string datumVreme, Lokacija lokacijaZaTaksi, TipAutomobila tipAutomobila, Musterija musterija, Lokacija odrediste, Dispecer dispecer, Vozac vozac, string iznos, string komentar, StatusVoznje statusVoznje)
        {
            this.datumVreme = datumVreme;
            this.lokacijaZaTaksi = lokacijaZaTaksi;
            this.tipAutomobila = tipAutomobila;
            this.musterija = musterija;
            this.odrediste = odrediste;
            this.dispecer = dispecer;
            this.vozac = vozac;
            this.iznos = iznos;
            this.komentar = komentar;
            this.statusVoznje = statusVoznje;
        }
    }
}