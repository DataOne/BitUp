using System;

namespace DataOne.BitUp
{
    public class BitbucketRepository
    {
        public string Name { get; set; }
        public string CloneUrl { get; set; }
        public bool HasWiki { get; set; }
        public bool HasIssues { get; set; }
        public string TeamName { get; set; }
        public DateTime UpdatedOn { get; set; }
        public DateTime LastIssueChange { get; set; }
        public string DirectoryName { get; set; }
    }
}
