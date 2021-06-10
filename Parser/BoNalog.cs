using Models;
using PlaywrightSharp;
using Parser.Extentions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Parser
{
    public class BoNalog:IParser
    {
        private readonly IBrowser browser;

        public BoNalog(IBrowser browser)
        {
            this.browser = browser;
        }

        private string GetText(string reg, string text)
        {
            var res = Regex.Match(text, reg).Groups[1].Value;
            if (string.IsNullOrEmpty(res))
            {
                return "0";
            }
            else
            {
                return res;
            }
        }

        public async Task<LegalEntity> Parse(LegalEntity legalEntity)
        {
            legalEntity.BoNalogModel = await Parse(legalEntity.Inn);
            return legalEntity;
        }

        public async Task<PhysicalPerson> Parse(PhysicalPerson physicalPerson)
        {
            return physicalPerson;
        }

        public async Task<IEnumerable<LegalEntity>> Parse(IEnumerable<LegalEntity> legalEntities)
        {
            var dict = await Parse(legalEntities.Select(x => x.Inn));
            foreach (var item in legalEntities)
            {
                if (dict.ContainsKey(item.Inn))
                {
                    item.BoNalogModel = dict[item.Inn];
                }
            }
            return legalEntities;
        }

        public async Task<IEnumerable<PhysicalPerson>> Parse(IEnumerable<PhysicalPerson> physicalPeople)
        {
            return physicalPeople;
        }

        private async Task<Dictionary<string, BoNalogModel>> Parse(IEnumerable<string> inns)
        {
            var dict = new Dictionary<string, BoNalogModel>();
            var context = await browser.NewContextAsync(javaScriptEnabled: true);
            try 
            {
                var page = await context.NewPageAsync();
                await page.GoToAsync("https://bo.nalog.ru/");

                await Task.Run(() => page.ExtClickElement("//div[contains(@class,'modal-content short-info')]//button[@class='btn-close']")
                    .OnBoth(() => page.ExtPrintTextToElement("//input[@placeholder='Введите ИНН, ОГРН, адрес или название организации']", inns.FirstOrDefault()))
                    .OnSuccess(() => page.ExtClickElement("//button[@class='button button_search']"))
                    .OnSuccess(() => page.ExtClickElement("//button[@class='button button_search']", 3000))
                    .OnSuccess(()=>page.WaitForLoadStateAsync(LifecycleEvent.DOMContentLoaded)));

                foreach (var inn in inns)
                {
                    try
                    {
                        await Task.Run(() => page.ExtFillTextToElement("//input[@placeholder='Введите ИНН, ОГРН, адрес или название организации']", inn)
                        .OnSuccess(() => page.ExtClickElement("//button[@class='button button_search']"))
                        .OnSuccess(() => page.ExtClickElement("//button[@class='button button_search']", 3000))
                        .OnSuccess(async () => await page.WaitForLoadStateAsync(LifecycleEvent.Networkidle))
                        .OnSuccess(() => page.ExtClickElement("//div[@class='img-wrap']/following-sibling::span[1]", 4000))
                        .OnSuccess(async () => await page.WaitForLoadStateAsync(LifecycleEvent.DOMContentLoaded))
                        .OnSuccess(async () => await Task.Delay(1000))
                        .OnBoth(() => page.ExtClickElement("//button[text()='Бухгалтерский баланс']", 1000))
                        .OnSuccess(async () => await page.WaitForLoadStateAsync(LifecycleEvent.DOMContentLoaded))
                        .OnBoth(() => page.ExtClickElement("//div[text()='I. Внеоборотные активы']"))
                        .OnSuccess(async () => await page.WaitForLoadStateAsync(LifecycleEvent.DOMContentLoaded))
                        .OnBoth(() => page.ExtClickElement("//div[text()='II. Оборотные активы']"))
                        .OnSuccess(async () => await page.WaitForLoadStateAsync(LifecycleEvent.DOMContentLoaded))
                        .OnBoth(() => page.ExtClickElement("//div[text()='III. Капитал и резервы']"))
                        .OnSuccess(async () => await page.WaitForLoadStateAsync(LifecycleEvent.DOMContentLoaded))
                        .OnBoth(() => page.ExtClickElement("//div[text()='IV. Долгосрочные обязательства']"))
                        .OnSuccess(async () => await page.WaitForLoadStateAsync(LifecycleEvent.DOMContentLoaded))
                        .OnBoth(() => page.ExtClickElement("//div[text()='V. Краткосрочные обязательства']"))
                        .OnSuccess(async () => await page.WaitForLoadStateAsync(LifecycleEvent.DOMContentLoaded))
                        .OnBoth(() => page.ExtClickElement("//button[text()='Финансовые результаты']", 1000))
                        .OnSuccess(async () => await page.WaitForLoadStateAsync(LifecycleEvent.DOMContentLoaded)));

                        var text = await page.GetInnerHtmlAsync("//body");

                        var startAction = GetText("<img src=\"/static/media/check-circle.add244b8.svg\" alt=\"\"></div><p>([^<]*)<", text);
                        var VNActive = GetText("Итого по разделу<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>1100<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>([^<]*)<br>", text);
                        var OActive = GetText("Итого по разделу<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>1200<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>([^<]*)<br>", text);
                        var VBalance = GetText("Баланс<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>1700<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>([^<]*)<br>", text);
                        var YK = GetText(@"Уставный капитал \(складочный капитал, уставный фонд, вклады товарищей\)<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>1310<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>([^<]*)<br>", text);
                        var CapitalRez = GetText("Итого по разделу<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>1300<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>([^<]*)<br>", text);
                        var LongDebts = GetText("Итого по разделу<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>1400<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>([^<]*)<div", text);
                        var ShortTDebts = GetText("Итого по разделу<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>1500<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>([^<]*)<br>", text);
                        var Zaem = GetText("Заемные средства<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>1510<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>([^<]*)<div", text);
                        var CreditorDebt = GetText("Кредиторская задолженность<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>1520<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>([^<]*)<br>", text);
                        var Revenue = GetText("Выручка<[^>]*><[^>]*><[^>]*>2110<[^>]*><[^>]*>([^<]*)<br>", text);
                        var Profit = GetText(@"Чистая прибыль \(убыток\)<[^>]*><[^>]*><[^>]*>2400<[^>]*><[^>]*>([^<]*)<br>", text);
                        var SalesProfit = GetText(@"Прибыль \(убыток\) от продаж<[^>]*><[^>]*><[^>]*>2200<[^>]*><[^>]*>([^<]*)<br>", text);

                        var model = new BoNalogModel
                        {
                            StartAction = startAction,
                            NonCurrentAssets = double.Parse(VNActive),
                            CurrentAssets = double.Parse(OActive),
                            BalanceCurrency = double.Parse(VBalance),
                            AuthorizedCapital = double.Parse(YK),
                            CapitalAndReserves = double.Parse(CapitalRez),
                            LongTermCommitment = double.Parse(LongDebts),
                            CurrentLiabilities = double.Parse(ShortTDebts),
                            Revenue = double.Parse(Revenue),
                            Profit = double.Parse(Profit),
                            Borrowings = double.Parse(Zaem),
                            AccountsPayable = double.Parse(CreditorDebt),
                            ProfitFromSale = double.Parse(SalesProfit)
                        };
                        dict.Add(inn, model);
                    }
                    catch (Exception ex) 
                    {

                    }
                }

            }
            catch(Exception ex) 
            {
            }
            finally
            {
                await context.CloseAsync();
            }
            return dict;
        }
        private async Task<BoNalogModel> Parse(string inn)
        {
            var context = await browser.NewContextAsync(javaScriptEnabled: true);
            BoNalogModel model = new BoNalogModel();
            try
            {
                var page = await context.NewPageAsync();
                await page.GoToAsync("https://bo.nalog.ru/");
                await page.WaitForLoadStateAsync();

                await Task.Run(() => page.ExtClickElement("//div[contains(@class,'modal-content short-info')]//button[@class='btn-close']")
                    .OnBoth(() => page.ExtPrintTextToElement("//input[@placeholder='Введите ИНН, ОГРН, адрес или название организации']", inn))
                    .OnSuccess(() => page.ExtClickElement("//button[@class='button button_search']"))
                    .OnSuccess(() => page.ExtClickElement("//button[@class='button button_search']", 3000))
                    .OnSuccess(async () => await page.WaitForLoadStateAsync(LifecycleEvent.Networkidle))
                    .OnSuccess(() => page.ExtClickElement("//div[@class='img-wrap']/following-sibling::span[1]", 4000))
                    .OnSuccess(async () => await page.WaitForLoadStateAsync(LifecycleEvent.DOMContentLoaded))
                    .OnSuccess(async () => await Task.Delay(1000))
                    .OnBoth(() => page.ExtClickElement("//button[text()='Бухгалтерский баланс']", 1000))
                    .OnSuccess(async () => await page.WaitForLoadStateAsync(LifecycleEvent.DOMContentLoaded))
                    .OnBoth(() => page.ExtClickElement("//div[text()='I. Внеоборотные активы']"))
                    .OnSuccess(async () => await page.WaitForLoadStateAsync(LifecycleEvent.DOMContentLoaded))
                    .OnBoth(() => page.ExtClickElement("//div[text()='II. Оборотные активы']"))
                    .OnSuccess(async () => await page.WaitForLoadStateAsync(LifecycleEvent.DOMContentLoaded))
                    .OnBoth(() => page.ExtClickElement("//div[text()='III. Капитал и резервы']"))
                    .OnSuccess(async () => await page.WaitForLoadStateAsync(LifecycleEvent.DOMContentLoaded))
                    .OnBoth(() => page.ExtClickElement("//div[text()='IV. Долгосрочные обязательства']"))
                    .OnSuccess(async () => await page.WaitForLoadStateAsync(LifecycleEvent.DOMContentLoaded))
                    .OnBoth(() => page.ExtClickElement("//div[text()='V. Краткосрочные обязательства']"))
                    .OnSuccess(async () => await page.WaitForLoadStateAsync(LifecycleEvent.DOMContentLoaded))
                    .OnBoth(() => page.ExtClickElement("//button[text()='Финансовые результаты']", 1000))
                    .OnSuccess(async () => await page.WaitForLoadStateAsync(LifecycleEvent.DOMContentLoaded)));

                var text = await page.GetInnerHtmlAsync("//body");

                var startAction = GetText("<img src=\"/static/media/check-circle.add244b8.svg\" alt=\"\"></div><p>([^<]*)<", text);
                var VNActive = GetText("Итого по разделу<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>1100<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>([^<]*)<br>", text);
                var OActive = GetText("Итого по разделу<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>1200<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>([^<]*)<br>", text);
                var VBalance = GetText("Баланс<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>1700<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>([^<]*)<br>", text);
                var YK = GetText(@"Уставный капитал \(складочный капитал, уставный фонд, вклады товарищей\)<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>1310<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>([^<]*)<br>", text);
                var CapitalRez = GetText("Итого по разделу<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>1300<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>([^<]*)<br>", text);
                var LongDebts = GetText("Итого по разделу<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>1400<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>([^<]*)<div", text);
                var ShortTDebts = GetText("Итого по разделу<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>1500<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>([^<]*)<br>", text);
                var Zaem = GetText("Заемные средства<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>1510<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>([^<]*)<div", text);
                var CreditorDebt = GetText("Кредиторская задолженность<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>1520<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>([^<]*)<br>", text);
                var Revenue = GetText("Выручка<[^>]*><[^>]*><[^>]*>2110<[^>]*><[^>]*>([^<]*)<br>", text);
                var Profit = GetText(@"Чистая прибыль \(убыток\)<[^>]*><[^>]*><[^>]*>2400<[^>]*><[^>]*>([^<]*)<br>", text);
                var SalesProfit = GetText(@"Прибыль \(убыток\) от продаж<[^>]*><[^>]*><[^>]*>2200<[^>]*><[^>]*>([^<]*)<br>", text);

                model = new BoNalogModel
                {
                    StartAction = startAction,
                    NonCurrentAssets = double.Parse(VNActive),
                    CurrentAssets = double.Parse(OActive),
                    BalanceCurrency = double.Parse(VBalance),
                    AuthorizedCapital = double.Parse(YK),
                    CapitalAndReserves = double.Parse(CapitalRez),
                    LongTermCommitment = double.Parse(LongDebts),
                    CurrentLiabilities = double.Parse(ShortTDebts),
                    Revenue = double.Parse(Revenue),
                    Profit = double.Parse(Profit),
                    Borrowings = double.Parse(Zaem),
                    AccountsPayable = double.Parse(CreditorDebt),
                    ProfitFromSale = double.Parse(SalesProfit)
                };

                
            }
            catch (Exception ex)
            {

            }
            finally
            {
                await context.CloseAsync();
            }
            return model;
        }

    }
}
