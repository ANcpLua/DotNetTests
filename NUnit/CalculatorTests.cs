namespace NUnit.Tests;

[TestFixture]
public class CalculatorTests
{
    [Test]
    public void Add_ReturnsSum()
    {
        var calculator = new Calculator();

        var result = calculator.Add(2, 3);

        Assert.That(result, Is.EqualTo(5));
    }

    [TestCase(1, 2, 3)]
    [TestCase(5, -3, 2)]
    [TestCase(0, 0, 0)]
    public void Add_WithTestCases(int a, int b, int expected)
    {
        Assert.That(new Calculator().Add(a, b), Is.EqualTo(expected));
    }

    [Test]
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
