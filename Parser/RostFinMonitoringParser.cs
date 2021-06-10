using Models.DAL;
using Models.Models;
using Parser.Extentions;
using PlaywrightSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Parser
{
    public class RostFinMonitoringParser
    {
        private IBrowser browser;
        public RostFinMonitoringParser(IBrowser browser)
        {
            this.browser = browser;
        }
        public void UpdateList()
        {
            var applicationContext = new ApplicationContext();

            var context = browser.NewContextAsync(javaScriptEnabled: true).Result;
            try
            {
                var page = context.NewPageAsync().Result;

                page.GoToAsync("https://www.fedsfm.ru/documents/terrorists-catalog-portal-act").Wait();

                page.WaitForLoadStateAsync(LifecycleEvent.DOMContentLoaded).Wait();

                var Ultext = page.GetInnerHtmlAsync("//div[@id='russianUL']").Result;

                var legal = GetTerosistLegals(Ultext);

                var Fltext = page.GetInnerHtmlAsync("//div[@id='russianFL']").Result;

                var people = GetTerosists(Fltext);

                applicationContext.TerosistLegals.AddRange(legal);
                applicationContext.Terosists.AddRange(people);
                applicationContext.SaveChanges();

                context.CloseAsync().Wait();
            }
            catch (Exception ex)
            {
                
            }
        }

        private List<TerosistLegal> GetTerosistLegals(string text) 
        {
            var result = new List<TerosistLegal>();

            var regex = Regex.Matches(text, "<li>([^<]*)</li>");
            foreach (Match item in regex)
            {
                var name = Regex.Match(item.Value, @">\d\d?\d?\d?\.([^,]*),[^<]*<").Groups[1].Value;
                var inn = Regex.Match(item.Value, @"ИНН:([^;]*);<").Groups[1].Value;

                if (!string.IsNullOrEmpty(inn) && !string.IsNullOrEmpty(name))
                {
                    result.Add(new TerosistLegal { Inn = inn.Trim(), Name = name.Trim() });
                }

            }

            return result;
        }

        private List<Terosist> GetTerosists(string text)
        {
            var result = new List<Terosist>();

            var regex = Regex.Matches(text, "<li>([^<]*)</li>");
            foreach (Match item in regex)
            {
                var name = Regex.Match(item.Value, @">\d\d?\d?\d?\.([^,]*)\*,").Groups[1].Value;
                var bithday = Regex.Match(item.Value, @"\d\d\.\d\d\.\d\d\d\d").Value;
                var address = Regex.Match(item.Value, @"г\.р\.\s,([^;]*);<").Groups[1].Value;

                if (!string.IsNullOrEmpty(bithday) && !string.IsNullOrEmpty(name)&& !string.IsNullOrEmpty(address))
                {
                    result.Add(new Terosist { Name=name.Trim(),Address=address.Trim(),BithDay=DateTime.Parse(bithday.Trim()) });
                }

            }

            return result;
        }
    }
}
