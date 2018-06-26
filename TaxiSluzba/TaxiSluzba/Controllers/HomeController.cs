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

        [HttpPost]
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
            Korisnik k = (Korisnik)Session["korisnikUser"];

            if(k == null)
            {
                k = new Korisnik();
                Session["korisnikUser"] = k;
            }



            //za dispecera procitam iz njegovog fajla i ako postoji onda mu otvaram view za dispecere..
            //ako je korisnik u pitanju onda na osnovu korisnickog imena cu ga naci u recniku i ako postoji otvoriti mu njegov view(za vozaca isto tako, njega dodaje dispecer)
            if(Database.registrovaniKorisnici.ContainsKey(user))
            {
                if (Database.registrovaniKorisnici[user].lozinka == pass)
                {
                    if (Database.registrovaniKorisnici[user].uloga == Uloga.DISPECER)
                    {
                        Session["korisnikUser"] = Database.registrovaniKorisnici[user];
                        ulogovanAdmin = user;
                        return View("Dispecer");
                    }
                    else if(Database.registrovaniKorisnici[user].uloga == Uloga.VOZAC)
                    {
                        Adresa a = new Adresa("NIJE_UNETO", "NIJE_UNETO", "NIJE_UNETO", "NIJE_UNETO");
                        Lokacija l = new Lokacija(1, 1, a);
                        Database.vozaci[user].lokacija = l;
                        Vozac v = Database.vozaci[user];
                        Session["korisnikUser"] = Database.registrovaniKorisnici[user];
                        return View("Vozac", v);
                    }
                    else
                    {
                        Korisnik m = new Musterija();
                        m = Database.registrovaniKorisnici[user];
                        Session["korisnikUser"] = Database.registrovaniKorisnici[user];
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
            Korisnik k = (Korisnik)Session["korisnikUser"];

            if (k == null)
            {
                k = new Korisnik();
                Session["korisnikUser"] = k;
            }

            foreach (Korisnik kor in Database.registrovaniKorisnici.Values)
            {
                if (kor.korisnickoIme == k.korisnickoIme && kor.uloga == Uloga.MUSTERIJA)
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

            }

            return View("Greska"); //smisli gresku
        }

        public ActionResult PoruciVoznju(string korisnickoIme, string ulica, string broj, string grad, string poBroj, string tipPrevoza)
        {
            Korisnik k = (Korisnik)Session["korisnikUser"];

            if (k == null)
            {
                k = new Korisnik();
                Session["korisnikUser"] = k;
            }

            foreach (Korisnik kor in Database.registrovaniKorisnici.Values)
            {
                if (kor.korisnickoIme == k.korisnickoIme && kor.uloga == Uloga.MUSTERIJA)
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
                    v.komentar = new Komentar("-", DateTime.Now, Database.registrovaniKorisnici[korisnickoIme], v, OcenaVoznje.NULA);
                    //
                    AzurirajVoznju(v);
                    Database.registrovaniKorisnici[korisnickoIme].voznje.Add(v);
                    Database.voznjeNaCekanju.Add(v.datumVreme.ToString(), v);

                    return View("Musterija", (Korisnik)m);
                }

            }

            return View("Greska");
        }

        public ActionResult MusterijaMenjaVoznju(string korisnickoIme, string datumVoznje)
        {
            Korisnik k = (Korisnik)Session["korisnikUser"];

            if (k == null)
            {
                k = new Korisnik();
                Session["korisnikUser"] = k;
            }

            foreach (Korisnik kor in Database.registrovaniKorisnici.Values)
            {
                if (kor.korisnickoIme == k.korisnickoIme && kor.uloga == Uloga.MUSTERIJA)
                {
                    Voznja vo = new Voznja();

                    foreach (Voznja v in Database.registrovaniKorisnici[korisnickoIme].voznje)
                    {
                        if (v.datumVreme.ToString() == datumVoznje)
                        {
                            vo = v;
                            AzurirajVoznju(vo);
                        }
                    }

                    return View("MusterijaMenjaVoznju", vo);
                }

            }

            return View("Greska");
        }

        public ActionResult IzmenaVoznje(string ulica, string broj, string grad, string poBroj, string tipAuto, string userKorisnika, string datumVoznje)
        {
            Korisnik k = (Korisnik)Session["korisnikUser"];

            if (k == null)
            {
                k = new Korisnik();
                Session["korisnikUser"] = k;
            }

            foreach (Korisnik kor in Database.registrovaniKorisnici.Values)
            {
                if (kor.korisnickoIme == k.korisnickoIme && kor.uloga == Uloga.MUSTERIJA)
                {
                    TipAutomobila tip;
                    if (tipAuto == "KOMBI")
                        tip = TipAutomobila.KOMBI;
                    else
                        tip = TipAutomobila.PUTNICKI_AUTOMOBIL;

                    foreach (Voznja v in Database.registrovaniKorisnici[userKorisnika].voznje)
                    {
                        if (v.datumVreme.ToString() == datumVoznje)
                        {
                            v.lokacijaZaTaksi.Adress.Ulica = ulica;
                            v.lokacijaZaTaksi.Adress.BrojUlice = broj;
                            v.lokacijaZaTaksi.Adress.Grad = grad;
                            v.lokacijaZaTaksi.Adress.PozivniBroj = poBroj;
                            v.tipAutomobila = tip;
                            AzurirajVoznju(v);
                        }
                    }
                    return View("Musterija", Database.registrovaniKorisnici[userKorisnika]);
                }

            }

            return View("Greska");
        }

        public ActionResult MusterijaOtkazujeVoznju(string korisnickoIme, string datumVoznje)
        {
            Korisnik k = (Korisnik)Session["korisnikUser"];

            if (k == null)
            {
                k = new Korisnik();
                Session["korisnikUser"] = k;
            }

            foreach (Korisnik kor in Database.registrovaniKorisnici.Values)
            {
                if (kor.korisnickoIme == k.korisnickoIme && kor.uloga == Uloga.MUSTERIJA)
                {
                    Voznja vo = new Voznja();

                    foreach (Voznja v in Database.registrovaniKorisnici[korisnickoIme].voznje)
                    {
                        if (v.datumVreme.ToString() == datumVoznje)
                        {
                            v.statusVoznje = StatusVoznje.OTKAZANA;
                            vo = v;
                            AzurirajVoznju(v);
                        }
                    }

                    return View("Komentar", vo);
                }

            }

            return View("Greska");
        }

        public ActionResult KomentarMusterija(string comment, string datumVoznje, string userKorisnika, string ocena)
        {
            Korisnik k123 = (Korisnik)Session["korisnikUser"];

            if (k123 == null)
            {
                k123 = new Korisnik();
                Session["korisnikUser"] = k123;
            }

            foreach (Korisnik kor in Database.registrovaniKorisnici.Values)
            {
                if (kor.korisnickoIme == k123.korisnickoIme && kor.uloga == Uloga.MUSTERIJA)
                {
                    foreach (Voznja v in Database.registrovaniKorisnici[userKorisnika].voznje)
                    {
                        if (v.datumVreme.ToString() == datumVoznje)
                        {
                            if (ocena == null)
                            {
                                Komentar k = new Komentar(comment, DateTime.Now, Database.registrovaniKorisnici[userKorisnika], v, OcenaVoznje.NULA);
                                v.komentar = k;
                                AzurirajVoznju(v);
                            }
                            else
                            {
                                OcenaVoznje rate;
                                if (ocena == "JEDAN")
                                    rate = OcenaVoznje.JEDAN;
                                else if (ocena == "DVA")
                                    rate = OcenaVoznje.DVA;
                                else if (ocena == "TRI")
                                    rate = OcenaVoznje.TRI;
                                else if (ocena == "CETIRI")
                                    rate = OcenaVoznje.CETIRI;
                                else
                                    rate = OcenaVoznje.PET;

                                Komentar k = new Komentar(comment, DateTime.Now, Database.registrovaniKorisnici[userKorisnika], v, rate);
                                v.komentar = k;
                                AzurirajVoznju(v);
                            }
                        }
                    }

                    return View("Musterija", Database.registrovaniKorisnici[userKorisnika]);
                }

            }

            return View("Greska");
        }

        public ActionResult KomentarUspesno(string datumVoznje, string korisnickoIme)
        {
            Korisnik k123 = (Korisnik)Session["korisnikUser"];

            if (k123 == null)
            {
                k123 = new Korisnik();
                Session["korisnikUser"] = k123;
            }

            foreach (Korisnik kor in Database.registrovaniKorisnici.Values)
            {
                if (kor.korisnickoIme == k123.korisnickoIme && kor.uloga == Uloga.MUSTERIJA)
                {
                    Voznja vo = new Voznja();

                    foreach (Voznja v in Database.registrovaniKorisnici[korisnickoIme].voznje)
                    {
                        if (v.datumVreme.ToString() == datumVoznje)
                        {
                            vo = v;
                            AzurirajVoznju(vo);
                        }
                    }

                    return View("Komentar", vo);
                }

            }

            return View("Greska");
        }
        #endregion

        #region
        public ActionResult KreirajVozaca(string user, string pass, string ime, string prezime, string pol, string jmbg, string telefon, string email, string godisteAuto, string regAuto, string brAuto, string tipAuto)
        {
            Korisnik k123 = (Korisnik)Session["korisnikUser"];

            if (k123 == null)
            {
                k123 = new Korisnik();
                Session["korisnikUser"] = k123;
            }

            foreach (Korisnik kor in Database.registrovaniKorisnici.Values)
            {
                if (kor.korisnickoIme == k123.korisnickoIme && kor.uloga == Uloga.DISPECER)
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

            }

            return View("Greska");
        }

        public ActionResult KreirajVoznju(string ulica, string broj, string grad, string poBroj, string tipPrevoza, string izabraniVozac)
        {
            Korisnik k123 = (Korisnik)Session["korisnikUser"];

            if (k123 == null)
            {
                k123 = new Korisnik();
                Session["korisnikUser"] = k123;
            }

            foreach (Korisnik kor in Database.registrovaniKorisnici.Values)
            {
                if (kor.korisnickoIme == k123.korisnickoIme && kor.uloga == Uloga.DISPECER)
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
                    //
                    v.dispecer.voznje.Add(v);
                    v.vozac = Database.slobodniVozaci[izabraniVozac];
                    Database.slobodniVozaci.Remove(izabraniVozac);
                    v.statusVoznje = StatusVoznje.FORMIRANA;
                    v.musterija = new Musterija("-", "-", "-", "-", Pol.MUSKI, "-", "-", "-");
                    //
                    AzurirajVoznju(v);
                    Database.vozaci[izabraniVozac].voznje.Add(v);

                    return View("Dispecer");
                }

            }

            return View("Greska");
        }

        public ActionResult DodeliVozacaVoznji(string voznja, string vozac)
        {
            Korisnik k123 = (Korisnik)Session["korisnikUser"];

            if (k123 == null)
            {
                k123 = new Korisnik();
                Session["korisnikUser"] = k123;
            }

            foreach (Korisnik kor in Database.registrovaniKorisnici.Values)
            {
                if (kor.korisnickoIme == k123.korisnickoIme && kor.uloga == Uloga.DISPECER)
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
                    //
                    AzurirajVoznju(retVoznja);

                    return View("Dispecer");
                }

            }

            return View("Greska");
        }

        public ActionResult DetaljiVoznje(string datumVoznje, string vozac)
        {
            Korisnik k123 = (Korisnik)Session["korisnikUser"];

            if (k123 == null)
            {
                k123 = new Korisnik();
                Session["korisnikUser"] = k123;
            }

            foreach (Korisnik kor in Database.registrovaniKorisnici.Values)
            {
                if (kor.korisnickoIme == k123.korisnickoIme && kor.uloga == Uloga.DISPECER)
                {
                    Voznja vo = new Voznja();
                    Korisnik k = new Korisnik("-", "-", "-", "-", Pol.MUSKI, "-", "-", "-");
                    Musterija m = new Musterija("-", "-", "-", "-", Pol.MUSKI, "-", "-", "-");
                    Adresa a = new Adresa("-", "-", "-", "-");
                    Lokacija l = new Lokacija(1, 1, a);

                    foreach (Voznja v in Database.vozaci[vozac].voznje)
                    {
                        if (v.datumVreme.ToString() == datumVoznje)
                        {
                            vo = v;
                        }
                    }

                    if (vo.dispecer == null)
                        vo.dispecer = new Dispecer("-", "-", "-", "-", Pol.MUSKI, "-", "-", "-");

                    if (vo.iznos == null)
                        vo.iznos = "-";

                    if (vo.komentar == null)
                        vo.komentar = new Komentar("-", DateTime.Now, k, vo, OcenaVoznje.NULA);

                    if (vo.musterija == null)
                        vo.musterija = m;

                    if (vo.odrediste == null)
                        vo.odrediste = l;

                    //
                    AzurirajVoznju(vo);
                    return View("DetaljiVoznje", vo);
                }

            }

            return View("Greska");
        }
        #endregion

        #region
        public ActionResult PocniSaVoznjom(string datumVoznje, string vozac)
        {
            Korisnik k123 = (Korisnik)Session["korisnikUser"];

            if (k123 == null)
            {
                k123 = new Korisnik();
                Session["korisnikUser"] = k123;
            }

            foreach (Korisnik kor in Database.registrovaniKorisnici.Values)
            {
                if (kor.korisnickoIme == k123.korisnickoIme && kor.uloga == Uloga.VOZAC)
                {
                    Voznja vo = new Voznja();

                    foreach (Voznja v in Database.vozaci[vozac].voznje)
                    {
                        if (v.datumVreme.ToString() == datumVoznje)
                        {
                            v.statusVoznje = StatusVoznje.U_TOKU;
                            vo = v;
                            Database.neuspesneVoznje.Add(vo.datumVreme.ToString(), vo);
                            AzurirajVoznju(vo);
                        }
                    }

                    return View("PocniSaVoznjom", vo);
                }

            }

            return View("Greska");
        }

        public ActionResult UspesnaVoznja(string datumVoznje, string vozac, string musterija)
        {
            Korisnik k123 = (Korisnik)Session["korisnikUser"];

            if (k123 == null)
            {
                k123 = new Korisnik();
                Session["korisnikUser"] = k123;
            }

            foreach (Korisnik kor in Database.registrovaniKorisnici.Values)
            {
                if (kor.korisnickoIme == k123.korisnickoIme && kor.uloga == Uloga.VOZAC)
                {
                    Voznja vo = new Voznja();
                    bool ret = false;

                    if (musterija != "-")
                    {
                        ret = true;
                        foreach (Voznja v in Database.registrovaniKorisnici[musterija].voznje)
                        {
                            if (datumVoznje == v.datumVreme.ToString())
                            {
                                v.statusVoznje = StatusVoznje.USPESNA;
                                vo = v;
                                AzurirajVoznju(vo);
                            }
                        }
                    }

                    foreach (Voznja v in Database.vozaci[vozac].voznje)
                    {
                        if (datumVoznje == v.datumVreme.ToString())
                        {
                            if (v.dispecer == null)
                            {
                                Dispecer d = new Dispecer("-", "-", "-", "-", Pol.MUSKI, "-", "-", "-");
                                v.dispecer = d;
                                AzurirajVoznju(v);
                            }
                            v.statusVoznje = StatusVoznje.USPESNA;
                            if (!ret)
                            {
                                vo = v;
                                AzurirajVoznju(vo);
                            }
                        }
                    }

                    return View("UspesnaVoznja", vo);
                }

            }

            return View("Greska");
        }

        public ActionResult NeuspesnaVoznja(string datumVoznje, string musterija)
        {
            Korisnik k123 = (Korisnik)Session["korisnikUser"];

            if (k123 == null)
            {
                k123 = new Korisnik();
                Session["korisnikUser"] = k123;
            }

            foreach (Korisnik kor in Database.registrovaniKorisnici.Values)
            {
                if (kor.korisnickoIme == k123.korisnickoIme && kor.uloga == Uloga.VOZAC)
                {
                    Voznja vo = new Voznja();

                    if (musterija != "-")
                    {
                        foreach (Voznja v in Database.registrovaniKorisnici[musterija].voznje)
                        {
                            if (datumVoznje == v.datumVreme.ToString())
                            {
                                vo = v;
                                AzurirajVoznju(vo);
                            }
                        }
                    }
                    else
                    {
                        foreach (Voznja v in Database.neuspesneVoznje.Values)
                        {
                            if (datumVoznje == v.datumVreme.ToString())
                            {
                                vo = v;
                                AzurirajVoznju(vo);
                            }
                        }
                    }
                    return View("KomentarVozac", vo);
                }

            }

            return View("Greska");
        }

        public ActionResult KomentarVozac(string comment, string datumVoznje, string userKorisnika)
        {
            Korisnik k123 = (Korisnik)Session["korisnikUser"];

            if (k123 == null)
            {
                k123 = new Korisnik();
                Session["korisnikUser"] = k123;
            }

            foreach (Korisnik kor in Database.registrovaniKorisnici.Values)
            {
                if (kor.korisnickoIme == k123.korisnickoIme && kor.uloga == Uloga.VOZAC)
                {
                    Vozac vo = new Vozac();

                    if (userKorisnika != "-")
                    {
                        foreach (Voznja v in Database.registrovaniKorisnici[userKorisnika].voznje)
                        {
                            if (datumVoznje == v.datumVreme.ToString())
                            {
                                Komentar k = new Komentar(comment, DateTime.Now, v.musterija, v, OcenaVoznje.NULA);
                                v.komentar = k;
                                v.statusVoznje = StatusVoznje.NEUSPESNA;
                                AzurirajVoznju(v);
                                vo = v.vozac;
                                if (v.dispecer == null)
                                {
                                    Dispecer d = new Dispecer("-", "-", "-", "-", Pol.MUSKI, "-", "-", "-");
                                    v.dispecer = d;
                                    Database.voznjeNepoznatihDisp.Add(v.datumVreme.ToString(), v);
                                }
                                Database.slobodniVozaci.Add(v.vozac.korisnickoIme, v.vozac);
                            }
                        }
                    }
                    else
                    {
                        foreach (Voznja v in Database.neuspesneVoznje.Values)
                        {
                            if (datumVoznje == v.datumVreme.ToString())
                            {
                                Komentar k = new Komentar(comment, DateTime.Now, v.musterija, v, OcenaVoznje.NULA);
                                v.komentar = k;
                                v.statusVoznje = StatusVoznje.NEUSPESNA;
                                vo = v.vozac;
                                AzurirajVoznju(v);
                                Database.slobodniVozaci.Add(v.vozac.korisnickoIme, v.vozac);
                            }
                        }
                    }

                    return View("Vozac", vo);
                }

            }

            return View("Greska");
        }

        public ActionResult ZavrsiVoznju(string ulica, string broj, string grad, string poBroj, string iznos, string datumVoznje, string musterija, string vozac)
        {
            Korisnik k123 = (Korisnik)Session["korisnikUser"];

            if (k123 == null)
            {
                k123 = new Korisnik();
                Session["korisnikUser"] = k123;
            }

            foreach (Korisnik kor in Database.registrovaniKorisnici.Values)
            {
                if (kor.korisnickoIme == k123.korisnickoIme && kor.uloga == Uloga.VOZAC)
                {
                    if (musterija != "-")
                    {
                        foreach (Voznja v in Database.registrovaniKorisnici[musterija].voznje)
                        {
                            if (datumVoznje == v.datumVreme.ToString())
                            {
                                Adresa a = new Adresa(ulica, broj, grad, poBroj);
                                Lokacija l = new Lokacija(1, 1, a);
                                v.odrediste = l;
                                v.iznos = iznos;
                                if (v.dispecer.korisnickoIme == "-")
                                    Database.voznjeNepoznatihDisp.Add(v.datumVreme.ToString(), v);
                                //Database.vozaci[vozac].voznje.Add(v);
                                //dodaj voznju vozacu u registrovanim korisnicima!!!!!
                                AzurirajVoznju(v);
                            }
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
                            AzurirajVoznju(v);
                        }
                    }

                    Database.slobodniVozaci.Add(vozac, Database.vozaci[vozac]);

                    return View("Vozac", Database.vozaci[vozac]);
                }

            }

            return View("Greska");
        }

        public ActionResult LokacijaTaksiste(string vozac, string ulica, string broj, string grad, string poBroj)
        {
            Korisnik k123 = (Korisnik)Session["korisnikUser"];

            if (k123 == null)
            {
                k123 = new Korisnik();
                Session["korisnikUser"] = k123;
            }

            foreach (Korisnik kor in Database.registrovaniKorisnici.Values)
            {
                if (kor.korisnickoIme == k123.korisnickoIme && kor.uloga == Uloga.VOZAC)
                {
                    Adresa a = new Adresa(ulica, broj, grad, poBroj);
                    Lokacija l = new Lokacija(1, 1, a);
                    Database.vozaci[vozac].lokacija = l;

                    if (Database.slobodniVozaci.ContainsKey(vozac))
                        Database.slobodniVozaci[vozac].lokacija = l;

                    return View("Vozac", Database.vozaci[vozac]);
                }

            }

            return View("Greska");
        }

        public ActionResult PreuzimanjeVoznje(string voznja, string vozac)
        {
            Korisnik k123 = (Korisnik)Session["korisnikUser"];

            if (k123 == null)
            {
                k123 = new Korisnik();
                Session["korisnikUser"] = k123;
            }

            foreach (Korisnik kor in Database.registrovaniKorisnici.Values)
            {
                if (kor.korisnickoIme == k123.korisnickoIme && kor.uloga == Uloga.VOZAC)
                {
                    Voznja voz = new Voznja();

                    foreach (Korisnik k in Database.registrovaniKorisnici.Values)
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
                                        Database.vozaci[vozac].voznje.Add(voz);
                                        AzurirajVoznju(voz);
                                    }
                                }
                            }
                        }
                    }

                    Database.slobodniVozaci.Remove(vozac);

                    return View("PocniSaVoznjom", voz);
                }

            }

            return View("Greska");
        }
        #endregion

        public void AzurirajVoznju(Voznja voznja)
        {
            Voznja retVoznja = new Voznja();
            retVoznja = PopuniPolja(voznja);
            //DodeliKorisnikuVoznju();

            if (Database.sveVoznje.ContainsKey(voznja.datumVreme.ToString()))
                Database.sveVoznje[voznja.datumVreme.ToString()] = retVoznja;
            else
                Database.sveVoznje.Add(voznja.datumVreme.ToString(), retVoznja);
        }

        public ActionResult Filtriranje(string statusVoznje, string korisnickoIme, string korisnickoImeVozac)
        {
            List<Voznja> voznje = new List<Voznja>();

            if (korisnickoIme != null && korisnickoIme !="-")
            {
                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.musterija.korisnickoIme == korisnickoIme)
                        if (v.statusVoznje.ToString() == statusVoznje)
                            voznje.Add(v);
                }
            }
            else if (korisnickoImeVozac != null)
            {
                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.komentar == null)
                        v.komentar = new Komentar("-", DateTime.Now, v.musterija, v, OcenaVoznje.NULA);

                    if (v.vozac.korisnickoIme == korisnickoImeVozac)
                        if (v.statusVoznje.ToString() == statusVoznje)
                            voznje.Add(v);
                }
            }
            else
            {
                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.statusVoznje.ToString() == statusVoznje)
                    {
                        PopuniPolja(v);
                        voznje.Add(v);
                    }
                }
            }

            return View("RezultatAkcije", voznje);
        }

        public ActionResult Sortiranje(string sortirajPo, string korisnickoIme, string korisnickoImeVozac)
        {
            List<Voznja> voznje = new List<Voznja>();
            List<Voznja> retVoznje = new List<Voznja>();
            List<Voznja> sortirano = new List<Voznja>();

            foreach(Voznja v in Database.sveVoznje.Values)
            {
                voznje.Add(v);
            }

            if(korisnickoIme != null)
            {
                foreach (Voznja v in voznje)
                {
                    if (v.musterija.korisnickoIme == korisnickoIme)
                    {
                        retVoznje.Add(v);
                    }
                }

                if (sortirajPo == "datumu")
                    sortirano = retVoznje.OrderBy(o => o.datumVreme).ToList();
                else
                    sortirano = retVoznje.OrderBy(o => o.komentar.ocenaVoznje).ToList();
            }
            else if(korisnickoImeVozac != null)
            {
                foreach (Voznja v in voznje)
                {
                    if (v.komentar == null)
                        v.komentar = new Komentar("-", DateTime.Now, v.musterija, v, OcenaVoznje.NULA);

                    if (v.vozac.korisnickoIme == korisnickoImeVozac)
                    {
                        retVoznje.Add(v);
                    }
                }

                if (sortirajPo == "datumu")
                    sortirano = retVoznje.OrderBy(o => o.datumVreme).ToList();
                else
                    sortirano = retVoznje.OrderBy(o => o.komentar.ocenaVoznje).ToList();
            }
            else
            {
                if (sortirajPo == "datumu")
                    sortirano = voznje.OrderBy(o => o.datumVreme).ToList();
                else
                    sortirano = voznje.OrderBy(o => o.komentar.ocenaVoznje).ToList();
            }

            return View("RezultatAkcije", sortirano);
        }

        public ActionResult Pretraga(string datumOd, string datumDo, string ocenaOd, string ocenaDo, string cenaOd, string cenaDo, string korisnickoIme)
        {
            List<Voznja> voznje = new List<Voznja>();

            if (datumOd != "" && datumDo != "")
            {
                string[] pom = datumOd.Split(' ', '.', ':');
                DateTime dateTimeOd = new DateTime(Int32.Parse(pom[2]), Int32.Parse(pom[1]), Int32.Parse(pom[0]), Int32.Parse(pom[4]), Int32.Parse(pom[5]), Int32.Parse(pom[6]));

                pom = datumDo.Split(' ', '.', ':');
                DateTime dateTimeDo = new DateTime(Int32.Parse(pom[2]), Int32.Parse(pom[1]), Int32.Parse(pom[0]), Int32.Parse(pom[4]), Int32.Parse(pom[5]), Int32.Parse(pom[6]));

                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.musterija.korisnickoIme == korisnickoIme)
                        if (v.datumVreme > dateTimeOd)
                            voznje.Add(v);
                }

                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.musterija.korisnickoIme == korisnickoIme)
                        if (v.datumVreme > dateTimeDo)
                            voznje.Remove(v);
                }
            }
            else if(datumOd != "")
            {
                string[] pom = datumOd.Split(' ', '.', ':');
                DateTime dateTimeOd = new DateTime(Int32.Parse(pom[2]), Int32.Parse(pom[1]), Int32.Parse(pom[0]), Int32.Parse(pom[4]), Int32.Parse(pom[5]), Int32.Parse(pom[6]));

                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.musterija.korisnickoIme == korisnickoIme)
                        if (v.datumVreme > dateTimeOd)
                            voznje.Add(v);
                }
            }
            else if(datumDo != "")
            {
                string[] pom = datumDo.Split(' ', '.', ':');
                DateTime dateTimeDo = new DateTime(Int32.Parse(pom[2]), Int32.Parse(pom[1]), Int32.Parse(pom[0]), Int32.Parse(pom[4]), Int32.Parse(pom[5]), Int32.Parse(pom[6]));

                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.musterija.korisnickoIme == korisnickoIme)
                        if (v.datumVreme < dateTimeDo)
                            voznje.Add(v);
                }
            }

            if(ocenaOd != "NULA" && ocenaDo != "NULA")
            {
                OcenaVoznje ocena1;
                OcenaVoznje ocena2;

                if (ocenaOd == "JEDAN")
                    ocena1 = OcenaVoznje.JEDAN;
                else if (ocenaOd == "DVA")
                    ocena1 = OcenaVoznje.DVA;
                else if (ocenaOd == "TRI")
                    ocena1 = OcenaVoznje.TRI;
                else if (ocenaOd == "CETIRI")
                    ocena1 = OcenaVoznje.CETIRI;
                else if (ocenaOd == "PET")
                    ocena1 = OcenaVoznje.PET;
                else
                    ocena1 = OcenaVoznje.NULA;

                if (ocenaDo == "JEDAN")
                    ocena2 = OcenaVoznje.JEDAN;
                else if (ocenaDo == "DVA")
                    ocena2 = OcenaVoznje.DVA;
                else if (ocenaDo == "TRI")
                    ocena2 = OcenaVoznje.TRI;
                else if (ocenaDo == "CETIRI")
                    ocena2 = OcenaVoznje.CETIRI;
                else if (ocenaDo == "PET")
                    ocena2 = OcenaVoznje.PET;
                else
                    ocena2 = OcenaVoznje.NULA;

                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.musterija.korisnickoIme == korisnickoIme)
                        if(v.komentar != null && v.komentar.opis != "-")
                            if (v.komentar.ocenaVoznje >= ocena1)
                                voznje.Add(v);
                }

                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.musterija.korisnickoIme == korisnickoIme)
                        if (v.komentar != null && v.komentar.opis != "-")
                            if (v.komentar.ocenaVoznje > ocena2)
                                voznje.Remove(v);
                }
            }
            else if(ocenaOd != "NULA")
            {
                OcenaVoznje ocena1;

                if (ocenaOd == "JEDAN")
                    ocena1 = OcenaVoznje.JEDAN;
                else if (ocenaOd == "DVA")
                    ocena1 = OcenaVoznje.DVA;
                else if (ocenaOd == "TRI")
                    ocena1 = OcenaVoznje.TRI;
                else if (ocenaOd == "CETIRI")
                    ocena1 = OcenaVoznje.CETIRI;
                else if (ocenaOd == "PET")
                    ocena1 = OcenaVoznje.PET;
                else
                    ocena1 = OcenaVoznje.NULA;

                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.musterija.korisnickoIme == korisnickoIme)
                        if (v.komentar != null && v.komentar.opis != "-")
                            if (v.komentar.ocenaVoznje >= ocena1)
                                voznje.Add(v);
                }
            }
            else if(ocenaDo != "NULA")
            {
                OcenaVoznje ocena2;

                if (ocenaDo == "JEDAN")
                    ocena2 = OcenaVoznje.JEDAN;
                else if (ocenaDo == "DVA")
                    ocena2 = OcenaVoznje.DVA;
                else if (ocenaDo == "TRI")
                    ocena2 = OcenaVoznje.TRI;
                else if (ocenaDo == "CETIRI")
                    ocena2 = OcenaVoznje.CETIRI;
                else if (ocenaDo == "PET")
                    ocena2 = OcenaVoznje.PET;
                else
                    ocena2 = OcenaVoznje.NULA;

                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.musterija.korisnickoIme == korisnickoIme)
                        if (v.komentar != null && v.komentar.opis != "-")
                            if (v.komentar.ocenaVoznje <= ocena2)
                                voznje.Add(v);
                }
            }



            if(cenaOd != "" && cenaDo != "")
            {
                int min = Int32.Parse(cenaOd);
                int max = Int32.Parse(cenaDo);

                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.musterija.korisnickoIme == korisnickoIme)
                        if (Int32.Parse(v.iznos) >= min)
                            voznje.Add(v);
                }

                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.musterija.korisnickoIme == korisnickoIme)
                        if (Int32.Parse(v.iznos) > max)
                            voznje.Remove(v);
                }
            }
            else if(cenaOd != "")
            {
                int min = Int32.Parse(cenaOd);

                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.musterija.korisnickoIme == korisnickoIme)
                        if (Int32.Parse(v.iznos) >= min)
                            voznje.Add(v);
                }
            }
            else if(cenaDo != "")
            {
                int max = Int32.Parse(cenaDo);

                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.musterija.korisnickoIme == korisnickoIme)
                        if (Int32.Parse(v.iznos) <= max)
                            voznje.Add(v);
                }
            }

            return View("RezultatAkcije", voznje);
        }

        public ActionResult PretragaVozac(string datumOd, string datumDo, string ocenaOd, string ocenaDo, string cenaOd, string cenaDo, string korisnickoIme)
        {
            List<Voznja> voznje = new List<Voznja>();

            if (datumOd != "" && datumDo != "")
            {
                string[] pom = datumOd.Split(' ', '.', ':');
                DateTime dateTimeOd = new DateTime(Int32.Parse(pom[2]), Int32.Parse(pom[1]), Int32.Parse(pom[0]), Int32.Parse(pom[4]), Int32.Parse(pom[5]), Int32.Parse(pom[6]));

                pom = datumDo.Split(' ', '.', ':');
                DateTime dateTimeDo = new DateTime(Int32.Parse(pom[2]), Int32.Parse(pom[1]), Int32.Parse(pom[0]), Int32.Parse(pom[4]), Int32.Parse(pom[5]), Int32.Parse(pom[6]));

                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.vozac.korisnickoIme == korisnickoIme)
                        if (v.datumVreme > dateTimeOd)
                            voznje.Add(v);
                }

                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.vozac.korisnickoIme == korisnickoIme)
                        if (v.datumVreme > dateTimeDo)
                            voznje.Remove(v);
                }
            }
            else if (datumOd != "")
            {
                string[] pom = datumOd.Split(' ', '.', ':');
                DateTime dateTimeOd = new DateTime(Int32.Parse(pom[2]), Int32.Parse(pom[1]), Int32.Parse(pom[0]), Int32.Parse(pom[4]), Int32.Parse(pom[5]), Int32.Parse(pom[6]));

                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.vozac.korisnickoIme == korisnickoIme)
                        if (v.datumVreme > dateTimeOd)
                            voznje.Add(v);
                }
            }
            else if (datumDo != "")
            {
                string[] pom = datumDo.Split(' ', '.', ':');
                DateTime dateTimeDo = new DateTime(Int32.Parse(pom[2]), Int32.Parse(pom[1]), Int32.Parse(pom[0]), Int32.Parse(pom[4]), Int32.Parse(pom[5]), Int32.Parse(pom[6]));

                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.vozac.korisnickoIme == korisnickoIme)
                        if (v.datumVreme < dateTimeDo)
                            voznje.Add(v);
                }
            }

            if (ocenaOd != "NULA" && ocenaDo != "NULA")
            {
                OcenaVoznje ocena1;
                OcenaVoznje ocena2;

                if (ocenaOd == "JEDAN")
                    ocena1 = OcenaVoznje.JEDAN;
                else if (ocenaOd == "DVA")
                    ocena1 = OcenaVoznje.DVA;
                else if (ocenaOd == "TRI")
                    ocena1 = OcenaVoznje.TRI;
                else if (ocenaOd == "CETIRI")
                    ocena1 = OcenaVoznje.CETIRI;
                else if (ocenaOd == "PET")
                    ocena1 = OcenaVoznje.PET;
                else
                    ocena1 = OcenaVoznje.NULA;

                if (ocenaDo == "JEDAN")
                    ocena2 = OcenaVoznje.JEDAN;
                else if (ocenaDo == "DVA")
                    ocena2 = OcenaVoznje.DVA;
                else if (ocenaDo == "TRI")
                    ocena2 = OcenaVoznje.TRI;
                else if (ocenaDo == "CETIRI")
                    ocena2 = OcenaVoznje.CETIRI;
                else if (ocenaDo == "PET")
                    ocena2 = OcenaVoznje.PET;
                else
                    ocena2 = OcenaVoznje.NULA;

                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.vozac.korisnickoIme == korisnickoIme)
                    {
                        PopuniPolja(v);
                        if (v.komentar.ocenaVoznje >= ocena1)
                            voznje.Add(v);
                    }
                }

                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.vozac.korisnickoIme == korisnickoIme)
                    {
                        PopuniPolja(v);
                        if (v.komentar.ocenaVoznje > ocena2)
                            voznje.Remove(v);
                    }
                }
            }
            else if (ocenaOd != "NULA")
            {
                OcenaVoznje ocena1;

                if (ocenaOd == "JEDAN")
                    ocena1 = OcenaVoznje.JEDAN;
                else if (ocenaOd == "DVA")
                    ocena1 = OcenaVoznje.DVA;
                else if (ocenaOd == "TRI")
                    ocena1 = OcenaVoznje.TRI;
                else if (ocenaOd == "CETIRI")
                    ocena1 = OcenaVoznje.CETIRI;
                else if (ocenaOd == "PET")
                    ocena1 = OcenaVoznje.PET;
                else
                    ocena1 = OcenaVoznje.NULA;

                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.vozac.korisnickoIme == korisnickoIme)
                    {
                        PopuniPolja(v);
                        if (v.komentar.ocenaVoznje >= ocena1)
                            voznje.Add(v);
                    }
                }
            }
            else if (ocenaDo != "NULA")
            {
                OcenaVoznje ocena2;

                if (ocenaDo == "JEDAN")
                    ocena2 = OcenaVoznje.JEDAN;
                else if (ocenaDo == "DVA")
                    ocena2 = OcenaVoznje.DVA;
                else if (ocenaDo == "TRI")
                    ocena2 = OcenaVoznje.TRI;
                else if (ocenaDo == "CETIRI")
                    ocena2 = OcenaVoznje.CETIRI;
                else if (ocenaDo == "PET")
                    ocena2 = OcenaVoznje.PET;
                else
                    ocena2 = OcenaVoznje.NULA;

                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.vozac.korisnickoIme == korisnickoIme)
                    {
                        PopuniPolja(v);
                        if (v.komentar.ocenaVoznje <= ocena2)
                            voznje.Add(v);
                    }
                }
            }



            if (cenaOd != "" && cenaDo != "")
            {
                int min = Int32.Parse(cenaOd);
                int max = Int32.Parse(cenaDo);

                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.vozac.korisnickoIme == korisnickoIme)
                    {
                        PopuniPolja(v);
                        if (Int32.Parse(v.iznos) >= min)
                            voznje.Add(v);
                    }
                }

                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.vozac.korisnickoIme == korisnickoIme)
                    {
                        PopuniPolja(v);
                        if (Int32.Parse(v.iznos) > max)
                            voznje.Remove(v);
                    }
                }
            }
            else if (cenaOd != "")
            {
                int min = Int32.Parse(cenaOd);

                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.vozac.korisnickoIme == korisnickoIme)
                    {
                        PopuniPolja(v);
                        if (Int32.Parse(v.iznos) >= min)
                            voznje.Add(v);
                    }
                }
            }
            else if (cenaDo != "")
            {
                int max = Int32.Parse(cenaDo);

                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.vozac.korisnickoIme == korisnickoIme)
                    {
                        PopuniPolja(v);
                        if (Int32.Parse(v.iznos) <= max)
                            voznje.Add(v);
                    }
                }
            }

            return View("RezultatAkcije", voznje);
        }

        public ActionResult PretragaDispecer(string datumOd, string datumDo, string ocenaOd, string ocenaDo, string cenaOd, string cenaDo, string imeVozaca, string prezimeVozaca, string imeMusterije, string prezimeMusterije)
        {
            List<Voznja> voznje = new List<Voznja>();

            if(datumOd != "" && datumDo != "")
            {
                string[] pom = datumOd.Split(' ', '.', ':');
                DateTime dateTimeOd = new DateTime(Int32.Parse(pom[2]), Int32.Parse(pom[1]), Int32.Parse(pom[0]), Int32.Parse(pom[4]), Int32.Parse(pom[5]), Int32.Parse(pom[6]));

                pom = datumDo.Split(' ', '.', ':');
                DateTime dateTimeDo = new DateTime(Int32.Parse(pom[2]), Int32.Parse(pom[1]), Int32.Parse(pom[0]), Int32.Parse(pom[4]), Int32.Parse(pom[5]), Int32.Parse(pom[6]));

                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.datumVreme > dateTimeOd)
                        voznje.Add(v);
                }

                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.datumVreme > dateTimeDo)
                      voznje.Remove(v);
                }
            }
            else if(datumOd != "")
            {
                string[] pom = datumOd.Split(' ', '.', ':');
                DateTime dateTimeOd = new DateTime(Int32.Parse(pom[2]), Int32.Parse(pom[1]), Int32.Parse(pom[0]), Int32.Parse(pom[4]), Int32.Parse(pom[5]), Int32.Parse(pom[6]));

                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.datumVreme > dateTimeOd)
                        voznje.Add(v);
                }
            }
            else if(datumDo != "")
            {
                string[] pom = datumDo.Split(' ', '.', ':');
                DateTime dateTimeDo = new DateTime(Int32.Parse(pom[2]), Int32.Parse(pom[1]), Int32.Parse(pom[0]), Int32.Parse(pom[4]), Int32.Parse(pom[5]), Int32.Parse(pom[6]));

                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.datumVreme < dateTimeDo)
                        voznje.Add(v);
                }
            }


            if (ocenaOd != "NULA" && ocenaDo != "NULA")
            {
                OcenaVoznje ocena1;
                OcenaVoznje ocena2;

                if (ocenaOd == "JEDAN")
                    ocena1 = OcenaVoznje.JEDAN;
                else if (ocenaOd == "DVA")
                    ocena1 = OcenaVoznje.DVA;
                else if (ocenaOd == "TRI")
                    ocena1 = OcenaVoznje.TRI;
                else if (ocenaOd == "CETIRI")
                    ocena1 = OcenaVoznje.CETIRI;
                else if (ocenaOd == "PET")
                    ocena1 = OcenaVoznje.PET;
                else
                    ocena1 = OcenaVoznje.NULA;

                if (ocenaDo == "JEDAN")
                    ocena2 = OcenaVoznje.JEDAN;
                else if (ocenaDo == "DVA")
                    ocena2 = OcenaVoznje.DVA;
                else if (ocenaDo == "TRI")
                    ocena2 = OcenaVoznje.TRI;
                else if (ocenaDo == "CETIRI")
                    ocena2 = OcenaVoznje.CETIRI;
                else if (ocenaDo == "PET")
                    ocena2 = OcenaVoznje.PET;
                else
                    ocena2 = OcenaVoznje.NULA;

                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.komentar.ocenaVoznje >= ocena1)
                        voznje.Add(v);
                }

                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.komentar.ocenaVoznje > ocena2)
                        voznje.Remove(v);
                }
            }
            else if (ocenaOd != "NULA")
            {
                OcenaVoznje ocena1;

                if (ocenaOd == "JEDAN")
                    ocena1 = OcenaVoznje.JEDAN;
                else if (ocenaOd == "DVA")
                    ocena1 = OcenaVoznje.DVA;
                else if (ocenaOd == "TRI")
                    ocena1 = OcenaVoznje.TRI;
                else if (ocenaOd == "CETIRI")
                    ocena1 = OcenaVoznje.CETIRI;
                else if (ocenaOd == "PET")
                    ocena1 = OcenaVoznje.PET;
                else
                    ocena1 = OcenaVoznje.NULA;

                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.komentar.ocenaVoznje >= ocena1)
                        voznje.Add(v);
                }
            }
            else if (ocenaDo != "NULA")
            {
                OcenaVoznje ocena2;

                if (ocenaDo == "JEDAN")
                    ocena2 = OcenaVoznje.JEDAN;
                else if (ocenaDo == "DVA")
                    ocena2 = OcenaVoznje.DVA;
                else if (ocenaDo == "TRI")
                    ocena2 = OcenaVoznje.TRI;
                else if (ocenaDo == "CETIRI")
                    ocena2 = OcenaVoznje.CETIRI;
                else if (ocenaDo == "PET")
                    ocena2 = OcenaVoznje.PET;
                else
                    ocena2 = OcenaVoznje.NULA;

                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.komentar.ocenaVoznje <= ocena2)
                        voznje.Add(v);
                }
            }


            if (cenaOd != "" && cenaDo != "")
            {
                int min = Int32.Parse(cenaOd);
                int max = Int32.Parse(cenaDo);

                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (Int32.Parse(v.iznos) >= min)
                        voznje.Add(v);
                }

                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (Int32.Parse(v.iznos) > max)
                        voznje.Remove(v);
                }
            }
            else if (cenaOd != "")
            {
                int min = Int32.Parse(cenaOd);

                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (Int32.Parse(v.iznos) >= min)
                        voznje.Add(v);
                }
            }
            else if (cenaDo != "")
            {
                int max = Int32.Parse(cenaDo);

                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (Int32.Parse(v.iznos) <= max)
                        voznje.Add(v);
                }
            }
            //
            if(imeVozaca != "" && prezimeVozaca != "")
            {
                foreach(Voznja v in Database.sveVoznje.Values)
                {
                    if (v.vozac.ime == imeVozaca && v.vozac.prezime == prezimeVozaca)
                        voznje.Add(v);
                }
            }
            else if(imeVozaca != "")
            {
                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.vozac.ime == imeVozaca)
                        voznje.Add(v);
                }
            }
            else if(prezimeVozaca != "")
            {
                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.vozac.prezime == prezimeVozaca)
                        voznje.Add(v);
                }
            }
            
            if(imeMusterije != "" && prezimeMusterije != "")
            {
                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.musterija.ime == imeMusterije && v.musterija.prezime == prezimeMusterije)
                        voznje.Add(v);
                }
            }
            else if(imeMusterije != "")
            {
                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.musterija.ime == imeMusterije)
                        voznje.Add(v);
                }
            }
            else if(prezimeMusterije != "")
            {
                foreach (Voznja v in Database.sveVoznje.Values)
                {
                    if (v.musterija.prezime == prezimeMusterije)
                        voznje.Add(v);
                }
            }

            return View("RezultatAkcije", voznje);
        }

        public Voznja PopuniPolja(Voznja v)
        {
            if (v.dispecer == null)
                v.dispecer = new Dispecer("-", "-", "-", "-", Pol.MUSKI, "-", "-", "-");

            if (v.musterija == null)
                v.musterija = new Musterija("-", "-", "-", "-", Pol.MUSKI, "-", "-", "-");

            if (v.odrediste == null)
                v.odrediste = new Lokacija(1, 1, new Adresa("-", "-", "-", "-"));

            if (v.vozac == null)
            {
                Korisnik k = new Korisnik("-", "-", "-", "-", Pol.MUSKI, "-", "-", "-");
                Lokacija l = new Lokacija(1, 1, new Adresa("-", "-", "-", "-"));
                Automobil a = new Automobil(new Vozac(), "-", "-", "-", TipAutomobila.PUTNICKI_AUTOMOBIL);
                v.vozac = new Vozac(k, l, a);
            }

            if (v.komentar == null)
                v.komentar = new Komentar("-", DateTime.Now, v.musterija, v, OcenaVoznje.NULA);

            if (v.iznos == null)
                v.iznos = "-1";

            return v;
        }

        //public void DodeliKorisnikuVoznju()
        //{
        //    Korisnik k123 = (Korisnik)Session["korisnikUser"];

        //    if (k123 == null)
        //    {
        //        k123 = new Korisnik();
        //        Session["korisnikUser"] = k123;
        //    }

        //    foreach (Korisnik kor in Database.registrovaniKorisnici.Values)
        //    {
        //        if (kor.korisnickoIme == k123.korisnickoIme && kor.uloga == Uloga.MUSTERIJA)
        //        {
        //            foreach(Voznja v in Database.sveVoznje.Values)
        //            {
        //                if(v.musterija.korisnickoIme == k123.korisnickoIme)
        //                {
        //                    Database.registrovaniKorisnici[k123.korisnickoIme].voznje.Add(v);
        //                }
        //            }
        //        }

        //    }
        //}
    }
}