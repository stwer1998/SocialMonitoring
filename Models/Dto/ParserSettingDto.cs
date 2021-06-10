using Models.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Dto
{
    public class ParserSettingDto
    {
        public ParserSettingDto(ParserSetting parserSetting)
        {
            Id = parserSetting.Id;
            Name = parserSetting.ParserName;
            Settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(parserSetting.JsonSettings);
        }
        public int Id { get; set; }

        public string Name { get; set; }

        public Dictionary<string,string> Settings { get; set; }
    }
}
