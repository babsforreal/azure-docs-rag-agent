using HtmlAgilityPack;

var sources = new (string Cert, string Url)[]
{
    ("AZ-900", "https://learn.microsoft.com/en-us/credentials/certifications/azure-fundamentals/"),
    ("SC-900", "https://learn.microsoft.com/en-us/credentials/certifications/security-compliance-and-identity-fundamentals/"),
    ("AZ-104", "https://learn.microsoft.com/en-us/credentials/certifications/azure-administrator/"),
    ("AZ-204", "https://learn.microsoft.com/en-us/credentials/certifications/azure-developer/"),
    ("AZ-305", "https://learn.microsoft.com/en-us/credentials/certifications/azure-solutions-architect/"),
    ("AZ-400", "https://learn.microsoft.com/en-us/credentials/certifications/devops-engineer/"),
};

using var http = new HttpClient();
http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; AzureDocsRagAgent/1.0)");

foreach (var (cert, url) in sources)
{
    var html = await http.GetStringAsync(url);

    var doc = new HtmlDocument();
    doc.LoadHtml(html);

    // le contenu réel de la page vit dans <main>, le reste c'est nav/footer
    var text = doc.DocumentNode.SelectSingleNode("//main")?.InnerText ?? "";
    text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();

    Console.WriteLine($"{cert}: {text.Length} chars");
    // TODO: chunking + embeddings + push vers AI Search
}