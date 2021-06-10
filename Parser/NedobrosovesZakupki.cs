using Models;
using Parser.Extentions;
using PlaywrightSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Parser
{
    public class NedobrosovesZakupki : IParser
    {
        private readonly IBrowser browser;

        public NedobrosovesZakupki(IBrowser browser)
        {
            this.browser = browser;
        }        

        public async Task<LegalEntity> Parse(LegalEntity legalEntity)
        {
            legalEntity.OnUnscrupulousVendors = await Parse(legalEntity.Inn);
            return legalEntity;
        }

        public async Task<PhysicalPerson> Parse(PhysicalPerson physicalPerson)
        {
            physicalPerson.OnUnscrupulousVendors = await Parse(physicalPerson.Inn);
            return physicalPerson;
        }

        public async Task<IEnumerable<LegalEntity>> Parse(IEnumerable<LegalEntity> legalEntities)
        {
            var dict = await Parse(legalEntities.Select(x => x.Inn));
            foreach (var item in legalEntities)
            {
                if (dict.ContainsKey(item.Inn))
                {
                    item.OnUnscrupulousVendors = dict[item.Inn];
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
                    item.OnUnscrupulousVendors = dict[item.Inn];
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

                await page.GoToAsync("https://zakupki.gov.ru/epz/dishonestsupplier/search/results.html");

                await page.WaitForLoadStateAsync(LifecycleEvent.Networkidle);

                await Task.Run(() => page.ExtPrintTextToElement("//input[contains(@placeholder,'Введите полностью или часть наименования (ФИО), ИНН (аналог ИНН)')]", inn)
                    .OnSuccess(() => page.ExtClickElement("//button[@class='search__btn']//img[1]")));

                await page.WaitForLoadStateAsync(LifecycleEvent.Networkidle);

                var result = await page.GetInnerTextAsync("//div[contains(@class,'search-registry-entrys-block')]");

                if (string.IsNullOrWhiteSpace(result))
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

                await page.GoToAsync("https://zakupki.gov.ru/epz/dishonestsupplier/search/results.html");

                await page.WaitForLoadStateAsync(LifecycleEvent.Networkidle);

                foreach (var inn in inns)
                {

                    await Task.Run(() => page.ExtFillTextToElement("//input[contains(@class,'text-truncate search__input')]", inn)
                    .OnSuccess(() => page.ExtClickElement("//button[@class='search__btn']//img[1]",1000)));

                    await page.WaitForLoadStateAsync(LifecycleEvent.Networkidle);

                    var result = await page.GetInnerTextAsync("//div[contains(@class,'search-registry-entrys-block')]");

                    if (string.IsNullOrWhiteSpace(result))
                    {
                        //не состоит
                        dict.Add(inn, false);
                    }
                    else
                    {
                        //состоит
                        dict.Add(inn, true);

                    }
                    page.ExtClickElement("//form[@id='quickSearchForm_header']/section[1]/div[1]/div[1]/div[1]/div[2]/div[1]/div[1]/div[1]/img[1]", 100);

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
