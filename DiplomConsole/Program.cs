using Microsoft.EntityFrameworkCore;
using Models;
using Models.DAL;
using Models.Models;
using Newtonsoft.Json;
using Parser;
using PlaywrightSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiplomConsole
{
    class Program
    {
        static void Main(string[] args)
        {

            Some();

            Console.WriteLine();

            Console.ReadLine();

        }


        

        static async void Some()
        {
            var list = new List<string>
            {

            };
            //var some = new LegalEntity { Inn = "1660056570" };
            ////var some1 = new LegalEntity { Inn = "2904017523" };

            //var list1 = new List<LegalEntity> { some };

            ////Parser.Parser.UpdateData();

            //var r = new PhysicalPerson { Inn = "166015120180" };




            //var t = Task.Run(() => Parser.Parser.UpdateData());
            //Task.WaitAll(t);

            //var t1 = Task.Run(() => Parser.Parser.Parse(some));
            //Task.WaitAll(t1);

            //var a1 = JsonConvert.SerializeObject(t.Result);
            //var a2 = JsonConvert.SerializeObject(t1.Result);

            //Console.WriteLine(a1==a2);

            Console.WriteLine("Start parsing");

            //parser.StartParse(list1);

            //var listq = list.Select(x => new LegalEntity() { Inn = x }).Take(10).ToList();
            //var stopwatch = new Stopwatch();
            //stopwatch.Start();
            //var t = Task.Run(()=>Parser.Parser.Parse(listq));
            //Task.WaitAll(t);
            //stopwatch.Stop();
            //TimeSpan timeSpan = stopwatch.Elapsed;
            //Console.WriteLine(timeSpan.ToString($"{timeSpan.Minutes}:{timeSpan.Seconds}"));

            //foreach (var item in list)
            //{
            //    var s = new LegalEntity { Inn = item };
            //    parser.StartParsing(s);
            //}
            Console.WriteLine("Finish parsing");

        }
    }
}
