using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaxiSluzba.Models
{
    public class Adresa
    {
        private string ulica;
        private string brojUlice;
        private string grad;
        private string pozivniBroj;

        public Adresa() { }

        public Adresa(string ulica, string brojUlice, string grad, string pozivniBroj)
        {
            this.ulica = ulica;
            this.brojUlice = brojUlice;
            this.grad = grad;
            this.pozivniBroj = pozivniBroj;
        }

        public string Ulica
        {
            get { return ulica; }
            set { ulica = value; }
        }

        public string BrojUlice
        {
            get { return brojUlice; }
            set { brojUlice = value; }
        }

        public string Grad
        {
            get { return grad; }
            set { grad = value; }
        }

        public string PozivniBroj
        {
            get { return pozivniBroj; }
            set { pozivniBroj = value; }
        }
    }
}