using System;
using System.Threading.Tasks;
using Lakerfield.RpcTest.Models;

namespace Lakerfield.RpcTest;

public partial interface IRpcTestService
{

    Task<Company> CompanyFindById(Guid id);
    Task<Company[]> CompanyFindAll();
    Task<Company> CompanySave(Company entity);
    Task<bool> CompanyDelete(Company entity);

    Task<(Company, string)> CompanyTest(Company entity, Company entity2, string wouter, int bert);
}
