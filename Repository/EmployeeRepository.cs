﻿using Contracts;
using Entities;
using Entities.Models;

namespace Repository;

public class EmployeeRepository : RepositoryBase<Employee>, IEmployeeRepository
{
    public EmployeeRepository(RepositoryContext repositoryContext)
        : base(repositoryContext)
    {
    }

    public IEnumerable<Employee> GetEmployees(Guid companyId, bool trackChanges) =>
        FindByCondition(employee => employee.CompanyId.Equals(companyId), trackChanges)
            .OrderBy(employee => employee.Name);

    public Employee? GetEmployee(Guid companyId, Guid id, bool trackChanges) =>
        FindByCondition(employee => employee.CompanyId.Equals(companyId) && employee.Id.Equals(id), trackChanges)
            .SingleOrDefault();
}