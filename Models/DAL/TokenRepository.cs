using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Models.DAL
{

    public interface ITokenRepository
    {
        FsspToken GetFreeFsspToken();
        void UpdateToken(FsspToken fsspToken);
    }
    public class TokenRepository : ITokenRepository
    {
        private readonly ApplicationContext applicationContext;
        public TokenRepository()
        {
            applicationContext = new ApplicationContext();
        }

        public FsspToken GetFreeFsspToken()
        {
            return applicationContext.FsspTokens.FirstOrDefault(x => x.TokenUsings.Where(x => x.UsingDateTime < DateTime.Now.AddHours(-1)).ToList().Count < 100);
        }

        public void UpdateToken(FsspToken fsspToken)
        {
            applicationContext.FsspTokens.Update(fsspToken);
            applicationContext.SaveChanges();
        }
    }
}
