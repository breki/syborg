using System.IO;
using System.Net;
using System.Text;

namespace Syborg.Tests
{
    public class RestClient
    {
        public int LastHttpStatus
        {
            get { return lastHttpStatus; }
        }

        public string LastContentType
        {
            get { return lastContentType; }
        }

        public byte[] Get (string url)
        {
            WebRequest request = WebRequest.Create (url);
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse ())
            {
                lastHttpStatus = (int)response.StatusCode;
                lastContentType = response.ContentType;

                byte[] buffer = new byte[32768];
                using (Stream stream = response.GetResponseStream ())
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    while (true)
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        int read = stream.Read (buffer, 0, buffer.Length);
                        if (read <= 0)
                            return memoryStream.ToArray ();
                        memoryStream.Write (buffer, 0, read);
                    }
                }
            }
        }

        public string GetString (string url)
        {
            WebRequest request = WebRequest.Create (url);
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse ())
            {
                lastHttpStatus = (int)response.StatusCode;
                using (Stream stream = response.GetResponseStream ())
                    // ReSharper disable once AssignNullToNotNullAttribute
                using (StreamReader reader = new StreamReader (stream, Encoding.UTF8))
                {
                    return reader.ReadToEnd ();
                }
            }
        }

        public void Head (string url)
        {
            WebRequest request = WebRequest.Create (url);
            request.Method = "HEAD";
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse ())
            {
                lastHttpStatus = (int)response.StatusCode;
                lastContentType = response.ContentType;
            }
        }

        public string PostString (string url, string body)
        {
            WebRequest request = WebRequest.Create (url);
            request.Method = "POST";

            using (Stream requestStream = request.GetRequestStream ())
            using (StreamWriter writer = new StreamWriter (requestStream, Encoding.UTF8))
                writer.Write (body);

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse ())
            {
                lastHttpStatus = (int)response.StatusCode;
                using (Stream responseStream = response.GetResponseStream ())
                    // ReSharper disable once AssignNullToNotNullAttribute
                using (StreamReader reader = new StreamReader (responseStream, Encoding.UTF8))
                {
                    return reader.ReadToEnd ();
                }
            }
        }

        private int lastHttpStatus;
        private string lastContentType;
    }
}