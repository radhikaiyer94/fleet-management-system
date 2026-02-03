using Microsoft.AspNetCore.Mvc;
using FleetManagementApi.Data;
using FleetManagementApi.Domain.Entities;

namespace FleetManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DriversController : ControllerBase
{
    private readonly FleetDbContext _dbContext;

    public DriversController(FleetDbContext context)
    {
        _dbContext = context;
    }

    [HttpGet]
    public IActionResult GetDrivers()
    {
        var drivers = _dbContext.Drivers.Where(driver => !driver.IsDeleted).ToList();
        return Ok(drivers);
    }

    [HttpGet("{id}")]
    public IActionResult GetDriver(Guid id)
    {
        var driver = _dbContext.Drivers.Where(d => !d.IsDeleted).FirstOrDefault(d => d.Id == id);
        return driver != null ? Ok(driver) : NotFound(new { message = "Driver not found." });
    }

    [HttpGet("{id}/assignments")]
    public IActionResult GetDriverAssignments(Guid id)
    {
        var existingDriver = _dbContext.Drivers.Where(d => !d.IsDeleted).FirstOrDefault(d => d.Id == id);
        if (existingDriver == null)
        {
            return NotFound(new { message = "Driver not found." });
        }

        var assignments = _dbContext.Assignments.Where(a => a.DriverId == id && !a.IsDeleted).ToList();
        return assignments.Count > 0 ? Ok(assignments) : NotFound(new { message = "No assignments found for this driver." });
    }

    [HttpPost]
    public IActionResult CreateDriver([FromBody] Driver driver)
    {
        if (driver.Id == default) driver.Id = Guid.NewGuid();
        driver.CreatedAt = DateTime.UtcNow;
        driver.UpdatedAt = DateTime.UtcNow;

        _dbContext.Drivers.Add(driver);
        _dbContext.SaveChanges();

        return CreatedAtAction(nameof(GetDriver), new { id = driver.Id }, driver);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateDriver(Guid id, [FromBody] Driver driver)
    {
        if (id != driver.Id)
        {
            return BadRequest(new { message = "Driver ID mismatch." });
        }

        var existingDriver = _dbContext.Drivers.Where(d => !d.IsDeleted).FirstOrDefault(d => d.Id == id);
        if (existingDriver == null)
        {
            return NotFound(new { message = "Driver not found." });
        }

        existingDriver.FirstName = driver.FirstName;
        existingDriver.LastName = driver.LastName;
        existingDriver.Email = driver.Email;
        existingDriver.PhoneNumber = driver.PhoneNumber;
        existingDriver.LicenseNumber = driver.LicenseNumber;
        existingDriver.LicenseExpiryDate = driver.LicenseExpiryDate;
        existingDriver.DateOfEmployment = driver.DateOfEmployment;
        existingDriver.Status = driver.Status;
        existingDriver.UpdatedAt = DateTime.UtcNow;

        _dbContext.SaveChanges();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteDriver(Guid id)
    {
        var existingDriver = _dbContext.Drivers.Where(d => !d.IsDeleted).FirstOrDefault(d => d.Id == id);
        if (existingDriver == null)
        {
            return NotFound(new { message = "Driver not found." });
        }

        existingDriver.IsDeleted = true;
        existingDriver.UpdatedAt = DateTime.UtcNow;

        _dbContext.SaveChanges();

        return NoContent();
    }
}
