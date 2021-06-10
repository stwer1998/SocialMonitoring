using Newtonsoft.Json;

namespace Models
{
    public class FsspModel
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string Name { get; set; }
        public string ExeProduction { get; set; }
        public string Details { get; set; }
        public string Subject { get; set; }
        public string Department { get; set; }
        public string Bailiff { get; set; }
        public string IpEnd { get; set; }
    }
}
