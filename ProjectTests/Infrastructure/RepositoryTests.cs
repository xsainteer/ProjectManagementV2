using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace ProjectTests.Infrastructure;

[TestFixture]
public class RepositoryTests
{
    private AppDbContext _context;
    private Repository<Employee> _repository;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new Repository<Employee>(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }
    
    [Test]
    public async Task AddAsync_AddsEntityToDatabase()
    {
        // Arrange
        var employee = new Employee { Id = 1, FirstName = "John", LastName = "Doe" };

        // Act
        await _repository.AddAsync(employee);
        await _repository.SaveChangesAsync();

        // Assert
        var result = await _context.Employees.FindAsync(1);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.FirstName, Is.EqualTo("John"));
    }

    [Test]
    public async Task GetByIdAsync_ReturnsCorrectEntity()
    {
        // Arrange
        var employee = new Employee { Id = 1, FirstName = "John", LastName = "Doe" };
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(1));
    }
}
