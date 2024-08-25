using SimpleBankApi.Domain.Contract;
using SimpleBankApi.Domain.Entities;

namespace SimpleBankApi.Infra;

public class TokenProvider : ITokenProvider
{
    public string Generate(IAccount account)
    {
        throw new NotImplementedException();
    }
}