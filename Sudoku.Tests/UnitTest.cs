using Xunit;

namespace Sudoku.Tests;

public class UnitTest
{
    [Fact]
    public void TestRunSettings()
    {
        Assert.Equal("ortools", System.Environment.GetEnvironmentVariable("SOLVER"));
    }
}