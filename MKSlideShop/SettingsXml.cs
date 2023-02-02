using System.Collections.Generic;

namespace MKSlideShop
{
    /// <summary>
    /// Helper class to De/serialize ShowSettings as copy
    /// </summary>
    public class SettingsXml
    {
        public int ShowTime { get; set; }

        /// <summary>
        /// MainWindow State
        /// </summary>
        public int MState { get; set; }
        public double MLeft { get; set; }
        public double MTop { get; set; }
        public double MWidth { get; set; }
        public double MHeight { get; set; }

        public int MKeep { get; set; }

        /// <summary>
        /// SlideWindow State
        /// </summary>
        public int SState { get; set; }
        public double SLeft { get; set; }
        public double STop { get; set; }
        public double SWidth { get; set; }
        public double SHeight { get; set; }

        public string SettingsPath { get; set; } = string.Empty;
        public string Browser { get; set; } = string.Empty;
        public List<string> Paths { get; set; } = new List<string>();

        public SettingsXml() { }

        internal SettingsXml(ShowSettings show)
        {

            Browser = show.BrowserPath;
            SettingsPath = show.SettingsPath;

            ShowTime = show.ShowTime;

            MState = show.MainState;
            MHeight = show.MainHeight;
            MLeft = show.MainLeft;
            MTop = show.MainTop;
            MWidth =show.MainWidth;

            MKeep = show.MainOnStart;

            SState = show.SlideState;

            SLeft = show.SlideLeft;
            STop = show.SlideTop;
            SWidth =show.SlideWidth;
            SHeight = show.SlideHeight;

            string[] paths = new string[show.LastPaths.Count];
            show.LastPaths.CopyTo(paths, 0);

            Paths = new List<string>(paths);
        }
    }
}
