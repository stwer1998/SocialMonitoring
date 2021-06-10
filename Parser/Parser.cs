using Models;
using PlaywrightSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Parser
{
    public static class Parser
    {
        private static readonly IPlaywright playwright;
        private static readonly IBrowser browser;
        private static readonly KadArbitrParser KadArbitr;
        private static readonly EgrulNalog EgrulNalog;
        private static readonly BoNalog BoNalog;
        private static readonly Bankrot Bankrot;
        private static readonly NedobrosovesZakupki NedobrosovesZakupki;
        private static readonly FsspParser FsspParser;
        private static readonly RostFinMonitoringParser RostFinMonitoringParser;
        private static readonly MassDataParse MassDataParse;
        private static readonly CheckData CheckData;
        private static readonly Analitics Analitics;
        private static readonly VkParser VkParser;
        private static bool ParserWorking;
        static Parser()
        {
            playwright = Playwright.CreateAsync().Result;
            browser = playwright.Firefox.LaunchAsync(headless: false
                //, proxy: new ProxySettings{Server= "88.205.225.203:3128" }
                ).Result;
            Bankrot = new Bankrot(browser);
            BoNalog = new BoNalog(browser);
            EgrulNalog = new EgrulNalog(browser);
            FsspParser = new FsspParser();
            KadArbitr = new KadArbitrParser(browser);
            NedobrosovesZakupki = new NedobrosovesZakupki(browser);
            RostFinMonitoringParser = new RostFinMonitoringParser(browser);
            MassDataParse = new MassDataParse();
            Analitics = new Analitics();
            CheckData = new CheckData();
            VkParser = new VkParser();
            ParserWorking = false;
        }

        public static void UpdateData()
        {
            RostFinMonitoringParser.UpdateList();
            MassDataParse.ParseAddress();
            MassDataParse.ParseDirectors();
            MassDataParse.ParseOwners();
            MassDataParse.AddParserSettings();
        }

        public static async Task<IEnumerable<LegalEntity>> Parse(IEnumerable<LegalEntity> legalEntities)
        {
            var t1 = Task.Run(()=>KadArbitr.Parse(legalEntities));
            var t2 = Task.Run(()=>Bankrot.Parse(legalEntities));
            var t3 = Task.Run(()=>BoNalog.Parse(legalEntities));
            var t4 = Task.Run(()=>EgrulNalog.Parse(legalEntities));
            var t5 = Task.Run(()=>NedobrosovesZakupki.Parse(legalEntities));
            Task.WaitAll(t1, t2, t3, t4, t5);
            return legalEntities;
        }

        public static async Task<PhysicalPerson> Parse(PhysicalPerson physicalPerson) 
        {
            if (!ParserWorking)
            {
                ParserWorking = true;
                await KadArbitr.Parse(physicalPerson);
                await FsspParser.Parse(physicalPerson);
                await Bankrot.Parse(physicalPerson);
                await NedobrosovesZakupki.Parse(physicalPerson);
                await CheckData.Parse(physicalPerson);
                await VkParser.Parse(physicalPerson);
                ParserWorking = false;
            }
            return physicalPerson;
        }

        public static async Task<LegalEntity> Parse(LegalEntity legalEntity)
        {
            if (!ParserWorking)
            {
                try
                {
                    ParserWorking = true;
                    legalEntity.QueueState = Models.Models.QueueState.InProgress;
                    await EgrulNalog.Parse(legalEntity);
                    await FsspParser.Parse(legalEntity);
                    await KadArbitr.Parse(legalEntity);
                    await BoNalog.Parse(legalEntity);//
                    await Bankrot.Parse(legalEntity);
                    await NedobrosovesZakupki.Parse(legalEntity);
                    await CheckData.Parse(legalEntity);
                    await Analitics.Parse(legalEntity);
                    if (legalEntity.QueueState == Models.Models.QueueState.InProgress)
                    {
                        legalEntity.QueueState = Models.Models.QueueState.Parsed;
                    }
                    return legalEntity;
                }
                catch(Exception ex)
                {
                    legalEntity.QueueState = Models.Models.QueueState.ExeptionOnParsing;
                }
                ParserWorking = false;

            }
            return legalEntity;

        }


    }
}
