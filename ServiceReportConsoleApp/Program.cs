using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper;
using System.Globalization;
using System.Linq;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;


namespace ConsoleApp1
{


    class Program
    {
        static void Main(string[] args)
        {
            HelpFunctions helpFunctions = new HelpFunctions();
            TikruInfo TikruInfo = new TikruInfo();
            SigicomFTP SigicomFTP = new SigicomFTP();
            GetAva GetAva = new GetAva();
            LoggerErrors LoggerErrors = new LoggerErrors();
            ErrorReport ErrorReport = new ErrorReport();

            //"TikruInfoTable"-oliotaulukko Tikruinfoja varten. (Taulukon indeksit määräytyvät TikruInfo-luokan "CountTikruRecords"-metodin kautta.)
            TikruInfo[] TikruInfoTable = new TikruInfo[0];
            // "ListWithoutDuplicates"-oliolista Tikruinfoja varten. (Taulukon tiedot palautuvat "FindDuplicates"-metodilta = tupla laskutettavat mittarit jäävät pois.)
            List<TikruInfo> TikruInfoList = new List<TikruInfo>();
            TikruInfoTable = TikruInfo.GetTikruInfo();
            int TikruInfoCountRecords = TikruInfoTable.Count();
            //Etsii mittarien "duplikaatit" Tikrun/ERP:n tietueista
            TikruInfoList = TikruInfo.FindDuplicates(TikruInfoTable, TikruInfoCountRecords);


            //WebClient objekti useampaa html hakua varten
            Settings.CookieAwareWebClient client = GetAva.GetAvanetClient();
            //Kaikkien palvelimella olevien AVA-tärinämittarien lista (html-haku)
            List<GetAva> AvaLIST = new List<GetAva>();
            AvaLIST = GetAva.GetAvaFromTxtReport(client);

            //Manuaalisen/automaattisen AVA paristolistan luku            
            Console.WriteLine("Suoritetaanko manuaalisen AVA paristolistan luku?");
            Console.WriteLine("'Y' = Kyllä. Mikäli haluat automaattisen haun, paina mitä tahansa näppäintä.");
            string lueParistoLista = Console.ReadLine();
            if (lueParistoLista == "Y" || lueParistoLista == "y")
            {
                AvaLIST = GetAva.ManualAvaBatteryErrors(AvaLIST);
                Console.WriteLine("Manuaalisen AVA paristolistan luku suoritetaan.");
            }
            else
            {
                Console.WriteLine("Automaattisen AVA paristolistan luku suoritetaan.");
                double AVAbatteryInput = 0;
                while (AVAbatteryInput == 0)
                {
                    try
                    {
                        Console.WriteLine(" Syötä AVA paristojen jännitteen raja-arvo yhden tai kahden desimaalin tarkkuudella (esim: '6,5'). Suorita painamalla enter.");
                        AVAbatteryInput = Convert.ToDouble(Console.ReadLine());
                        Console.WriteLine("Lataus aloitetaan...");
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Virheellinen syöte.");
                    }

                }
                AvaLIST = GetAva.AutomaticAvaBatteryErrors(AvaLIST, client, AVAbatteryInput);

            }
            Console.WriteLine();



            //Kaikkien palvelimella olevien Sigicom-mittarien lista
            List<SigicomFTP> SigicomLIST = new List<SigicomFTP>();
            SigicomLIST = SigicomFTP.SaveStatusFiles();

            //Viallisten Sigicom-mittarien lista ("GetSigicomErrors"-metodi palauttaa listan, johon on tallennettu vialliset Sigicom mittarit.)
            List<SigicomFTP> SigicomErrors = new List<SigicomFTP>();
            SigicomErrors = LoggerErrors.GetSigicomErrors(SigicomLIST);
            List<GetAva> AvaErrors = new List<GetAva>();
            AvaErrors = LoggerErrors.GetAvaErrors(AvaLIST);

            //DEBUG FUNCTIONS
            //SAVE SIGICOM RECORDS TO A TEMPORARY FILE\\             
            helpFunctions.CreateSigicomObjectFile(SigicomErrors, "sigicomListFile");
            //Open temporary Sigicom file
            SigicomErrors = helpFunctions.OpenSigicomObjectFile("sigicomListFile");
            //**

            //Asennusten ja mittareiden tiedot kattavan vikaraportin luonti ErrorReport-luokassa            
            TikruInfoList = ErrorReport.UnitErrorReport(SigicomErrors, AvaErrors, TikruInfoList);
            //Tuloksien tallennus log-tiedostoihin (viestit asentajille ja projektien vetäjille)
            ErrorReport.OrderedByNames(TikruInfoList);
            ErrorReport.OrderedByProjectOwners(TikruInfoList);


            Console.WriteLine("Mittarien vikaLista on luotu. Lopeta painamalla enter.");
            Console.ReadLine();





        }

    }

}
