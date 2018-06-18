using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaxiSluzba.Models
{
    public class Vozac : Korisnik
    {
        public Lokacija lokacija { get; set; }
        public Automobil automobil { get; set; }

        public Vozac() { }

        public Vozac(Lokacija lokacija, Automobil automobil)
        {
            this.lokacija = lokacija;
            this.automobil = automobil;
        }
    }
}