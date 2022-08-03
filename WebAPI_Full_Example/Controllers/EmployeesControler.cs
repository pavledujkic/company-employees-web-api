﻿using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI_Full_Example.Controllers
{
    [Route("api/companies/{companyId}/employees")]
    [ApiController]
    public class EmployeesControler : ControllerBase
    {
        private readonly ILoggerManager _logger;
        private readonly IRepositoryManager _repository;
        private readonly IMapper _mapper;

        public EmployeesControler(ILoggerManager logger, IRepositoryManager repository, IMapper mapper)
        {
            _logger = logger;
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetEmployeesForCompany(Guid companyId)
        {
            _logger.LogInfo("GetEmployeesForCompany was called");

            Company? company = _repository.Company.GetCompany(companyId, false);
            
            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
                
                return NotFound();
            }

            _logger.LogInfo($"Company with id: {companyId} found in the database.");

            var employeesFromDb = _repository.Employee.GetEmployees(companyId, false);

            var employeesDto = _mapper.Map<IEnumerable<EmployeeDto>>(employeesFromDb);
            
            return Ok(employeesDto);
        }

        [HttpGet("{id:guid}", Name="GetEmployeeForCompany")]
        public IActionResult GetEmployeeForCompany(Guid companyId, Guid id)
        {
            Company? company = _repository.Company.GetCompany(companyId, false);

            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");

                return NotFound();
            }

            Employee? employeeDb = _repository.Employee.GetEmployee(companyId, id, false);

            if (employeeDb == null)
            {
                _logger.LogInfo($"Employee with id: {id} doesn't exist in the database.");

                return NotFound();
            }

            _logger.LogInfo($"Employee with id: {id} found in the database.");

            var employeeDto = _mapper.Map<EmployeeDto>(employeeDb);

            return Ok(employeeDto);
        }

        [HttpPost]
        public IActionResult CreateEmployeeForCompany(Guid companyId, [FromBody] EmployeeForCreationDto? employee)
        {
            if (employee == null)
            {
                _logger.LogError("EmployeeForCreationDto object sent from client is null.");
                
                return BadRequest("EmployeeForCreationDto object is null");
            }

            Company? company = _repository.Company.GetCompany(companyId, false);

            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");

                return NotFound();
            }

            var employeeEntity = _mapper.Map<Employee>(employee);

            _repository.Employee.CreateEmployeeForCompany(companyId, employeeEntity);

            _repository.Save();

            var employeeToReturn = _mapper.Map<EmployeeDto>(employeeEntity);

            return CreatedAtRoute("GetEmployeeForCompany", 
                new { companyId, id = employeeToReturn.Id }, employeeToReturn);
        }

        [HttpDelete("{id:guid}")]
        public IActionResult DeleteEmployeeForCompany(Guid companyId, Guid id)
        {
            Company? company = _repository.Company.GetCompany(companyId, false);

            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");

                return NotFound();
            }

            Employee? employeeForCompany = _repository.Employee.GetEmployee(companyId, id, false);

            if (employeeForCompany == null)
            {
                _logger.LogInfo($"Employee with id: {id} doesn't exist in the database.");

                return NotFound();
            }

            _repository.Employee.DeleteEmployee(employeeForCompany);

            _repository.Save();

            _logger.LogInfo($"Employee with id: {id} was deleted from the database.");

            return NoContent();
        }

        [HttpPut("{id:guid}")]
        public IActionResult UpdateEmployeeForCompany(Guid companyId, Guid id, [FromBody] EmployeeForUpdateDto? employee)
        {
            if (employee == null)
            {
                _logger.LogError("EmployeeForUpdateDto object sent from client is null.");

                return BadRequest("EmployeeForUpdateDto object is null");
            }

            Company? company = _repository.Company.GetCompany(companyId, false);

            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");

                return NotFound();
            }

            Employee? employeeEntity = _repository.Employee.GetEmployee(companyId, id, true);

            if (employeeEntity == null)
            {
                _logger.LogInfo($"Employee with id: {id} doesn't exist in the database.");

                return NotFound();
            }

            _mapper.Map(employee, employeeEntity);

            _repository.Save();

            _logger.LogInfo($"Employee with id: {id} was updated in the database.");

            return NoContent();
        }

        [HttpPatch("{id:guid}")]
        public IActionResult PartiallyUpdateEmployeeForCompany(Guid companyId, Guid id, 
            [FromBody] JsonPatchDocument<EmployeeForUpdateDto>? patchDoc)
        {
            if (patchDoc == null)
            {
                _logger.LogError("JsonPatchDocument object sent from client is null.");

                return BadRequest("JsonPatchDocument object is null");
            }

            Company? company = _repository.Company.GetCompany(companyId, false);

            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");

                return NotFound();
            }

            Employee? employeeEntity = _repository.Employee.GetEmployee(companyId, id, true);

            if (employeeEntity == null)
            {
                _logger.LogInfo($"Employee with id: {id} doesn't exist in the database.");

                return NotFound();
            }

            var employeeToPatch = _mapper.Map<EmployeeForUpdateDto>(employeeEntity);

            patchDoc.ApplyTo(employeeToPatch, ModelState);

            TryValidateModel(employeeToPatch);

            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model object sent from client.");

                return BadRequest(ModelState);
            }

            _mapper.Map(employeeToPatch, employeeEntity);

            _repository.Save();

            _logger.LogInfo($"Employee with id: {id} was updated in the database.");

            return NoContent();
        }
    }
}
