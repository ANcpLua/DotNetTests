using System.Text.RegularExpressions;
using TUnit.Playwright;

namespace PlaywrightTests;

public class Tests : PageTest
{
    [Test]
    public async Task Test()
    {
        await Page.GotoAsync("https://playwright.dev");

        await Expect(Page).ToHaveTitleAsync(new Regex("Playwright"));

        var getStarted = Page.Locator("text=Get Started");

        await Expect(getStarted).ToHaveAttributeAsync("href", "/docs/intro");

        await getStarted.ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex(".*intro"));
    }
}
