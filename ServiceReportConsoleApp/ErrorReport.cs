using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ConsoleApp1
{
    class ErrorReport
    {


        public List<TikruInfo> UnitErrorReport(List<SigicomFTP> sigicomErrors, List<GetAva> AvaErrors, List<TikruInfo> Tikru)
        {
            List<TikruInfo> ErrorReportList = new List<TikruInfo>();
            TikruInfo ErrorReportObject = new TikruInfo();


            //Viallisten Sigicom-mittarien ja asennusten tietojen yhdistäminen (Tikruinfo / asennus-objektiin lisätään laitteen mallista riippuen vikaraportti-objekti)
            foreach (var Sigicom_item in sigicomErrors)
            {

                foreach (var Tikru_item in Tikru)
                {
                    //string trimmedLoggerinSarjanumero = Tikru_item.LoggerinSarjanumero.Replace(@"-", "");
                    if (Sigicom_item.sigicomId == Tikru_item.LoggerinSarjanumero)
                    {
                        ErrorReportObject = Tikru_item;
                        ErrorReportObject.SigicomErrorReport = Sigicom_item;
                        ErrorReportList.Add(ErrorReportObject);
                        break;
                    }
                }


            }
            //Viallisten AVA-mittarien ja asennusten tietojen yhdistäminen (Tikruinfo / asennus-objektiin lisätään laitteen mallista riippuen vikaraportti-objekti)
            foreach (var Ava_item in AvaErrors)
            {
                foreach (var Tikru_item in Tikru)
                {
                    string trimmed_AvaID = Ava_item.AvaID;
                    if (("AvaT-" + trimmed_AvaID) == Tikru_item.LoggerinSarjanumero || ("bs-" + trimmed_AvaID) == Tikru_item.LoggerinSarjanumero)
                    {
                        ErrorReportObject = Tikru_item;
                        ErrorReportObject.AvaErrorReport = Ava_item;
                        ErrorReportList.Add(ErrorReportObject);
                        break;
                    }
                }

            }


            return ErrorReportList;
        }
        // **************************** Listaa virheraportit listaan MITTARIASENTAJIEN perusteella ****************************
        public void OrderedByNames(List<TikruInfo> errorReportList)
        {
            List<string> nameList = new List<string>();
            List<TikruInfo> errorReports = new List<TikruInfo>();

            //Asentajien nimien erittely string-listaan
            foreach (var item in errorReportList)
            {
                if (item.Asentaja != "")
                {
                    if (!nameList.Contains(item.Asentaja))
                    {
                        nameList.Add(item.Asentaja);
                    }

                }
            }

            //Virheraporttien lisääminen uuteen listaan, jonka indeksien nimiksi annetaan asentajien nimet
            foreach (var item in nameList)
            {
                TikruInfo TikruInfo = new TikruInfo();
                List<TikruInfo> errorReportListByNames = new List<TikruInfo>();

                foreach (var item2 in errorReportList)
                {
                    if (item == item2.Asentaja)
                    {
                        errorReportListByNames.Add(item2);
                    }
                }
                TikruInfo.errorReportList = errorReportListByNames;
                TikruInfo.Asentaja = item;
                errorReports.Add(TikruInfo);
            }


            //Virheraporttien laadinta
            string report = "";
            foreach (var item in errorReports)
            {

                report = (report + ("Hei " + item.Asentaja + ", asentamasi mittarit saattavat vaatia huoltoa." + System.Environment.NewLine + System.Environment.NewLine));

                foreach (var item2 in item.errorReportList)
                {
                    if (item2.SigicomErrorReport != null)
                    {

                        report = (report + item2.SigicomErrorReport.errorReport);
                        report = (report + "- Projekti: " + item2.ProjektinNimi + System.Environment.NewLine);
                        report = (report + "- Mittarin sijainti: " + item2.MittarinSijainti + System.Environment.NewLine);
                        report = (report + "- Laskutuksen aloituspäivämäärä: " + item2.LaskutuksenAloitus + System.Environment.NewLine);
                        report = (report + "----------------------------------------------------------------------------");
                        report = (report + System.Environment.NewLine + System.Environment.NewLine);
                    }
                    if (item2.AvaErrorReport != null)
                    {
                        report = (report + item2.AvaErrorReport.errorReport);
                        report = (report + "- Projekti: " + item2.ProjektinNimi + System.Environment.NewLine);
                        report = (report + "- Mittarin sijainti: " + item2.MittarinSijainti + System.Environment.NewLine);
                        report = (report + "- Laskutuksen aloituspäivämäärä: " + item2.LaskutuksenAloitus + System.Environment.NewLine);
                        report = (report + "----------------------------------------------------------------------------");
                        report = (report + System.Environment.NewLine + System.Environment.NewLine);
                    }

                }

                report = (report + "****************************************************************************" + System.Environment.NewLine + System.Environment.NewLine + System.Environment.NewLine);


            }
            //Log-tiedoston luonti. Logi sisältää kaikki huoltoraportit.
            string dir = @"C:\Users\fortokan\Desktop\UNIT SERVICE-REPORT\ErrorReportLog\";

            string logName = ("ErrorReportLog_ByInstaller_" + DateTime.Now);
            Console.WriteLine();
            System.IO.File.WriteAllText(dir + logName + ".txt", report);

            //Avaa ByInstaller log-tiedosto Notepadilla
            try
            {
                System.Diagnostics.Process.Start(dir + logName + ".txt");
            }
            catch (Exception)
            {

                Console.WriteLine("ByInstaller log-tiedoston avaaminen ei onnistunut.");
            }

            Console.WriteLine(report);
            Console.WriteLine();
        }


        // **************************** Listaa virheraportit listaan PROJEKTIEN VETÄJIEN perusteella ****************************
        public void OrderedByProjectOwners(List<TikruInfo> errorReportList)
        {
            List<string> nameList = new List<string>();
            List<TikruInfo> errorReports = new List<TikruInfo>();

            //Projektinvetäjien nimien erittely string-listaan
            foreach (var item in errorReportList)
            {
                if (item.HuoltoraportinVastaanottaja != "")
                {
                    if (!nameList.Contains(item.HuoltoraportinVastaanottaja))
                    {
                        nameList.Add(item.HuoltoraportinVastaanottaja);
                    }

                }
            }

            //Virheraporttien selaaminen ja lisääminen uuteen listaan, jonka indeksien nimiksi annetaan projektin vetäjien nimet
            foreach (var item in nameList)
            {
                TikruInfo TikruInfo = new TikruInfo();
                List<TikruInfo> errorReportListByNames = new List<TikruInfo>();

                foreach (var item2 in errorReportList)
                {
                    if (item == item2.HuoltoraportinVastaanottaja)
                    {
                        errorReportListByNames.Add(item2);
                    }
                }
                TikruInfo.errorReportList = errorReportListByNames;
                TikruInfo.HuoltoraportinVastaanottaja = item;
                errorReports.Add(TikruInfo);
            }


            //Virheraporttien laadinta
            string report = "";
            foreach (var item in errorReports)
            {

                report = (report + ("Hei " + item.HuoltoraportinVastaanottaja + ", projekteillasi olevat mittarit saattavat vaatia huoltoa." + System.Environment.NewLine + System.Environment.NewLine));

                foreach (var item2 in item.errorReportList)
                {
                    //SIGICOM RAPORTIN LUONTI
                    if (item2.SigicomErrorReport != null)
                    {
                        report = (report + item2.SigicomErrorReport.errorReport);

                        if (item2.MittauksestaVastaava != null && item2.MittauksestaVastaava != "" && item2.MittauksestaVastaava != item2.ProjektinVetaja)
                        {
                            report = (report + System.Environment.NewLine + "Sinut on merkitty Tikruun mittauksesta vastaavaksi tälle projektille." + System.Environment.NewLine);
                        }
                        report = (report + "- Projekti: " + item2.ProjektinNimi + System.Environment.NewLine);
                        report = (report + "- Mittarin sijainti: " + item2.MittarinSijainti + System.Environment.NewLine);
                        report = (report + "- Laskutuksen aloituspäivämäärä: " + item2.LaskutuksenAloitus + System.Environment.NewLine);

                        if (item2.MittauksestaVastaava != null && item2.MittauksestaVastaava != "" && item2.MittauksestaVastaava != item2.ProjektinVetaja)
                        {
                            report = (report + "- Projektista vastaava: " + item2.ProjektinVetaja + System.Environment.NewLine);
                        }

                        report = (report + "- Mittarin asentaja: " + item2.Asentaja + System.Environment.NewLine);

                        if (item2.AsennuksenLisatiedot != "-")
                        {
                            report = (report + "- Mittariasennuksen lisätiedot: " + item2.AsennuksenLisatiedot + System.Environment.NewLine);
                        }

                        report = (report + "----------------------------------------------------------------------------");
                        report = (report + System.Environment.NewLine + System.Environment.NewLine);
                    }

                    // AVA RAPORTIN LUONTI
                    if (item2.AvaErrorReport != null)
                    {
                        report = (report + item2.AvaErrorReport.errorReport);

                        if (item2.MittauksestaVastaava != null && item2.MittauksestaVastaava != "" && item2.MittauksestaVastaava != item2.ProjektinVetaja)
                        {
                            report = (report + System.Environment.NewLine + "Sinut on merkitty Tikruun mittauksesta vastaavaksi tälle projektille." + System.Environment.NewLine);
                        }

                        report = (report + "- Projekti: " + item2.ProjektinNimi + System.Environment.NewLine);
                        report = (report + "- Mittarin sijainti: " + item2.MittarinSijainti + System.Environment.NewLine);
                        report = (report + "- Laskutuksen aloituspäivämäärä: " + item2.LaskutuksenAloitus + System.Environment.NewLine);

                        if (item2.MittauksestaVastaava != null && item2.MittauksestaVastaava != "" && item2.MittauksestaVastaava != item2.ProjektinVetaja)
                        {
                            report = (report + "- Projektista vastaava: " + item2.ProjektinVetaja + System.Environment.NewLine);
                        }

                        report = (report + "- Mittarin asentaja: " + item2.Asentaja + System.Environment.NewLine);

                        if (item2.AsennuksenLisatiedot != "-")
                        {
                            report = (report + "- Mittariasennuksen lisätiedot: " + item2.AsennuksenLisatiedot + System.Environment.NewLine);
                        }

                        report = (report + "----------------------------------------------------------------------------");
                        report = (report + System.Environment.NewLine + System.Environment.NewLine);
                    }

                }

                report = (report + "****************************************************************************" + System.Environment.NewLine + System.Environment.NewLine + System.Environment.NewLine);


            }
            //Log-tiedoston luonti. Logi sisältää kaikki huoltoraportit.
            string dir = @"C:\Users\fortokan\Desktop\UNIT SERVICE-REPORT\ErrorReportLog\";

            string logName = ("ErrorReportLog_ByProjectOwner_" + DateTime.Now);
            Console.WriteLine();
            System.IO.File.WriteAllText(dir + logName + ".txt", report);

            //Avaa ProjectOwner log-tiedosto Notepadilla
            try
            {
                System.Diagnostics.Process.Start(dir + logName + ".txt");
            }
            catch (Exception)
            {

                Console.WriteLine("ProjectOwner log-tiedoston avaaminen ei onnistunut.");
            }

            Console.WriteLine(report);
            Console.WriteLine();
        }

    }
}
