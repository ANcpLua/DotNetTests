namespace UnitTests;

public class BasicTests
{
    [Test]
    public async Task Add_ReturnsSum()
    {
        var calculator = new Calculator();

        var result = calculator.Add(1, 2);

        await Assert.That(result).IsEqualTo(3);
    }

    [Test]
    public async Task Divide_ByZero_Throws()
    {
        var calculator = new Calculator();

        var action = () => calculator.Divide(1, 0);

        await Assert.That(action).ThrowsException()
            .WithMessage("Attempted to divide by zero.");
    }
}
