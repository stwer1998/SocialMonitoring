using System;
using System.Collections.Generic;

namespace Models
{
    public class FsspToken
    {
        public int Id { get; set; }

        public string Token { get; set; }

        public IList<FsspTokenUsing> TokenUsings { get; set; }

    }

    public class FsspTokenUsing
    {
        public int Id { get; set; }

        public DateTime UsingDateTime { get; set; }

        public string Data { get; set; }

        public string Result { get; set; }
    }
}
