using System.Collections.Generic;

namespace SlideStore
{
    /// <summary>
    /// Lifetime store the image file addresses of images to be displayed
    /// </summary>
    /// <remarks>
    /// In a first program develpment step its a simple List of FileCheckEventArgs
    /// may later be enhanced (if useful) to a database system
    /// Actually my ~ 45000 images in a single path are handled without trouble ...
    /// </remarks>
    public class Storage
    {
        /// <summary>
        /// Files in this list
        /// </summary>
        public List<FileCheckEventArgs> Slides { get; private set; } = new List<FileCheckEventArgs>();

        /// <summary>
        /// Add a file to the storage
        /// </summary>
        /// <param name="slide"></param>
        public void AddSlide(FileCheckEventArgs slide)
        {
            Slides.Add(slide);
        }
    }
}
