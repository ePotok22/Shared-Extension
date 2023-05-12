namespace FFF.Shared
{
    public static class FormatFileExtensions
    {
        private static string[] _suffix = { "bytes", "KB", "MB", "GB" };

        public static string ToFormatFileSize(this long fileSize)
        {
            long j = 0;

            while (fileSize > 1024 && j < 4)
            {
                fileSize = fileSize / 1024;
                j++;
            }
            return (fileSize + " " + _suffix[j]);
        }

        public static string ToFormatFileSize(this ulong fileSize)
        {
            ulong j = 0;

            while (fileSize > 1024 && j < 4)
            {
                fileSize = fileSize / 1024;
                j++;
            }
            return (fileSize + " " + _suffix[j]);
        }

        public static string ToFormatFileSize(this int fileSize)
        {
            int j = 0;

            while (fileSize > 1024 && j < 4)
            {
                fileSize = fileSize / 1024;
                j++;
            }
            return (fileSize + " " + _suffix[j]);
        }

        public static string ToFormatFileSize(this uint fileSize)
        {
            uint j = 0;

            while (fileSize > 1024 && j < 4)
            {
                fileSize = fileSize / 1024;
                j++;
            }
            return (fileSize + " " + _suffix[j]);
        }

    }
}
