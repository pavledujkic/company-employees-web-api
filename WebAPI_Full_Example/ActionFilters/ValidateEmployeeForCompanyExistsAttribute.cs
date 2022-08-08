﻿using Contracts;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebAPI_Full_Example.ActionFilters;

public class ValidateEmployeeForCompanyExistsAttribute : IAsyncActionFilter
{
    private readonly IRepositoryManager _repository;
    private readonly ILoggerManager _logger;

    public ValidateEmployeeForCompanyExistsAttribute(IRepositoryManager repository, ILoggerManager logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var method = context.HttpContext.Request.Method;
        var trackChanges = method.Equals("PUT") || method.Equals("PATCH");

        var companyId = (Guid)context.ActionArguments["companyId"]!;
        Company? company = await _repository.Company.GetCompanyAsync(companyId, false);

        if (company == null)
        {
            _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
            context.Result = new NotFoundResult();
            return;
        }

        var id = (Guid)context.ActionArguments["id"]!;
        Employee? employee = await _repository.Employee.GetEmployeeAsync(companyId, id, trackChanges);

        if (employee == null)
        {
            _logger.LogInfo($"Employee with id: {id} doesn't exist in the database.");
            context.Result = new NotFoundResult();
        }
        else
        {
            context.HttpContext.Items.Add("employee", employee);
            await next();
        }
    }
}