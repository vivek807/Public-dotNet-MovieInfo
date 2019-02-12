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
    public class MovieTextTraverser
    {
        private BackgroundWorker mWorker = null;
        private MainPage mWindow = null;
        bool mMerge = false;
        List<String> mFileList = new List<String>();
        static List<string> excludedFolders = new List<string>();

        public int FilesCount() { return mFileList.Count; }


        /**
         * @fn  public TextTraverser(IDirectoryVisitor visitor)
         *
         * @brief   Constructor.
         *
         * @param   visitor The visitor.
         */
        public MovieTextTraverser(BackgroundWorker worker, String[] path, MainPage mainWindow, bool mergeResult)
        {
            this.mWorker = worker;
            this.mWindow = mainWindow;
            this.mMerge = mergeResult;

            foreach (String filePath in path)
            {
                if (filePath.Trim() != "")
                {
                    String file = filePath.Replace("\"", String.Empty);

                    EnumerateFiles(file);
                }
            }
        }

        private void EnumerateFiles(String filePath)
        {
            try
            {
                mFileList.AddRange(Array.FindAll(File.ReadAllLines(filePath),
                    s => Utility.filters.Contains(Path.GetExtension(s).Trim().ToLower())));
            }
            catch (Exception ex)
            {
                Console.WriteLine("[WARNING] " + ex.Message);
                MetroMessageBox.ShowErrorMessage(null, ex.Message, "Error !!", System.Drawing.SystemIcons.Error, MessageBoxButton.OK, MessageBoxResult.OK);
            }
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

            foreach (String fileName in mFileList)
            {
                if (mWorker.CancellationPending) break;

                this.VisitFile(fileName);

                mWorker.ReportProgress(++index);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        private void VisitFile(String filePath)
        {
            try
            {
                String fileName = Path.GetFileNameWithoutExtension(filePath);
                String cleanedUpName = Utility.GetCleanName(fileName);

                if (!String.IsNullOrEmpty(cleanedUpName))
                {
                    IMDBXML movie = new IMDBXML(filePath.Contains(Path.DirectorySeparatorChar) ? filePath : "",
                        Path.GetFileName(filePath), cleanedUpName, String.Empty);

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
