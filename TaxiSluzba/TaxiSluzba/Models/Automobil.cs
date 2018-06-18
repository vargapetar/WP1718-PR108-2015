using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaxiSluzba.Models
{
    public class Automobil
    {
        public Vozac vozac { get; set; }
        public string godisteAutomobila { get; set; }
        public string registracija { get; set; }
        public string brojTaksija { get; set; }
        public TipAutomobila tipAutomobila { get; set; }

        public Automobil() { }

        public Automobil(Vozac vozac, string godisteAutomobila, string registracija, string brojTaksija, TipAutomobila tipAutomobila)
        {
            this.vozac = vozac;
            this.godisteAutomobila = godisteAutomobila;
            this.registracija = registracija;
            this.brojTaksija = brojTaksija;
            this.tipAutomobila = tipAutomobila;
        }
    }
}