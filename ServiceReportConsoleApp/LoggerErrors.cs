using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp1
{
    class LoggerErrors
    {
        //Viallisten Sigicom-mittarien määrittely vikakoodien perusteella. Vialliset mittarit palautetaan listana.
        public List<SigicomFTP> GetSigicomErrors(List<SigicomFTP> SigicomLIST)
        {
            //try
            //{
            //Lista johon kerätään huoltoa vaativat Sigicom mittarit
            List<SigicomFTP> SigicomErrors = new List<SigicomFTP>();
            Settings settings = new Settings();
            string SigicomErrorReport = "";
            string PrintError = "";



            foreach (var item in SigicomLIST)
            {

                SigicomErrorReport = "";
                double BatteryPercent = -1;
                double Temperature = 0;
                double Voltage = -1;
                bool ErrorTrueOrNot = false;

                //BatteryPercent
                if (item.D10_BatteryPercent != "")
                {

                    BatteryPercent = double.Parse(item.D10_BatteryPercent, System.Globalization.CultureInfo.InvariantCulture);
                }
                //Temperature
                if (item.D10_C12_Temperature != null)
                {

                    string S1 = item.D10_C12_Temperature;
                    try
                    {
                        if (S1.Length > 4)
                        {
                            S1 = item.D10_C12_Temperature.Substring(0, 4);
                        }
                    }
                    catch (Exception)
                    {

                        Console.WriteLine("IM Temperature error!");
                    }


                    Temperature = double.Parse(S1, System.Globalization.CultureInfo.InvariantCulture);
                }
                //Voltage
                if (item.IM_Voltage != null)
                {
                    string V1 = item.IM_Voltage;
                    string V2 = V1;

                    try
                    {
                        if (V1.Length > 5)
                        {
                            V2 = V1.Substring(0, 5);
                        }
                    }
                    catch (Exception)
                    {

                        Console.WriteLine("IM Voltage error!");
                    }

                    Voltage = double.Parse(V2, System.Globalization.CultureInfo.InvariantCulture);
                }


                PrintError = ("Laite: " + item.sigicomId + " on antanut virheilmoituksen järjestelmään." + System.Environment.NewLine + "Diagnoosi: " + System.Environment.NewLine);

                //Huoltoa vaativien mittarien määrittely ja tallentaminen

                //Connection missed = jos mittari ei ole ottanut yhteyttä viimeiseen vuorokauteen.
                if (item.connectionMissed == true)
                {
                    SigicomErrorReport = (SigicomErrorReport + "[CONNECTION MISSED] - " + settings.SetConnectionMissedInfo + " Viimeisin yhteydenottoaika: " + Convert.ToString(item.statusCreated) + System.Environment.NewLine);
                    ErrorTrueOrNot = true;
                }

                //Laitteen virta määrän määrittely = onko mittarin malli IM, C12 vai D10
                //C12 = IMxxxxx
                if (item.sigicomId.Contains("C12"))
                {
                    if (item.IM_Voltage != null && Voltage < 3.5)
                    {
                        SigicomErrorReport = (SigicomErrorReport + "[BATTERY LOW] - Laitteen virta on vähissä. Jäljellä oleva virta: " + Convert.ToString(Voltage) + "V" + System.Environment.NewLine);
                        ErrorTrueOrNot = true;
                    }
                }
                //D10 = IMxxxxxx
                if (item.sigicomId.Contains("D10"))
                {
                    if (item.IM_Voltage != null && Voltage < 11.7)
                    {
                        SigicomErrorReport = (SigicomErrorReport + "[BATTERY LOW] - Laitteen virta on vähissä. Jäljellä oleva virta: " + Convert.ToString(Voltage) + "V" + System.Environment.NewLine);
                        ErrorTrueOrNot = true;
                    }
                }

                //IM = IMxxxx
                if (item.sigicomId.Contains("IM") || item.sigicomId.Contains("ABE"))
                {
                    if (item.IM_Voltage != null && Voltage < 11.7)
                    {
                        SigicomErrorReport = (SigicomErrorReport + "[BATTERY LOW] - Laitteen virta on vähissä. Jäljellä oleva virta: " + Convert.ToString(Voltage) + "V" + System.Environment.NewLine + "(Mikäli laitteen virta on vain paristojen varassa, paristojen vaihtoa suositellaan lähiaikoina.)" + System.Environment.NewLine);
                        ErrorTrueOrNot = true;
                    }
                }

                //Akun varaus prosentteina (tämä ominaisuus vain Sigicom-D10 mittareissa.
                //Ilmoitetaan vikaraportissa vain silloin kun prosenttimäärä on alle 20%.)

                if (item.D10_BatteryPercent != "0.0" && BatteryPercent < 15 && BatteryPercent > -1)
                {
                    if (item.D10_BatteryPercent != null && BatteryPercent < 15 && BatteryPercent > -1)
                    {
                        if (item.D10_BatteryPercent != "" && BatteryPercent < 15 && BatteryPercent > -1)
                        {
                            SigicomErrorReport = (SigicomErrorReport + "Akkujen varausprosentti: " + Convert.ToString(BatteryPercent) + "%" + System.Environment.NewLine + "(Mikäli laitteen virtaa on jäljellä alle 15%, Sigicom D10-akkujen vaihtoa suositellaan lähiaikoina.)" + System.Environment.NewLine);
                            ErrorTrueOrNot = true;
                        }
                    }

                }

                //Nodelost = laitteen ja anturin välillä on yhteyshäiriö
                if (item.nodeLost == true)
                {
                    SigicomErrorReport = (SigicomErrorReport + "[NODE LOST] - Laitteen ja anturin välillä on yhteyshäiriö." + System.Environment.NewLine);
                    ErrorTrueOrNot = true;
                }

                //Memory low = jos muisti on alle 20mb
                if (item.memoryLow == true)
                {
                    string availableSpace = Convert.ToString(item.availableSpace);
                    //Low memory / availableSpace. Tietueen lopusta poistetaan 6 merkkiä (lopullinen MB tulos kahden desimaalin tarkkuudella).
                    availableSpace = availableSpace.Substring(0, availableSpace.Length - 6);

                    SigicomErrorReport = (SigicomErrorReport + "[MEMORY LOW] - Laitteen muisti on vähissä. Muistia on jäljellä: " + availableSpace + "MB." + System.Environment.NewLine + "(Mikäli muistia on alle 20MB, muistin tyhjentämistä suositellaan lähiaikoina.)" + System.Environment.NewLine);
                    ErrorTrueOrNot = true;
                }
                //Temperature = liian alhaisen tai korkean lämpötilan määrittely (-35 tai yli 40 astetta)
                if (item.D10_C12_Temperature != null && Temperature < -30 || Temperature > 40)
                {
                    if (Temperature < -30)
                    {
                        SigicomErrorReport = (SigicomErrorReport + "Laitteen lämpötila saattaa olla liian alhainen: -" + Convert.ToString(Temperature) + "°C" + System.Environment.NewLine);
                    }
                    if (Temperature > 40)
                    {
                        SigicomErrorReport = (SigicomErrorReport + "Laitteen lämpötila saattaa olla liian korkea: +" + Convert.ToString(Temperature) + "°C" + System.Environment.NewLine);
                    }
                    ErrorTrueOrNot = true;
                }

                //Huoltoraportti lisätään olioon, ja olio huollettavien asennusten/mittarien taulukkoon
                if (ErrorTrueOrNot)
                {
                    PrintError = (PrintError + SigicomErrorReport);
                    item.errorReport = PrintError;
                    //Huoltoraportti lisätään listaan
                    SigicomErrors.Add(item);
                }
            }
            return SigicomErrors;
        }

        //Viallisten AvaTrace-mittarien määrittely vikakoodien perusteella. Vialliset mittarit palautetaan listana.
        public List<GetAva> GetAvaErrors(List<GetAva> AvaLIST)
        {
            List<GetAva> AvaErrors = new List<GetAva>();
            Settings settings = new Settings();

            foreach (var item in AvaLIST)
            {
                //Luo AVA:lle vikaraportti muuttuja, jossa kerrotaan viimeisin yhteydenottoaika, tai jos laitteessa on AVAnetin mukaan ongelma / error=true
                string AvaErrorReport = ("Laite: " + item.AvaType + "-" + item.AvaID + " on antanut virheilmoituksen järjestelmään." + System.Environment.NewLine);
                bool errorOrNot = false;

                //"Connection missed"-virheilmoituksen ajan määrittely (kuinka kauan on aikaa laitteen viimeisestä yhteydenotosta)
                DateTime compareDate = item.LastConnection.AddDays(settings.SetConnectionMissed);

                //Jos laitteessa on Avanetin mukaan virheilmoitus, tämä ilmoitus lisätään raporttiin.
                if (item.AvanetErrorTrue == true)
                {
                    errorOrNot = true;
                    AvaErrorReport = (AvaErrorReport + "Avanetin mukaan laite vaatii huoltoa, tai anturissa on vikaa. Tarkista mittarin tämän hetkinen status Avanetistä." + System.Environment.NewLine);
                }

                //"Connection missed"-ilmoituksen lisääminen raporttiin
                if (compareDate < DateTime.Now)
                {
                    errorOrNot = true;
                    AvaErrorReport = (AvaErrorReport + "[CONNECTION MISSED] - " + settings.SetConnectionMissedInfo + " Viimeisin yhteydenottoaika: " + Convert.ToString(item.LastConnection) + System.Environment.NewLine);
                }
                //Ilmoitus paristojen matalasta jännitteestä. Muiden virheilmoituksen määrittely sen mukaan ovatko vain paristot vähissä vai onko muutakin vikaa.
                if (item.AvaBatteryErrorTrue == true)
                {
                    errorOrNot = true;
                    AvaErrorReport = (AvaErrorReport + "[BATTERY LOW] - Laitteen virta on vähissä. Jäljellä oleva virta: " + Convert.ToString(item.AvaBattery) + "V" + System.Environment.NewLine);

                }

                //Yllä luodun raporttikokonaisuuden lisääminen listaan.
                if (errorOrNot == true)
                {
                    item.errorReport = AvaErrorReport;
                    AvaErrors.Add(item);
                }

            }
            return AvaErrors;
        }
    }
}
