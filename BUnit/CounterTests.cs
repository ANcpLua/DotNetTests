using WebApp.Components.Pages;

namespace BUnit.Tests;

public class CounterTests : BunitContext
{
    [Fact]
    public void Counter_StartsAtZero()
    {
        var counter = Render<Counter>();

        counter.Find("p[role='status']").MarkupMatches("<p role=\"status\">Current count: 0</p>");
    }

    [Fact]
    public void Counter_IncrementsOnClick()
    {
        var counter = Render<Counter>();

        counter.Find("button").Click();

        Assert.Contains("Current count: 1", counter.Find("p[role='status']").TextContent);
    }

    [Fact]
    public void Counter_IncrementsMultipleTimesOnRepeatedClicks()
    {
        var counter = Render<Counter>();

        var button = counter.Find("button");
        button.Click();
        button.Click();
        button.Click();

        Assert.Contains("Current count: 3", counter.Find("p[role='status']").TextContent);
    }
}
