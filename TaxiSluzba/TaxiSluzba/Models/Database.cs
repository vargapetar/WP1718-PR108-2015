using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace TaxiSluzba.Models
{
    public class Database
    {
        public static Dictionary<string, Korisnik> registrovaniKorisnici = new Dictionary<string, Korisnik>();
        public static Dictionary<string, Vozac> vozaci = new Dictionary<string, Vozac>();
        public static Dictionary<string, Vozac> slobodniVozaci = new Dictionary<string, Vozac>();
        public static Dictionary<string, Voznja> voznjeNaCekanju = new Dictionary<string, Voznja>(); // voznje koje se prikazuju dispeceru(kljuc je vreme porudzbine voznje)
        public static Dictionary<string, Voznja> neuspesneVoznje = new Dictionary<string, Voznja>(); //kljuc je vreme voznje
        public static Dictionary<string, Voznja> voznjeNepoznatihDisp = new Dictionary<string, Voznja>(); //kljuce je vreme voznje
        public static Dictionary<string, Voznja> sveVoznje = new Dictionary<string, Voznja>(); //sve moguce voznje u sistemu, kljuc je datum voznje

        //baza
        public static void UpisiVoznje()
        {
            string ret = "";
            string pom = "";

            if (File.Exists(@"C:\Users\Petar\Desktop\WP1718-PR108-2015\TaxiSluzba\SveVoznje.txt"))
            {
                StreamReader sr = new StreamReader(@"C:\Users\Petar\Desktop\WP1718-PR108-2015\TaxiSluzba\SveVoznje.txt");

                //while((pom = sr.ReadLine()) != null)
                //{
                //    ret += pom + "\n";
                //}

                foreach (Voznja v in sveVoznje.Values)
                {
                    ret += string.Format(v.datumVreme.ToString() + "#" + v.dispecer.korisnickoIme + "#" + v.iznos + "#" + v.komentar.opis + "#" +
                                        v.komentar.ocenaVoznje.ToString() + "#" + v.komentar.datumVreme.ToString() + "#" + v.lokacijaZaTaksi.Adress.Ulica + "#" +
                                        v.lokacijaZaTaksi.Adress.BrojUlice + "#" + v.lokacijaZaTaksi.Adress.Grad + "#" + v.lokacijaZaTaksi.Adress.PozivniBroj + "#" +
                                        v.musterija.korisnickoIme + "#" + v.odrediste.Adress.Ulica + "#" +
                                        v.odrediste.Adress.BrojUlice + "#" + v.odrediste.Adress.Grad + "#" + v.odrediste.Adress.PozivniBroj + "#" +
                                        v.statusVoznje.ToString() + "#" + v.tipAutomobila.ToString() + "#" + v.vozac.korisnickoIme + "\n");
                }

                sr.Close();

                StreamWriter sw = new StreamWriter(@"C:\Users\Petar\Desktop\WP1718-PR108-2015\TaxiSluzba\SveVoznje.txt");
                sw.WriteLine(ret);
                sw.Close();
            }
            else
            {
                foreach (Voznja v in sveVoznje.Values)
                {
                    ret += string.Format(v.datumVreme.ToString() + "#" + v.dispecer.korisnickoIme + "#" + v.iznos + "#" + v.komentar.opis + "#" +
                                        v.komentar.ocenaVoznje.ToString() + "#" + v.komentar.datumVreme.ToString() + "#" + v.lokacijaZaTaksi.Adress.Ulica + "#" +
                                        v.lokacijaZaTaksi.Adress.BrojUlice + "#" + v.lokacijaZaTaksi.Adress.Grad + "#" + v.lokacijaZaTaksi.Adress.PozivniBroj + "#" +
                                        v.musterija.korisnickoIme + "#" + v.odrediste.Adress.Ulica + "#" +
                                        v.odrediste.Adress.BrojUlice + "#" + v.odrediste.Adress.Grad + "#" + v.odrediste.Adress.PozivniBroj + "#" +
                                        v.statusVoznje.ToString() + "#" + v.tipAutomobila.ToString() + "#" + v.vozac.korisnickoIme + "\n");
                }

                StreamWriter sw = new StreamWriter(@"C:\Users\Petar\Desktop\WP1718-PR108-2015\TaxiSluzba\SveVoznje.txt");
                sw.WriteLine(ret);
                sw.Close();
            }
        }

        public static void UpisiVozace()
        {
            string ret = "";
            string pom = "";

            if (File.Exists(@"C:\Users\Petar\Desktop\WP1718-PR108-2015\TaxiSluzba\SviVozaci.txt"))
            {
                StreamReader sr = new StreamReader(@"C:\Users\Petar\Desktop\WP1718-PR108-2015\TaxiSluzba\SviVozaci.txt");

                //while ((pom = sr.ReadLine()) != null)
                //{
                //    ret += pom + "\n";
                //}

                foreach (Vozac v in vozaci.Values)
                {
                    ret += string.Format(v.korisnickoIme + "#" + v.lozinka + "#" + v.ime + "#" + v.prezime + "#" + v.pol.ToString() + "#" +
                                        v.email + "#" + v.jmbg + "#" + v.telefon + "#" + v.automobil.brojTaksija + ":" + v.automobil.godisteAutomobila + ":" + 
                                        v.automobil.registracija + ":" + v.automobil.tipAutomobila.ToString() + "#" + 
                                        v.lokacija.Adress.Ulica + ":" + v.lokacija.Adress.BrojUlice + ":" + v.lokacija.Adress.Grad + ":" + v.lokacija.Adress.PozivniBroj + "#" +
                                        v.uloga.ToString() + "\n");
                }

                sr.Close();

                StreamWriter sw = new StreamWriter(@"C:\Users\Petar\Desktop\WP1718-PR108-2015\TaxiSluzba\SviVozaci.txt");
                sw.WriteLine(ret);
                sw.Close();
            }
            else
            {
                if (vozaci.Count != 0)
                {
                    foreach (Vozac v in vozaci.Values)
                    {
                        ret += string.Format(v.korisnickoIme + "#" + v.lozinka + "#" + v.ime + "#" + v.prezime + "#" + v.pol.ToString() + "#" +
                                            v.email + "#" + v.jmbg + "#" + v.telefon + "#" + v.automobil.brojTaksija + ":" + v.automobil.godisteAutomobila + ":" +
                                            v.automobil.registracija + ":" + v.automobil.tipAutomobila.ToString() + "#" +
                                            v.lokacija.Adress.Ulica + ":" + v.lokacija.Adress.BrojUlice + ":" + v.lokacija.Adress.Grad + ":" + v.lokacija.Adress.PozivniBroj + "#" +
                                            v.uloga.ToString() + "\n");
                    }

                    StreamWriter sw = new StreamWriter(@"C:\Users\Petar\Desktop\WP1718-PR108-2015\TaxiSluzba\SviVozaci.txt");
                    sw.WriteLine(ret);
                    sw.Close();
                }
            }
        }

        public static void UpisiRegKorisnike()
        {
            string ret = "";

            if (File.Exists(@"C:\Users\Petar\Desktop\WP1718-PR108-2015\TaxiSluzba\RegistrovaniKorisnici.txt"))
            {
                StreamReader sr = new StreamReader(@"C:\Users\Petar\Desktop\WP1718-PR108-2015\TaxiSluzba\RegistrovaniKorisnici.txt");

                foreach (Korisnik k in registrovaniKorisnici.Values)
                {
                    if(k.uloga != Uloga.DISPECER)
                        ret += string.Format(k.korisnickoIme + "#" + k.lozinka + "#" + k.ime + "#" + k.prezime + "#" + k.email + "#" + k.jmbg + "#" + k.pol.ToString() + "#" + k.telefon + "#" + k.uloga.ToString() + "\n");
                }

                sr.Close();

                StreamWriter sw = new StreamWriter(@"C:\Users\Petar\Desktop\WP1718-PR108-2015\TaxiSluzba\RegistrovaniKorisnici.txt");
                sw.WriteLine(ret);
                sw.Close();
            }
            else
            {
                foreach (Korisnik k in registrovaniKorisnici.Values)
                {
                    if(k.uloga != Uloga.DISPECER)
                        ret += string.Format(k.korisnickoIme + "#" + k.lozinka + "#" + k.ime + "#" + k.prezime + "#" + k.email + "#" + k.jmbg + "#" + k.pol.ToString() + "#" + k.telefon + "#" + k.uloga.ToString() + "\n");
                }

                StreamWriter sw = new StreamWriter(@"C:\Users\Petar\Desktop\WP1718-PR108-2015\TaxiSluzba\RegistrovaniKorisnici.txt");
                sw.WriteLine(ret);
                sw.Close();
            }
        }
    }
}