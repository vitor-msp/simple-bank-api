using SimpleBankApi.Domain.Entities;

namespace SimpleBankApi.Domain.Contract;

public interface ITokenProvider
{
    string Generate(IAccount account);
}