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

            

            if (File.Exists(@"C:\WEBProjekat\Dispeceri.txt"))
            {
                StreamReader sr = new StreamReader(@"C:\WEBProjekat\Dispeceri.txt");
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
                Korisnik k = new Korisnik("petar", "petar", "Petar", "Varga", Pol.MUSKI, "1508996800056", "0695057788", "petar.varga@protonmail.com");
                Database.registrovaniKorisnici.Add(k.korisnickoIme, k);
                Korisnik k2 = new Korisnik("zarko", "dsa", "Zarko", "Mihajlovic", Pol.MUSKI, "1904997800054", "0645177899", "nesto@gmail.com");
                k2.uloga = Uloga.VOZAC;
                Vozac v = new Vozac(k2, null, null);
                Automobil a = new Automobil(v, "2011", "NS-053-ZJ", "1", TipAutomobila.PUTNICKI_AUTOMOBIL);
                v.automobil = a;
                a.vozac = v;
                Database.registrovaniKorisnici.Add(k2.korisnickoIme, k2);
                Database.vozaci.Add(v.korisnickoIme, v);
                Database.slobodniVozaci.Add(v.korisnickoIme, v);
                //OVO!!!!!!!!!!!

                sr.Close();
            }
        }
    }
}
