using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LunaUpdater.Github.Contracts
{
    [DataContract]
    public class GitHubRelease
    {
        [DataMember(Name = "url")]
        public string Url { get; set; }

        [DataMember(Name = "assets_url")]
        public string AssetsUrl { get; set; }

        [DataMember(Name = "upload_url")]
        public string UploadUrl { get; set; }

        [DataMember(Name = "html_url")]
        public string HtmlUrl { get; set; }

        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "tag_name")]
        public string TagName { get; set; }

        [DataMember(Name = "target_commitish")]
        public string TargetCommitish { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "draft")]
        public bool Draft { get; set; }

        [DataMember(Name = "author")]
        public GitHubAuthor Author { get; set; }

        [DataMember(Name = "prerelease")]
        public bool Prerelease { get; set; }

        [DataMember(Name = "created_at")]
        public DateTime CreatedAt { get; set; }

        [DataMember(Name = "published_at")]
        public DateTime PublishedAt { get; set; }

        [DataMember(Name = "assets")]
        public List<GitHubAsset> Assets { get; set; }

        [DataMember(Name = "tarball_url")]
        public string TarballUrl { get; set; }

        [DataMember(Name = "zipball_url")]
        public string ZipballUrl { get; set; }

        [DataMember(Name = "body")]
        public string Body { get; set; }
    }
}
