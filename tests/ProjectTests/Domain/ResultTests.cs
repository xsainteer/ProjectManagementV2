using Domain.Common;
using NUnit.Framework;

namespace ProjectTests.Domain;

[TestFixture]
public class ResultTests
{
    [Test]
    public void Success_ShouldReturnIsSuccessTrue()
    {
        // Act
        var result = Result.Success();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.EqualTo(Error.None));
        });
    }

    [Test]
    public void Failure_ShouldReturnIsSuccessFalse()
    {
        // Arrange
        var error = Error.Failure("Test failure");

        // Act
        var result = Result.Failure(error);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo(error));
        });
    }

    [Test]
    public void ValueSuccess_ShouldReturnCorrectValue()
    {
        // Arrange
        var value = 42;

        // Act
        var result = Result<int>.Success(value);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo(value));
        });
    }

    [Test]
    public void ValueFailure_AccessingValue_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var error = Error.Failure("Test failure");
        var result = Result<int>.Failure(error);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }
}
