using System;
using System.Collections.Generic;
using System.Text;

namespace Models.DAL
{
    public interface IProxyRepository
    {
        Proxy GetFreeProxy();
    }
}
