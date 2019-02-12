using MovieInfo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MovieInfo
{
    public class MovieDirectoryTraverser
    {
        private BackgroundWorker mWorker = null;
        private MainPage mWindow = null;
        List<FileInfo> mFileList = new List<FileInfo>();
        bool mMerge = false;
        static List<string> excludedFolders = new List<string>();

        public int FilesCount() { return mFileList.Count; }

        static MovieDirectoryTraverser()
        {
            excludedFolders.Add(Environment.GetEnvironmentVariable("windir"));
            excludedFolders.Add(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
            excludedFolders.Add(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache));
        }

        /**
         * @fn  public DirectoryTraverser(IDirectoryVisitor visitor)
         *
         * @brief   Constructor.
         *
         * @param   visitor The visitor.
         */
        public MovieDirectoryTraverser(BackgroundWorker worker, String[] path, MainPage window, bool mergeResult)
        {
            this.mWorker = worker;
            this.mWindow = window;
            this.mMerge = mergeResult;

            foreach (String folderPath in path)
            {
                if (folderPath.Trim() != "")
                {
                    EnumerateFiles(new DirectoryInfo(folderPath));
                }
                // EnumerateFiles_Debug(new DirectoryInfo(folderPath));
            }
        }

        public MovieDirectoryTraverser(BackgroundWorker worker, MainPage window, bool mergeResult)
        {
            this.mWindow = window;
            this.mWorker = worker;
            this.mMerge = mergeResult;

            foreach (string dr in System.Environment.GetLogicalDrives())
            {
                System.IO.DriveInfo drive = new System.IO.DriveInfo(dr);

                // Here we skip the drive if it is not ready to be read.
                if (drive.IsReady)
                {
                    EnumerateFiles(drive.RootDirectory);
                }
            }
        }

        private void EnumerateFiles(DirectoryInfo dir)
        {
            try
            {
                if (dir.Parent == null)
                {
                    mFileList.AddRange(dir.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly)
                                 .Where(f => Utility.filters.Contains(f.Extension.ToLower()))
                                 .ToArray());

                    foreach (DirectoryInfo childDir in dir.GetDirectories("*", SearchOption.TopDirectoryOnly))
                    {
                        if (IsValid(childDir.Attributes, childDir.FullName))
                        {
                            try
                            {
                                mFileList.AddRange(childDir.EnumerateFiles("*.*", SearchOption.AllDirectories)
                                   .Where(f => Utility.filters.Contains(f.Extension.ToLower()))
                                   .ToArray());
                            }
                            catch (Exception ex) 
                            {
                                Console.WriteLine("[WARNING] " + ex.Message);
                            }
                        }
                    }

                }
                else
                {
                    mFileList.AddRange(dir.EnumerateFiles("*.*", SearchOption.AllDirectories)
                                .Where(f => Utility.filters.Contains(f.Extension.ToLower()))
                                .ToArray());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[WARNING] " + ex.Message);
                MetroMessageBox.ShowErrorMessage(null, ex.Message, "Error !!", System.Drawing.SystemIcons.Error, MessageBoxButton.OK, MessageBoxResult.OK);
            }
        }

        private void EnumerateFiles_Debug(DirectoryInfo dir)
        {
            try
            {
                mFileList.AddRange(dir.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly)
                              .Where(f => Utility.filters.Contains(f.Extension.ToLower()))
                              .ToArray());

                foreach (DirectoryInfo childDir in dir.GetDirectories("*", SearchOption.TopDirectoryOnly))
                {
                    this.EnumerateFiles_Debug(childDir);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[WARNING] " + ex.Message);
                MetroMessageBox.ShowErrorMessage(null, ex.Message, "Error !!", System.Drawing.SystemIcons.Error, MessageBoxButton.OK, MessageBoxResult.OK);
            }
        }

        static bool IsValid(FileAttributes attributes, string fullpath)
        {
            bool retval = true;

            if (((attributes & FileAttributes.System) == FileAttributes.System) ||
                ((attributes & FileAttributes.Hidden) == FileAttributes.Hidden))
            {
                retval = false;
            }

            if (retval && excludedFolders.Contains(fullpath))
            {
                retval = false;
            }

            return retval;
        }

        /**
         * @fn  private void Traverse(DirectoryInfo dirInfo)
         *
         * @brief   Traverses the given dir information.
         *
         * @param   dirInfo Information describing the dir.
         */
        public void Traverse()
        {
            int len = mFileList.Count;
            int index = 0;

            foreach (FileInfo file in mFileList)
            {
                if (mWorker.CancellationPending) break;

                this.VisitFile(file);
                mWorker.ReportProgress(++index);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        private void VisitFile(FileInfo fileInfo)
        {
            try
            {
                String fileName = Path.GetFileNameWithoutExtension(fileInfo.Name);
                String cleanedUpName = Utility.GetCleanName(fileName);

                if (cleanedUpName != null)
                {
                    IMDBXML movie = new IMDBXML(fileInfo.FullName, fileInfo.Name, cleanedUpName, String.Empty);

                    this.mWindow.AddRowToCollection(movie, this.mMerge);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[WARNING] " + ex.Message);
            }
        }
    }
}
