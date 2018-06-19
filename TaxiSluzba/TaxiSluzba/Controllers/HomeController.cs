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
                Korisnik k = new Korisnik(user, pass, ime, prezime, gender, jmbg, telefon, email);
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
            if(Database.registrovaniKorisnici.ContainsKey(user))
            {
                if (Database.registrovaniKorisnici[user].lozinka == pass)
                {
                    if (Database.registrovaniKorisnici[user].uloga == Uloga.DISPECER)
                    {
                        return View("Dispecer");
                    }
                    else
                    {
                        HttpContext.Cache["ulogovan"] = user;
                        return View("Musterija");
                    }
                }

                return View("Greska"); //napraviti view za pogresan unos lozinke!!!!!
            }
            else
            {
                return View("Greska");
            }
        }

        //izmena podataka kod musterije
        public ActionResult IzmeniPodatke(string user, string pass, string ime, string prezime, string pol, string jmbg, string telefon, string email)
        {
            Database.registrovaniKorisnici[user].lozinka = pass;
            Database.registrovaniKorisnici[user].ime = ime;
            Database.registrovaniKorisnici[user].prezime = prezime;

            Pol gender;
            if (pol == "MUSKI")
                gender = Pol.MUSKI;
            else
                gender = Pol.ZENSKI;

            Database.registrovaniKorisnici[user].pol = gender;
            Database.registrovaniKorisnici[user].jmbg = jmbg;
            Database.registrovaniKorisnici[user].telefon = telefon;
            Database.registrovaniKorisnici[user].email = email;
            
            return View("Musterija");
        }

        public ActionResult PoruciVoznju(string korisnickoIme, string ulica, string broj, string grad, string poBroj, string tipPrevoza)
        {
            Adresa a = new Adresa(ulica, broj, grad, poBroj);
            Lokacija l = new Lokacija(1, 1, a);
            TipAutomobila tipAuto;
            if (tipPrevoza == "KOMBI")
                tipAuto = TipAutomobila.KOMBI;
            else
                tipAuto = TipAutomobila.PUTNICKI_AUTOMOBIL;

            Musterija m = new Musterija(Database.registrovaniKorisnici[korisnickoIme].korisnickoIme, Database.registrovaniKorisnici[korisnickoIme].lozinka, Database.registrovaniKorisnici[korisnickoIme].ime, Database.registrovaniKorisnici[korisnickoIme].prezime, Database.registrovaniKorisnici[korisnickoIme].pol, Database.registrovaniKorisnici[korisnickoIme].jmbg, Database.registrovaniKorisnici[korisnickoIme].telefon, Database.registrovaniKorisnici[korisnickoIme].email);
            Voznja v = new Voznja(DateTime.Now.ToString(), l, tipAuto, m);
            v.statusVoznje = StatusVoznje.KREIRANA_NA_CEKANJU;
            Database.registrovaniKorisnici[korisnickoIme].voznje.Add(v);

            return View("Musterija"); // dodaj view za uspesno porucenu voznju
        }
    }
}