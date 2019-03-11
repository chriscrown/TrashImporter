using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Ical.Net;

namespace TrashImporter
{
    public class Program
    {
        // Stunden-Offsets vor/nach Leerungs-Zeitpunkt (00:00 Uhr)
        private const int startOffset = -9;
        private const int endOffset = 14;

        //const string calendarUrl = "http://abfallwirtschaft.landkreis-harburg.de/abfallkalender/icalendar/ical.html?id=900000031&gebietOwner=20100&strukturID=1079&year=2018";
        //const string calendarUrl = "http://abfallwirtschaft.landkreis-harburg.de/abfallkalender/icalendar/ical.html?id=900000031&gebietOwner=20100&strukturID=1079&year=2019";

        public static int Main(string[] args)
        {
            Console.WriteLine("Bitte iCal-URL oder Pfad zur ics-Datei eingeben: ");
            var calendarUrl = Console.ReadLine();

            if (string.IsNullOrEmpty(calendarUrl))
            {
                Console.WriteLine("Keine gültige URL/Datei");
                return -1;
            }
                
            var trashYear = DateTime.Now.Year.ToString();

            var urlParts = calendarUrl.Split('=');
            if (urlParts.Length >= 4)
                trashYear = urlParts[4];
            else
                Console.WriteLine("Kein Jahr in URL/Datei erkannt. Aktuelles Jahr wird verwendet.");

            var trashMapping = new Dictionary<string,string>()
            {
                { "Hausmüll 14-tägig", "0/2/30" },
                { "Gelber Sack", "0/2/31" },
                { "Altpapier", "0/2/32" },
                { "Grünabfall", "0/2/33" }
            };

            using (var webClient = new WebClient())
            {
                webClient.Encoding = Encoding.UTF8;
                try
                {
                    var calendarString = webClient.DownloadString(calendarUrl);
                    var calendar = Calendar.Load(calendarString);

                    using (var file = new StreamWriter($"muell{trashYear}.txt"))
                    {
                        foreach (var calEvent in calendar.Events)
                        {
                            if (!trashMapping.ContainsKey(calEvent.Summary)) continue;
                            file.WriteLine($"{calEvent.Start.Value.AddHours(startOffset):yyyyMMdd_HHmm}|{trashMapping[calEvent.Summary]}|1");
                            file.WriteLine($"{calEvent.Start.Value.AddHours(endOffset):yyyyMMdd_HHmm}|{trashMapping[calEvent.Summary]}|0");
                            file.WriteLine();
                        }
                    }

                    Console.WriteLine($"Datei muell{trashYear}.txt wurde erfolgreich erstellt!");
                }
                catch (WebException e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            Console.WriteLine("Zum Beenden Taste drücken...");
            Console.ReadKey();

            return 0;
        }
    }
}
