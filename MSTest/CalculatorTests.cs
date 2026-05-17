namespace MSTest.Tests;

[TestClass]
public class CalculatorTests
{
    [TestMethod]
    public void Add_ReturnsSum()
    {
        var calculator = new Calculator();

        var result = calculator.Add(2, 3);

        Assert.AreEqual(5, result);
    }

    [TestMethod]
    [DataRow(1, 2, 3)]
    [DataRow(5, -3, 2)]
    [DataRow(0, 0, 0)]
    public void Add_WithDataRow(int a, int b, int expected)
    {
        Assert.AreEqual(expected, new Calculator().Add(a, b));
    }

    [TestMethod]
    public void Divide_ByZero_Throws()
    {
        var calculator = new Calculator();

        Assert.ThrowsExactly<DivideByZeroException>(() => calculator.Divide(1, 0));
    }
}

file class Calculator
{
    public int Add(int a, int b) => a + b;
    public double Divide(int a, int b) =>
        b == 0 ? throw new DivideByZeroException() : (double)a / b;
}
