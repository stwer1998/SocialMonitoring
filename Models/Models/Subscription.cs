using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class Subscription
    {
        public int Id { get; set; }

        public LegalEntity LegalEntity { get; set; }

        public PhysicalPerson PhyicalPerson { get; set; }

        public DateTime DateTime { get; set; }
    }
}
