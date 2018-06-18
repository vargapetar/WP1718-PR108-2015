using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaxiSluzba.Models
{
    public class Registration
    {
        public string korisnickoIme { get; set; }
        public string lozinka { get; set; }

        public Registration() { }

        public Registration(string korisnickoIme, string lozinka)
        {
            this.korisnickoIme = korisnickoIme;
            this.lozinka = lozinka;
        }
    }
}