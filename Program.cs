using System;
using System.IO;
using System.Text;
using DataOne.BitUp.Properties;
using System.Collections.Generic;
using System.IO.Compression;
using DataOne.BitUp.Control;

namespace DataOne.BitUp
{
    public class Program
    {
        private static StringBuilder _log = new StringBuilder();

        public static void Main()
        {
            string sizeFormat = "{0:#,0.00} MB";
            long totalSize = 0;
            DateTime startTime = DateTime.Now;
            var logText = new List<string>();

            IStorage storage;
            if (Settings.Default.StoreInAzure)
            {
                storage = new AzureStorage();
            }
            else
            {
                storage = new LocalStorage();
            }
            var bitbucketTeams = Settings.Default.BitbucketTeams;
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            DateTime lastBackup = storage.IsEmpty() ? DateTime.MinValue : DateTime.Now.AddDays(-1.0);

            string workingDirectory = string.Concat(Directory.GetCurrentDirectory(), @"\DataOne.BitUp.TempRepositoryBackups");
            ForceDeleteDirectory(workingDirectory);
            Directory.CreateDirectory(workingDirectory);

            AppendLog("Last Backup: " + lastBackup);
            AppendLog(string.Empty);

            foreach (string bitbucketTeam in bitbucketTeams)
            {
                string[] nameAndKey = bitbucketTeam.Split(';');
                string teamName;
                IGitSourceControl sourceControl;
                var allRepositories = new List<BitbucketRepository>();

                if (nameAndKey.Length == 2)
                {
                    teamName = nameAndKey[0];
                    var key = nameAndKey[1];

                    sourceControl = new BitbucketSourceControl(teamName, key);
                }
                else if (nameAndKey.Length == 3)
                {
                    teamName = nameAndKey[0];
                    var serviceAccountUsername = nameAndKey[1];
                    var serviceAccountPassword = DecryptPassword(nameAndKey[2]);

                    sourceControl = new BitbucketSourceControl(teamName, serviceAccountUsername, serviceAccountPassword);
                }
                else
                {
                    AppendLog("Team configuration is falsy.");
                    return;
                }

                try
                {
                    AppendLog("---Repositories of " + teamName + ":");
                    var teamRepositories = sourceControl.GetTeamRepositories(teamName);
                    AppendLog("------------\n");
                    allRepositories.AddRange(teamRepositories);
                }
                catch
                {
                    AppendLog("Wrong name or API-Key: " + bitbucketTeam);
                }
                foreach (BitbucketRepository repository in allRepositories)
                {
                    ErrorType error = ErrorType.None;
                    AppendLog(string.Format("{0}\nUpdated on: {1}\nLast issue change: {2}", repository.CloneUrl, repository.UpdatedOn, repository.LastIssueChange));

                    long zipFileSize = 0;
                    string formattedSize = "0.0 B";
                    string repositoryDirectory = string.Concat(workingDirectory, @"\", teamName, "_", repository.DirectoryName);
                    string zipFile = repositoryDirectory + ".zip";

                    try
                    {
                        bool hasRepoChanged = lastBackup < repository.UpdatedOn;
                        bool haveIssuesChanged = repository.HasIssues && (lastBackup < repository.LastIssueChange);

                        if (hasRepoChanged || haveIssuesChanged)
                        {
                            error = DownloadRepository(repository, sourceControl, repositoryDirectory);
                        }
                        else
                        {
                            AppendLog(string.Concat(repository.Name, Messages.RepositoryNotChanged, lastBackup));
                            continue;
                        }
                        AppendLog(Messages.Zipping);
                        ZipFile.CreateFromDirectory(repositoryDirectory, zipFile);

                        try
                        {
                            zipFileSize = new FileInfo(zipFile).Length;
                            formattedSize = string.Format(System.Globalization.CultureInfo.GetCultureInfo("de-DE"), sizeFormat, (double)zipFileSize / 1024 / 1024);
                        }
                        catch
                        {
                            zipFileSize = 0;
                            formattedSize = "0,0 MB";
                        }
                        totalSize += zipFileSize;

                        AppendLog(string.Concat(Messages.StoreFile, Path.GetFileName(zipFile), " in ", storage.GetName()));
                        try
                        {
                            storage.SaveFile(zipFile);
                        }
                        catch (Exception e)
                        {
                            error = ErrorType.Fatal;
                            AppendLog(Messages.StoringError + e.Message);
                        }

                    }
                    catch (Exception e)
                    {
                        error = ErrorType.Fatal;
                        AppendLog(Messages.CloningError + e.Message);
                    }
                    finally
                    {
                        AppendLog("Deleting zip: " + Path.GetFileName(zipFile));
                        ForceDeleteFile(zipFile);
                        AppendLog("Deleting directory " + repositoryDirectory);
                        ForceDeleteDirectory(repositoryDirectory);
                        AppendLog("################\n");
                    }
                    string logColor;

                    switch (error)
                    {
                        case ErrorType.Fatal:
                            logColor = "red";
                            break;
                        case ErrorType.Error:
                            logColor = "orange";
                            break;
                        default:
                            logColor = "green";
                            break;
                    }
                    logText.Add("<p style=\"color:" + logColor + "\">" + repository.TeamName + " - " + repository.Name + " - " + formattedSize + "</p>");
                }
            }
            
            AppendLog("Backup finished.");
            AppendLog("Sending mail to " + Settings.Default.SendMailTo);

            try
            {
                var mailBody = new StringBuilder();
                logText.Sort();
                mailBody.Append("From: " + startTime);
                mailBody.Append("<br/>To:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + DateTime.Now.ToString());
                mailBody.Append("<p>Total size: " + string.Format(System.Globalization.CultureInfo.GetCultureInfo("de-DE"), sizeFormat, (double)totalSize / 1024 / 1024) + "</p>");
                logText.ForEach(i => mailBody.Append(i));

                var logPath = string.Concat(workingDirectory, "\\log.txt");

                File.AppendAllText(logPath, _log.ToString());

                MailSender.Send(mailBody.ToString(), "BitUp-Backup", logPath);
            }
            catch (Exception e)
            {
                AppendLog(e.Message);

                if (e.InnerException != null)
                {
                    AppendLog(e.InnerException.Message);
                }
            }
            ForceDeleteDirectory(workingDirectory);

            if (Environment.UserInteractive)
            {
                AppendLog("Press any key to continue...");
                Console.ReadKey(false);
            }
            AppendLog("Done!");
        }

        private static void AppendLog(string text)
        {
            Console.WriteLine(text);
            _log.AppendLine(text);
        }

        private static ErrorType DownloadRepository(BitbucketRepository repository, IGitSourceControl sourceControl, string repositoryDirectory)
        {
            ErrorType error = ErrorType.None;
            string cloneDirectory = string.Concat(repositoryDirectory, @"\", repository.DirectoryName, ".git");
            string wikiDirectory = cloneDirectory.Replace(".git", "-wiki.git");

            if (Directory.Exists(cloneDirectory))
            {
                ForceDeleteDirectory(cloneDirectory);
            }
            if (Directory.Exists(wikiDirectory))
            {
                ForceDeleteDirectory(wikiDirectory);
            }
            AppendLog(repository.Name + Messages.CloneRepository);
            sourceControl.CloneRepository(repository.CloneUrl, cloneDirectory);

            if (repository.HasIssues)
            {
                AppendLog(Messages.ExportIssues);
                string issueZipPath = string.Concat(repositoryDirectory, "/", repository.DirectoryName, "-issues.zip");

                try
                {
                    sourceControl.SaveIssues(repository, issueZipPath);
                }
                catch (Exception e)
                {
                    error = ErrorType.Error;
                    AppendLog(Messages.IssueExportingError + e.Message);
                }
            }
            if (repository.HasWiki)
            {
                AppendLog(Messages.CloneWiki);
                try
                {
                    sourceControl.CloneWiki(repository.CloneUrl, wikiDirectory);
                }
                catch (Exception e)
                {
                    error = ErrorType.Error;
                    AppendLog(Messages.WikiCloningError + e.Message);
                }
            }
            return error;
        }

        private static void ForceDeleteDirectory(string absolutePath)
        {
            if (Directory.Exists(absolutePath))
            {
                var dirInfo = new DirectoryInfo(absolutePath) { Attributes = FileAttributes.Normal };
                // can not delete readonly files
                foreach (var info in dirInfo.GetFileSystemInfos("*", SearchOption.AllDirectories))
                {
                    info.Attributes = FileAttributes.Normal;
                }
                dirInfo.Delete(true);
            }
        }

        private static void ForceDeleteFile(string absolutePath)
        {
            if (File.Exists(absolutePath))
            {
                FileInfo fileInfo = new FileInfo(absolutePath);
                fileInfo.IsReadOnly = false;

                fileInfo.Delete();
            }
        }

        private static string DecryptPassword(string password)
        {
            var passwordBytes = Convert.FromBase64String(password);
            return Encoding.UTF8.GetString(passwordBytes);
        }
    }
}
