using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Models
{
    public class MassAddress
    {
        public int Id { get; set; }
        public string RegionName { get; set; }

        public string LecationName { get; set; }

        public string City { get; set; }

        public string Settlement { get; set; }

        public string StreetName { get; set; }
        
        public string HomeNumber { get; set; }
        
        public string CorpusNumber { get; set; }
        
        public string ApartmentNumber { get; set; }

        public int NumberOfLegalEntities { get; set; }


    }
}
