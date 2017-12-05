using Newtonsoft.Json;

namespace Updater.Contracts
{
    public class GitHubUploader
    {
        [JsonProperty(PropertyName = "login")]
        public string Login { get; set; }

        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "avatar_url")]
        public string AvatarUrl { get; set; }

        [JsonProperty(PropertyName = "gravatar_id")]
        public string GravatarId { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "html_url")]
        public string HtmlUrl { get; set; }

        [JsonProperty(PropertyName = "followers_url")]
        public string FollowersUrl { get; set; }

        [JsonProperty(PropertyName = "following_url")]
        public string FollowingUrl { get; set; }

        [JsonProperty(PropertyName = "gists_url")]
        public string GistsUrl { get; set; }

        [JsonProperty(PropertyName = "starred_url")]
        public string StarredUrl { get; set; }

        [JsonProperty(PropertyName = "subscriptions_url")]
        public string SubscriptionsUrl { get; set; }

        [JsonProperty(PropertyName = "organizations_url")]
        public string OrganizationsUrl { get; set; }

        [JsonProperty(PropertyName = "repos_url")]
        public string ReposUrl { get; set; }

        [JsonProperty(PropertyName = "events_url")]
        public string EventsUrl { get; set; }

        [JsonProperty(PropertyName = "received_events_url")]
        public string ReceivedEventsUrl { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "site_admin")]
        public bool SiteAdmin { get; set; }
    }
}
