using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using Microsoft.Win32.SafeHandles;
using static ConsoleApp1.Settings;

namespace ConsoleApp1
{
    class TikruInfo
    {
        public DateTime LaskutuksenAloitus { get; set; }
        public string MittarinSijainti { get; set; }
        public string LoggerinSarjanumero { get; set; }
        public string MittauksestaVastaava { get; set; }
        public string HuoltoraportinVastaanottaja { get; set; }
        public string NoutoPaiva { get; set; }
        public string Asentaja { get; set; }
        public string ProjektinVetaja { get; set; }
        public string ProjektinNimi { get; set; }
        public string ProjektinNumero { get; set; }
        public string Asiakas { get; set; }
        public string AsennuksenLisatiedot { get; set; }

        public int CountTikruRecords { get; set; }
        public GetAva AvaErrorReport { get; set; }
        public SigicomFTP SigicomErrorReport { get; set; }
        public List<TikruInfo> errorReportList { get; set; }
        public SafeFileHandle ToSaveFileTo { get; private set; }

        public int TikruRecordsCount(TikruInfo info)
        {
            TikruInfo Count = new TikruInfo();
            Count = info;
            return Convert.ToInt32(info.CountTikruRecords);
        }


        public StreamReader TikruLogIn()
        {
            Console.WriteLine("Syötä Tikrun käyttäjätunnus ja paina enter:");
            string username = Console.ReadLine();
            Console.WriteLine("Syötä salasana ja paina enter:");

            //Salasanan piilottaminen
            var password = string.Empty;
            ConsoleKey key;
            do
            {
                var keyInfo = Console.ReadKey(intercept: true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && password.Length > 0)
                {
                    Console.Write("\b \b");
                    password = password[0..^1];
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write("*");
                    password += keyInfo.KeyChar;
                }
            } while (key != ConsoleKey.Enter);
            Console.WriteLine();

            //Kirjautuminen Tikruun/ERP-järjestelmään + CSV-tiedoston lataus
            var client = new CookieAwareWebClient();
            client.BaseAddress = @"http://xrm.forcit.org/FI01081666/";
            var loginData = new NameValueCollection();
            loginData.Add("username", username);
            loginData.Add("password", password);
            Console.WriteLine("Kirjaudutaan Tikruun...");
            client.UploadValues("index.php?module=Users&action=Login", "POST", loginData);
            string htmlSource = client.DownloadString("index.php");
            Console.WriteLine();
            if (htmlSource.Contains("Mittariasennukset"))
            {
                Console.WriteLine("Kirjautuminen onnistui. Ladataan tärinämittariasennusten tietoja... (tässä saattaa kestää hetki)");
            }
            else
            {
                Console.WriteLine("Kirjautuminen ei onnistunut");
                Environment.Exit(0);
            }

            var tikruByte = client.DownloadData("index.php?module=Reports&view=ExportReport&mode=GetCSV&record=222");
            Console.WriteLine("Tikrun tiedot ladattu onnistuneesti.");
            string tikruString = Encoding.UTF8.GetString(tikruByte, 0, tikruByte.Length);
            var tikruStreamReader = new StreamReader(new MemoryStream(tikruByte));

            return tikruStreamReader;
        }
        public TikruInfo[] GetTikruInfo()
        {
            TikruInfo tikru = new TikruInfo();
            // Hakee Tikrun palvelimelta laskutus ja mittaritiedot sisältävän .CSV raportin. (automaticStreamReader)
            var automaticStreamReader = tikru.TikruLogIn();

            // Hakee Tikrun tiedot paikalliselta .CSV tiedostolta (käytössä vaín debugausta varten) (manualStreamReader)

            using (var manualStreamReader = new StreamReader(@"C:\Users\fortokan\Desktop\UNIT SERVICE-REPORT\Tikru.csv"))
            {
                using (var csvReader = new CsvReader(automaticStreamReader, CultureInfo.InvariantCulture))


                {
                    var records = csvReader.GetRecords<dynamic>().ToList();

                    //CSV-listan indeksien määrä
                    int LoggerCounter = records.Count();

                    TikruInfo[] TIKRUTAULU = new TikruInfo[LoggerCounter];
                    int indexcount = 0;

                    foreach (var item1 in records)
                    {
                        TikruInfo TIKRUOBJECT = new TikruInfo();

                        //Asennusten tiedot tallennetaan TIKRUOBJECT-objektiin
                        foreach (var item in item1)
                        {
                            if (item.Key == "Mittariasennukset Projekti")
                            {
                                TIKRUOBJECT.ProjektinNimi = item.Value;
                            }
                            if (item.Key == "Mittariasennukset Mittalaitteen sijainti")
                            {
                                TIKRUOBJECT.MittarinSijainti = item.Value;
                            }
                            if (item.Key == "Mittariasennukset Laskutuksen aloituspvm")
                            {
                                if (item.Value != "-")
                                {
                                    TIKRUOBJECT.LaskutuksenAloitus = DateTime.ParseExact(item.Value, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                                }
                            }
                            if (item.Key == "Mittariasennukset Ohjattu")
                            {
                                TIKRUOBJECT.Asentaja = item.Value;
                            }
                            if (item.Key == "Projektit Ohjattu käyttäjälle")
                            {
                                TIKRUOBJECT.ProjektinVetaja = item.Value;
                            }
                            if (item.Key == "Projektit Mittauksesta vastaava")
                            {
                                TIKRUOBJECT.MittauksestaVastaava = item.Value;
                            }
                            if (TIKRUOBJECT.ProjektinVetaja != null)
                            {
                                if (TIKRUOBJECT.ProjektinVetaja != "")
                                {
                                    TIKRUOBJECT.HuoltoraportinVastaanottaja = TIKRUOBJECT.ProjektinVetaja;
                                }
                            }
                            if (TIKRUOBJECT.MittauksestaVastaava != null)
                            {
                                if (TIKRUOBJECT.MittauksestaVastaava != "")
                                {
                                    if (TIKRUOBJECT.MittauksestaVastaava == TIKRUOBJECT.ProjektinVetaja)
                                    {

                                    }
                                    if (TIKRUOBJECT.MittauksestaVastaava != TIKRUOBJECT.ProjektinVetaja)
                                    {
                                        TIKRUOBJECT.HuoltoraportinVastaanottaja = TIKRUOBJECT.MittauksestaVastaava;
                                    }

                                }
                            }


                            if (item.Key == "Projektit Projektin numero")
                            {
                                TIKRUOBJECT.ProjektinNumero = item.Value;
                            }

                            if (item.Key == "Mittariasennukset Mittalaite")
                            {
                                TIKRUOBJECT.LoggerinSarjanumero = item.Value;
                                if (TIKRUOBJECT.LoggerinSarjanumero.Contains("im"))
                                {
                                    TIKRUOBJECT.LoggerinSarjanumero = TIKRUOBJECT.LoggerinSarjanumero.Replace("im", "IM");
                                }

                                if (TIKRUOBJECT.LoggerinSarjanumero.Contains("ava"))
                                {
                                    TIKRUOBJECT.LoggerinSarjanumero = TIKRUOBJECT.LoggerinSarjanumero.Replace("ava", "Ava");
                                }
                                if (TIKRUOBJECT.LoggerinSarjanumero.Contains("avat"))
                                {
                                    TIKRUOBJECT.LoggerinSarjanumero = TIKRUOBJECT.LoggerinSarjanumero.Replace("avat", "AvaT");
                                }
                                if (TIKRUOBJECT.LoggerinSarjanumero.Contains("Avat"))
                                {
                                    TIKRUOBJECT.LoggerinSarjanumero = TIKRUOBJECT.LoggerinSarjanumero.Replace("Avat", "AvaT");
                                }
                                if (TIKRUOBJECT.LoggerinSarjanumero.Contains("LAINA"))
                                {
                                    TIKRUOBJECT.LoggerinSarjanumero = TIKRUOBJECT.LoggerinSarjanumero.Replace(" LAINA", "");
                                }
                                if (TIKRUOBJECT.LoggerinSarjanumero.Contains("abe"))
                                {
                                    TIKRUOBJECT.LoggerinSarjanumero = TIKRUOBJECT.LoggerinSarjanumero.Replace("abe", "ABE");
                                }

                            }

                            if (item.Key == "Projektit Asiakas")
                            {
                                TIKRUOBJECT.Asiakas = item.Value;
                            }
                            if (item.Key == "Mittariasennukset Lisätietoja")
                            {
                                TIKRUOBJECT.AsennuksenLisatiedot = item.Value;
                            }

                        }
                        TIKRUTAULU[indexcount] = TIKRUOBJECT;
                        indexcount++;

                    }


                    return TIKRUTAULU;
                }
            }

        }



        //-LASKUTUKSEN ALLA OLEVIEN ASENNUSTEN TALLENNUS YHTEEN LISTAAN
        //-ASENNUKSET TALLENNETAAN VIIMEISIMMÄN LASKUTUSPÄIVÄMÄÄRÄN PERUSTEELLA
        //-LISTASSA ON VAIN YKSI ASENNUSMERKINTÄ YHTÄ MITTARIA KOHTI
        public List<TikruInfo> FindDuplicates(TikruInfo[] TikruInfoTable, int TikruRecordsCount)
        {
            //TUPLASTI LASKULLA OLEVIEN MITTAREIDEN TIETOJEN LAJITTELU KAHDELLE ERI LISTALLE
            //Apumuuttujat looppeja varten
            int index = Convert.ToInt32(TikruRecordsCount);
            int arrayIndex = (index - 1);
            string[] sarjanumerojenVertailu = new string[index];

            //Loggerien sarjanumerot tallennetaan uuteen taulukkoon (sarjanumerotaulukkoon)
            foreach (var item in TikruInfoTable)
            {
                sarjanumerojenVertailu[arrayIndex] = item.LoggerinSarjanumero;
                arrayIndex = arrayIndex - 1;
            }
            //Apulista tuplana laskettavista loggereista
            List<string> ListaDuplikaateista_APULISTA = new List<string>();
            //LISTA TUPLANA OLEVISTA LOGGEREISTA
            List<string> ListaDuplikaateista = new List<string>();

            arrayIndex = (index - 1);
            foreach (var item in TikruInfoTable)
            {

                if (item.LoggerinSarjanumero == sarjanumerojenVertailu[arrayIndex])
                {
                    arrayIndex = arrayIndex - 1;

                    //Loggerien ID:n tallennus listaan jossa on pelkät laskutettavat duplikaatit
                    if (ListaDuplikaateista_APULISTA.Contains(item.LoggerinSarjanumero))
                    {
                        if (ListaDuplikaateista.Contains(item.LoggerinSarjanumero))
                        {

                        }
                        else
                        {
                            ListaDuplikaateista.Add(item.LoggerinSarjanumero);
                            continue;
                        }
                    }
                    else
                    {
                        ListaDuplikaateista_APULISTA.Add(item.LoggerinSarjanumero);
                    }

                }
            }

            List<string> allSerialNumbers = new List<string>();
            foreach (var item in TikruInfoTable)
            {
                allSerialNumbers.Add(item.LoggerinSarjanumero);
            }

            List<string> allSerialNumbersOnce = new List<string>();
            allSerialNumbersOnce = allSerialNumbers.Distinct().ToList();

            //LISTA EI DUPLIKAATEISTA
            List<string> ListaEiDuplikaateista = new List<string>();

            foreach (var item in allSerialNumbersOnce)
            {
                bool ohitaKirjoitus = false;

                foreach (var item2 in ListaDuplikaateista)
                {

                    if (item == item2)
                    {
                        ohitaKirjoitus = true;
                        break;
                    }

                }

                if (ohitaKirjoitus == false)
                {
                    ListaEiDuplikaateista.Add(item);
                }

            }

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////                        


            // *** ListaDuplikaateista tallennetaan objekteina listaan ***
            List<TikruInfo> TUPLAFINAL = new List<TikruInfo>();
            TikruInfo väliaikaObjekti = new TikruInfo();

            foreach (var item in ListaDuplikaateista)
            {
                väliaikaObjekti = null;

                foreach (var item2 in TikruInfoTable)
                {
                    if (item == item2.LoggerinSarjanumero)
                    {
                        if (väliaikaObjekti != null)
                        {
                            if (väliaikaObjekti.LaskutuksenAloitus < item2.LaskutuksenAloitus || väliaikaObjekti.LaskutuksenAloitus == item2.LaskutuksenAloitus)
                            {
                                väliaikaObjekti = item2;
                            }

                        }
                        if (väliaikaObjekti == null)
                        {
                            väliaikaObjekti = item2;
                        }

                    }
                }
                TUPLAFINAL.Add(väliaikaObjekti);

            }

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //ListaEiDuplikaateista

            // *** ListaEiDuplikaateista tallennetaan objekteina listaan ***
            List<TikruInfo> YKSITTÄISFINAL = new List<TikruInfo>();

            foreach (var item in ListaEiDuplikaateista)
            {
                väliaikaObjekti = null;

                foreach (var item2 in TikruInfoTable)
                {
                    if (item == item2.LoggerinSarjanumero)
                    {
                        if (väliaikaObjekti != null)
                        {
                            if (väliaikaObjekti.LaskutuksenAloitus < item2.LaskutuksenAloitus || väliaikaObjekti.LaskutuksenAloitus == item2.LaskutuksenAloitus)
                            {
                                väliaikaObjekti = item2;
                            }

                        }
                        if (väliaikaObjekti == null)
                        {
                            väliaikaObjekti = item2;
                        }

                    }
                }
                YKSITTÄISFINAL.Add(väliaikaObjekti);
            }

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////

            // *** YKSITTÄISFINAL ja TUPLAFINAL-listojen yhdistäminen SEMIFINAL-listaksi ***
            List<TikruInfo> SEMIFINAL = new List<TikruInfo>();

            foreach (var item in TUPLAFINAL)
            {
                SEMIFINAL.Add(item);
            }
            foreach (var item in YKSITTÄISFINAL)
            {
                SEMIFINAL.Add(item);
            }


            //////////////////////////////////////////////////////////////////////////////////////////////////////////////

            // *** Tässä SEMIFINAL-listasta poistetaan asennukset joissa ei ole määritelty laskutuksen aloituspäivämäärää. Loput lisätään FINAL-listaan ***
            List<TikruInfo> FINAL = new List<TikruInfo>();

            foreach (var item in SEMIFINAL)
            {

                DateTime a = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", CultureInfo.InvariantCulture);
                if (item.LaskutuksenAloitus > a)
                {
                    FINAL.Add(item);
                }

            }

            return FINAL;

        }












    }
}

