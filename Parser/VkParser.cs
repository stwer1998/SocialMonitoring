using Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    public class VkParser:IParser
    {
        public async Task<LegalEntity> Parse(LegalEntity legalEntity)
        {
            return legalEntity;
        }

        public async Task<PhysicalPerson> Parse(PhysicalPerson physicalPerson)
        {
            if (!string.IsNullOrEmpty(physicalPerson.VkName))
            {
                physicalPerson.VkData = ParseData(physicalPerson.VkName);
            }
            return physicalPerson;
        }

        public async Task<IEnumerable<LegalEntity>> Parse(IEnumerable<LegalEntity> legalEntities)
        {
            return legalEntities;
        }

        public async Task<IEnumerable<PhysicalPerson>> Parse(IEnumerable<PhysicalPerson> physicalPeople)
        {
            foreach (var item in physicalPeople)
            {
                await Parse(item);
            }
            return physicalPeople;
        }

        private string ParseData(string vkname) 
        {
            //var vkname = "pavlovsemen";
            string RunningPath = AppDomain.CurrentDomain.BaseDirectory;

            string scriptPath = string.Format("{0}Resources\\vkparser.py", Path.GetFullPath(Path.Combine(RunningPath, @"..\..\..\..\Parser\")));

            try
            {
                string python = @"c:\Users\stwer\AppData\Local\Programs\Python\Python39\python.exe";

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
                var args = $"{myPythonApp} {vkname}";
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
                //Encoding cp866 = Encoding.GetEncoding("IBM866");
                byte[] bytes = Encoding.ASCII.GetBytes(myString);
                myString = Encoding.UTF8.GetString(bytes);

                return myString;

            }
            catch (Exception ex)
            {
                return "Ошибка при парсинге";
            }
        }
    }
}
