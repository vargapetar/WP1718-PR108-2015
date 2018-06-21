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
        public string ulogovanAdmin;
        public string ulogovanaMusterija;

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
                        ulogovanAdmin = user;
                        return View("Dispecer");
                    }
                    else if(Database.registrovaniKorisnici[user].uloga == Uloga.VOZAC)
                    {
                        return View("Vozac");
                    }
                    else
                    {
                        Korisnik m = new Musterija();
                        m = Database.registrovaniKorisnici[user];
                        return View("Musterija", m);
                    }
                }

                return View("Greska"); //napraviti view za pogresan unos lozinke!!!!!
            }
            else
            {
                return View("Greska");
            }
        }

        #region
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
            
            return View("Musterija", Database.registrovaniKorisnici[user]);
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
            Database.voznjeNaCekanju.Add(v.datumVreme.ToString(), v);

            return View("Musterija", (Korisnik)m);
        }

        public ActionResult MusterijaMenjaVoznju(string korisnickoIme, string datumVoznje)
        {
            Voznja vo = new Voznja();

            foreach(Voznja v in Database.registrovaniKorisnici[korisnickoIme].voznje)
            {
                if(v.datumVreme.ToString() == datumVoznje)
                {
                    vo = v;
                }
            }
            
            return View("MusterijaMenjaVoznju", vo);
        }

        public ActionResult IzmenaVoznje(string ulica, string broj, string grad, string poBroj, string tipAuto, string userKorisnika, string datumVoznje)
        {
            TipAutomobila tip;
            if (tipAuto == "KOMBI")
                tip = TipAutomobila.KOMBI;
            else
                tip = TipAutomobila.PUTNICKI_AUTOMOBIL;

            foreach(Voznja v in Database.registrovaniKorisnici[userKorisnika].voznje)
            {
                if(v.datumVreme.ToString() == datumVoznje)
                {
                    v.lokacijaZaTaksi.Adress.Ulica = ulica;
                    v.lokacijaZaTaksi.Adress.BrojUlice = broj;
                    v.lokacijaZaTaksi.Adress.Grad = grad;
                    v.lokacijaZaTaksi.Adress.PozivniBroj = poBroj;
                    v.tipAutomobila = tip;
                }
            }
            return View("Musterija", Database.registrovaniKorisnici[userKorisnika]);
        }

        public ActionResult MusterijaOtkazujeVoznju(string korisnickoIme, string datumVoznje)
        {
            Voznja vo = new Voznja();

            foreach (Voznja v in Database.registrovaniKorisnici[korisnickoIme].voznje)
            {
                if (v.datumVreme.ToString() == datumVoznje)
                {
                    v.statusVoznje = StatusVoznje.OTKAZANA;
                    vo = v;
                }
            }

            return View("Komentar", vo);
        }

        public ActionResult KomentarMusterija(string comment, string datumVoznje, string userKorisnika)
        {
            foreach (Voznja v in Database.registrovaniKorisnici[userKorisnika].voznje)
            {
                if (v.datumVreme.ToString() == datumVoznje)
                {
                    Komentar k = new Komentar(comment, DateTime.Now, Database.registrovaniKorisnici[userKorisnika], v, OcenaVoznje.NULA);
                    v.komentar = k;
                }
            }

            return View("Musterija", Database.registrovaniKorisnici[userKorisnika]);
        }
        #endregion


        public ActionResult KreirajVozaca(string user, string pass, string ime, string prezime, string pol, string jmbg, string telefon, string email, string godisteAuto, string regAuto, string brAuto, string tipAuto)
        {
            Pol gender;
            if (pol == "MUSKI")
                gender = Pol.MUSKI;
            else
                gender = Pol.ZENSKI;

            Korisnik k = new Korisnik(user, pass, ime, prezime, gender, jmbg, telefon, email);
            k.uloga = Uloga.VOZAC;
            TipAutomobila tip;
            if (tipAuto == "KOMBI")
                tip = TipAutomobila.KOMBI;
            else
                tip = TipAutomobila.PUTNICKI_AUTOMOBIL;
            Automobil a = new Automobil(null, godisteAuto, regAuto, brAuto, tip);
            Vozac v = new Vozac(k, null, a);
            a.vozac = v;

            Database.vozaci.Add(v.korisnickoIme, v);
            Database.registrovaniKorisnici.Add(k.korisnickoIme, k);
            Database.slobodniVozaci.Add(v.korisnickoIme, v);

            return View("Dispecer");
        }

        public ActionResult KreirajVoznju(string ulica, string broj, string grad, string poBroj, string tipPrevoza, string izabraniVozac)
        {
            Adresa a = new Adresa(ulica, broj, grad, poBroj);
            Lokacija l = new Lokacija(1, 1, a);
            TipAutomobila tip;
            if (tipPrevoza == "KOMBI")
                tip = TipAutomobila.KOMBI;
            else
                tip = TipAutomobila.PUTNICKI_AUTOMOBIL;
            Voznja v = new Voznja(DateTime.Now.ToString(), l, tip, null);

            v.dispecer = (Dispecer)Database.registrovaniKorisnici[ulogovanAdmin];
            v.vozac = Database.slobodniVozaci[izabraniVozac];
            Database.slobodniVozaci.Remove(izabraniVozac);
            v.statusVoznje = StatusVoznje.FORMIRANA;

            Database.vozaci[izabraniVozac].voznje.Add(v);

            return View("Dispecer");
        }

        public ActionResult DodeliVozacaVoznji(string musterija, string vozac)
        {
            //dodeli voznji status obradjena

            return View("Dispecer");
        }


    }
}