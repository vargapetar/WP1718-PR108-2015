using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaxiSluzba.Models
{
    public class Odgovor
    {
        public string response { get; set; }

        public Odgovor() { }

        public Odgovor(string response)
        {
            this.response = response;
        }
    }
}