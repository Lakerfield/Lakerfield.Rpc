using System;
using System.Threading.Tasks;
using RpcSample.Models;

namespace RpcSample;

public partial interface IRpcTestService
{

    Task<Company> CompanyFindById(Guid id);
    Task<Company[]> CompanyFindAll();
    Task<Company> CompanySave(Company entity);
    Task<bool> CompanyDelete(Company entity);

    Task<(Company, string)> CompanyTest(Company entity, Company entity2, string wouter, int bert);

    IObservable<Company> GetObservable(Guid id);

}
