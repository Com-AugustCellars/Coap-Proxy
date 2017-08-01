using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using Com.AugustCellars.CoAP.Proxy.Http;
using Com.AugustCellars.CoAP;
using Com.AugustCellars.CoAP.Proxy;
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;

namespace Com.AugustCellars.CoAP
{
    [TestClass]
    public class Test_HttpTranslator
    {
        private const string Host = "Host test.example.com\n";

        [TestMethod]
        public void GetCoapRequest_BadMethod()
        {
            HttpRequest http = new HttpRequest("FOOBAR /foo HTTP/1.1\n" + Host);

            TranslationException e = Assert.Throws<TranslationException>(() =>
                HttpTranslator.GetCoapRequest(http, "xx", true));
            Assert.That(e.Message, Is.EqualTo("FOOBAR method not mapped"));
        }

        [TestMethod]
        public void GetCoapRequest_SimpleForm()
        {
            HttpRequest http;

            string proxyUri = "coap://coap.example.com/resource";
            http = new HttpRequest("GET /hc/" + proxyUri + " HTTP/1.1\n" + Host);

            Request req = HttpTranslator.GetCoapRequest(http, "hc/{+tu}", true);
            Assert.That(req.Method, Is.EqualTo(Method.GET));
            Assert.That(req.ProxyUri.ToString(), Is.EqualTo(proxyUri));


            proxyUri = "//coap.example.com/resource";
            http = new HttpRequest("GET /hc/" + proxyUri + " HTTP/1.1\n" + Host);

            TranslationException e = Assert.Throws<TranslationException>(() =>
            HttpTranslator.GetCoapRequest(http, "hc/{+tu}", true));
            Assert.That(e.Message, Is.EqualTo("Schema is required"));

            proxyUri = "coap://coap.example.com/?query=1";
            http = new HttpRequest("GET /hc/" + proxyUri + " HTTP/1.1\n" + Host);

            req = HttpTranslator.GetCoapRequest(http, "hc/{+tu}", true);
            Assert.That(req.Method, Is.EqualTo(Method.GET));
            Assert.That(req.ProxyUri.ToString(), Is.EqualTo(proxyUri));

            proxyUri = "coap://coap.example.com/resource?query=1";
            http = new HttpRequest("GET /hc/" + proxyUri + " HTTP/1.1\n" + Host);

            req = HttpTranslator.GetCoapRequest(http, "hc/{+tu}", true);
            Assert.That(req.Method, Is.EqualTo(Method.GET));
            Assert.That(req.ProxyUri.ToString(), Is.EqualTo(proxyUri));

            proxyUri = "coap://coap.example.com:5848/resource";
            http = new HttpRequest("GET /hc/" + proxyUri + " HTTP/1.1\n" + Host);

            req = HttpTranslator.GetCoapRequest(http, "hc/{+tu}", true);
            Assert.That(req.Method, Is.EqualTo(Method.GET));
            Assert.That(req.ProxyUri.ToString(), Is.EqualTo(proxyUri));
        }


        [TestMethod]
        public void GetCoapRequest_SimpleForm2()
        {

            string proxyUri = "coaps://coap.example.com/resource";
            HttpRequest http = new HttpRequest("GET /hc/?target=" + proxyUri + " HTTP/1.1\n" + Host);

            Request req = HttpTranslator.GetCoapRequest(http, "hc/?target={+tu}", true);
            Assert.That(req.Method, Is.EqualTo(Method.GET));
            Assert.That(req.ProxyUri.ToString(), Is.EqualTo(proxyUri));
        }

        private class HttpRequest : IHttpRequest
        {
            public HttpRequest(string requestString)
            {
                string[] lines = requestString.Split('\n');
                foreach (string line in lines) {
                    int i = line.IndexOf(" ", StringComparison.Ordinal);
                    if (i > 0) {
                        string key = line.Substring(0, i);
                        string value = line.Substring(i + 1);
                        Headers.Add(key, value);
                    }
                }

                string[] xxx = lines[0].Split(' ');
                Method = xxx[0];
                Url = "http://" + Host + xxx[1];

                Uri uri = new Uri(Url);
                RequestUri = uri.AbsolutePath;
                QueryString = uri.Query;
                UserAgent = "FireFox";
            }

            public string Url { get; }
            public string RequestUri { get; }
            public string QueryString { get; }
            public string Method { get; }
            public NameValueCollection Headers { get; } = new NameValueCollection();
            public Stream InputStream { get; }
            public string Host { get => Headers["Host"]; }
            public string UserAgent { get; }
            public string GetParameter(string name)
            {
                throw new NotImplementedException();
            }

            public string[] GetParameters(string name)
            {
                throw new NotImplementedException();
            }

            public object this[object key] {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }
        }

    }
}
