using System;
using System.Collections.Generic;

// ReSharper disable All

#pragma warning disable IDE1006
namespace LmpUpdater.Appveyor.Contracts
{
    public class Project
    {
        public int projectId { get; set; }
        public int accountId { get; set; }
        public string accountName { get; set; }
        public List<object> builds { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
        public string repositoryType { get; set; }
        public string repositoryScm { get; set; }
        public string repositoryName { get; set; }
        public string repositoryBranch { get; set; }
        public bool isPrivate { get; set; }
        public bool skipBranchesWithoutAppveyorYml { get; set; }
        public bool enableSecureVariablesInPullRequests { get; set; }
        public bool enableSecureVariablesInPullRequestsFromSameRepo { get; set; }
        public bool enableDeploymentInPullRequests { get; set; }
        public bool saveBuildCacheInPullRequests { get; set; }
        public bool rollingBuilds { get; set; }
        public bool rollingBuildsDoNotCancelRunningBuilds { get; set; }
        public bool alwaysBuildClosedPullRequests { get; set; }
        public string tags { get; set; }
        public NuGetFeed nuGetFeed { get; set; }
        public SecurityDescriptor securityDescriptor { get; set; }
        public DateTime created { get; set; }
        public DateTime updated { get; set; }
    }
}
