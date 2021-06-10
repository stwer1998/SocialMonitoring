using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Parser
{
    public class FsspParser : IParser
    {
        readonly HttpClient httpClient;
        readonly string token;

        public FsspParser()
        {
            httpClient = new HttpClient();
            token = "";
        }

        public async Task<PhysicalPerson> Parse(PhysicalPerson physicalPerson)
        {
            if(physicalPerson.BithDay==null || physicalPerson.BithDay == DateTime.MinValue)
            {
                return physicalPerson;
            }

            var request = $"https://api-ip.fssprus.ru/api/v1.0/search/physical?token={token}&region={physicalPerson.Inn.ToString().Substring(0,2)}&firstname={physicalPerson.Name}" +
                $"&lastname={physicalPerson.LastName}&secondname={physicalPerson.MiddleName}&birthdate={physicalPerson.BithDay.ToString("dd.MM.yyyy")}";
            var response = await httpClient.GetStringAsync(request);
            var result = JsonConvert.DeserializeObject<QueueObjecy>(response);

            int status = 1;
            response = await httpClient.GetStringAsync($"https://api-ip.fssprus.ru/api/v1.0/status?token={token}&task={result.response.task}");
            status = JsonConvert.DeserializeObject<MyRootobject>(response).response.status;
            while (status != 0)
            {
                Console.WriteLine("Sleep");
                Thread.Sleep(5000);
                response = await httpClient.GetStringAsync($"https://api-ip.fssprus.ru/api/v1.0/status?token={token}&task={result.response.task}");
                status = JsonConvert.DeserializeObject<MyRootobject>(response).response.status;
            }

            response = await httpClient.GetStringAsync($"https://api-ip.fssprus.ru/api/v1.0/result?token={token}&task={result.response.task}");

            var finalres = JsonConvert.DeserializeObject<ResultObject>(response);

            physicalPerson.FsspModels = finalres.response.result[0].result.Select(x => new FsspModel
            {
                Bailiff = x.bailiff,
                Department = x.department,
                Details = x.department,
                ExeProduction = x.exe_production,
                IpEnd = x.ip_end,
                Name = x.name,
                Subject = x.subject
            }).ToList();

            return physicalPerson;

        }

        public async Task<IEnumerable<LegalEntity>> Parse(IEnumerable<LegalEntity> legalEntities)
        {

            foreach (var item in legalEntities)
            {
                await Parse(item);
            }
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

        public async Task<LegalEntity> Parse(LegalEntity legalEntity)
        {
            if (string.IsNullOrEmpty(legalEntity.ErgulNalog.ShortName))
                return legalEntity;
            // to queue
            //var s = $"https://api-ip.fssprus.ru/api/v1.0/search/legal?token={token}&region={legalEntity.RegionNum}&name={legalEntity.ShortName.Replace("\"", "")}";
            var response = await httpClient.GetStringAsync($"https://api-ip.fssprus.ru/api/v1.0/search/legal?token={token}&region={legalEntity.RegionNum}&name={legalEntity.ShortName.Replace("\"","")}");
            var result = JsonConvert.DeserializeObject<QueueObjecy>(response);
            int status = 1;

            //check status request
            response = await httpClient.GetStringAsync($"https://api-ip.fssprus.ru/api/v1.0/status?token={token}&task={result.response.task}");
            status = JsonConvert.DeserializeObject<MyRootobject>(response).response.status;
            while (status != 0)
            {
                Console.WriteLine("Sleep");
                Thread.Sleep(5000);
                response = await httpClient.GetStringAsync($"https://api-ip.fssprus.ru/api/v1.0/status?token={token}&task={result.response.task}");
                status = JsonConvert.DeserializeObject<MyRootobject>(response).response.status;
            }

            //Максимальное число одиночных запросов в час ­— 100. (Ограничение на одиночные запросы считается, как минус час от текущего времени)
            //Максимальное число одиночных запросов в сутки ­— 1000. (Ограничение на одиночные запросы считается, как минус сутки от текущего времени)

            response = await httpClient.GetStringAsync($"https://api-ip.fssprus.ru/api/v1.0/result?token={token}&task={result.response.task}");

            var finalres = JsonConvert.DeserializeObject<ResultObject>(response);

            legalEntity.FsspModels = finalres.response.result[0].result.Select(x => new FsspModel
                    {
                        Bailiff = x.bailiff,
                        Department = x.department,
                        Details = x.department,
                        ExeProduction = x.exe_production,
                        IpEnd = x.ip_end,
                        Name = x.name,
                        Subject = x.subject
                    }).ToList();

            return legalEntity;


        }

        #region jsonObj
        private class QueueObjecy
        {
            public string status { get; set; }
            public int code { get; set; }
            public string exception { get; set; }
            public QueueResponse response { get; set; }
        }

        private class QueueResponse
        {
            public string task { get; set; }
        }


        private class ResultObject
        {
            public string status { get; set; }
            public int code { get; set; }
            public string exception { get; set; }
            public ResultResponse response { get; set; }
        }

        private class ResultResponse
        {
            public int status { get; set; }
            public string task_start { get; set; }
            public string task_end { get; set; }
            public Result[] result { get; set; }
        }

        private class Result
        {
            public int status { get; set; }
            public Query query { get; set; }
            public Result1[] result { get; set; }
        }

        private class Query
        {
            public int type { get; set; }
            public JsonParams _params { get; set; }
        }

        private class JsonParams
        {
            public string region { get; set; }
            public string name { get; set; }
        }

        private class Result1
        {
            public string name { get; set; }
            public string exe_production { get; set; }
            public string details { get; set; }
            public string subject { get; set; }
            public string department { get; set; }
            public string bailiff { get; set; }
            public string ip_end { get; set; }
        }


        private class MyRootobject
        {
            public string status { get; set; }
            public int code { get; set; }
            public string exception { get; set; }
            public Response response { get; set; }
        }

        private class Response
        {
            public int status { get; set; }
            public string progress { get; set; }
        }

        #endregion


    }
}
