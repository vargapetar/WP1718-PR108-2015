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

        public Vozac() { voznje = new List<Voznja>(); }

        public Vozac(Korisnik k, Lokacija lokacija, Automobil automobil)
        {
            this.korisnickoIme = k.korisnickoIme;
            this.lozinka = k.lozinka;
            this.ime = k.ime;
            this.prezime = k.prezime;
            this.pol = k.pol;
            this.jmbg = k.jmbg;
            this.telefon = k.telefon;
            this.email = k.email;
            this.uloga = Uloga.VOZAC;
            this.lokacija = lokacija;
            this.automobil = automobil;
            voznje = new List<Voznja>();
        }
    }
}