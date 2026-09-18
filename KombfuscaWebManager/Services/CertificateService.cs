using Microsoft.Playwright;

namespace KombfuscaWebManager.Services;

public class CertificateService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CertificateService> _logger;

    public CertificateService(IConfiguration configuration, ILogger<CertificateService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<byte[]> GenerateAsync(string html)
    {
        using var playwright = await Playwright.CreateAsync();
        var executablePath = ResolveBrowserExecutable(playwright);

        if (executablePath is not null)
            _logger.LogInformation("Gerando certificado com o navegador em {ExecutablePath}", executablePath);

        await using var browser = await playwright.Chromium.LaunchAsync(
            new BrowserTypeLaunchOptions
            {
                Headless = true,
                ExecutablePath = executablePath
            });

        var page = await browser.NewPageAsync();
        await page.SetContentAsync(html, new PageSetContentOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.EmulateMediaAsync(new PageEmulateMediaOptions { Media = Media.Print });

        return await page.PdfAsync(new PagePdfOptions
        {
            Format = "A4",
            Landscape = true,
            PrintBackground = true,
            Margin = new Margin { Top = "0", Bottom = "0", Left = "0", Right = "0" }
        });
    }

    private string? ResolveBrowserExecutable(IPlaywright playwright)
    {
        var configuredPath = _configuration["Playwright:ChromiumExecutablePath"];
        var candidates = new[]
        {
            configuredPath,
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Google", "Chrome", "Application", "chrome.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft", "Edge", "Application", "msedge.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft", "Edge", "Application", "msedge.exe"),
            playwright.Chromium.ExecutablePath,
            "/usr/bin/google-chrome",
            "/usr/bin/chromium",
            "/usr/bin/chromium-browser"
        };

        return candidates
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .FirstOrDefault(File.Exists);
    }
}
