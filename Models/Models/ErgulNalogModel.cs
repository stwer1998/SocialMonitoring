using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Models
{
    public class ErgulNalogModel
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string ShortName { get; set; }
        public string FullName { get; set; }
        public string ShortAddress { get; set; }
        public string FullAddress { get; set; }
        public PhysicalLite Director { get; set; }
        public IList<PhysicalLite> Owners { get; set; }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ErgulNalogModel item))
            {
                return false;
            }
            else
            {
                return ToString() == item.ToString();
            }
        }

        public override string ToString()
        {
            return $"{ShortName} {Director} {string.Join('\n',Owners.Select(x=>x.ToString()))}";
        }

    }

    public class PhysicalLite
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string Inn { get; set; }
        public string Name { get; set; }
        public double Percent { get; set; }
        [JsonIgnore]
        public OwnerType OwnerType { get; set; }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PhysicalLite item))
            {
                return false;
            }
            else
            {
                return ToString() == item.ToString();
            }
        }

        public override string ToString()
        {
            if (OwnerType.PhysicalOwner ==OwnerType)
            {
                return $"Физ лицо: {Inn} {Name} {Percent}"; 
            }
            else
            {
                return $"Юр лицо {Inn} {Name} {Percent}";
            }
        }
    }

    public enum OwnerType
    {
        LegalOwner = 1,
        PhysicalOwner = 2
    }
}
