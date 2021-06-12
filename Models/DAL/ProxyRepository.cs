using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Models.DAL
{
    public class ProxyRepository : IProxyRepository
    {
        private readonly ApplicationContext applicationContext;
        public ProxyRepository()
        {
            applicationContext = new ApplicationContext();
        }
        public Proxy GetFreeProxy()
        {
           return applicationContext.Proxies.FirstOrDefault(x => x.LastUsing < DateTime.Now.AddHours(-7));
        }

        public void Update(Proxy proxy)
        {
            applicationContext.Proxies.Update(proxy);
            applicationContext.SaveChanges();
        }
    }
}
