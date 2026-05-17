namespace XUnit.Tests;

public class CalculatorTests
{
    [Fact]
    public void Add_ReturnsSum()
    {
        var calculator = new Calculator();

        var result = calculator.Add(2, 3);

        Assert.Equal(5, result);
    }

    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(5, -3, 2)]
    [InlineData(0, 0, 0)]
    public void Add_WithInlineData(int a, int b, int expected)
    {
        Assert.Equal(expected, new Calculator().Add(a, b));
    }

    [Fact]
    public void Divide_ByZero_Throws()
    {
        var calculator = new Calculator();

        Assert.Throws<DivideByZeroException>(() => calculator.Divide(1, 0));
    }
}

file class Calculator
{
    public int Add(int a, int b) => a + b;
    public double Divide(int a, int b) =>
        b == 0 ? throw new DivideByZeroException() : (double)a / b;
}
