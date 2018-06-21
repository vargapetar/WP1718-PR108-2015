using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaxiSluzba.Models
{
    public class Musterija : Korisnik
    {
        public Musterija() { voznje = new List<Voznja>(); }

        public Musterija(string korisnickoIme, string lozinka, string ime, string prezime, Pol pol, string jmbg, string telefon, string email)
        {
            this.korisnickoIme = korisnickoIme;
            this.lozinka = lozinka;
            this.ime = ime;
            this.prezime = prezime;
            this.pol = pol;
            this.jmbg = jmbg;
            this.telefon = telefon;
            this.email = email;
            this.uloga = Uloga.MUSTERIJA;
            voznje = new List<Voznja>();
        }
    }
}