using AwesomeAssertions;
using Xunit;

namespace EleveRats.Tests.Core;

/// <summary>
/// Simple test class to verify if the testing infrastructure is working as expected.
/// </summary>
public class ExampleTests
{
    /// <summary>
    /// Verifies that basic math still works and AwesomeAssertions is correctly configured.
    /// </summary>
    [Fact]
    public void TestInfrastructure_ShouldBeOperacional()
    {
        // Arrange
        int expectedValue = 42;
        int actualValue = 40 + 2;

        // Act & Assert
        actualValue.Should().Be(expectedValue, "because the universe depends on it");
    }
}
