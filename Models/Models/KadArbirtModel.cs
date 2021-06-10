using Newtonsoft.Json;
using System.Collections.Generic;

namespace Models
{
    public class KadArbirtModel
    {
        [JsonIgnore]
        public int Id { get; set; }

        public int PlaintiffCaseNum { get; set; }

        public int PlaintiffCaseNumLast3Year { get; set; }

        public IList<CardCase> PlaintiffCases { get; set; }

        public int DefendantCaseNum { get; set; }

        public int DefendantCaseNumLast3Year { get; set; }

        public IList<CardCase> DefendantCase { get; set; }

        public override string ToString()
        {
            return $"{PlaintiffCaseNumLast3Year} {DefendantCaseNumLast3Year}";
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();

        }

        public enum TypeCourtCase
        {
            Administrative = 1,
            Civil = 2,
            Bankruptcy = 3,
            CourtOrders = 4
        }
    }

    public class CardCase
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string TypeCase { get; set; }

        public string CaseDate { get; set; }

        public string UrlCase { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is CardCase item))
            {
                return false;
            }
            else
            {
                return ToString() == item.ToString();
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return $"{TypeCase} {CaseDate} {UrlCase}";
        }
    }
}
