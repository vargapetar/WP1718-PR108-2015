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
                Database.UpisiRegKorisnike();
                //Database.UpisiUTXT(new Registration(user, pass));
                return View("UspesnaRegistracija");
            }
            else
            {
                Odgovor o = new Odgovor("Operacija nije uspela!");
                return View("Greska", o);
            }
        }

        [HttpPost]
        public ActionResult Logovanje(string user, string pass)
        {
            Korisnik k = (Korisnik)Session["korisnikUser"];

            if(k == null)
            {
                k = new Korisnik();
                Session["korisnikUser"] = k;
            }

            if(Database.registrovaniKorisnici.ContainsKey(user))
            {
                if (Database.registrovaniKorisnici[user].lozinka == pass)
                {
                    if (Database.registrovaniKorisnici[user].uloga == Uloga.DISPECER)
                    {
                        Session["korisnikUser"] = Database.registrovaniKorisnici[user];
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

        public ActionResult Odjava()
        {
            Session.Abandon();
            Korisnik d = new Korisnik();
            Session["korisnik"] = d;
            return View("HomePage");
        }

        [HttpPost]
        public ActionResult Preusmeri()
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
                    return View("Musterija", k123);
                }
            }

            foreach (Korisnik kor in Database.registrovaniKorisnici.Values)
            {
                if (kor.korisnickoIme == k123.korisnickoIme && kor.uloga == Uloga.DISPECER)
                {
                    return View("Dispecer");
                }
            }

            foreach (Korisnik kor in Database.registrovaniKorisnici.Values)
            {
                if (kor.korisnickoIme == k123.korisnickoIme && kor.uloga == Uloga.VOZAC)
                {
                    return View("Vozac", Database.vozaci[k123.korisnickoIme]);
                }
            }

            Odgovor o = new Odgovor("Nesto ne valja!");
            return View("Greska", o);
        }

        #region
        //izmena podataka kod musterije
        [HttpPost]
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

                    Odgovor odg = new Odgovor("Podaci su uspesno izmenjeni.");
                    return View("Greska", odg);
                }

            }

            Odgovor o = new Odgovor("Podaci nisu izmenjeni, doslo je do greske!");
            return View("Greska"); //smisli gresku
        }

        [HttpPost]
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
                    Database.registrovaniKorisnici[korisnickoIme].voznje.Add(v);
                    AzurirajVoznju(v, k.korisnickoIme);

                    Database.UpisiVoznje();

                    Odgovor odg = new Odgovor("Voznja je uspesno porucena!");
                    return View("Greska", odg);
                }

            }

            Odgovor o = new Odgovor("Voznja nije narucena, doslo je do greske!");
            return View("Greska", o);
        }

        [HttpPost]
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
                            AzurirajVoznju(vo, k.korisnickoIme);
                        }
                    }

                    return View("MusterijaMenjaVoznju", vo);
                }

            }

            Odgovor o = new Odgovor("Voznja se ne moze promeniti, doslo je do greske!");
            return View("Greska", o);
        }

        [HttpPost]
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
                            AzurirajVoznju(v, k.korisnickoIme);
                        }
                    }

                    Database.UpisiVoznje();

                    Odgovor odg = new Odgovor("Voznja je uspesno izmenjena.");
                    return View("Greska", odg);
                }

            }

            Odgovor o = new Odgovor("Voznja se ne moze izmeniti, doslo je do greske!");
            return View("Greska", o);
        }

        [HttpPost]
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
                            AzurirajVoznju(v, k.korisnickoIme);
                        }
                    }

                    return View("Komentar", vo);
                }

            }

            Odgovor o = new Odgovor("Nemoguce je otkazati voznju, doslo je do greske!");
            return View("Greska");
        }

        [HttpPost]
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
                                AzurirajVoznju(v, k123.korisnickoIme);
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
                                AzurirajVoznju(v, k123.korisnickoIme);
                            }
                        }
                    }

                    Database.UpisiVoznje();

                    Odgovor odg = new Odgovor("Komentar uspesno postavljen");
                    return View("Greska", odg);
                }

            }

            Odgovor o = new Odgovor("Nemoguce je ostaviti komentar, doslo je do greske!");
            return View("Greska");
        }

        [HttpPost]
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
                            AzurirajVoznju(vo, k123.korisnickoIme);
                        }
                    }

                    return View("Komentar", vo);
                }

            }

            Odgovor o = new Odgovor("Nemoguce je ostaviti komentar, doslo je do greske!");
            return View("Greska");
        }
        #endregion

        #region
        [HttpPost]
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
                    v.lokacija = new Lokacija(1, 1, new Adresa("-", "-", "-", "-"));

                    Database.vozaci.Add(v.korisnickoIme, v);
                    Database.registrovaniKorisnici.Add(k.korisnickoIme, k);
                    //Database.slobodniVozaci.Add(v.korisnickoIme, v);

                    Database.UpisiVozace();
                    Database.UpisiRegKorisnike();

                    Odgovor o = new Odgovor("Vozac uspesno kreiran.");
                    return View("Greska", o);
                }

            }

            Odgovor odg = new Odgovor("Vozac nije kreiran, doslo je do greske!");
            return View("Greska");
        }

        [HttpPost]
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

                    v.dispecer = (Dispecer)Database.registrovaniKorisnici[k123.korisnickoIme];
                    //
                    v.dispecer.voznje.Add(v);
                    v.vozac = Database.vozaci[izabraniVozac];
                    v.statusVoznje = StatusVoznje.FORMIRANA;
                    v.musterija = new Musterija("-", "-", "-", "-", Pol.MUSKI, "-", "-", "-");
                    //
                    AzurirajVoznju(v, k123.korisnickoIme);
                    Database.vozaci[izabraniVozac].voznje.Add(v);

                    Odgovor odg = new Odgovor("Voznja uspesno kreirana.");
                    return View("Greska", odg);
                }

            }

            Odgovor o = new Odgovor("Voznja nije kreirana, doslo je do greske!");
            return View("Greska", o);
        }

        [HttpPost]
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
                    Voznja retVoznja = Database.sveVoznje[voznja];
                    retVoznja.statusVoznje = StatusVoznje.OBRADJENA;
                    retVoznja.dispecer = (Dispecer)Database.registrovaniKorisnici[k123.korisnickoIme];
                    Database.registrovaniKorisnici[k123.korisnickoIme].voznje.Add(retVoznja);

                    Vozac retVozac = Database.vozaci[vozac];
                    retVozac.voznje.Add(retVoznja);

                    retVoznja.vozac = retVozac;
                    //
                    AzurirajVoznju(retVoznja, k123.korisnickoIme);

                    Odgovor odg = new Odgovor("Voznja uspesno kreirana.");
                    return View("Greska", odg);
                }

            }

            Odgovor o = new Odgovor("Voznja nije kreirana, doslo je do greske!");
            return View("Greska", o);
        }

        [HttpPost]
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
                if (kor.korisnickoIme == k123.korisnickoIme)
                {
                    if (kor.uloga == Uloga.VOZAC || kor.uloga == Uloga.DISPECER)
                    {
                        Voznja vo = new Voznja();
                        Korisnik k = new Korisnik("-", "-", "-", "-", Pol.MUSKI, "-", "-", "-");
                        Musterija m = new Musterija("-", "-", "-", "-", Pol.MUSKI, "-", "-", "-");
                        Adresa a = new Adresa("-", "-", "-", "-");
                        Lokacija l = new Lokacija(1, 1, a);

                        vo = Database.sveVoznje[datumVoznje];

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
                        AzurirajVoznju(vo, k123.korisnickoIme);
                        return View("DetaljiVoznje", vo);
                    }
                }

            }

            Odgovor o = new Odgovor("Nemoguce je otvoriti detalje voznje, doslo je do greske!");
            return View("Greska", o);
        }
        #endregion

        #region
        [HttpPost]
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
                            Database.vozaci[vozac].slobodan = false;
                            v.statusVoznje = StatusVoznje.U_TOKU;
                            vo = v;
                            AzurirajVoznju(vo, k123.korisnickoIme);
                        }
                    }

                    return View("PocniSaVoznjom", vo);
                }

            }

            Odgovor o = new Odgovor("Nemoguce je zapoceti sa voznjom, doslo je do greske!");
            return View("Greska", o);
        }

        [HttpPost]
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
                                AzurirajVoznju(vo, k123.korisnickoIme);
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
                                AzurirajVoznju(v, k123.korisnickoIme);
                            }
                            v.statusVoznje = StatusVoznje.USPESNA;
                            if (!ret)
                            {
                                vo = v;
                                AzurirajVoznju(vo, k123.korisnickoIme);
                            }
                        }
                    }

                    return View("UspesnaVoznja", vo);
                }

            }

            Odgovor o = new Odgovor("Doslo je do greske!");
            return View("Greska", o);
        }

        [HttpPost]
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
                                AzurirajVoznju(vo, k123.korisnickoIme);
                            }
                        }
                    }
                    else
                    {
                        foreach (Voznja v in Database.sveVoznje.Values)
                        {
                            if (datumVoznje == v.datumVreme.ToString())
                            {
                                vo = v;
                                AzurirajVoznju(vo, k123.korisnickoIme);
                            }
                        }
                    }
                    return View("KomentarVozac", vo);
                }

            }

            Odgovor o = new Odgovor("Nemoguce je ostaviti komentar, doslo je do greske!");
            return View("Greska");
        }

        [HttpPost]
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
                    if (userKorisnika != "-")
                    {
                        foreach (Voznja v in Database.registrovaniKorisnici[userKorisnika].voznje)
                        {
                            if (datumVoznje == v.datumVreme.ToString())
                            {
                                Komentar k = new Komentar(comment, DateTime.Now, v.musterija, v, OcenaVoznje.NULA);
                                v.komentar = k;
                                v.statusVoznje = StatusVoznje.NEUSPESNA;
                                AzurirajVoznju(v, k123.korisnickoIme);
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
                                AzurirajVoznju(v, k123.korisnickoIme);
                                Database.slobodniVozaci.Add(v.vozac.korisnickoIme, v.vozac);
                            }
                        }
                    }

                    Database.UpisiVoznje();

                    Odgovor odg = new Odgovor("Voznja je neuspesno zavrsena.");
                    return View("Greska", odg);
                }

            }

            Odgovor o = new Odgovor("Nemoguce postaviti komentar, doslo je do greske!");
            return View("Greska", o);
        }

        [HttpPost]
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
                                AzurirajVoznju(v, k123.korisnickoIme);
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
                            AzurirajVoznju(v, k123.korisnickoIme);
                        }
                    }

                    Database.UpisiVoznje();

                    Odgovor odg = new Odgovor("Voznja uspesno zavrsena.");
                    return View("Greska", odg);
                }

            }

            Odgovor o = new Odgovor("Voznja nije zavrsena, doslo je do greske!");
            return View("Greska", o);
        }

        [HttpPost]
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

                    Odgovor odg = new Odgovor("Lokacija uspesno dodata.");
                    return View("Greska", odg);
                }

            }

            Odgovor o = new Odgovor("Nemoguce dodati lokaciju, doslo je do greske!");
            return View("Greska", o);
        }

        [HttpPost]
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
                                        AzurirajVoznju(voz, k123.korisnickoIme);
                                    }
                                }
                            }
                        }
                    }

                    Database.slobodniVozaci.Remove(vozac);

                    return View("PocniSaVoznjom", voz);
                }

            }

            Odgovor o = new Odgovor("Nemoguce preuzeti voznju, doslo je do greske");
            return View("Greska");
        }
        #endregion

        public void AzurirajVoznju(Voznja voznja, string korisnik)
        {
            Voznja retVoznja = new Voznja();
            retVoznja = PopuniPolja(voznja);
            //DodeliKorisnikuVoznju();

            if (Database.sveVoznje.ContainsKey(voznja.datumVreme.ToString()))
                Database.sveVoznje[voznja.datumVreme.ToString()] = retVoznja;
            else
                Database.sveVoznje.Add(voznja.datumVreme.ToString(), retVoznja);
        }

        [HttpPost]
        public ActionResult Filtriranje(string statusVoznje, string korisnickoIme, string korisnickoImeVozac)
        {
            List<Voznja> voznje = new List<Voznja>();

            if (korisnickoIme != null && korisnickoIme !="-")
            {
                foreach (Voznja v in Database.vozaci[korisnickoIme].voznje)
                {
                    if (v.musterija.korisnickoIme == korisnickoIme)
                        if (v.statusVoznje.ToString() == statusVoznje)
                            voznje.Add(v);
                }
            }
            else if (korisnickoImeVozac != null)
            {
                foreach (Voznja v in Database.vozaci[korisnickoImeVozac].voznje)
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

        [HttpPost]
        public ActionResult Sortiranje(string sortirajPo, string korisnickoIme, string korisnickoImeVozac)
        {
            List<Voznja> voznje = new List<Voznja>();
            List<Voznja> retVoznje = new List<Voznja>();
            List<Voznja> sortirano = new List<Voznja>();

            

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
                voznje = Database.vozaci[korisnickoImeVozac].voznje;

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
                foreach (Voznja v in Database.sveVoznje.Values)
                    voznje.Add(v);

                if (sortirajPo == "datumu")
                    sortirano = voznje.OrderBy(o => o.datumVreme).ToList();
                else
                    sortirano = voznje.OrderBy(o => o.komentar.ocenaVoznje).ToList();
            }

            return View("RezultatAkcije", sortirano);
        }

        [HttpPost]
        public ActionResult Pretraga(string datumOd, string datumDo, string ocenaOd, string ocenaDo, string cenaOd, string cenaDo, string korisnickoIme)
        {
            List<Voznja> voznje = new List<Voznja>();

            if (datumOd != "" && datumDo != "")
            {
                string[] pom = datumOd.Split(' ', '.', ':');
                DateTime dateTimeOd = new DateTime(Int32.Parse(pom[2]), Int32.Parse(pom[1]), Int32.Parse(pom[0]), Int32.Parse(pom[4]), Int32.Parse(pom[5]), Int32.Parse(pom[6]));

                pom = datumDo.Split(' ', '.', ':');
                DateTime dateTimeDo = new DateTime(Int32.Parse(pom[2]), Int32.Parse(pom[1]), Int32.Parse(pom[0]), Int32.Parse(pom[4]), Int32.Parse(pom[5]), Int32.Parse(pom[6]));

                foreach (Voznja v in Database.registrovaniKorisnici[korisnickoIme].voznje)
                {
                    if (v.datumVreme > dateTimeOd)
                        voznje.Add(v);
                }

                foreach (Voznja v in Database.registrovaniKorisnici[korisnickoIme].voznje)
                {
                    if (v.datumVreme > dateTimeDo)
                        voznje.Remove(v);
                }
            }
            else if(datumOd != "")
            {
                string[] pom = datumOd.Split(' ', '.', ':');
                DateTime dateTimeOd = new DateTime(Int32.Parse(pom[2]), Int32.Parse(pom[1]), Int32.Parse(pom[0]), Int32.Parse(pom[4]), Int32.Parse(pom[5]), Int32.Parse(pom[6]));

                foreach (Voznja v in Database.registrovaniKorisnici[korisnickoIme].voznje)
                {
                    if (v.datumVreme > dateTimeOd)
                        voznje.Add(v);
                }
            }
            else if(datumDo != "")
            {
                string[] pom = datumDo.Split(' ', '.', ':');
                DateTime dateTimeDo = new DateTime(Int32.Parse(pom[2]), Int32.Parse(pom[1]), Int32.Parse(pom[0]), Int32.Parse(pom[4]), Int32.Parse(pom[5]), Int32.Parse(pom[6]));

                foreach (Voznja v in Database.registrovaniKorisnici[korisnickoIme].voznje)
                {
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

                foreach (Voznja v in Database.registrovaniKorisnici[korisnickoIme].voznje)
                {
                    if (v.komentar.ocenaVoznje >= ocena1)
                        voznje.Add(v);
                }

                foreach (Voznja v in Database.registrovaniKorisnici[korisnickoIme].voznje)
                {
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

                foreach (Voznja v in Database.registrovaniKorisnici[korisnickoIme].voznje)
                {
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

                foreach (Voznja v in Database.registrovaniKorisnici[korisnickoIme].voznje)
                {
                    if (v.komentar.ocenaVoznje <= ocena2)
                        voznje.Add(v);
                }
            }



            if(cenaOd != "" && cenaDo != "")
            {
                int min = Int32.Parse(cenaOd);
                int max = Int32.Parse(cenaDo);

                foreach (Voznja v in Database.registrovaniKorisnici[korisnickoIme].voznje)
                {
                    if (Int32.Parse(v.iznos) >= min)
                        voznje.Add(v);
                }

                foreach (Voznja v in Database.registrovaniKorisnici[korisnickoIme].voznje)
                {
                    if (Int32.Parse(v.iznos) > max)
                        voznje.Remove(v);
                }
            }
            else if(cenaOd != "")
            {
                int min = Int32.Parse(cenaOd);

                foreach (Voznja v in Database.registrovaniKorisnici[korisnickoIme].voznje)
                {
                    if (Int32.Parse(v.iznos) >= min)
                        voznje.Add(v);
                }
            }
            else if(cenaDo != "")
            {
                int max = Int32.Parse(cenaDo);

                foreach (Voznja v in Database.registrovaniKorisnici[korisnickoIme].voznje)
                {
                    if (Int32.Parse(v.iznos) <= max)
                        voznje.Add(v);
                }
            }

            return View("RezultatAkcije", voznje);
        }

        [HttpPost]
        public ActionResult PretragaVozac(string datumOd, string datumDo, string ocenaOd, string ocenaDo, string cenaOd, string cenaDo, string korisnickoIme)
        {
            List<Voznja> voznje = new List<Voznja>();

            if (datumOd != "" && datumDo != "")
            {
                string[] pom = datumOd.Split(' ', '.', ':');
                DateTime dateTimeOd = new DateTime(Int32.Parse(pom[2]), Int32.Parse(pom[1]), Int32.Parse(pom[0]), Int32.Parse(pom[4]), Int32.Parse(pom[5]), Int32.Parse(pom[6]));

                pom = datumDo.Split(' ', '.', ':');
                DateTime dateTimeDo = new DateTime(Int32.Parse(pom[2]), Int32.Parse(pom[1]), Int32.Parse(pom[0]), Int32.Parse(pom[4]), Int32.Parse(pom[5]), Int32.Parse(pom[6]));

                foreach (Voznja v in Database.vozaci[korisnickoIme].voznje)
                {
                    if (v.vozac.korisnickoIme == korisnickoIme)
                        if (v.datumVreme > dateTimeOd)
                            voznje.Add(v);
                }

                foreach (Voznja v in Database.vozaci[korisnickoIme].voznje)
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

                foreach (Voznja v in Database.vozaci[korisnickoIme].voznje)
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

                foreach (Voznja v in Database.vozaci[korisnickoIme].voznje)
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

                foreach (Voznja v in Database.vozaci[korisnickoIme].voznje)
                {
                    if (v.vozac.korisnickoIme == korisnickoIme)
                    {
                        PopuniPolja(v);
                        if (v.komentar.ocenaVoznje >= ocena1)
                            voznje.Add(v);
                    }
                }

                foreach (Voznja v in Database.vozaci[korisnickoIme].voznje)
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

                foreach (Voznja v in Database.vozaci[korisnickoIme].voznje)
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

                foreach (Voznja v in Database.vozaci[korisnickoIme].voznje)
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

                foreach (Voznja v in Database.vozaci[korisnickoIme].voznje)
                {
                    if (v.vozac.korisnickoIme == korisnickoIme)
                    {
                        PopuniPolja(v);
                        if (Int32.Parse(v.iznos) >= min)
                            voznje.Add(v);
                    }
                }

                foreach (Voznja v in Database.vozaci[korisnickoIme].voznje)
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

                foreach (Voznja v in Database.vozaci[korisnickoIme].voznje)
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

                foreach (Voznja v in Database.vozaci[korisnickoIme].voznje)
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

        [HttpPost]
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
    }
}