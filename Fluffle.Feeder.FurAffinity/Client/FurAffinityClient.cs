using Fluffle.Feeder.FurAffinity.Client.Models;
using HtmlAgilityPack;
using System.Globalization;
using System.Text.RegularExpressions;

// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract

namespace Fluffle.Feeder.FurAffinity.Client;

internal partial class FurAffinityClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public FurAffinityClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<FaSubmission?> GetSubmissionAsync(int submissionId)
    {
        var viewLocation = new Uri($"https://www.furaffinity.net/view/{submissionId}");
        var document = await GetAsync(viewLocation.AbsolutePath);

        var hasSubmission = document.GetElementbyId("submission_page") != null;
        if (!hasSubmission)
        {
            var knownErrors = new[]
            {
                "The submission you are trying to find is not in our database.",
                "The page you are trying to reach has been deactivated by the owner.",
                "The page you are trying to reach is currently pending deletion by a request from",
                "Access has been disabled to the account and contents of user"
            };

            if (knownErrors.Any(x => document.DocumentNode.InnerText.Contains(x, StringComparison.InvariantCultureIgnoreCase)))
                return null;

            throw new InvalidOperationException($"Submission with ID {submissionId} didn't contain a submission and also not a known error.");
        }

        ValidateLogin(document);
        
        // Extract number of views, comments and favorites, and the submission its rating
        var pageStatsNode = document.DocumentNode.SelectSingleNode("//div[contains(@class, 'submission-page-stats')]")!;
        var pageStats = pageStatsNode.ChildNodes
            .Where(x => x.NodeType == HtmlNodeType.Element)
            .Select(x =>
            {
                var nodes = x.ChildNodes.Where(y => y.NodeType == HtmlNodeType.Element).ToArray();
                
                if (nodes.Length != 2)
                    throw new InvalidOperationException("Submission page stats must have exactly 2 elements.");
                
                return new
                {
                    Key = nodes[1].InnerText.Trim(),
                    Value = nodes[0].InnerText.Trim()
                };
            })
            .ToDictionary(x => x.Key, x => x.Value);

        var ratingUnparsed = pageStats["Rating"];
        var rating = string.IsNullOrWhiteSpace(ratingUnparsed)
            ? FaSubmissionRating.General // Apparently some submissions have an empty rating and are publicly accessible, hence we're defaulting to general here
            : Enum.Parse<FaSubmissionRating>(ratingUnparsed, true);

        // Extract the submission its size
        var contentStatsNode = document.DocumentNode.SelectSingleNode("//div[contains(@class, 'submission-content-stats')]");
        var contentStatsNodeChildren = contentStatsNode.ChildNodes
                .Where(x => x.NodeType == HtmlNodeType.Element)
                .ToArray();
        
        if (contentStatsNodeChildren.Length != 2)
            throw new InvalidOperationException("Submission content stats must have exactly 2 elements.");
        
        var contentStats = contentStatsNodeChildren[0].ChildNodes.Where(x => x.NodeType == HtmlNodeType.Element)
            .Zip(contentStatsNodeChildren[1].ChildNodes.Where(x => x.NodeType == HtmlNodeType.Element))
            .Select(x => new
            {
                Key = x.First.InnerText.Trim(),
                Value = x.Second.InnerText.Trim()
            }).ToDictionary(x => x.Key, x => x.Value);
        
        // Not all submission have a size (Flash files for example)
        FaSize? size = null;
        if (contentStats.TryGetValue("Resolution", out var infoSize))
        {
            var widthAndHeight = infoSize.Split("x").Select(int.Parse).ToArray();
            size = new FaSize
            {
                Width = widthAndHeight[0],
                Height = widthAndHeight[1]
            };
        }
        
        // Extract the submission its download URL and time
        var buttonsNode = document.DocumentNode.SelectSingleNode("//div[contains(@class, 'submission-controls-upper')]")!;
        var downloadButton = buttonsNode.SelectSingleNode("./a[text()='Download']")!;
        var downloadUrl = "https:" + downloadButton.Attributes["href"].Value;
        var fileLocation = new Uri(downloadUrl);

        var thumbnailUnixTime = long.Parse(string.Concat(Path.GetFileName(Path.GetDirectoryName(downloadUrl))!.TakeWhile(char.IsDigit)));
        var thumbnailWhen = DateTimeOffset.FromUnixTimeSeconds(thumbnailUnixTime).ToUniversalTime();

        // Extract the owner
        var descriptionArtistNode = document.DocumentNode.SelectSingleNode("//div[contains(@class, 'submission-description-artist')]");
        var descriptionArtistDesktopNode = descriptionArtistNode.ChildNodes
            .Where(x => x.NodeType == HtmlNodeType.Element)
            .Skip(1)
            .Last();
        
        var ownerNode = descriptionArtistDesktopNode.SelectSingleNode(".//a[contains(@href, '/user/')]")!;
        var owner = new FaOwner
        {
            Id = OwnerIdRegex().Match(ownerNode.Attributes["href"].Value).Groups[1].Value,
            Name = ownerNode.InnerText.Trim()
        };
        
        var whenNode = descriptionArtistDesktopNode.SelectSingleNode(".//span[contains(@class, 'popup_date')]")!;
        var whenUnparsed = whenNode.Attributes["title"].Value;
        var when = DateTimeOffset.Parse(whenUnparsed, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);

        var submission = new FaSubmission
        {
            Id = submissionId,
            ViewLocation = viewLocation,
            Owner = owner,
            Rating = rating,
            Size = size,
            FileLocation = fileLocation,
            When = when,
            ThumbnailWhen = thumbnailWhen
        };

        return submission;
    }

    public async Task<int> GetNewestIdAsync()
    {
        var submissions = await GetNewestSubmissions();
        var newestId = submissions.Max(x => x.Id);

        return newestId;
    }

    public async Task<ICollection<FaGallerySubmission>> GetNewestSubmissions()
    {
        var document = await GetAsync("/");
        ValidateLogin(document);

        var node = document.GetElementbyId("gallery-frontpage-submissions");
        var recentSubmissions = node.ChildNodes
            .Where(x => x.Name == "figure")
            .Select(x =>
            {
                if (!x.Id.StartsWith("sid-"))
                    throw new InvalidOperationException("Recent submission starts with unexpected identifier.");

                var id = int.Parse(x.Id["sid-".Length..]);
                return new FaGallerySubmission
                {
                    Id = id
                };
            }).ToList();

        return recentSubmissions;
    }

    public static void ValidateLogin(HtmlDocument htmlDocument)
    {
        var navbar = htmlDocument.DocumentNode.SelectSingleNode("//ul[contains(@class, 'navhideonmobile')]")!;
        var loginControls = navbar.SelectSingleNode("./li[contains(@class, 'no-sub')]");

        if (loginControls == null)
            return;

        throw new InvalidOperationException("FA client does not seem to be authenticated.");
    }

    private async Task<HtmlDocument> GetAsync(string uri)
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(FurAffinityClient));
        await using var stream = await httpClient.GetStreamAsync(uri);

        var document = new HtmlDocument();
        document.Load(stream);

        return document;
    }

    [GeneratedRegex(@"\/user\/(.*?)\/?$")]
    private static partial Regex OwnerIdRegex();
}
