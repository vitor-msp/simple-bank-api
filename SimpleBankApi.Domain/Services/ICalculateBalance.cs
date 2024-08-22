using SimpleBankApi.Domain.Entities;

namespace SimpleBankApi.Domain.Services;

public interface ICalculateBalance
{
    Task<double> FromAccount(IAccount account);
}