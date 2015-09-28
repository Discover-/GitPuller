using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace GitPuller
{
    class Program
    {
        static void Main(string[] args)
        {
            int totalFoldersScanned = 0;
            List<string> skippedFolders = new List<string>();

            //! Set the git directory setting if it wasn't set yet.
            if (String.IsNullOrWhiteSpace(Settings.Default.GitDirectory))
            {
                Console.Write("Point me to where Git.exe is located: ");
                string newGitDir = Console.ReadLine();

                while (String.IsNullOrWhiteSpace(newGitDir) || !Path.HasExtension(newGitDir))
                {
                    if (!String.IsNullOrWhiteSpace(newGitDir) && !Path.HasExtension(newGitDir) && Directory.Exists(newGitDir))
                    {
                        var di = new DirectoryInfo(newGitDir);

                        if (di.GetFiles("git.exe").Length == 1)
                        {
                            if (File.Exists(newGitDir + "\\git.exe"))
                            {
                                newGitDir += "\\git.exe";
                                break;
                            }
                        }
                    }

                    Console.Write("This is not a valid Git directory. Try again: ");
                    newGitDir = Console.ReadLine();
                }

                Settings.Default.GitDirectory = newGitDir;
            }

            //! Set the repositories directory setting if it wasn't set yet.
            if (String.IsNullOrWhiteSpace(Settings.Default.RepositoriesDirectory))
            {
                Console.Write("Set the directory containing all repositories: ");
                string newRepoDir = Console.ReadLine();

                while (String.IsNullOrWhiteSpace(newRepoDir) || Path.HasExtension(newRepoDir) || Directory.GetDirectories(newRepoDir).Length == 0)
                {
                    Console.Write("This is not a valid directory. Try again: ");
                    newRepoDir = Console.ReadLine();
                }

                Settings.Default.RepositoriesDirectory = newRepoDir;
            }

            Settings.Default.Save();

            Process gitProcess = new Process();
            ProcessStartInfo gitInfo = new ProcessStartInfo();
            gitInfo.CreateNoWindow = true;
            gitInfo.RedirectStandardError = true;
            gitInfo.RedirectStandardOutput = true;
            gitInfo.FileName = Settings.Default.GitDirectory;
            gitInfo.Arguments = "pull origin master";
            gitInfo.UseShellExecute = false;

            //! Iterate over the folder the app was ran from
            foreach (string dir in Directory.GetDirectories(Settings.Default.RepositoriesDirectory))
            {
                totalFoldersScanned++;

                gitInfo.WorkingDirectory = dir;
                gitProcess.StartInfo = gitInfo;
                gitProcess.Start();

                string stderr_str = gitProcess.StandardError.ReadToEnd();
                string stdout_str = gitProcess.StandardOutput.ReadToEnd();

                //! If it's not a GIT repository...
                if (stderr_str.Contains(".git"))
                {
                    skippedFolders.Add(Path.GetFileName(dir));
                    continue;
                }

                Console.WriteLine("- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -");
                Console.WriteLine("Pulling: " + Path.GetFileName(dir));
                Console.WriteLine("Output: " + stdout_str + "\n");

                gitProcess.WaitForExit();
                gitProcess.Close();
            }

            Console.WriteLine("\n\n- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -");
            Console.WriteLine("Finished! A total of {0} folders were scanned of which {1} were skipped as they were not Git repositories.", totalFoldersScanned, skippedFolders.Count);

            if (skippedFolders.Count > 0)
            {
                Console.WriteLine("\nSkipped repositories:");

                foreach (string skippedDir in skippedFolders)
                    Console.WriteLine(skippedDir);

                Console.WriteLine();
            }

            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();
        }
    }
}
