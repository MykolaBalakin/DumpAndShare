using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace Balakin.LinqPad.DumpAndShare {
    public static class DumpAndShareExtensions {
        public static T DumpAndShare<T>(this T o) {
            return DumpAndShare(o, null);
        }

        public static T DumpAndShare<T>(this T o, String description) {
            var dumpObject = (Object)o;
            if (!String.IsNullOrEmpty(description)) {
                dumpObject = WrapObjectWithHeadingPresenter(o, description);
            }
            var data = SerializeObject(dumpObject);
            var fileName = o == null ? "null.html" : o.GetType() + ".html";
            var uri = SendFile(fileName, "text/html", data);
            DumpUri(uri);
            return o;
        }

        private static Object WrapObjectWithHeadingPresenter(Object o, String description) {
            var wrapperType = typeof(LINQPad.Util).Assembly.GetType("LINQPad.ObjectGraph.HeadingPresenter");
            return Activator.CreateInstance(wrapperType, description, o);
        }

        private static Byte[] SerializeObject(Object o) {
            Byte[] bytes;
            using (var writer = LINQPad.Util.CreateXhtmlWriter(true)) {
                writer.Write(o);
                bytes = Encoding.UTF8.GetBytes(writer.ToString());
            }
            return bytes;
        }

        private static Uri SendFile(String fileName, String contentType, Byte[] content) {
            var baseUri = LINQPad.Util.GetPassword("DumpAndShare uri");
            var uriBuilder = new UriBuilder(baseUri);
            var queryPart = uriBuilder.Query;
            if (!String.IsNullOrEmpty(queryPart)) {
                queryPart = queryPart.TrimStart('?') + "&";
            }
            queryPart += "fileName=" + Uri.EscapeUriString(fileName);
            uriBuilder.Query = queryPart;
            var request = WebRequest.CreateHttp(uriBuilder.Uri);
            request.Method = "POST";
            request.ContentType = contentType;
            using (var requestStream = request.GetRequestStream()) {
                requestStream.Write(content, 0, content.Length);
            }

            using (var response = request.GetResponse())
            using (var responseStream = response.GetResponseStream()) {
                using (var reader = new StreamReader(responseStream)) {
                    return new Uri(reader.ReadToEnd());
                }
            }
        }

        private static void DumpUri(Uri uri) {
            LINQPad.Util.RawHtml(new XElement("a", new XAttribute("href", uri.ToString()), uri.ToString()));
        }
    }
}
