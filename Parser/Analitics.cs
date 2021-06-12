using Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Parser
{
    public class Analitics : IParser
    {
        public async Task<LegalEntity> Parse(LegalEntity legalEntity)
        {
            legalEntity.AnaliticsModuleResult = ParseData(legalEntity);
            return legalEntity;
        }

        public async Task<PhysicalPerson> Parse(PhysicalPerson physicalPerson)
        {
            return physicalPerson;
        }

        public async Task<IEnumerable<LegalEntity>> Parse(IEnumerable<LegalEntity> legalEntities)
        {
            foreach (var item in legalEntities)
            {
                item.AnaliticsModuleResult = ParseData(item);
            }
            return legalEntities;
        }

        public async Task<IEnumerable<PhysicalPerson>> Parse(IEnumerable<PhysicalPerson> physicalPeople)
        {
            return physicalPeople;
        }

        private string ParseData(LegalEntity legalEntity)
        {
            if (legalEntity.BoNalogModel == null)
                return null;

            var data = new JsonDto
            {
                VNActive = legalEntity.BoNalogModel.NonCurrentAssets,
                OActive = legalEntity.BoNalogModel.CurrentAssets,
                VBalance = legalEntity.BoNalogModel.BalanceCurrency,
                YK = legalEntity.BoNalogModel.AuthorizedCapital,
                CapitalRez = legalEntity.BoNalogModel.CapitalAndReserves,
                LongDebts = legalEntity.BoNalogModel.LongTermCommitment,
                ShortTDebts = legalEntity.BoNalogModel.CurrentLiabilities,
                Revenue = legalEntity.BoNalogModel.Revenue,
                Profit = legalEntity.BoNalogModel.Profit,
                Zaem = legalEntity.BoNalogModel.Borrowings,
                CreditorDebt = legalEntity.BoNalogModel.AccountsPayable,
                SalesProfit = legalEntity.BoNalogModel.ProfitFromSale,
                Bancrot = legalEntity.OnBankruptcy,
                RNP = legalEntity.OnBankruptcy,
                path = "forestbezssch2.pickle",
                result = "your result"
            };

            string RunningPath = AppDomain.CurrentDomain.BaseDirectory;
            string scriptPath = string.Format("{0}Resources\\file1.py", Path.GetFullPath(Path.Combine(RunningPath, @"..\..\..\..\Parser\")));
            string picleFilePath = string.Format("{0}Resources\\forestbezssch2.pickle", Path.GetFullPath(Path.Combine(RunningPath, @"..\..\..\..\Parser\")));

            try
            {
                string python = @"Path\python.exe";

                // python app to call 
                string myPythonApp = scriptPath;

                // dummy parameters to send Python script

                // Create new process start info 
                ProcessStartInfo myProcessStartInfo = new ProcessStartInfo(python);

                // make sure we can read the output from stdout 
                myProcessStartInfo.UseShellExecute = false;
                myProcessStartInfo.RedirectStandardOutput = true;

                // start python app with 3 arguments  
                // 1st arguments is pointer to itself,  
                // 2nd and 3rd are actual arguments we want to send 
                var args = $"{myPythonApp} {data.VNActive} {data.OActive} {data.VBalance} {data.YK} {data.CapitalRez} {data.LongDebts} " +
                    $"{data.ShortTDebts} {data.Revenue} {data.Profit} {data.Zaem} {data.CreditorDebt} {data.SalesProfit} {data.Bancrot.ToString().ToLowerInvariant()} {data.RNP.ToString().ToLowerInvariant()} {picleFilePath}";

                myProcessStartInfo.Arguments = args;

                Process myProcess = new Process();
                // assign start information to the process 
                myProcess.StartInfo = myProcessStartInfo;
                // start the process 
                myProcess.Start();

                // Read the standard output of the app we called.  
                // in order to avoid deadlock we will read output first 
                // and then wait for process terminate: 
                StreamReader myStreamReader = myProcess.StandardOutput;
                string myString = myStreamReader.ReadLine();

                /*if you need to read multiple lines, you might use: 
                    string myString = myStreamReader.ReadToEnd() */

                // wait exit signal from the app we called and then close it. 
                myProcess.WaitForExit();
                myProcess.Close();

                // write the output we got from python app 
                var result = int.Parse(myString?.Trim());

                return result switch
                {
                    1 => "Высокий риск, обратите внимание на индексы, реестры и финансовые показатели",
                    2 => "Риск низкий, сотрудничество возможно",
                    _ => "Ошибка при анализе",
                };
            }
            catch (Exception ex)
            {
                return "Ошибка при анализе";
            }
        }

        class JsonDto
        {
            public double VNActive { get; set; }
            public double OActive { get; set; }
            public double VBalance { get; set; }
            public double YK { get; set; }
            public double CapitalRez { get; set; }
            public double LongDebts { get; set; }
            public double ShortTDebts { get; set; }
            public double Revenue { get; set; }
            public double Profit { get; set; }
            public double Zaem { get; set; }
            public double CreditorDebt { get; set; }
            public double SalesProfit { get; set; }
            public bool Bancrot { get; set; }
            public bool RNP { get; set; }
            public string path { get; set; }
            public string result { get; set; }
        }
    }
}
