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

        public static void UpisiUTXT(Registration r)
        {
            string pom = "";

            if (File.Exists(@"C:\WEBProjekat\RegistrovaniKorisnici.txt"))
            {
                StreamReader sr = new StreamReader(@"C:\WEBProjekat\RegistrovaniKorisnici.txt");
                pom = sr.ReadToEnd();
                sr.Close();
            }

            string res = pom + string.Format(r.korisnickoIme + "_" + r.lozinka);

            StreamWriter sw = new StreamWriter(@"C:\WEBProjekat\RegistrovaniKorisnici.txt");
            sw.WriteLine(res);
            sw.Close();
        }

        //public static void CitajIzTxt()
        //{
        //    string pom = "";
        //    string[] res;
        //    Pol gender;

        //    if (File.Exists(@"C:\WEBProjekat\Dispeceri.txt"))
        //    {
        //        StreamReader sr = new StreamReader(@"C:\WEBProjekat\Dispeceri.txt");
        //        while((pom = sr.ReadLine()) != null)
        //        {
        //            res = pom.Split('_');

        //            if (res[4] == "MUSKI")
        //                gender = Pol.MUSKI;
        //            else
        //                gender = Pol.ZENSKI;

        //            Dispecer d = new Dispecer(res[0], res[1], res[2], res[3], gender, res[5], res[6], res[7]);
        //            registrovaniKorisnici.Add(res[0], d);
        //        }
        //        sr.Close();
        //    }
        //}
    }
}