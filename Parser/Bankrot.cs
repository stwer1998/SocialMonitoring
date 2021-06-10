using Models;
using Parser.Extentions;
using PlaywrightSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Parser
{
    public class Bankrot : IParser
    {
        private readonly IBrowser browser;

        public Bankrot(IBrowser browser)
        {
            this.browser = browser;
        }

        public async Task<LegalEntity> Parse(LegalEntity legalEntity)
        {
            legalEntity.OnBankruptcy = await Parse(legalEntity.Inn);
            return legalEntity;
        }

        public async Task<PhysicalPerson> Parse(PhysicalPerson physicalPerson)
        {
            physicalPerson.OnBankruptcy = await Parse(physicalPerson.Inn);
            return physicalPerson;
        }

        public async Task<IEnumerable<LegalEntity>> Parse(IEnumerable<LegalEntity> legalEntities)
        {
            var dict = await Parse(legalEntities.Select(x=>x.Inn));

            foreach (var item in legalEntities)
            {
                if (dict.ContainsKey(item.Inn))
                {
                    item.OnBankruptcy = dict[item.Inn];
                }
            }

            return legalEntities;
        }

        public async Task<IEnumerable<PhysicalPerson>> Parse(IEnumerable<PhysicalPerson> physicalPeople)
        {
            var dict = await Parse(physicalPeople.Select(x => x.Inn));

            foreach (var item in physicalPeople)
            {
                if (dict.ContainsKey(item.Inn))
                {
                    item.OnBankruptcy = dict[item.Inn];
                }
            }

            return physicalPeople;
        }

        private async Task<bool> Parse(string inn)
        {
            var context = await browser.NewContextAsync(javaScriptEnabled: true);
            try
            {
                var page = await context.NewPageAsync();

                //var task = page.WaitForLoadStateAsync(LifecycleEvent.Networkidle);

                await page.GoToAsync("https://bankrot.fedresurs.ru/");
                await page.WaitForLoadStateAsync(LifecycleEvent.DOMContentLoaded);

                var r = await page.GetInnerHtmlAsync("//a[contains(@href,'/DebtorsSearch.aspx?Name=')]");


                //page.ExtClickElement("//a[contains(@href,'/DebtorsSearch.aspx?Name=')]", 100);
                page.ExtClickElement("//div[@id='small']//a[1]", 1000);

                if (inn.Length == 12)
                {
                    await Task.Run(() => page.ExtClickElement("//input[@value='Persons']", 1000)
                    .OnSuccess(() => page.WaitForLoadStateAsync(LifecycleEvent.DOMContentLoaded))
                    .OnSuccess(() => page.ExtPrintTextToElement("//input[@class='delimiter-digit form']", inn))
                    .OnSuccess(() => page.ExtClickElement("//input[@src='img/but_search.png']", 1000)));



                }
                else
                {

                    await Task.Run(() => page.ExtPrintTextToElement("(//table[@id='ctl00_cphBody_tblOrgSearchFilter']//input)[3]", inn)
                        .OnSuccess(() => page.ExtClickElement("//input[@src='img/but_search.png']", 1000)));
                }

                var el = await page.GetInnerTextAsync("//table[@class='bank']");

                if (el.StartsWith("По заданным"))
                {
                    //не состоит
                    return false;
                }
                else
                {
                    //состоит
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                await context.CloseAsync();
            }
        }

        private async Task<Dictionary<string, bool>> Parse(IEnumerable<string> inns)
        {
            var dict = new Dictionary<string, bool>();
            var context = await browser.NewContextAsync(javaScriptEnabled: true);
            try
            {
                var page = await context.NewPageAsync();

                //var task = page.WaitForLoadStateAsync(LifecycleEvent.Networkidle);

                await page.GoToAsync("https://bankrot.fedresurs.ru/");
                await page.WaitForLoadStateAsync(LifecycleEvent.Networkidle);

                await Task.Run(() => page.ExtClickElement("//a[contains(@href,'/DebtorsSearch.aspx?Name=')]"));

                if (inns.First().Length == 12)
                {
                    page.ExtClickElement("//input[@value='Persons']", 100);
                }

                foreach (var inn in inns)
                {
                    await Task.Run(() => page.ExtClickElement("//input[@src='img/but_clear.png']", 100)
                    .OnSuccess(() => page.ExtFillTextToElement("(//table[@id='ctl00_cphBody_tblOrgSearchFilter']//input)[3]", inn))
                    .OnSuccess(() => page.ExtClickElement("//input[@src='img/but_search.png']", 1000)));

                    var el = await page.GetInnerTextAsync("//table[@class='bank']");

                    if (el.StartsWith("По заданным"))
                    {
                        dict.Add(inn, false);
                        //не состоит
                    }
                    else
                    {
                        dict.Add(inn, true);
                        //состоит
                    }
                }
                return dict;
            }
            catch (Exception ex)
            {
                return dict;
            }
            finally
            {
                await context.CloseAsync();
            }
        }
    }
}
