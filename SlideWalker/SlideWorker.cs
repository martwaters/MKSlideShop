using NLog;
using SlideStore;
using System;
using System.ComponentModel;
using System.IO;

namespace SlideWalker
{
    /// <summary>
    /// Worker to find slide files across PathWalker folders
    /// </summary>
    public class SlideWorker
    {
        #region Properties

        static readonly Logger log = LogManager.GetCurrentClassLogger();

        private BackgroundWorker bkgWorker;
        private string PathOperation { get; set; }

        public event EventHandler<object[]> SlideRetrieved;

        public event EventHandler<object[]> FileWorkDone;

        public event EventHandler<string> FileWorkerError;

        object lockObject = new object();


        public string ActiveFile { get; set; }
        public string ActiveDir { get; set; }

        /// <summary>
        /// Number of files found as string
        /// </summary>
        public string NFiles
        {
            get { return nFiles.ToString(); }
            set
            {
                if (int.TryParse(value, out int n))
                    nFiles = n;
            }
        }
        private int nFiles;

        #endregion // Properties

        /// <summary>
        /// COnfigure and run the search
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="paths"></param>
        public void FindSlides(PathWalker handler, string[] paths)
        {
            PathOperation = "FindSlides";

            log.Trace($"RunAsync FindSlides: {paths.Length} path(s)");

            bkgWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            bkgWorker.DoWork += FindSlides_DoWork;
            bkgWorker.ProgressChanged += FileOp_Update;
            bkgWorker.RunWorkerCompleted += FindSlides_Completed;
            bkgWorker.RunWorkerAsync(new object[] { handler, paths });

        }

        #region Common 

        private bool HandleSuccess(RunWorkerCompletedEventArgs e)
        {
            return !e.Cancelled && e.Error == null;
        }

        private void HandelError(RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {

                FileWorkerError?.Invoke(this, string.Format("{0}: Operation cancelled", PathOperation));
            }
            else if (e.Error != null)
            {

                FileWorkerError?.Invoke(this, string.Format("{0} Error: {1}", PathOperation, e.Error.Message));
            }
        }

        private void FileOp_Update(object sender, ProgressChangedEventArgs e)
        {
            // Log.Trace(PathOperation + "_Update " + e.ProgressPercentage + "%");
        }

        #endregion // Common 

        #region Get Images from Tree

        public void Stop()
        {
            if (bkgWorker != null && bkgWorker.IsBusy)
            {
                bkgWorker.CancelAsync();              
            }
        }


        /// <summary>
        /// Found a slide - call handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void CheckSlideEvent(object sender, FileCheckEventArgs args)
        {
            lock (lockObject)
            {
                NFiles = (++nFiles).ToString();
                ActiveDir = args.FInfo.DirectoryName;
                ActiveFile = args.FInfo.Name;
                log.Trace($"Invoke SlideRetrieved: {args.FInfo.Name}");
                SlideRetrieved?.Invoke(this, new object[] { args });
            }
        }

        /// <summary>
        /// WalkDirectories
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FindSlides_DoWork(object sender, DoWorkEventArgs e)
        {
            PathWalker pathWalk = (e.Argument as object[])[0] as PathWalker;
            string[] pathNames = (e.Argument as object[])[1] as string[];

            NFiles = 0.ToString();

            pathWalk.SlideCheckEvent += CheckSlideEvent;

            foreach (string pathName in pathNames)
            {
                log.Trace($"FindSlide DoWork: {pathName}");
                pathWalk.WalkDirectories(new DirectoryInfo(pathName));
            }

            //lock (lockObject)
            //{
            //    foreach (List<FileCheckEventArgs> list in pathWalk.Matches)
            //    {
            //        Multiples.Add(list.FirstOrDefault().FInfo.Name);
            //    }
            //}

            pathWalk.SlideCheckEvent -= CheckSlideEvent;

            e.Result = new object[] { pathWalk, NFiles };
        }

        private void FindSlides_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (HandleSuccess(e))
            {
                //Log.Trace(PathOperation + " completed");
                object[] objects = e.Result as object[];
                if (objects != null && objects.Length == 2)
                {
                    log.Trace($"Invoke Slide Done: {objects[0]}, {objects[1]}");
                    FileWorkDone?.Invoke(this, objects);
                }
                else
                    FileWorkerError?.Invoke(this, string.Format("{0} no data returned", PathOperation));

            }
            else
                HandelError(e);

        }

        #endregion // Get Images in Tree

    }
}
