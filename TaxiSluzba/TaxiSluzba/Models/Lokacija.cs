using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaxiSluzba.Models
{
    public class Lokacija
    {
        private double xKordinata;
        private double yKordinata;
        private Adresa adress;

        public Lokacija() { }

        public Lokacija(double xKordinata, double yKordinata, Adresa adresa)
        {
            this.xKordinata = xKordinata;
            this.yKordinata = yKordinata;
            this.adress = adresa;
        }

        public double XKordinata
        {
            get { return xKordinata; }
            set { xKordinata = value; }
        }
        public double YKordinata
        {
            get { return yKordinata; }
            set { yKordinata = value; }
        }
        public Adresa Adress
        {
            get { return adress; }
            set { adress = value; }
        }
    }
}