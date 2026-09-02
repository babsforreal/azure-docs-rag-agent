using System.Text.Json;

var targetCerts = new[]
{
    "certification.azure-fundamentals",
    "certification.security-compliance-and-identity-fundamentals",
    "certification.azure-administrator",
    "certification.azure-developer",
    "certification.azure-solutions-architect",
    "certification.devops-engineer",
};

using var http = new HttpClient();
var stream = await http.GetStreamAsync("https://learn.microsoft.com/api/catalog/");
using var doc = await JsonDocument.ParseAsync(stream);

var certifs = doc.RootElement.GetProperty("certifications")
    .EnumerateArray()
    .Where(c => targetCerts.Contains(c.GetProperty("uid").GetString()))
    .ToList();

// index par uid les learning paths, pour les récupérer vite quand une certification en référence
var pathsByUid = doc.RootElement.GetProperty("learningPaths")
    .EnumerateArray()
    .ToDictionary(p => p.GetProperty("uid").GetString()!, p => p);

foreach (var certif in certifs)
{
    var title = certif.GetProperty("title").GetString();
    var pathUids = certif.GetProperty("study_guide")
        .EnumerateArray()
        .Select(sg => sg.GetProperty("uid").GetString()!)
        .ToList();

    Console.WriteLine($"{title}: {pathUids.Count} learning paths");

    foreach (var uid in pathUids)
    {
        if (!pathsByUid.TryGetValue(uid, out var path)) continue;
        var pathTitle = path.GetProperty("title").GetString();
        var summary = path.GetProperty("summary").GetString();
        Console.WriteLine($"  - {pathTitle}: {summary}");
    }
}