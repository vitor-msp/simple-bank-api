using System.ComponentModel.DataAnnotations;
using SimpleBankApi.Domain.Entities;

namespace SimpleBankApi.Application.Input;

public class UpdateAccountInput
{
    [Required]
    public string Name { get; set; } = "";

    internal void Update(IAccount account)
    {
        account.Owner.Name = Name;
    }
}