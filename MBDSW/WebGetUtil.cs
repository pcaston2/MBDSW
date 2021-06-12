using System;
using System.Net.Http;

namespace MBDSW
{
    public class WebGetUtil
    {
        public static string GetStringContentFromUrl(string url)
        {
            var client = new HttpClient();
            return client.GetStringAsync(url).Result;
        }

        public static byte[] GetByteContentFromUrl(string url)
        {
            var client = new HttpClient();
            return client.GetByteArrayAsync(url).Result;
        }
    }
}
