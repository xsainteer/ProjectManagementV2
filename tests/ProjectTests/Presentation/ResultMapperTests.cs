using Domain.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Presentation.Endpoints;
using NUnit.Framework;

namespace ProjectTests.Presentation;

[TestFixture]
public class ResultMapperTests
{
    [Test]
    public void ToActionResult_SuccessResult_ReturnsCorrectStatusCode()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var actionResult = ResultMapper.ToActionResult(result);

        // Assert
        Assert.That(actionResult, Is.InstanceOf<StatusCodeHttpResult>());
        var statusCodeResult = (StatusCodeHttpResult)actionResult;
        Assert.That(statusCodeResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
    }

    [Test]
    public void ToActionResult_SuccessResultWithCustomCode_ReturnsCorrectStatusCode()
    {
        // Arrange
        var result = Result.Success();
        var customCode = StatusCodes.Status201Created;

        // Act
        var actionResult = ResultMapper.ToActionResult(result, customCode);

        // Assert
        Assert.That(actionResult, Is.InstanceOf<StatusCodeHttpResult>());
        var statusCodeResult = (StatusCodeHttpResult)actionResult;
        Assert.That(statusCodeResult.StatusCode, Is.EqualTo(customCode));
    }

    [Test]
    public void ToActionResultWithValue_SuccessResult_ReturnsJsonWithValue()
    {
        // Arrange
        var value = new { Name = "Test" };
        var result = Result<object>.Success(value);

        // Act
        var actionResult = ResultMapper.ToActionResult(result);

        // Assert
        Assert.That(actionResult, Is.InstanceOf<JsonHttpResult<object>>());
        var jsonResult = (JsonHttpResult<object>)actionResult;
        Assert.Multiple(() =>
        {
            Assert.That(jsonResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(jsonResult.Value, Is.EqualTo(value));
        });
    }

    [Test]
    public void ToActionResult_NotFoundError_Returns404Problem()
    {
        // Arrange
        var errorMessage = "Not found error";
        var result = Result.Failure(Error.NotFound(errorMessage));

        // Act
        var actionResult = ResultMapper.ToActionResult(result);

        // Assert
        Assert.That(actionResult, Is.InstanceOf<ProblemHttpResult>());
        var problemResult = (ProblemHttpResult)actionResult;
        Assert.Multiple(() =>
        {
            Assert.That(problemResult.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
            Assert.That(problemResult.ProblemDetails.Detail, Is.EqualTo(errorMessage));
        });
    }

    [Test]
    public void ToActionResult_ValidationError_Returns400Problem()
    {
        // Arrange
        var errorMessage = "Invalid input";
        var result = Result.Failure(Error.Validation(errorMessage));

        // Act
        var actionResult = ResultMapper.ToActionResult(result);

        // Assert
        Assert.That(actionResult, Is.InstanceOf<ProblemHttpResult>());
        var problemResult = (ProblemHttpResult)actionResult;
        Assert.Multiple(() =>
        {
            Assert.That(problemResult.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
            Assert.That(problemResult.ProblemDetails.Detail, Is.EqualTo(errorMessage));
        });
    }
}
