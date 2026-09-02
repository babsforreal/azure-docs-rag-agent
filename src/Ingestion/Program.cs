using System.Text.Json;
using HtmlAgilityPack;

var certs = new[]
{
    new CertSource("certification.azure-fundamentals", "AZ-900", "https://learn.microsoft.com/en-us/credentials/certifications/azure-fundamentals/"),
    new CertSource("certification.security-compliance-and-identity-fundamentals", "SC-900", "https://learn.microsoft.com/en-us/credentials/certifications/security-compliance-and-identity-fundamentals/"),
    new CertSource("certification.azure-administrator", "AZ-104", "https://learn.microsoft.com/en-us/credentials/certifications/azure-administrator/"),
    new CertSource("certification.azure-developer", "AZ-204", "https://learn.microsoft.com/en-us/credentials/certifications/azure-developer/"),
    new CertSource("certification.azure-solutions-architect", "AZ-305", "https://learn.microsoft.com/en-us/credentials/certifications/azure-solutions-architect/"),
    new CertSource("certification.devops-engineer", "AZ-400", "https://learn.microsoft.com/en-us/credentials/certifications/devops-engineer/"),
};

using var http = new HttpClient();
http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; AzureDocsRagAgent/1.0)");
var pageTexts = new Dictionary<string, string>();
foreach (var cert in certs)
{
    var html = await http.GetStringAsync(cert.Url);
    var doc = new HtmlDocument();
    doc.LoadHtml(html);

    var text = doc.DocumentNode.SelectSingleNode("//main")?.InnerText ?? "";
    pageTexts[cert.Uid] = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();
}

JsonDocument catalog;
try
{
    var stream = await http.GetStreamAsync("https://learn.microsoft.com/api/catalog/");
    catalog = await JsonDocument.ParseAsync(stream);
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Catalog API indisponible ({ex.Message}), on continue sans les learning paths");
    catalog = JsonDocument.Parse("{\"certifications\":[],\"learningPaths\":[]}");
}

var pathsByUid = catalog.RootElement.GetProperty("learningPaths")
    .EnumerateArray()
    .ToDictionary(p => p.GetProperty("uid").GetString()!, p => p);

var certsByUid = catalog.RootElement.GetProperty("certifications")
    .EnumerateArray()
    .ToDictionary(c => c.GetProperty("uid").GetString()!, c => c);

var contents = new List<CertContent>();

foreach (var cert in certs)
{
    var text = pageTexts[cert.Uid];

    if (certsByUid.TryGetValue(cert.Uid, out var catalogCert))
    {
        var pathSummaries = catalogCert.GetProperty("study_guide")
            .EnumerateArray()
            .Select(sg => sg.GetProperty("uid").GetString()!)
            .Where(pathsByUid.ContainsKey)
            .Select(uid => pathsByUid[uid])
            .Select(p => $"{p.GetProperty("title").GetString()}: {p.GetProperty("summary").GetString()}");

        text += " " + string.Join(" ", pathSummaries);
    }

    contents.Add(new CertContent(cert.Title, text));
}

foreach (var c in contents)
    Console.WriteLine($"{c.Title}: {c.Text.Length} chars");

record CertSource(string Uid, string Title, string Url);
record CertContent(string Title, string Text);