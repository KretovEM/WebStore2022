using System.Net.Http.Json;
using WebStore.Domain.Entities;
using WebStore.Interfaces.Services;
using WebStroe.WebAPI.Clients.Base;

namespace WebStroe.WebAPI.Clients.Employees;

public class EmployessClient : BaseClient, IEmployeesData
{
    public EmployessClient(HttpClient Client) : base(Client, "api/employees")
    {
    }

    public int Add(Employee employee)
    {
        var response = Post(Address, employee);
        var addedEmployee = response.Content.ReadFromJsonAsync<Employee>().Result;
        if (addedEmployee is null)
            return -1;
        var id = addedEmployee.Id;
        employee.Id = id;
        return id;
    }

    public bool Delete(int id)
    {
        var response = Delete($"{Address}/{id}");
        var success = response.IsSuccessStatusCode;
        return success;
    }

    public bool Edit(Employee employee)
    {
        var response = Put(Address, employee);
        var success = response.EnsureSuccessStatusCode()
           .Content
           .ReadFromJsonAsync<bool>()
           .Result;
        return success;
    }

    public IEnumerable<Employee> GetAll()
    {
        var employees = Get<IEnumerable<Employee>>(Address);
        return employees!;
    }

    public Employee? GetById(int id)
    {
        var result = Get<Employee>($"{Address}/{id}");
        return result;
    }
}
