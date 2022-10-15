using System.IO;
using System.Security.Cryptography;

namespace SlideStore
{
    /// <summary>
    /// Enahnced FileInfo about an image file
    /// </summary>
    /// <remarks>Uses a Hash for easy equality comparison</remarks>
    public class FileCheckEventArgs
    {
        /// <summary>
        /// this class enhances a FileInfo
        /// </summary>
        /// <param name="fInfo"></param>
        public FileCheckEventArgs(FileInfo fInfo)
        {
            FInfo = fInfo;
        }

        /// <summary>
        /// Details about the file
        /// </summary>
        public FileInfo FInfo { get; private set; }

        /// <summary>
        /// Hash to compare
        /// </summary>
        public byte[] Hash
        {
            get
            {
                if (hash == null)
                {
                    hash = ComputeHash(FInfo.FullName);
                }
                return hash;
            }
        }
        private byte[] hash;

        /// <summary>
        /// creates the md5 hash of a file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private byte[] ComputeHash(string filePath)
        {
            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(filePath))
                return md5.ComputeHash(stream);
        }
    }
}

