using Models.DAL;
using Models.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Parser
{
    public class MassDataParse
    {
        public void ParseAddress() 
        {
            var db = new ApplicationContext();
            string text = File.ReadAllText(@"C:\Users\stwer\OneDrive\Рабочий стол\Diplom\массовые адреса.txt", Encoding.UTF8);
            var some = text.Split('\n').Select(x => x.Split(";")).ToList();
            var list = new List<MassAddress>(3199);
            foreach (var item in some)
            {
                if (item.Length < 8)
                    break;

                var model = new MassAddress
                {
                    RegionName = item[0],
                    LecationName = item[1],
                    City = item[2],
                    Settlement = item[3],
                    StreetName = item[4],
                    HomeNumber = item[5],
                    CorpusNumber = item[6],
                    ApartmentNumber = item[7],
                    NumberOfLegalEntities = int.Parse(item[8].Trim())
                };
                list.Add(model);
            }
            db.AddRange(list);
            db.SaveChanges();
        }

        public void ParseOwners()
        {
            var db = new ApplicationContext();
            string text = File.ReadAllText(@"C:\Users\stwer\OneDrive\Рабочий стол\Diplom\массовые учредители.txt", Encoding.UTF8);
            var some = text.Split('\n').Select(x => x.Split(";")).ToList();
            var list = new List<MassOwners>(3199);
            foreach (var item in some)
            {
                if (item.Length < 4)
                    break;

                if (string.IsNullOrEmpty(item[0]))
                {

                }
                else
                {
                    var model = new MassOwners
                    {
                        Inn = item[0],
                        Surname = item[1],
                        Name = item[2],
                        MidleName = item[3],
                        NumberOfLegalEntities = int.Parse(item[4].Trim())
                    };
                    list.Add(model);
                }
            }
            db.AddRange(list);
            db.SaveChanges();
        }
        public void ParseDirectors()
        {
            var db = new ApplicationContext();
            string text = File.ReadAllText(@"C:\Users\stwer\OneDrive\Рабочий стол\Diplom\массовые директоры.txt", Encoding.UTF8);
            var some = text.Split('\n').Select(x => x.Split(";")).ToList();
            var list = new List<MassDirectors>(3199);
            foreach (var item in some)
            {
                if (item.Length < 4)
                    break;

                if (string.IsNullOrEmpty(item[0]))
                {

                }
                else
                {
                    var model = new MassDirectors
                    {
                        Inn = item[0],
                        Surname = item[1],
                        Name = item[2],
                        MidleName = item[3],
                        NumberOfLegalEntities = int.Parse(item[4].Trim())
                    };
                    list.Add(model);
                }
            }
            db.AddRange(list);
            db.SaveChanges();
        }

        public void AddParserSettings() 
        {
            var bankrot = new Dictionary<string, string>();
            bankrot.Add("optionSelector", "//a[contains(@href,'/DebtorsSearch.aspx?Name=')]");
            bankrot.Add("inputSelector", "(//table[@id='ctl00_cphBody_tblOrgSearchFilter']//input)[3]");
            bankrot.Add("submitSelector", "//input[@src='img/but_search.png']");
            bankrot.Add("resultSelector", "//table[@class='bank']");

            var boNalog = new Dictionary<string, string>();
            boNalog.Add("startAction", "<img src=\"/static/media/check-circle.add244b8.svg\" alt=\"\"></div><p>([^<]*)<");
            boNalog.Add("VNActive", "Итого по разделу<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>1100<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>([^<]*)<br>");
            boNalog.Add("OActive", "Итого по разделу<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>1200<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>([^<]*)<br>");
            boNalog.Add("VBalance", "Баланс<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>1700<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>([^<]*)<br>");
            boNalog.Add("YK", @"Уставный капитал \(складочный капитал, уставный фонд, вклады товарищей\)<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>1310<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>([^<]*)<br>");
            boNalog.Add("CapitalRez", "Итого по разделу<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>1300<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>([^<]*)<br>");
            boNalog.Add("LongDebts", "Итого по разделу<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>1400<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>([^<]*)<div");
            boNalog.Add("ShortTDebts", "Итого по разделу<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>1500<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>([^<]*)<br>");
            boNalog.Add("Zaem", "Заемные средства<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>1510<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>([^<]*)<div");
            boNalog.Add("CreditorDebt", "Кредиторская задолженность<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>1520<[^>]*><[^>]*><[^>]*><[^>]*><[^>]*><[^>]*>([^<]*)<br>");
            boNalog.Add("Revenue", "Выручка<[^>]*><[^>]*><[^>]*>2110<[^>]*><[^>]*>([^<]*)<br>");
            boNalog.Add("Profit", @"Чистая прибыль \(убыток\)<[^>]*><[^>]*><[^>]*>2400<[^>]*><[^>]*>([^<]*)<br>");
            boNalog.Add("SalesProfit", @"Прибыль \(убыток\) от продаж<[^>]*><[^>]*><[^>]*>2200<[^>]*><[^>]*>([^<]*)<br>");

            var nedobrosevest = new Dictionary<string, string>();
            nedobrosevest.Add("inputSelector", "//input[contains(@placeholder,'Введите полностью или часть наименования (ФИО), ИНН (аналог ИНН)')]");
            nedobrosevest.Add("submitSelector", "//button[@class='search__btn']//img[1]");
            nedobrosevest.Add("textBlock", "//div[contains(@class,'search-registry-entrys-block')]");

            var rostFinMonitoring = new Dictionary<string, string>();
            rostFinMonitoring.Add("legalSelector", "//div[@id='russianUL']");
            rostFinMonitoring.Add("physicalSelector", "//div[@id='russianFL']");

            var list = new List<ParserSetting>()
            {
                new ParserSetting{ParserName="bakrot",JsonSettings=JsonConvert.SerializeObject(bankrot)},
                new ParserSetting{ParserName="boNalog",JsonSettings=JsonConvert.SerializeObject(boNalog)},
                new ParserSetting{ParserName="nedobrosevest",JsonSettings=JsonConvert.SerializeObject(nedobrosevest)},
                new ParserSetting{ParserName="rostFinMonitoring",JsonSettings=JsonConvert.SerializeObject(rostFinMonitoring)}
            };



            var db = new ApplicationContext();
            db.ParserSettings.AddRange(list);
            db.SaveChanges();


        }
    }
}
