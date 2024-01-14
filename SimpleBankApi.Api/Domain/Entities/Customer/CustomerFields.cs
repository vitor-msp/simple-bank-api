using System.Text.Json.Serialization;

namespace Models;

public class CustomerFields
{
    [JsonIgnore]
    public int Id { get; private set; }
    public string? Cpf { get; set; }
    public string? Name { get; set; }


    public CustomerFields() { }

    private CustomerFields(int id)
    {
        Id = id;
    }

    public static CustomerFields Rebuild(int id, string cpf, string name)
    {
        return new CustomerFields(id)
        {
            Cpf = cpf,
            Name = name
        };
    }
}