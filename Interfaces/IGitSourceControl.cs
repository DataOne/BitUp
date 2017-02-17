using System.Collections.Generic;

namespace DataOne.BitUp
{
    interface IGitSourceControl
    {
        /// <summary>
        /// Finds every repository of a team
        /// </summary>
        /// <param name="teamName">The name of the team</param>
        /// <returns>A list of repositories with all values set</returns>
        IEnumerable<BitbucketRepository> GetTeamRepositories(string teamName);

        /// <summary>
        /// Clones a repository
        /// </summary>
        /// <param name="repository">The clone url of the repository</param>
        /// <param name="path">The path where the repository is saved</param>
        void CloneRepository(string cloneUrl, string path);

        /// <summary>
        /// Clones a wiki
        /// </summary>
        /// <param name="repository">The clone url of the wiki</param>
        /// <param name="path">The path where the wiki is saved</param>
        void CloneWiki(string cloneUrl, string path);

        /// <summary>
        /// Exports the issues of a repository and saves them
        /// </summary>
        /// <param name="repository">The repository to save the issues</param>
        /// <param name="path">The path where the issues are saved</param>
        void SaveIssues(BitbucketRepository repo, string path);
    }
}
