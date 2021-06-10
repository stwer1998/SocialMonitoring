using Models;
using PlaywrightSharp;
using Parser.Extentions;
using System.IO;
using UglyToad.PdfPig;
using System.Linq;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;

namespace Parser
{
    public class EgrulNalog:IParser
    {
        private readonly IBrowser browser;
        private readonly string DownloadforderPath;

        public EgrulNalog(IBrowser browser)
        {
            this.browser = browser;
            DownloadforderPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\";
        }

        public async Task<LegalEntity> Parse(LegalEntity legalEntity)
        {
            legalEntity.ErgulNalog = await Parse(legalEntity.Inn);
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
                    item.ErgulNalog = dict[item.Inn];
                }
            }
            return legalEntities;
        }

        public async Task<IEnumerable<PhysicalPerson>> Parse(IEnumerable<PhysicalPerson> physicalPeople)
        {
            return physicalPeople;
        }

        private async Task<Dictionary<string, ErgulNalogModel>> Parse(IEnumerable<string> inns)
        {
            var context = await browser.NewContextAsync(javaScriptEnabled: true, acceptDownloads: true);
            var dict = new Dictionary<string, ErgulNalogModel>();
            try
            {
                var page = await context.NewPageAsync();
                await page.GoToAsync("https://egrul.nalog.ru/index.html");

                foreach (var inn in inns)
                {
                    try
                    {
                        await Task.Run(() => page.ExtClickElement("//input[@id='query']/following-sibling::b[1]")
                            .OnBoth(() => page.ExtPrintTextToElement("//input[@class='txt-wide txt-string']", inn)
                            .OnSuccess(() => page.ExtClickElement("//button[@class='btn-with-icon btn-search']", 1000))
                            .OnSuccess(async () => await page.WaitForLoadStateAsync(LifecycleEvent.Load))));

                        var downloadTask = page.WaitForEventAsync<DownloadEventArgs>(pageEvent: PageEvent.Download);
                        page.ExtClickElement("//button[contains(@class,'btn-with-icon btn-excerpt')]");

                        var d = downloadTask.Result.Download;
                        var filePath = $"{DownloadforderPath}{inn}.pdf";

                        await d.SaveAsAsync(filePath);
                        await downloadTask.Result.Download.DeleteAsync();

                        var text = "";

                        using (PdfDocument document = PdfDocument.Open(filePath))
                        {
                            foreach (UglyToad.PdfPig.Content.Page page1 in document.GetPages())
                            {
                                text += ContentOrderTextExtractor.GetText(page1, true);
                            }
                        }

                        var shortName = GetShortName(text);

                        var shortAdres = GetShortAddress(text);

                        var longAdress = GetLongAddress(text);

                        var directorName = GetDirector(text);

                        var founders = GetFounders(text);
                        var directorInn = GetDirectorInn(text);

                        var director = new PhysicalLite
                        {
                            Inn = directorInn,
                            Name = directorName

                        };

                        var ErgulNalog = new ErgulNalogModel
                        {
                            ShortName = shortName,
                            ShortAddress = shortAdres,
                            FullAddress = longAdress,
                            Director = director,
                            Owners = founders
                        };

                        File.Delete(filePath);

                        dict.Add(inn, ErgulNalog);
                    }
                    catch(Exception ex)
                    {

                    }
                }

            }
            catch (Exception ex)
            {

            }
            finally
            {
                await context.CloseAsync();
            }
            return dict;
        }

        private async Task<ErgulNalogModel> Parse(string inn) 
        {
            var context = await browser.NewContextAsync(javaScriptEnabled: true, acceptDownloads: true);
            var ErgulNalog = new ErgulNalogModel();
            try
            {
                var page = await context.NewPageAsync();
                await page.GoToAsync("https://egrul.nalog.ru/index.html");

                await Task.Run(() =>
                page.ExtPrintTextToElement("//input[@class='txt-wide txt-string']", inn)
                    .OnSuccess(() => page.ExtClickElement("//button[@class='btn-with-icon btn-search']", 1000))
                    .OnSuccess(async () => await page.WaitForLoadStateAsync(LifecycleEvent.Load)));

                var downloadTask = page.WaitForEventAsync<DownloadEventArgs>(pageEvent: PageEvent.Download);
                page.ExtClickElement("//button[contains(@class,'btn-with-icon btn-excerpt')]");

                var d = downloadTask.Result.Download;
                var filePath = $"{DownloadforderPath}{inn}.pdf";

                await d.SaveAsAsync(filePath);
                await downloadTask.Result.Download.DeleteAsync();

                var text = "";

                using (PdfDocument document = PdfDocument.Open(filePath))
                {
                    foreach (UglyToad.PdfPig.Content.Page page1 in document.GetPages())
                    {
                        text += ContentOrderTextExtractor.GetText(page1, true);
                    }
                }

                var shortName = GetShortName(text);

                var shortAdres = GetShortAddress(text);

                var longAdress = GetLongAddress(text);

                var directorName = GetDirector(text);

                var founders = GetFounders(text);

                var fullName = GetLongName(text);

                var directorInn = GetDirectorInn(text);

                var director = new PhysicalLite
                {
                    Inn =  directorInn,
                    Name = directorName

                };

                ErgulNalog = new ErgulNalogModel
                {
                    ShortName = shortName,
                    ShortAddress = shortAdres,
                    FullAddress = longAdress,
                    Director = director,
                    Owners = founders,
                    FullName=fullName
                };

                File.Delete(filePath);

            }
            catch(Exception ex)
            {

            }
            finally
            {
                await context.CloseAsync();
            }
            return ErgulNalog;

        }

        private string GetShortName(string text)
        {
            var startWord = "Сокращенное наименование на русском\r\nязыке\r\n";
            var startIndex = text.IndexOf(startWord);

            var text2 = text.Substring(startIndex, text.Length - startIndex);

            var endINdex = text2.IndexOf("ГРН и дата внесения в ЕГРЮЛ записи,\r\nсодержащей указанные сведения\r\n");

            var result = text.Substring(startIndex, endINdex).Replace(startWord, string.Empty);
            result = result.Remove(result.Length - 3, 3);
            return ExtStringNormal(result);
        }

        private string GetShortAddress(string text)
        {
            var startWord = "Место нахождения юридического лица";
            var startIndex = text.IndexOf(startWord);

            var text2 = text.Substring(startIndex, text.Length - startIndex);

            var endINdex = text2.IndexOf("ГРН и дата внесения в ЕГРЮЛ записи,\r\nсодержащей указанные сведения");

            var result = text.Substring(startIndex, endINdex).Replace(startWord, string.Empty);
            result = result.Remove(result.Length - 3, 3);
            result = ExtStringNormal(result);
            return result;
        }

        private string GetLongName(string text)
        {
            var startWord = "Настоящая выписка содержит сведения о юридическом лице";
            var startIndex = text.IndexOf(startWord);

            var text2 = text.Substring(startIndex, text.Length - startIndex);

            var endINdex = text2.IndexOf("полное наименование юридического лица");

            var result = text.Substring(startIndex, endINdex).Replace(startWord, string.Empty);
            result = result.Remove(result.Length - 3, 3);
            result = ExtStringNormal(result);
            return result;

        }

        private string GetLongAddress(string text)
        {
            var startWord = "Адрес юридического лица";
            var startIndex = text.IndexOf(startWord);

            var text2 = text.Substring(startIndex, text.Length - startIndex);

            var endINdex = text2.IndexOf("ГРН и дата внесения в ЕГРЮЛ записи,\r\nсодержащей указанные сведения\r\n");

            var result = text.Substring(startIndex, endINdex).Replace(startWord, string.Empty);
            result = result.Remove(result.Length - 3, 3);
            result = ExtStringNormal(result);
            return result;

        }

        private string GetDirector(string text)
        {
            var startWord = "Сведения о лице, имеющем право без доверенности действовать от имени юридического\r\nлица";
            var startIndex = text.IndexOf(startWord);

            var text2 = text.Substring(startIndex, text.Length - startIndex);

            var endINdex = text2.IndexOf("ИНН");

            var result1 = text.Substring(startIndex, endINdex).Replace(startWord, string.Empty);
            result1 = result1.Remove(result1.Length - 3, 3);
            result1 = ExtStringNormal(result1);

            var startWord1 = "Фамилия Имя Отчество";
            var startIndex1 = result1.IndexOf(startWord1);
            var result = result1.Substring(startIndex1, result1.Length - startIndex1).Replace(startWord1, string.Empty);

            return result;

        }

        private string GetDirectorInn(string text)
        {
            var startWord = "Сведения о лице, имеющем право без доверенности действовать от имени юридического\r\nлица";
            var startIndex = text.IndexOf(startWord);

            var text2 = text.Substring(startIndex, text.Length - startIndex);

            var endINdex = text2.IndexOf("ГРН и дата внесения в ЕГРЮЛ записи");

            var result1 = text.Substring(startIndex, endINdex).Replace(startWord, string.Empty);
            result1 = result1.Remove(result1.Length - 3, 3);
            result1 = ExtStringNormal(result1);

            var startWord1 = "ИНН";
            var startIndex1 = result1.IndexOf(startWord1);
            var result = result1.Substring(startIndex1, result1.Length - startIndex1).Replace(startWord1, string.Empty);

            return result.Trim();

        }

        private List<PhysicalLite> GetFounders(string text)
        {
            var startWord = "Сведения об участниках / учредителях юридического лица";
            var startIndex = text.IndexOf(startWord);

            var text2 = text.Substring(startIndex, text.Length - startIndex);

            var endINdex = text2.IndexOf("Сведения об учете в налоговом органе");

            var foundersblock = text.Substring(startIndex, endINdex).Replace(startWord, string.Empty);
            foundersblock = foundersblock.Remove(foundersblock.Length - 3, 3);

            var regexLegalOwners = new Regex(@"ИНН \d*\r\n\d* Полное наименование([\s\S]{0,400})Размер доли \(в процентах\) [.0-9]*\r\n");
            var owners = new List<PhysicalLite>();

            var legalOwners = regexLegalOwners.Matches(foundersblock);
            foreach (Match item in legalOwners)
            {
                owners.Add(GetLegalOwner(item.Value));
            }


            var regexPersonOwners = new Regex(@"Фамилия\r?\nИмя\r?\nОтчество([\s\S]{0,400})Размер доли \(в процентах\) [.0-9]*\r\n");
            var physicalOwners = regexPersonOwners.Matches(foundersblock);
            foreach (Match item in physicalOwners)
            {
                owners.Add(GetPersonOwner(item.Value));
            }

            return owners;
        }

        private PhysicalLite GetLegalOwner(string text) 
        {
            var regexname = new Regex(@"Полное наименование([\s\S]*)\s\d* ГРН и дата внесения", RegexOptions.Compiled);
            var name = regexname.Match(text).Groups[1].Value;

            var regexInn = new Regex(@"ИНН (\d*)", RegexOptions.Compiled);
            var inn = regexInn.Match(text).Groups[1].Value;

            var regexPersent = new Regex(@"Размер доли \(в процентах\) ([.0-9]*)", RegexOptions.Compiled);
            var persent = regexPersent.Match(text).Groups[1].Value;

            var own = new PhysicalLite
            {
                OwnerType = OwnerType.LegalOwner,
                Inn = inn.Trim(),
                Name = ExtStringNormal(name),
                Percent = double.Parse(persent.Replace('.', ','))
            };
            return own;
        }

        private PhysicalLite GetPersonOwner(string text)
        {
            var regexname = new Regex(@"Фамилия\r?\nИмя\r?\nОтчество([\s\S]*)\s\d* ИНН", RegexOptions.Compiled);
            var name = regexname.Match(text).Groups[1].Value;

            var regexInn = new Regex(@"ИНН (\d*)", RegexOptions.Compiled);
            var inn = regexInn.Match(text).Groups[1].Value;

            var regexPersent = new Regex(@"Размер доли \(в процентах\) ([.0-9]*)", RegexOptions.Compiled);
            var persent = regexPersent.Match(text).Groups[1].Value;

            var own = new PhysicalLite
            {
                OwnerType= OwnerType.PhysicalOwner,
                Inn = inn.Trim(),
                Name = ExtStringNormal(name),
                Percent = double.Parse(persent.Replace('.', ','))
            };
            return own;
        }

        private string ExtStringNormal(string str)
        {
            return str.Replace("\r\n", " ").Replace("\r", string.Empty).Replace("\n", string.Empty).Trim();
        }

        
    }
}
