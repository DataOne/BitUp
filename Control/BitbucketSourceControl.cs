using DataOne.BitUp.Properties;
using LibGit2Sharp;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DataOne.BitUp
{
    public class BitbucketSourceControl : IGitSourceControl
    {
        private string _newApiUrl = Resources.BitbucketRepositoryNewApi;
        private string _oldApiUrl = Resources.BitbucketRepositoryOldApi;
        private string _issueExportUri = Resources.BitbucketIssueExportUri;
        private string _issueExportDownloadUri = Resources.BitbucketIssueExportDownloadUri;
        private Request _request;
        private string _teamName, _apiKey;
        private const int MaxPagelen = 100;

        public BitbucketSourceControl(string teamName, string apiKey)
        {
            _teamName = teamName;
            _apiKey = apiKey;
            _request = new Request(teamName, apiKey);
        }

        public IEnumerable<BitbucketRepository> GetTeamRepositories(string teamName)
        {
            List<BitbucketRepository> repositories = new List<BitbucketRepository>();

            string apiUrl = string.Concat(_newApiUrl, teamName);
            JToken nextPageUrl = string.Concat(apiUrl, "?pagelen=", MaxPagelen);

            try
            {
                while (nextPageUrl != null)
                {
                    JObject response = _request.GetResponse(nextPageUrl.ToString());
                    JToken values = response["values"];

                    foreach (JToken value in values ?? Enumerable.Empty<JToken>())
                    {
                        BitbucketRepository repository = new BitbucketRepository();
                        repository.Name = value["name"].ToString().ToLower();
                        repository.DirectoryName = repository.Name.Replace(" ", "-");
                        repository.TeamName = teamName;
                        repository.CloneUrl = value["links"]["clone"].First["href"].ToString();
                        repository.HasWiki = (bool)value["has_wiki"];
                        repository.HasIssues = (bool)value["has_issues"];
                        repository.UpdatedOn = (DateTime)value["updated_on"];

                        if (repository.HasIssues)
                        {
                            try
                            {
                                repository.LastIssueChange = GetLastIssueChange(repository);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(repository.Name + " - Error: " + e.Message);
                                repository.LastIssueChange = DateTime.MinValue;
                            }
                        }
                        else
                        {
                            repository.LastIssueChange = DateTime.MinValue;
                        }
                        repositories.Add(repository);
                        Console.WriteLine(repository.Name);
                    }
                    nextPageUrl = response["next"];
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(teamName + " - Error: " + e.Message);
            }
            return repositories;
        }

        private IEnumerable<JObject> GetIssues(BitbucketRepository repository)
        {
            var issues = new List<JObject>();
            JToken nextPageUrl = string.Concat(_newApiUrl, repository.TeamName, "/", repository.DirectoryName, "/issues?pagelen=", MaxPagelen);

            while (nextPageUrl != null)
            {
                JObject response = _request.GetResponse(nextPageUrl.ToString());
                issues.Add(response);

                nextPageUrl = response["next"];
            }
            return issues;
        }

        private DateTime GetLastIssueChange(BitbucketRepository repository)
        {
            DateTime latestChange = DateTime.MinValue;

            IEnumerable<JObject> allIssues = GetIssues(repository);

            foreach (var issues in allIssues ?? Enumerable.Empty<JObject>())
            {
                foreach (var issue in issues["values"])
                {
                    DateTime updatedOn = (DateTime)issue["updated_on"];
                    if (latestChange < updatedOn)
                    {
                        latestChange = updatedOn;
                    }
                }
            }
            return latestChange;
        }

        public void CloneRepository(string cloneUrl, string path)
        {
            var userAndPass = new UsernamePasswordCredentials();
            userAndPass.Username = _teamName;
            userAndPass.Password = _apiKey;

            var cloneOptions = new CloneOptions();
            cloneOptions.IsBare = true;
            cloneOptions.CredentialsProvider = (_url, _user, _cred) => userAndPass;
            Repository.Clone(cloneUrl, path, cloneOptions);
        }

        public void CloneWiki(string cloneUrl, string path)
        {
            CloneRepository(cloneUrl + "/wiki", path);
        }

        public void SaveIssues(BitbucketRepository repository, string path)
        {
            var exportUrl = string.Concat(_oldApiUrl, repository.TeamName, "/", repository.DirectoryName, _issueExportUri);
            // to prepare export
            _request.GetResponseFileStream(exportUrl);

            var exportDownloadUrl = string.Concat(_oldApiUrl, repository.TeamName, "/", repository.DirectoryName, _issueExportDownloadUri);

            Stream responseStream = _request.GetResponseFileStream(exportDownloadUrl);

            FileStream fileStream = File.Create(path);

            int bufferSize = 1024;
            var buffer = new byte[bufferSize];
            int bytesRead = 0;

            try
            {
                while ((bytesRead = responseStream.Read(buffer, 0, bufferSize)) != 0)
                {
                    fileStream.Write(buffer, 0, bytesRead);
                }
            }
            finally
            {
                fileStream.Dispose();
            }
        }

    }
}
