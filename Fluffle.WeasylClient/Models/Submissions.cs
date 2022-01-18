using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Noppes.Fluffle.Weasyl.Models
{
    public abstract class MinimalSubmission<T>
    {
        [JsonProperty("submitid")]
        public int SubmitId { get; set; }

        public string Title { get; set; }

        public SubmissionRating Rating { get; set; }

        public ICollection<T> Tags { get; set; }

        public SubmissionType Type { get; set; }

        public SubmissionSubtype Subtype { get; set; }

        public string Owner { get; set; }

        public string OwnerLogin { get; set; }

        public SubmissionMediaKeys Media { get; set; }

        public Uri Link { get; set; }
    }

    public class FontPageSubmission : MinimalSubmission<int>
    {
    }

    public class Submission : MinimalSubmission<string>
    {
        public SubmissionMediaKeys OwnerMedia { get; set; }

        [JsonProperty("embedlink")]
        public Uri EmbedLink { get; set; }

        public string Description { get; set; }

        [JsonProperty("folderid")]
        public int? FolderId { get; set; }

        public string FolderName { get; set; }

        [JsonProperty("views")]
        public int ViewsCount { get; set; }

        [JsonProperty("favorites")]
        public int FavoritesCount { get; set; }

        [JsonProperty("comments")]
        public int CommentCount { get; set; }

        [JsonProperty("favorited")]
        public bool IsFavorited { get; set; }

        [JsonProperty("friends_only")]
        public bool IsFriendsOnly { get; set; }
    }

    public class SubmissionMediaKeys
    {
        [JsonProperty("thumbnail-generated-webp")]
        public IList<SubmissionMedia> ThumbnailGeneratedWebP { get; set; }

        [JsonProperty("thumbnail-generated")]
        public IList<SubmissionMedia> ThumbnailGenerated { get; set; }

        public IList<SubmissionMedia> Thumbnail { get; set; }

        public IList<SubmissionMedia> Cover { get; set; }

        public IList<SubmissionMedia> Submission { get; set; }

        public SubmissionMediaKeys Links { get; set; }
    }

    public class SubmissionMedia
    {
        /// <summary>
        /// This can be null to indicate the url is already unambiguous.
        /// </summary>
        [JsonProperty("mediaid")]
        public int? Id { get; set; }

        public Uri Url { get; set; }
    }

    public enum SubmissionRating
    {
        General,
        Moderate,
        Mature,
        Explicit
    }

    public enum SubmissionType
    {
        Submission,
        Character
    }

    public enum SubmissionSubtype
    {
        Visual,
        Literary,
        Multimedia
    }
}
