
using System.IO;

namespace Ekr.Core.Helper
{
    public static class StreamHelpers
    {
        public static string ReadToEndString(this Stream stream)
        {
            if (stream.Length <= 0) return string.Empty;
            using (var st = new StreamReader(stream))
            {
                return st.ReadToEnd();
            }
        }
    }
}
