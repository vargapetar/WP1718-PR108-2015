using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TaxiSluzba.Models;

namespace TaxiSluzba.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult HomePage()
        {
            return View();
        }

        public ActionResult Registracija(string user, string pass, string ime, string prezime, string pol, string jmbg, string telefon, string email)
        {
            Pol gender;
            //string pom = "";

            if (pol == "ZENSKI")
                gender = Pol.ZENSKI;
            else
                gender = Pol.MUSKI;

            if(!Database.registrovaniKorisnici.ContainsKey(user))
            {
                Korisnik k = new Korisnik(user, pass, ime, prezime, gender, jmbg, telefon, email, Uloga.MUSTERIJA);
                Database.registrovaniKorisnici.Add(user, k);
                Database.UpisiUTXT(new Registration(user, pass));
                return View("UspesnaRegistracija");
            }
            else
            {
                return View("Greska");
            }
        }

        public ActionResult Logovanje(string user, string pass)
        {
            //za dispecera procitam iz njegovog fajla i ako postoji onda mu otvaram view za dispecere..
            //ako je korisnik u pitanju onda na osnovu korisnickog imena cu ga naci u recniku i ako postoji otvoriti mu njegov view(za vozaca isto tako, njega dodaje dispecer)
            return View();
        }
    }
}