using System;
using System.IO;

namespace MonikaBot.Launcher
{
    public static class GitHelper
    {

        public static string GetNameFromGitUrl(string url)
        {
            string repoName = url.Substring(0, url.LastIndexOf('/')).Split('/').Last();
            if (repoName.EndsWith(".git"))
                repoName = repoName.Replace(".git", "");

            return repoName;
        }

        public static string CloneGitRepository(string url)
        {
            string command = $"git clone {url}";
            try
            {
                var output = command.Bash(MainClass.GitModulesPath);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exc: {ex.Message}");
            }

            return Path.Combine(MainClass.GitModulesPath, GetNameFromGitUrl(url));
        }

        public static bool PullGitRepository(string url)
        {
            string command = $"git pull";
            try
            {
                command.Bash(MainClass.GitModulesPath);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error Pulling Git Repo: {ex.Message}");
                return false;
            }
            return true;
        }
    }
}
