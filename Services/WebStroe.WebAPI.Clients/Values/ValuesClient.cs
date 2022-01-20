using WebStore.Interfaces.TestAPI;
using WebStroe.WebAPI.Clients.Base;

namespace WebStroe.WebAPI.Clients.Values;

public class ValuesClient : BaseClient, IValuesService
{
    
    public ValuesClient(HttpClient Client) : base(Client, "api/values")
    {

    }

    public void Add(string Value)
    {
        throw new NotImplementedException();
    }

    public int Count()
    {
        throw new NotImplementedException();
    }

    public bool Delete(int Id)
    {
        throw new NotImplementedException();
    }

    public void Edit(int Id, string Value)
    {
        throw new NotImplementedException();
    }

    public string GetById(int Id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<string> GetValues()
    {
        throw new NotImplementedException();
    }
}

