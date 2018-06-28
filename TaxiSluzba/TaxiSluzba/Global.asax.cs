using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using TaxiSluzba.Models;

namespace TaxiSluzba
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            string pom = "";
            string[] res;
            Pol gender;

            

            if (File.Exists(@"C:\Users\Petar\Desktop\WP1718-PR108-2015\TaxiSluzba\Dispeceri.txt"))
            {
                StreamReader sr = new StreamReader(@"C:\Users\Petar\Desktop\WP1718-PR108-2015\TaxiSluzba\Dispeceri.txt");
                while ((pom = sr.ReadLine()) != null)
                {
                    res = pom.Split('_');

                    if (res[4] == "MUSKI")
                        gender = Pol.MUSKI;
                    else
                        gender = Pol.ZENSKI;

                    Dispecer d = new Dispecer(res[0], res[1], res[2], res[3], gender, res[5], res[6], res[7]);
                    Database.registrovaniKorisnici.Add(res[0], d);
                }

                //OBRISI
                //Korisnik k = new Korisnik("petar", "petar", "Petar", "Varga", Pol.MUSKI, "1508996800056", "0695057788", "petar.varga@protonmail.com");
                //Database.registrovaniKorisnici.Add(k.korisnickoIme, k);
                //Korisnik k2 = new Korisnik("zarko", "dsa", "Zarko", "Mihajlovic", Pol.MUSKI, "1904997800054", "0645177899", "nesto@gmail.com");
                //k2.uloga = Uloga.VOZAC;
                //Vozac v = new Vozac(k2, null, null);
                //Automobil a = new Automobil(v, "2011", "NS-053-ZJ", "1", TipAutomobila.PUTNICKI_AUTOMOBIL);
                //v.automobil = a;
                //a.vozac = v;
                //Database.registrovaniKorisnici.Add(k2.korisnickoIme, k2);
                //Database.vozaci.Add(v.korisnickoIme, v);
                //Database.slobodniVozaci.Add(v.korisnickoIme, v);
                //OVO!!!!!!!!!!!

                sr.Close();

                UcitajBazu();
            }
        }

        public void UcitajBazu()
        {
            string pom = "";
            string[] ret;

            //ucitavanje korisnika
            if (File.Exists(@"C:\Users\Petar\Desktop\WP1718-PR108-2015\TaxiSluzba\RegistrovaniKorisnici.txt"))
            {
                StreamReader srKorisnici = new StreamReader(@"C:\Users\Petar\Desktop\WP1718-PR108-2015\TaxiSluzba\RegistrovaniKorisnici.txt");
                while((pom = srKorisnici.ReadLine()) != "")
                {
                    ret = pom.Split('#');
                    Pol gender;

                    if (ret[6] == "MUSKI")
                        gender = Pol.MUSKI;
                    else
                        gender = Pol.ZENSKI;

                    Uloga uloga;
                    if (ret[8] == "MUSTERIJA")
                        uloga = Uloga.MUSTERIJA;
                    else if (ret[8] == "VOZAC")
                        uloga = Uloga.VOZAC;
                    else
                        uloga = Uloga.DISPECER;

                    Korisnik k = new Korisnik(ret[0], ret[1], ret[2], ret[3], gender, ret[5], ret[7], ret[4]);
                    k.uloga = uloga;

                    if (!Database.registrovaniKorisnici.ContainsKey(ret[0]))
                        Database.registrovaniKorisnici.Add(k.korisnickoIme, k);
                }
                srKorisnici.Close();   
            }

            //ucitavanje vozaca
            if (File.Exists(@"C:\Users\Petar\Desktop\WP1718-PR108-2015\TaxiSluzba\SviVozaci.txt"))
            {
                StreamReader srVozaci = new StreamReader(@"C:\Users\Petar\Desktop\WP1718-PR108-2015\TaxiSluzba\SviVozaci.txt");
                while ((pom = srVozaci.ReadLine()) != null)
                {
                    ret = pom.Split('#');
                    Pol gender;
                    string[] auto;
                    string[] lokacija;

                    if (ret[4] == "MUSKI")
                        gender = Pol.MUSKI;
                    else
                        gender = Pol.ZENSKI;

                    Korisnik k = new Korisnik(ret[0], ret[1], ret[2], ret[3], gender, ret[6], ret[7], ret[5]);
                    k.uloga = Uloga.VOZAC;
                    auto = ret[8].Split(':');
                    TipAutomobila tipAutomobila;
                    if (auto[3] == "KOMBI")
                        tipAutomobila = TipAutomobila.KOMBI;
                    else
                        tipAutomobila = TipAutomobila.PUTNICKI_AUTOMOBIL;
                    Automobil a = new Automobil(null, auto[1], auto[2], auto[0], tipAutomobila);
                    lokacija = ret[9].Split(':');
                    Lokacija l = new Lokacija(1, 1, new Adresa(lokacija[0], lokacija[1], lokacija[2], lokacija[3]));
                    Vozac v = new Vozac(k, l, a);
                    a.vozac = v;

                    if (!Database.vozaci.ContainsKey(v.korisnickoIme))
                        Database.vozaci.Add(v.korisnickoIme, v);
                }
                srVozaci.Close();
            }

            //ucitavanje voznji
            if (File.Exists(@"C:\Users\Petar\Desktop\WP1718-PR108-2015\TaxiSluzba\SveVoznje.txt"))
            {
                StreamReader srKorisnici = new StreamReader(@"C:\Users\Petar\Desktop\WP1718-PR108-2015\TaxiSluzba\SveVoznje.txt");
                while ((pom = srKorisnici.ReadLine()) != "")
                {
                    ret = pom.Split('#');
                    DateTime dtVoznje = DateTime.Parse(ret[0]);

                    OcenaVoznje ov;
                    if (ret[4] == "JEDAN")
                        ov = OcenaVoznje.JEDAN;
                    else if (ret[4] == "DVA")
                        ov = OcenaVoznje.DVA;
                    else if (ret[4] == "TRI")
                        ov = OcenaVoznje.TRI;
                    else if (ret[4] == "CETIRI")
                        ov = OcenaVoznje.CETIRI;
                    else if (ret[4] == "PET")
                        ov = OcenaVoznje.PET;
                    else
                        ov = OcenaVoznje.NULA;

                    DateTime dtKomentara = DateTime.Parse(ret[5]);

                    StatusVoznje sv;
                    if (ret[15] == "KREIRANA_NA_CEKANJU")
                        sv = StatusVoznje.KREIRANA_NA_CEKANJU;
                    else if (ret[15] == "FORMIRANA")
                        sv = StatusVoznje.FORMIRANA;
                    else if (ret[15] == "OBRADJENA")
                        sv = StatusVoznje.OBRADJENA;
                    else if (ret[15] == "PRIHVACENA")
                        sv = StatusVoznje.PRIHVACENA;
                    else if (ret[15] == "U_TOKU")
                        sv = StatusVoznje.U_TOKU;
                    else if (ret[15] == "OTKAZANA")
                        sv = StatusVoznje.OTKAZANA;
                    else if (ret[15] == "NEUSPESNA")
                        sv = StatusVoznje.NEUSPESNA;
                    else
                        sv = StatusVoznje.USPESNA;

                    TipAutomobila ta;
                    if (ret[16] == "KOMBI")
                        ta = TipAutomobila.KOMBI;
                    else
                        ta = TipAutomobila.PUTNICKI_AUTOMOBIL;

                    Lokacija l = new Lokacija(1, 1, new Adresa(ret[6], ret[7], ret[8], ret[9]));
                    Musterija m;
                    string musterija = ret[10];
                    if (ret[10] != "-")
                    {
                        Korisnik kor = Database.registrovaniKorisnici[musterija];
                        m = new Musterija(kor.korisnickoIme, kor.lozinka, kor.ime, kor.prezime, kor.pol, kor.jmbg, kor.telefon, kor.email);
                    }
                    else
                    {
                        m = new Musterija("-", "-", "-", "-", Pol.MUSKI, "-", "-", "-");
                    }
                    //vidi sta treba dodati musteriji da bude sve finoo, gde god cackam database obrati paznju
                    Voznja v = new Voznja(dtVoznje, l, ta, m);
                    v.odrediste = new Lokacija(1, 1, new Adresa(ret[11], ret[12], ret[13], ret[14]));
                    if (ret[1] != "-")
                        v.dispecer = (Dispecer)Database.registrovaniKorisnici[ret[1]];
                    else
                        v.dispecer = new Dispecer("-", "-", "-", "-", Pol.MUSKI, "-", "-", "-");
                    //obrati paznju na to kad su musterija ili disp nepoznati
                    if (ret[17] != "-")
                        v.vozac = Database.vozaci[ret[17]];
                    else
                    {
                        Lokacija lok = new Lokacija(1, 1, new Adresa("-", "-", "-", "-"));
                        v.vozac = new Vozac(new Korisnik("-", "-", "-", "-", Pol.MUSKI, "-", "-", "-"), lok, new Automobil(null, "-", "-", "-", TipAutomobila.KOMBI));
                    }
                    v.iznos = ret[2];
                    v.komentar = new Komentar(ret[3], dtKomentara, Database.registrovaniKorisnici[musterija], null, ov);
                    //dodaj voznju kad odradis u kontruktor komentara!!!! ***
                    v.statusVoznje = sv;
                    v.komentar.voznja = v;

                    Database.sveVoznje.Add(v.datumVreme.ToString(), v);

                    if (Database.registrovaniKorisnici.ContainsKey(musterija))
                        Database.registrovaniKorisnici[musterija].voznje.Add(v);

                    if (Database.vozaci.ContainsKey(ret[17]))
                        Database.vozaci[ret[17]].voznje.Add(v);
                }
                srKorisnici.Close();
            }
        }
    }
}
