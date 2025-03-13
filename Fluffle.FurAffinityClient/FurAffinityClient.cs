using Flurl.Http;
using HtmlAgilityPack;
using Noppes.Fluffle.FurAffinity.Models;
using Noppes.Fluffle.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Noppes.Fluffle.FurAffinity;

public class FurAffinityClient : ApiClient
{
    public const int BotThreshold = 10_000;

    private readonly string _a, _b;
    private readonly string _userAgent;

    public FurAffinityClient(string baseUrl, string userAgent, string a, string b) : base(baseUrl)
    {
        _userAgent = userAgent;
        _a = a;
        _b = b;
    }

    public Task<Stream> GetStreamAsync(string url) => Request(url).GetStreamAsync();

    public async Task<FaResult<FaSubmission>> GetSubmissionAsync(int submissionId)
    {
        var submission = new FaSubmission
        {
            Id = submissionId,
            ViewLocation = new Uri(new Uri(FlurlClient.BaseUrl, UriKind.Absolute), $"view/{submissionId}")
        };

        var response = await Request(submission.ViewLocation.AbsolutePath)
            .GetHtmlExplicitlyAsync();

        var containsSubmission = response.GetElementbyId("submission_page") != null;
        if (!containsSubmission)
        {
            // FA doesn't return a 404 response
            if (response.DocumentNode.InnerText.Contains("The submission you are trying to find is not in our database."))
                return null;

            // Submission removed by owner
            if (response.DocumentNode.InnerText.Contains("The page you are trying to reach has been deactivated by the owner."))
                return null;

            // Submission marked for deletion by owner, the administration, or possibly another entity
            if (response.DocumentNode.InnerText.Contains("The page you are trying to reach is currently pending deletion by a request from"))
                return null;

            throw new InvalidOperationException($"Submission with ID {submissionId} didn't contain a submission and also not a known error.");
        }

        ValidateLogin(response);

        var submissionContent = response.DocumentNode.SelectSingleNode("//div[contains(@class, 'submission-content')]");
        var sidebar = response.DocumentNode.SelectSingleNode("//div[contains(@class, 'submission-sidebar')]");

        // Extract number of views, comments and favorites, and the submission its rating
        var statsNode = sidebar.SelectSingleNode("./section[contains(@class, 'stats-container')]");
        var stats = statsNode.ChildNodes
            .Where(n => n.NodeType != HtmlNodeType.Text)
            .Select(n => n.ChildNodes.First(cn => cn.Name == "span"))
            .Select(n => n.InnerText)
            .ToArray();

        submission.Stats = new FaSubmissionStats
        {
            Views = int.Parse(stats[0]),
            Comments = int.Parse(stats[1]),
            Favorites = int.Parse(stats[2])
        };

        var ratingUnparsed = stats[3];
        submission.Rating = string.IsNullOrWhiteSpace(ratingUnparsed)
            ? FaSubmissionRating.General // Apparently some submissions have an empty rating and are publicly accessible, hence we're defaulting to general here
            : Enum.Parse<FaSubmissionRating>(ratingUnparsed, true);

        // Extract the submission its (sub)category, species, gender and size
        var infoNode = sidebar.SelectSingleNode("./section[contains(@class, 'info')]");
        var info = infoNode.ChildNodes
            .Where(n => n.NodeType != HtmlNodeType.Text)
            .Select(n => new
            {
                Category = n.FirstChild.InnerText.Trim(),
                Value = n.ChildNodes.Skip(1).First(cn => cn.NodeType != HtmlNodeType.Text).InnerText.Trim()
            }).ToDictionary(n => n.Category, n => n.Value);

        submission.Species = info.TryGetValue("Species", out var species) ? species : null;
        submission.Gender = info.TryGetValue("Gender", out var gender) ? gender : null;

        // Not all submission have a size (Flash files for example)
        if (info.TryGetValue("Size", out var infoSize))
        {
            var size = infoSize.Split("x").Select(int.Parse).ToArray();
            submission.Size = new FaSize(size[0], size[1]);
        }

        // Extract the submission its download URL and time
        var buttonsNode = sidebar.SelectSingleNode("./section[contains(@class, 'buttons')]");
        var downloadButton = buttonsNode.SelectSingleNode("./div[contains(@class, 'download')]");
        var downloadUrl = "https:" + downloadButton.FirstChild.Attributes["href"].Value;
        submission.FileLocation = new Uri(downloadUrl);

        var thumbnailWhen = long.Parse(string.Concat(Path.GetFileName(Path.GetDirectoryName(downloadUrl))!.TakeWhile(char.IsDigit)));
        submission.ThumbnailWhen = DateTimeOffset.FromUnixTimeSeconds(thumbnailWhen);

        var match = Regex.Match(downloadUrl, "(?<=\\/)[0-9]+?(?=\\.)");
        if (!match.Success)
            match = Regex.Match(downloadUrl, "(?<=_)[0-9]+?(?=_)");

        var when = match.Success ? long.Parse(match.Value) : thumbnailWhen;
        submission.When = DateTimeOffset.FromUnixTimeSeconds(when);

        // Extract title
        var titleNode = submissionContent.SelectSingleNode(".//div[contains(@class, 'submission-title')]");
        submission.Title = titleNode.InnerText.Trim();

        // Extract the owner
        var headerInfoNode = submissionContent.SelectSingleNode(".//div[contains(@class, 'submission-id-sub-container')]");
        var ownerNode = headerInfoNode.SelectSingleNode(".//a[contains(@href, '/user/')]");
        submission.Owner = new FaArtist
        {
            Id = Regex.Match(ownerNode.Attributes["href"].Value, "\\/user\\/(.*?)\\/?$").Groups[1].Value,
            Name = ownerNode.InnerText.Trim()
        };

        return new FaResult<FaSubmission>
        {
            Result = submission,
            Stats = ExtractOnlineStats(response)
        };
    }

    public async Task<ICollection<FaGallerySubmission>> GetRecentSubmissions()
    {
        var response = await Request().GetHtmlExplicitlyAsync();
        ValidateLogin(response);

        var node = response.GetElementbyId("gallery-frontpage-submissions");
        var recentSubmissions = node.ChildNodes
            .Where(cn => cn.Name == "figure")
            .Select(cn =>
            {
                var id = int.Parse(cn.Id["sid-".Length..]);
                return new FaGallerySubmission
                {
                    Id = id
                };
            }).ToList();

        return recentSubmissions;
    }

    public static void ValidateLogin(HtmlDocument htmlDocument)
    {
        var navbar = htmlDocument.DocumentNode.SelectSingleNode("//ul[contains(@class, 'navhideonmobile')]");
        var loginControls = navbar.SelectSingleNode("./li[contains(@class, 'no-sub')]");

        if (loginControls == null)
            return;

        throw new InvalidOperationException();
    }

    private static FaOnlineStats ExtractOnlineStats(HtmlDocument document)
    {
        var onlineNode = document.DocumentNode.SelectSingleNode("//div[contains(@class, 'online-stats')]");
        var onlineStats = onlineNode.ChildNodes
            .Select(n => HttpUtility.HtmlDecode(n.InnerText))
            .Where(n => n != null)
            .Select(s => string.Concat(s.Where(char.IsDigit)))
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();

        return new FaOnlineStats
        {
            Online = int.Parse(onlineStats[0]),
            Guests = int.Parse(onlineStats[1]),
            Registered = int.Parse(onlineStats[2]),
            Other = int.Parse(onlineStats[3])
        };
    }

    public override IFlurlRequest Request(params object[] urlSegments)
    {
        return base.Request(urlSegments)
            .WithCookies(new
            {
                a = _a,
                b = _b
            })
            .WithHeader("User-Agent", _userAgent);
    }
}
