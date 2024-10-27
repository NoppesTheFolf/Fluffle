using Flurl.Http;
using HtmlAgilityPack;
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

    public async Task<FaOnlineStats> GetRegisteredUsersOnlineAsync()
    {
        var response = await Request().GetHtmlExplicitlyAsync();

        return ExtractOnlineStats(response);
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

        if (info.TryGetValue("Category", out var categoryString))
        {
            submission.Category = SubmissionCategoryHelper.CategoryFromString(categoryString);
            submission.Type = SubmissionTypeHelper.TypeFromString(categoryString);
        }

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

        var thumbnailWhen = long.Parse(string.Concat(Path.GetFileName(Path.GetDirectoryName(downloadUrl)).TakeWhile(char.IsDigit)));
        submission.ThumbnailWhen = DateTimeOffset.FromUnixTimeSeconds(thumbnailWhen);

        var match = Regex.Match(downloadUrl, "(?<=\\/)[0-9]+?(?=\\.)");
        if (!match.Success)
            match = Regex.Match(downloadUrl, "(?<=_)[0-9]+?(?=_)");

        var when = match.Success ? long.Parse(match.Value) : thumbnailWhen;
        submission.When = DateTimeOffset.FromUnixTimeSeconds(when);

        // Extract title
        var titleNode = submissionContent.SelectSingleNode(".//div[contains(@class, 'submission-title')]");
        submission.Title = titleNode.InnerText.Trim();

        // Extract description
        var descriptionNode = submissionContent.SelectSingleNode(".//div[contains(@class, 'submission-description')]");
        submission.Description = descriptionNode.InnerText.Replace("\r\n", "\n").Trim();

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

    public Task<FaResult<FaGallery>> GetScrapsAsync(string artistId, int page = 1)
    {
        var url = $"scraps/{artistId}";

        return GetGalleryAsync(artistId, url, page);
    }

    public Task<FaResult<FaGallery>> GetGalleryAsync(string artistId, int page = 1, FaFolder folder = null)
    {
        var url = $"gallery/{artistId}";

        if (folder != null)
            url += $"/folder/{folder.Id}/{folder.NormalizedTitle}";

        return GetGalleryAsync(artistId, url, page, folder);
    }

    private static bool CheckIfDisabled(HtmlNode node)
    {
        var noticeMessage = node.SelectSingleNode("//section[contains(@class, 'notice-message')]");

        return noticeMessage != null && noticeMessage.InnerText.Contains("has voluntarily disabled access to their account");
    }

    private async Task<FaResult<FaGallery>> GetGalleryAsync(string artistId, string url, int page, FaFolder folder = null)
    {
        url += $"/{page}";
        var response = await Request(url).GetHtmlExplicitlyAsync();
        ValidateLogin(response);

        if (CheckIfDisabled(response.DocumentNode))
            return null;

        var gallery = new FaGallery
        {
            ArtistId = artistId,
            Page = page
        };
        var root = response.GetElementbyId("columnpage");

        // Extract folders
        var foldersNode = root.SelectSingleNode(".//div[contains(@class, 'user-folders')]");
        if (foldersNode == null)
        {
            gallery.Folders = new List<FaFolder>();
        }
        else
        {
            var folderNodes = foldersNode.SelectNodes(".//a[contains(@href, '/folder/')]");

            if (folderNodes == null && folder == null)
                throw new InvalidOperationException("There were folders, but we couldn't scrape them for whatever reason. This likely means Fur Affinity has changed their HTML-markup.");

            var folders = folder == null
                ? new List<FaFolder>()
                : new List<FaFolder> { folder };

            if (folderNodes != null)
            {
                var scrapedFolders = folderNodes.Select(n =>
                {
                    var match = Regex.Match(n.Attributes["href"].Value, ".*\\/([0-9]*)\\/(.*)");

                    return new FaFolder
                    {
                        Id = int.Parse(match.Groups[1].Value),
                        NormalizedTitle = match.Groups[2].Value,
                        Title = n.InnerText
                    };
                });
                folders.AddRange(scrapedFolders);
            }

            gallery.Folders = folders;
        }

        // Extract navigation 
        var submissionsListNode = root.SelectSingleNode(".//div[contains(@class, 'submission-list')]");
        var bottomNavigationNode = submissionsListNode.ChildNodes.Last(n => n.Name == "div");
        var nextPageContainerNode = bottomNavigationNode.ChildNodes.Last(n => n.NodeType != HtmlNodeType.Text);
        gallery.HasNextPage = nextPageContainerNode.ChildNodes.All(n => n.NodeType != HtmlNodeType.Comment);

        // Extract submissions
        var galleryNode = response.GetElementbyId("gallery-gallery");
        if (galleryNode.InnerText.Contains("There are no submissions"))
        {
            gallery.SubmissionIds = new List<int>();
        }
        else
        {
            gallery.SubmissionIds = galleryNode.ChildNodes
                .Where(n => n.Name == "figure")
                .Select(n => int.Parse(n.Id.Replace("sid-", string.Empty)))
                .ToList();
        }

        return new FaResult<FaGallery>
        {
            Stats = ExtractOnlineStats(response),
            Result = gallery
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
                var artistNode = cn.SelectSingleNode("./figcaption").ChildNodes[^1].SelectSingleNode("./a");
                var artistUrl = artistNode.Attributes["href"].Value;
                var artistMatch = Regex.Match(artistUrl, "(?<=\\/user\\/)(.*)(?=\\/)");

                return new FaGallerySubmission
                {
                    Id = id,
                    ArtistId = artistMatch.Value
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
