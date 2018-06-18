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
    }
}