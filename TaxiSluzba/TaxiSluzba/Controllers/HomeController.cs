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
        public static string ulogovanAdmin;

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
                        Adresa a = new Adresa("NIJE_UNETO", "NIJE_UNETO", "NIJE_UNETO", "NIJE_UNETO");
                        Lokacija l = new Lokacija(1, 1, a);
                        Database.vozaci[user].lokacija = l;
                        Vozac v = Database.vozaci[user];
                        return View("Vozac", v);
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
            Voznja v = new Voznja(DateTime.Now, l, tipAuto, m);
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

        public ActionResult KomentarMusterija(string comment, string datumVoznje, string userKorisnika, string ocena)
        {
            foreach (Voznja v in Database.registrovaniKorisnici[userKorisnika].voznje)
            {
                if (v.datumVreme.ToString() == datumVoznje)
                {
                    if (ocena == null)
                    {
                        Komentar k = new Komentar(comment, DateTime.Now, Database.registrovaniKorisnici[userKorisnika], v, OcenaVoznje.NULA);
                        v.komentar = k;
                    }
                    else
                    {
                        OcenaVoznje rate;
                        if (ocena == "JEDAN")
                            rate = OcenaVoznje.JEDAN;
                        else if (ocena == "JEDAN")
                            rate = OcenaVoznje.DVA;
                        else if (ocena == "JEDAN")
                            rate = OcenaVoznje.TRI;
                        else if (ocena == "JEDAN")
                            rate = OcenaVoznje.CETIRI;
                        else
                            rate = OcenaVoznje.PET;

                        Komentar k = new Komentar(comment, DateTime.Now, Database.registrovaniKorisnici[userKorisnika], v, rate);
                        v.komentar = k;
                    }
                }
            }

            return View("Musterija", Database.registrovaniKorisnici[userKorisnika]);
        }

        public ActionResult KomentarUspesno(string datumVoznje, string korisnickoIme)
        {
            Voznja vo = new Voznja();

            foreach (Voznja v in Database.registrovaniKorisnici[korisnickoIme].voznje)
            {
                if (v.datumVreme.ToString() == datumVoznje)
                {
                    vo = v;
                }
            }

            return View("Komentar", vo);
        }
        #endregion


        #region
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
            Voznja v = new Voznja(DateTime.Now, l, tip, null);

            if (tipPrevoza != Database.slobodniVozaci[izabraniVozac].automobil.tipAutomobila.ToString())
                return View("Greska");

            v.dispecer = (Dispecer)Database.registrovaniKorisnici[ulogovanAdmin];
            v.vozac = Database.slobodniVozaci[izabraniVozac];
            Database.slobodniVozaci.Remove(izabraniVozac);
            v.statusVoznje = StatusVoznje.FORMIRANA;

            Database.vozaci[izabraniVozac].voznje.Add(v);

            return View("Dispecer");
        }

        public ActionResult DodeliVozacaVoznji(string voznja, string vozac)
        {
            Voznja retVoznja = Database.voznjeNaCekanju[voznja];
            retVoznja.statusVoznje = StatusVoznje.OBRADJENA;
            retVoznja.dispecer = (Dispecer)Database.registrovaniKorisnici[ulogovanAdmin];
            Database.registrovaniKorisnici[ulogovanAdmin].voznje.Add(retVoznja);
            Database.voznjeNaCekanju.Remove(voznja);

            Vozac retVozac = Database.slobodniVozaci[vozac];
            Database.slobodniVozaci.Remove(vozac);
            retVozac.voznje.Add(retVoznja);

            retVoznja.vozac = retVozac;

            return View("Dispecer");
        }
        #endregion

        #region
        public ActionResult PocniSaVoznjom(string datumVoznje, string vozac)
        {
            Voznja vo = new Voznja();

            foreach(Voznja v in Database.vozaci[vozac].voznje)
            {
                if(v.datumVreme.ToString() == datumVoznje)
                {
                    v.statusVoznje = StatusVoznje.U_TOKU;
                    vo = v;
                }
            }

            return View("PocniSaVoznjom", vo);
        }

        public ActionResult UspesnaVoznja(string datumVoznje, string vozac, string musterija)
        {
            Voznja vo = new Voznja();

            foreach(Voznja v in Database.registrovaniKorisnici[musterija].voznje)
            {
                if(datumVoznje == v.datumVreme.ToString())
                {
                    v.statusVoznje = StatusVoznje.USPESNA;
                    vo = v;
                }
            }

            foreach (Voznja v in Database.vozaci[vozac].voznje)
            {
                if (datumVoznje == v.datumVreme.ToString())
                {
                    v.statusVoznje = StatusVoznje.USPESNA;
                }
            }

            return View("UspesnaVoznja", vo); //menjaj view na koji saljes!! stavljeno samo radi probe
        }

        public ActionResult NeuspesnaVoznja(string datumVoznje, string musterija)
        {
            Voznja vo = new Voznja();

            foreach(Voznja v in Database.registrovaniKorisnici[musterija].voznje)
            {
                if(datumVoznje == v.datumVreme.ToString())
                {
                    vo = v;
                }
            }

            return View("KomentarVozac", vo);
        }

        public ActionResult KomentarVozac(string comment, string datumVoznje, string userKorisnika)
        {

            Vozac vo = new Vozac();

            foreach(Voznja v in Database.registrovaniKorisnici[userKorisnika].voznje)
            {
                if(datumVoznje == v.datumVreme.ToString())
                {
                    Komentar k = new Komentar(comment, DateTime.Now, v.musterija, v, OcenaVoznje.NULA);
                    v.komentar = k;
                    v.statusVoznje = StatusVoznje.NEUSPESNA;
                    vo = v.vozac;
                }
            }

            return View("Vozac", vo);
        }

        public ActionResult ZavrsiVoznju(string ulica, string broj, string grad, string poBroj, string iznos, string datumVoznje, string musterija, string vozac)
        {
            foreach (Voznja v in Database.registrovaniKorisnici[musterija].voznje)
            {
                if (datumVoznje == v.datumVreme.ToString())
                {
                    Adresa a = new Adresa(ulica, broj, grad, poBroj);
                    Lokacija l = new Lokacija(1, 1, a);
                    v.odrediste = l;
                    v.iznos = iznos;
                    Database.vozaci[vozac].voznje.Add(v);
                    //dodaj voznju vozacu u registrovanim korisnicima!!!!!
                }
            }

            foreach (Voznja v in Database.vozaci[vozac].voznje)
            {
                if (datumVoznje == v.datumVreme.ToString())
                {
                    Adresa a = new Adresa(ulica, broj, grad, poBroj);
                    Lokacija l = new Lokacija(1, 1, a);
                    v.odrediste = l;
                    v.iznos = iznos;
                }
            }

            Database.slobodniVozaci.Add(vozac, Database.vozaci[vozac]);

            return View("Vozac", Database.vozaci[vozac]); // dodaj u musteriji da mu izlista sve uspesne voznje i da moze da ostavi komentar
        }

        public ActionResult LokacijaTaksiste(string vozac, string ulica, string broj, string grad, string poBroj)
        {
            Adresa a = new Adresa(ulica, broj, grad, poBroj);
            Lokacija l = new Lokacija(1, 1, a);
            Database.vozaci[vozac].lokacija = l;

            if (Database.slobodniVozaci.ContainsKey(vozac))
                Database.slobodniVozaci[vozac].lokacija = l;

            return View("Vozac", Database.vozaci[vozac]);
        }

        public ActionResult PreuzimanjeVoznje(string voznja, string vozac)
        {
            Voznja voz = new Voznja();

            foreach(Korisnik k in Database.registrovaniKorisnici.Values)
            {
                if (k.uloga == Uloga.MUSTERIJA)
                {
                    foreach (Voznja vo in k.voznje)
                    {
                        if (vo.datumVreme.ToString() == voznja)
                        {
                            if (vo.statusVoznje == StatusVoznje.KREIRANA_NA_CEKANJU || vo.statusVoznje == StatusVoznje.FORMIRANA)
                            {
                                vo.statusVoznje = StatusVoznje.PRIHVACENA;
                                voz = vo;
                                voz.vozac = Database.vozaci[vozac];
                                Database.voznjeNaCekanju.Remove(vo.datumVreme.ToString());
                            }
                        }
                    }
                }
            }

            Database.slobodniVozaci.Remove(vozac);

            return View("PocniSaVoznjom", voz);
        }
        #endregion
    }
}