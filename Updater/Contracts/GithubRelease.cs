using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Updater.Contracts
{
    public class GitHubRelease
    {
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "assets_url")]
        public string AssetsUrl { get; set; }

        [JsonProperty(PropertyName = "upload_url")]
        public string UploadUrl { get; set; }

        [JsonProperty(PropertyName = "html_url")]
        public string HtmlUrl { get; set; }

        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "tag_name")]
        public string TagName { get; set; }

        [JsonProperty(PropertyName = "target_commitish")]
        public string TargetCommitish { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "draft")]
        public bool Draft { get; set; }

        [JsonProperty(PropertyName = "author")]
        public GitHubAuthor Author { get; set; }

        [JsonProperty(PropertyName = "prerelease")]
        public bool Prerelease { get; set; }

        [JsonProperty(PropertyName = "created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty(PropertyName = "published_at")]
        public DateTime PublishedAt { get; set; }

        [JsonProperty(PropertyName = "assets")]
        public List<GitHubAsset> Assets { get; set; }

        [JsonProperty(PropertyName = "tarball_url")]
        public string TarballUrl { get; set; }

        [JsonProperty(PropertyName = "zipball_url")]
        public string ZipballUrl { get; set; }

        [JsonProperty(PropertyName = "body")]
        public string Body { get; set; }
    }
}
