using FluentAssertions;
using Xunit;

namespace EleveRats.Tests.Core;

/// <summary>
/// Simple test class to verify if the testing infrastructure is working as expected.
/// </summary>
public class ExampleTests
{
    /// <summary>
    /// Verifies that basic math still works and FluentAssertions is correctly configured.
    /// </summary>
    [Fact]
    public void TestInfrastructure_ShouldBeOperacional()
    {
        // Arrange
        var expectedValue = 42;
        var actualValue = 40 + 2;

        // Act & Assert
        actualValue.Should().Be(expectedValue, "because the universe depends on it");
    }
}
