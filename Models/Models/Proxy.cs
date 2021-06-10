using System;

namespace Models
{
    public class Proxy
    {
        public int Id { get; set; }

        public string IpAddress { get; set; }

        public string Port { get; set; }

        public ProxyState State { get; set; }

        public DateTime LastUsing { get; set; }
    }

    public enum ProxyState
    {

    }
}
