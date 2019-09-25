﻿using SearchEngine.UtilityClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.WebCrawler
{
    //This is the URL Frontier element.

    /// <summary>
    /// Responsible for handling the set of urls.
    /// </summary>
    public class UrlFrontier
    {
        List<string> frontier = new List<string>();

        //sorted according to timespan. So first is the one with the lowest timespan i.e. the oldest.
        List<string> queue = new List<string>();

        public void AddUrl(string url)
        {
            frontier.Add(url);
        }

        public int Size()
        {
            return frontier.Count;
        }      


        public string GetNewUrl()
        {
            string url = frontier[0];
            frontier.RemoveAt(0);
            return url;
        }

        //Denne er lidt svær at forstå.
        //Tror den virker, men er lidt usikker, da den godt kan
        //ende med at tage samme domæne 3 gange i streg.
        //Kan være at det er hvis den finder 3 gange streg, 3 andre
        //domæner som ikke er current, men som blot ikke har nogle i links i frontier 
        //med det domæne, så den ender blot med at tage den første i frontier
        //som så uheldigvis er et link som har samme domæne som det vi lige har besøgt.
        //jeg prøver lige at ændre den til at tage random istedet for first..
        public string GetNewUrl1(string currentUrl)
        {
            string domainWeMayChoose = "";
            string newUrl = "";
            try
            {
                //Tjekker om der er et andet domæne som vi kan crawle
                domainWeMayChoose = queue.First(x => x != Utility.GetPartialDomainOfUrl(currentUrl));

                //Hvis der er, så tjekker vi om der er nogle 
                //links i vores frontier med det domæne.
                //PartialDomain, da vi ikke gider at have http og https med
                //da to med samme host, men forskellige sikkerhed vil fremstå som forskellige.
                newUrl = frontier.First(x => Utility.GetPartialDomainOfUrl(x) == domainWeMayChoose);

            }
            catch (Exception)
            {
                //vi fandt et domæne, men inden link i frontier med det domæne
                //så ligger vi det bagerst i queueen, ellers vil vi blot ende med at 
                //tage det først link i frontier indtil der bliver addet 
                //et link til frontier med det domæne.
                if (newUrl == "" && domainWeMayChoose != "")
                {
                    int i = queue.IndexOf(domainWeMayChoose);
                    queue.RemoveAt(i);
                    queue.Add(domainWeMayChoose);
                }
                //Hvis der ikke var et andet domæne, eller der ikke var 
                //et link i frontier med det nye domæne så siger vi blot
                //at vi tager og besøger et random element i frontier og 
                //bogfører at vi har besøgt det, ved at tilføje det domæne bagerst i køen.
                //den eneste der ikke rigtigt bliver bogført 
                //er den aller første url vi besøger.
                Random rd = new Random();
                newUrl = frontier[rd.Next(0, frontier.Count)]; //from 0 til count-1 see docs
                domainWeMayChoose = Utility.GetPartialDomainOfUrl(newUrl);
            }
            int indexToDelete = frontier.FindIndex(x => x == newUrl);
            frontier.RemoveAt(indexToDelete);

            //Adding and element. Or updating one, by deleting it and recreating it,
            //so it comes back in the line.
            int indexOfDomainInQueueIfAny = QueueContainsDomain(domainWeMayChoose);
            if (indexOfDomainInQueueIfAny != -1) //it exists.
            {
                queue.RemoveAt(indexOfDomainInQueueIfAny);
            }            
            queue.Add(domainWeMayChoose);
            return newUrl;
        }

        private int QueueContainsDomain(string domain)
        {
           for(int i = 0; i < queue.Count; i++)
            {
                if (queue[i] == domain)
                {
                    return i;
                }    
            }
            return -1;
        }

        public void SaveState()
        {
            System.IO.File.WriteAllLines(WebCrawler.folderPath + @"\Frontier\state", frontier);

        }

        public void LoadState()
        {
            frontier = System.IO.File.ReadAllLines(WebCrawler.folderPath + @"\Frontier\state").ToList();
        }

    }
}



