using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaxiSluzba.Models
{
    public class Korisnik
    {
        public string korisnickoIme { get; set; }
        public string lozinka { get; set; }
        public string ime { get; set; }
        public string prezime { get; set; }
        public Pol pol { get; set; }
        public string jmbg { get; set; }
        public string telefon { get; set; }
        public string email { get; set; }
        public Uloga uloga { get; set; }
        public string tipVoznje { get; set; }
        public List<Voznja> voznje { get; set; }

        public Korisnik() { voznje = new List<Voznja>(); }

        public Korisnik(string korisnickoIme, string lozinka, string ime, string prezime, Pol pol, string jmbg, string telefon, string email)
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
            //this.tipVoznje = tipVoznje;
            this.voznje = new List<Voznja>();
        }
    }
}