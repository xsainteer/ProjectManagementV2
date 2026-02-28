using Application.DTOs.Project;
using Application.Interfaces;
using Application.Interfaces.Services;
using Application.Services;
using Domain.Common;
using Domain.Entities;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace ProjectTests.Application;

[TestFixture]
public class ProjectServiceTests
{
    private Mock<IProjectRepository> _projectRepositoryMock;
    private Mock<IEmployeeRepository> _employeeRepositoryMock;
    private Mock<ILogger<ProjectService>> _loggerMock;
    private Mock<IValidator<CreateProjectDto>> _createValidatorMock;
    private Mock<IValidator<CreateFullProjectDto>> _createFullValidatorMock;
    private Mock<IValidator<UpdateProjectDto>> _updateValidatorMock;
    private ProjectService _projectService;

    [SetUp]
    public void SetUp()
    {
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _employeeRepositoryMock = new Mock<IEmployeeRepository>();
        _loggerMock = new Mock<ILogger<ProjectService>>();
        _createValidatorMock = new Mock<IValidator<CreateProjectDto>>();
        _createFullValidatorMock = new Mock<IValidator<CreateFullProjectDto>>();
        _updateValidatorMock = new Mock<IValidator<UpdateProjectDto>>();

        _projectService = new ProjectService(
            _projectRepositoryMock.Object,
            _employeeRepositoryMock.Object,
            _loggerMock.Object,
            _createValidatorMock.Object,
            _createFullValidatorMock.Object,
            _updateValidatorMock.Object);
    }

    [Test]
    public async Task GetByIdAsync_ExistingProject_ReturnsSuccess()
    {
        // Arrange
        var projectId = 1;
        var project = new Project { Id = projectId, Name = "Test Project" };
        _projectRepositoryMock.Setup(r => r.GetByIdAsync(projectId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        // Act
        var result = await _projectService.GetByIdAsync(projectId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Id, Is.EqualTo(projectId));
            Assert.That(result.Value.Name, Is.EqualTo("Test Project"));
        });
    }
    
    [Test]
    public async Task CreateAsync_ValidDto_CallsRepositoryAndReturnsSuccess()
    {
        // Arrange
        var createDto = new CreateProjectDto(
            "New Project",
            "Customer Co",
            "Performer Co",
            1,
            DateTime.UtcNow,
            null,
            1
        );
        var manager = new Employee { Id = 1, FirstName = "Manager" };

        _createValidatorMock.Setup(v => v.ValidateAsync(createDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _employeeRepositoryMock.Setup(r => r.GetByIdAsync(createDto.ProjectManagerId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(manager);

        // Act
        var result = await _projectService.CreateAsync(createDto);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        _projectRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()), Times.Once);
        _projectRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
