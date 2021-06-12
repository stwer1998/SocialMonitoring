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

            Init();

            Console.WriteLine();

            Console.ReadLine();

        }

        static void Init()
        {
            var t = Task.Run(() => Parser.Parser.UpdateData());
            Task.WaitAll(t);
        }
    }
}
