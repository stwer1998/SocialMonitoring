using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Models
{
    public enum QueueState
    {
        InQueue=1,
        InProgress=2,
        NotFullInfoForParsing=3,
        Parsed=4,
        ExeptionOnParsing=5
    }
}
