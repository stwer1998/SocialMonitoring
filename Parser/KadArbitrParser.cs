using HtmlAgilityPack;
using Models;
using Models.DAL;
using Parser.Extentions;
using PlaywrightSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Parser
{
    public class KadArbitrParser : IParser
    {
        private readonly IBrowser browser;

        public KadArbitrParser(IBrowser browser)
        {
            this.browser = browser;
        }

        public async Task<LegalEntity> Parse(LegalEntity legalEntity)
        {
            legalEntity.KadArbirt = await ParseStart(legalEntity.Inn);
            return legalEntity;
        }

        public async Task<PhysicalPerson> Parse(PhysicalPerson physicalPerson)
        {
            physicalPerson.KadArbirt = await ParseStart(physicalPerson.Inn);
            return physicalPerson;
        }

        public async Task<IEnumerable<LegalEntity>> Parse(IEnumerable<LegalEntity> legalEntities)
        {
            var dict = await ParseStart(legalEntities.Select(x => x.Inn));
            foreach (var legalEntity in legalEntities)
            {
                if (dict.ContainsKey(legalEntity.Inn))
                {
                    legalEntity.KadArbirt = dict[legalEntity.Inn];
                }
            }
            return legalEntities;
        }

        public async Task<IEnumerable<PhysicalPerson>> Parse(IEnumerable<PhysicalPerson> physicalPeople)
        {
            var dict = await ParseStart(physicalPeople.Select(x => x.Inn));
            foreach (var physicalPerson in physicalPeople)
            {
                if (dict.ContainsKey(physicalPerson.Inn))
                {
                    physicalPerson.KadArbirt = dict[physicalPerson.Inn];
                }
            }
            return physicalPeople;
        }

        private async Task<Dictionary<string, KadArbirtModel>> ParseStart(IEnumerable<string> inns)
        {
            IProxyRepository proxyRepository = new ProxyRepository();
            var playwright = Playwright.CreateAsync().Result;
            var dict = new Dictionary<string, KadArbirtModel>();
            foreach (var inn in inns)
            {
                var proxy = proxyRepository.GetFreeProxy();
                var browser = playwright.Firefox.LaunchAsync(headless: false
                    , proxy: new ProxySettings { Server = $"{proxy.IpAddress}:{proxy.Port}" }
                    ).Result;
                var model = await ParseStart(inn, browser);
                dict.Add(inn, model);
                proxy.LastUsing = DateTime.Now;
                proxyRepository.Update(proxy);
            }
            return dict;

        }

        private async Task<KadArbirtModel> ParseStart(string inn) 
        {
            var data = new KadArbirtModel();
            var context = await browser.NewContextAsync(javaScriptEnabled: true);

            try
            {
                var page = await context.NewPageAsync();
                await page.GoToAsync("https://kad.arbitr.ru/");
                await page.WaitForLoadStateAsync();


                await Task.Run(()=> page.ExtClickElement("//a[@class='b-promo_notification-popup-close js-promo_notification-popup-close']", 3000)
                            .OnBoth(()=>page.WaitForLoadStateAsync())
                           .OnBoth(() => page.ExtFillTextToElement("//textarea[@placeholder='название, ИНН или ОГРН']", inn))
                           .OnSuccess(() => page.ExtFocusToElement("//input[@placeholder='фамилия судьи']"))
                           .OnSuccess(() => page.ExtClickElement("//span[@class='type-switcher-text']//span[1]"))
                           .OnSuccess(() => page.ExtClickElement("(//span[text()='Любой']/following::input)[2]"))
                           .OnSuccess(() => page.ExtClickElement("//button[@alt='Найти']", 3000))
                           .OnSuccess(async () => await page.WaitForLoadStateAsync(LifecycleEvent.Load)));

                var html = page.GetInnerHtmlAsync("//body").Result.ToString();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);

                data.PlaintiffCaseNum = GetCaseCount(doc);

                await Task.Run(()=> page.ExtClickElement("//span[@class='type-switcher-text']//span[1]", 2000)
                    .OnSuccess(() => page.ExtClickElement("(//span[text()='Истец']/following::input)[3]"))
                    .OnSuccess(() => page.ExtClickElement("//button[@alt='Найти']", 3000))
                    .OnSuccess(async () => await page.WaitForLoadStateAsync(LifecycleEvent.Load)));


                html = page.GetInnerHtmlAsync("//body").Result.ToString();

                doc.LoadHtml(html);

                data.DefendantCaseNum = GetCaseCount(doc);

                if (data.DefendantCaseNum > 0)
                {

                    await Task.Run(() => page.ExtPrintTextToElement("//label[@class='from']//input[1]", DateTime.Now.AddYears(-3).ToString("dd.MM.yyyy"))
                        .OnSuccess(() => page.ExtPrintTextToElement("//label[@class='to']//input[1]", DateTime.Now.ToString("dd.MM.yyyy")))
                        .OnSuccess(() => page.ExtFocusToElement("//input[@placeholder='фамилия судьи']"))
                        .OnSuccess(() => page.ExtClickElement("//button[@alt='Найти']", 4000))
                        .OnSuccess(async () => await page.WaitForLoadStateAsync(LifecycleEvent.Load)));

                    html = page.GetInnerHtmlAsync("//body").Result.ToString();

                    doc.LoadHtml(html);

                    data.DefendantCaseNumLast3Year = GetCaseCount(doc);
                }

                if (data.DefendantCaseNumLast3Year > 0)
                {

                    var pageCount = GetPageCount(doc);

                    var list = new List<CardCase>(data.DefendantCaseNum);

                    list.AddRange(ParseCase(doc));
                    for (int i = 2; i < pageCount + 1; i++)
                    {
                        page.ExtClickElement($"//a[@href='#page{i}']", 4000).OnSuccess( async () => await page.WaitForLoadStateAsync(LifecycleEvent.Networkidle));
                        html = await page.GetInnerHtmlAsync("//body");
                        doc.LoadHtml(html);
                        list.AddRange(ParseCase(doc));
                    }

                    data.DefendantCase = list;

                }

                if (data.PlaintiffCaseNum>0)
                {
                    await Task.Run(() => page.ExtClickElement("//span[@class='type-switcher-text']//span[1]", 3000)
                        .OnSuccess(() => page.ExtClickElement("(//span[text()='Ответчик']/following::input)[2]"))
                        .OnSuccess(() => page.ExtClickElement("//button[@alt='Найти']", 4000))
                        .OnSuccess(async () => await page.WaitForLoadStateAsync(LifecycleEvent.Networkidle)));

                    html = await page.GetInnerHtmlAsync("//body");

                    doc.LoadHtml(html);

                    data.PlaintiffCaseNumLast3Year = GetCaseCount(doc);
                }
                if (data.PlaintiffCaseNumLast3Year > 0)
                {

                    var pageCount = GetPageCount(doc);
                    var list = new List<CardCase>(data.PlaintiffCaseNum);
                    list.AddRange(ParseCase(doc));
                    for (int i = 2; i < pageCount + 1; i++)
                    {
                        page.ExtClickElement($"//a[@href='#page{i}']", 4000).OnSuccess(async () => await page.WaitForLoadStateAsync(LifecycleEvent.Networkidle));
                        html =await page.GetInnerHtmlAsync("//body");
                        doc.LoadHtml(html);
                        list.AddRange(ParseCase(doc));
                    }

                    data.PlaintiffCases = list;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exeption :  {ex.Message}");
            }
            finally
            {
                await context.CloseAsync();
            }
            return data;
        }

        private async Task<KadArbirtModel> ParseStart(string inn, IBrowser browser)
        {
            var data = new KadArbirtModel();
            var context = await browser.NewContextAsync(javaScriptEnabled: true);

            try
            {
                var page = await context.NewPageAsync();
                await page.GoToAsync("https://kad.arbitr.ru/");
                await page.WaitForLoadStateAsync();


                await Task.Run(() => page.ExtClickElement("//a[@class='b-promo_notification-popup-close js-promo_notification-popup-close']", 3000)
                           .OnBoth(() => page.ExtFillTextToElement("//textarea[@placeholder='название, ИНН или ОГРН']", inn))
                           .OnSuccess(() => page.ExtFocusToElement("//input[@placeholder='фамилия судьи']"))
                           .OnSuccess(() => page.ExtClickElement("//span[@class='type-switcher-text']//span[1]"))
                           .OnSuccess(() => page.ExtClickElement("(//span[text()='Любой']/following::input)[2]"))
                           .OnSuccess(() => page.ExtClickElement("//button[@alt='Найти']", 3000))
                           .OnSuccess(async () => await page.WaitForLoadStateAsync(LifecycleEvent.Load)));

                var html = page.GetInnerHtmlAsync("//body").Result.ToString();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);

                data.PlaintiffCaseNum = GetCaseCount(doc);

                await Task.Run(() => page.ExtClickElement("//span[@class='type-switcher-text']//span[1]", 2000)
                    .OnSuccess(() => page.ExtClickElement("(//span[text()='Истец']/following::input)[3]"))
                    .OnSuccess(() => page.ExtClickElement("//button[@alt='Найти']", 3000))
                    .OnSuccess(async () => await page.WaitForLoadStateAsync(LifecycleEvent.Load)));


                html = page.GetInnerHtmlAsync("//body").Result.ToString();

                doc.LoadHtml(html);

                data.DefendantCaseNum = GetCaseCount(doc);

                if (data.DefendantCaseNum > 0)
                {

                    await Task.Run(() => page.ExtPrintTextToElement("//label[@class='from']//input[1]", DateTime.Now.AddYears(-3).ToString("dd.MM.yyyy"))
                        .OnSuccess(() => page.ExtPrintTextToElement("//label[@class='to']//input[1]", DateTime.Now.ToString("dd.MM.yyyy")))
                        .OnSuccess(() => page.ExtFocusToElement("//input[@placeholder='фамилия судьи']"))
                        .OnSuccess(() => page.ExtClickElement("//button[@alt='Найти']", 4000))
                        .OnSuccess(async () => await page.WaitForLoadStateAsync(LifecycleEvent.Load)));

                    html = page.GetInnerHtmlAsync("//body").Result.ToString();

                    doc.LoadHtml(html);

                    data.DefendantCaseNumLast3Year = GetCaseCount(doc);
                }

                if (data.DefendantCaseNumLast3Year > 0)
                {

                    var pageCount = GetPageCount(doc);

                    var list = new List<CardCase>(data.DefendantCaseNum);

                    list.AddRange(ParseCase(doc));
                    for (int i = 2; i < pageCount + 1; i++)
                    {
                        page.ExtClickElement($"//a[@href='#page{i}']", 4000).OnSuccess(async () => await page.WaitForLoadStateAsync(LifecycleEvent.Networkidle));
                        html = await page.GetInnerHtmlAsync("//body");
                        doc.LoadHtml(html);
                        list.AddRange(ParseCase(doc));
                    }

                    data.DefendantCase = list;

                }

                if (data.PlaintiffCaseNum > 0)
                {
                    await Task.Run(() => page.ExtClickElement("//span[@class='type-switcher-text']//span[1]", 3000)
                        .OnSuccess(() => page.ExtClickElement("(//span[text()='Ответчик']/following::input)[2]"))
                        .OnSuccess(() => page.ExtClickElement("//button[@alt='Найти']", 4000))
                        .OnSuccess(async () => await page.WaitForLoadStateAsync(LifecycleEvent.Networkidle)));

                    html = await page.GetInnerHtmlAsync("//body");

                    doc.LoadHtml(html);

                    data.PlaintiffCaseNumLast3Year = GetCaseCount(doc);
                }
                if (data.PlaintiffCaseNumLast3Year > 0)
                {

                    var pageCount = GetPageCount(doc);
                    var list = new List<CardCase>(data.PlaintiffCaseNum);
                    list.AddRange(ParseCase(doc));
                    for (int i = 2; i < pageCount + 1; i++)
                    {
                        page.ExtClickElement($"//a[@href='#page{i}']", 4000).OnSuccess(async () => await page.WaitForLoadStateAsync(LifecycleEvent.Networkidle));
                        html = await page.GetInnerHtmlAsync("//body");
                        doc.LoadHtml(html);
                        list.AddRange(ParseCase(doc));
                    }

                    data.PlaintiffCases = list;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exeption :  {ex.Message}");
            }
            finally
            {
                await context.CloseAsync();
            }
            return data;
        }

        private IList<CardCase> ParseCase(HtmlDocument htmlDocument) 
        {
            var list = new List<CardCase>();
            var nodes = htmlDocument.DocumentNode.SelectNodes("//div[@id='table']//tr");
            foreach (var node in nodes)
            {
                var cardCase = new CardCase();
                cardCase.TypeCase = node.SelectSingleNode("td[@class='num']//div//div").GetAttributeValue("class","");
                cardCase.CaseDate = node.SelectSingleNode("td[@class='num']//div//div").GetAttributeValue("title", "");
                cardCase.UrlCase = node.SelectSingleNode("td[@class='num']//div//a").GetAttributeValue("href", "");
                list.Add(cardCase);
            }

            return list;
        }
        
        private int GetPageCount(HtmlDocument htmlDocument) 
        {
            return htmlDocument.DocumentNode.SelectNodes("//div[@class='pager1']//div//ul//li//a")
                                            .Select(x => x.InnerText.TryToInt())
                                            .Max();
        }

        private int GetCaseCount(HtmlDocument htmlDocument) 
        {
            try
            {
                //var el1 = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='b-found-total']//@style").Attributes.ToList();//.GetDataAttribute("@style");
                var style = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='b-found-total']").Attributes.FirstOrDefault(x => x.Name == "style").Value;
                if (style == "display: block;")
                {

                    var el = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='b-button']/following-sibling::div[1]").InnerText;
                    MatchCollection collection = Regex.Matches(el, @"\d+");

                    foreach (Match item in collection)
                    {
                        return item.Value.TryToInt();
                    }
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        
    }
}
