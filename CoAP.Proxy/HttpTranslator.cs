/*
 * Copyright (c) 2011-2014, Longxiang He <helongxiang@smeshlink.com>,
 * SmeshLink Technology Co.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY.
 * 
 * This file is part of the CoAP.NET, a CoAP framework in C#.
 * Please see README for more information.
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using OO = UriTemplate.Core;
using Com.AugustCellars.CoAP.Proxy.Http;
using Com.AugustCellars.CoAP.Util;

namespace Com.AugustCellars.CoAP.Proxy
{
    /// <summary>
    /// This class supports the converstion of CoAP messages to HTTP messages and back.
    /// 
    /// This is used for proxy operations where requests and responses need to be converted back and forth.
    /// Currently there is a well defined way to do HTTP requests to CoAP requests, but not the opposite.
    /// </summary>
    public static class HttpTranslator
    {
        private static readonly Dictionary<HttpStatusCode, StatusCode> _Http2CoapCode = new Dictionary<HttpStatusCode, StatusCode>();
        private static readonly Dictionary<StatusCode, HttpStatusCode> _Coap2HttpCode = new Dictionary<StatusCode, HttpStatusCode>();
        private static readonly Dictionary<string, OptionType> _Http2CoapOption = new Dictionary<string, OptionType>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<OptionType, string> _Coap2HttpHeader = new Dictionary<OptionType, string>();
        private static readonly Dictionary<string, Int32> _Http2CoapMediaType = new Dictionary<string, Int32>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<Int32, string> _Coap2HttpContentType = new Dictionary<Int32, string>();
        private static readonly Dictionary<string, Method> _Http2CoapMethod = new Dictionary<string, Method>(StringComparer.OrdinalIgnoreCase);

        static HttpTranslator()
        {
            _Http2CoapOption["etag"] = OptionType.ETag;
            _Http2CoapOption["accept"] = OptionType.Accept;
            _Http2CoapOption["content-type"] = OptionType.ContentType;
            _Http2CoapOption["cache-control"] = OptionType.MaxAge;
            _Http2CoapOption["if-match"] = OptionType.IfMatch;
            _Http2CoapOption["if-none-match"] = OptionType.IfNoneMatch;

            _Coap2HttpHeader[OptionType.IfMatch] = "If-Match";
            _Coap2HttpHeader[OptionType.ETag] = "Etag";
            _Coap2HttpHeader[OptionType.IfNoneMatch] = "If-None-Match";
            _Coap2HttpHeader[OptionType.ContentType] = "Content-Type";
            _Coap2HttpHeader[OptionType.MaxAge] = "Cache-Control";
            _Coap2HttpHeader[OptionType.Accept] = "Accept";
            _Coap2HttpHeader[OptionType.LocationPath] = "Location";
            _Coap2HttpHeader[OptionType.LocationQuery] = "Location";

            _Http2CoapMediaType["text/plain"] = MediaType.TextPlain;
            _Http2CoapMediaType["text/html"] = MediaType.TextHtml;
            _Http2CoapMediaType["image/jpeg"] = MediaType.ImageJpeg;
            _Http2CoapMediaType["image/tiff"] = MediaType.ImageTiff;
            _Http2CoapMediaType["image/png"] = MediaType.ImagePng;
            _Http2CoapMediaType["image/gif"] = MediaType.ImageGif;
            _Http2CoapMediaType["application/xml"] = MediaType.ApplicationXml;
            _Http2CoapMediaType["application/json"] = MediaType.ApplicationJson;
            _Http2CoapMediaType["application/link-format"] = MediaType.ApplicationLinkFormat;

            _Coap2HttpContentType[MediaType.TextPlain] = "text/plain; charset=utf-8";
            _Coap2HttpContentType[MediaType.TextHtml] = "text/html";
            _Coap2HttpContentType[MediaType.ImageJpeg] = "image/jpeg";
            _Coap2HttpContentType[MediaType.ImageTiff] = "image/tiff";
            _Coap2HttpContentType[MediaType.ImagePng] = "image/png";
            _Coap2HttpContentType[MediaType.ImageGif] = "image/gif";
            _Coap2HttpContentType[MediaType.ApplicationXml] = "application/xml";
            _Coap2HttpContentType[MediaType.ApplicationJson] = "application/json; charset=UTF-8";
            _Coap2HttpContentType[MediaType.ApplicationLinkFormat] = "application/link-format";

            _Http2CoapCode[HttpStatusCode.Continue] = StatusCode.BadGateway;
            _Http2CoapCode[HttpStatusCode.SwitchingProtocols] = StatusCode.BadGateway;
            _Http2CoapCode[HttpStatusCode.OK] = StatusCode.Content;
            _Http2CoapCode[HttpStatusCode.Created] = StatusCode.Created;
            _Http2CoapCode[HttpStatusCode.Accepted] = StatusCode.Content;
            _Http2CoapCode[HttpStatusCode.NonAuthoritativeInformation] = StatusCode.Content;
            _Http2CoapCode[HttpStatusCode.ResetContent] = StatusCode.Content;
            _Http2CoapCode[HttpStatusCode.PartialContent] = 0;
            _Http2CoapCode[HttpStatusCode.MultipleChoices] = StatusCode.BadGateway;
            _Http2CoapCode[HttpStatusCode.Moved] = StatusCode.BadGateway;
            _Http2CoapCode[HttpStatusCode.Redirect] = StatusCode.BadGateway;
            _Http2CoapCode[HttpStatusCode.RedirectMethod] = StatusCode.BadGateway;
            _Http2CoapCode[HttpStatusCode.NotModified] = StatusCode.Valid;
            _Http2CoapCode[HttpStatusCode.UseProxy] = StatusCode.BadGateway;
            _Http2CoapCode[HttpStatusCode.TemporaryRedirect] = StatusCode.BadGateway;
            _Http2CoapCode[HttpStatusCode.BadRequest] = StatusCode.BadRequest;
            _Http2CoapCode[HttpStatusCode.Unauthorized] = StatusCode.Unauthorized;
            _Http2CoapCode[HttpStatusCode.PaymentRequired] = StatusCode.BadRequest;
            _Http2CoapCode[HttpStatusCode.Forbidden] = StatusCode.Forbidden;
            _Http2CoapCode[HttpStatusCode.NotFound] = StatusCode.NotFound;
            _Http2CoapCode[HttpStatusCode.MethodNotAllowed] = StatusCode.MethodNotAllowed;
            _Http2CoapCode[HttpStatusCode.NotAcceptable] = StatusCode.NotAcceptable;
            _Http2CoapCode[HttpStatusCode.Gone] = StatusCode.BadRequest;
            _Http2CoapCode[HttpStatusCode.LengthRequired] = StatusCode.BadRequest;
            _Http2CoapCode[HttpStatusCode.PreconditionFailed] = StatusCode.PreconditionFailed;
            _Http2CoapCode[HttpStatusCode.RequestEntityTooLarge] = StatusCode.RequestEntityTooLarge;
            _Http2CoapCode[HttpStatusCode.RequestUriTooLong] = StatusCode.BadRequest;
            _Http2CoapCode[HttpStatusCode.UnsupportedMediaType] = StatusCode.UnsupportedMediaType;
            _Http2CoapCode[HttpStatusCode.RequestedRangeNotSatisfiable] = StatusCode.BadRequest;
            _Http2CoapCode[HttpStatusCode.ExpectationFailed] = StatusCode.BadRequest;
            _Http2CoapCode[HttpStatusCode.InternalServerError] = StatusCode.InternalServerError;
            _Http2CoapCode[HttpStatusCode.NotImplemented] = StatusCode.NotImplemented;
            _Http2CoapCode[HttpStatusCode.BadGateway] = StatusCode.BadGateway;
            _Http2CoapCode[HttpStatusCode.ServiceUnavailable] = StatusCode.ServiceUnavailable;
            _Http2CoapCode[HttpStatusCode.GatewayTimeout] = StatusCode.GatewayTimeout;
            _Http2CoapCode[HttpStatusCode.HttpVersionNotSupported] = StatusCode.BadGateway;
            _Http2CoapCode[(HttpStatusCode) 507] = StatusCode.InternalServerError;

            _Coap2HttpCode[StatusCode.Created] = HttpStatusCode.Created;
            _Coap2HttpCode[StatusCode.Deleted] = HttpStatusCode.NoContent;
            _Coap2HttpCode[StatusCode.Valid] = HttpStatusCode.NotModified;
            _Coap2HttpCode[StatusCode.Changed] = HttpStatusCode.NoContent;
            _Coap2HttpCode[StatusCode.Content] = HttpStatusCode.OK;
            _Coap2HttpCode[StatusCode.BadRequest] = HttpStatusCode.BadRequest;
            _Coap2HttpCode[StatusCode.Unauthorized] = HttpStatusCode.Unauthorized;
            _Coap2HttpCode[StatusCode.BadOption] = HttpStatusCode.BadRequest;
            _Coap2HttpCode[StatusCode.Forbidden] = HttpStatusCode.Forbidden;
            _Coap2HttpCode[StatusCode.NotFound] = HttpStatusCode.NotFound;
            _Coap2HttpCode[StatusCode.MethodNotAllowed] = HttpStatusCode.MethodNotAllowed;
            _Coap2HttpCode[StatusCode.NotAcceptable] = HttpStatusCode.NotAcceptable;
            _Coap2HttpCode[StatusCode.PreconditionFailed] = HttpStatusCode.PreconditionFailed;
            _Coap2HttpCode[StatusCode.RequestEntityTooLarge] = HttpStatusCode.RequestEntityTooLarge;
            _Coap2HttpCode[StatusCode.UnsupportedMediaType] = HttpStatusCode.UnsupportedMediaType;
            _Coap2HttpCode[StatusCode.InternalServerError] = HttpStatusCode.InternalServerError;
            _Coap2HttpCode[StatusCode.NotImplemented] = HttpStatusCode.NotImplemented;
            _Coap2HttpCode[StatusCode.BadGateway] = HttpStatusCode.BadGateway;
            _Coap2HttpCode[StatusCode.ServiceUnavailable] = HttpStatusCode.ServiceUnavailable;
            _Coap2HttpCode[StatusCode.GatewayTimeout] = HttpStatusCode.GatewayTimeout;
            _Coap2HttpCode[StatusCode.ProxyingNotSupported] = HttpStatusCode.BadGateway;

            _Http2CoapMethod["get"] = Method.GET;
            _Http2CoapMethod["post"] = Method.POST;
            _Http2CoapMethod["put"] = Method.PUT;
            _Http2CoapMethod["delete"] = Method.DELETE;
            _Http2CoapMethod["head"] = Method.GET;
            _Http2CoapMethod["patch"] = Method.PATCH;
            // http2coapMethod["fetch"] = Method.FETCH;  Not an HTTP verb
            // http2coapMethod["ipatch"] = Method.iPATCH;  Not an HTTP verb
        }

        /// <summary>
        /// Gets the CoAP response from an incoming HTTP response. No null value is
        /// returned. The response is created from a predefined mapping of the HTTP
        /// response codes. If the code is 204, which has
        /// multiple meaning, the mapping is handled looking on the request method
        /// that has originated the response. The options are set thorugh the HTTP
        /// headers and the option max-age, if not indicated, is set to the default
        /// value (60 seconds). if the response has an enclosing entity, it is mapped
        /// to a CoAP payload and the content-type of the CoAP message is set
        /// properly.
        /// </summary>
        /// <param name="httpResponse">the http response</param>
        /// <param name="coapRequest">the coap response</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="TranslationException"></exception>
        public static Response GetCoapResponse(HttpWebResponse httpResponse, Request coapRequest)
        {
            if (httpResponse == null) throw ThrowHelper.ArgumentNull("httpResponse");
            if (coapRequest == null) throw ThrowHelper.ArgumentNull("coapRequest");

            HttpStatusCode httpCode = httpResponse.StatusCode;
            StatusCode coapCode;

            // the code 204-"no content" should be managed
            // separately because it can be mapped to different coap codes
            // depending on the request that has originated the response
            if (httpCode == HttpStatusCode.NoContent) {
                if (coapRequest.Method == Method.DELETE) coapCode = StatusCode.Deleted;
                else coapCode = StatusCode.Changed;
            }
            else {
                if (!_Http2CoapCode.TryGetValue(httpCode, out coapCode)) throw ThrowHelper.TranslationException("Cannot convert the HTTP status " + httpCode);
            }

            // create the coap reaponse
            Response coapResponse = new Response(coapCode);

            // translate the http headers in coap options
            IEnumerable<Option> coapOptions = GetCoapOptions(httpResponse.Headers);
            coapResponse.SetOptions(coapOptions);

            // the response should indicate a max-age value (CoAP 10.1.1)
            if (!coapResponse.HasOption(OptionType.MaxAge)) {
                // The Max-Age Option for responses to POST, PUT or DELETE requests
                // should always be set to 0 (draft-castellani-core-http-mapping).
                coapResponse.MaxAge = coapRequest.Method == Method.GET ? CoapConstants.DefaultMaxAge : 0;
            }

            Byte[] buffer = new Byte[4096];
            using (Stream ms = new MemoryStream(buffer.Length), dataStream = httpResponse.GetResponseStream()) {
                Int32 read;
                while ((read = dataStream.Read(buffer, 0, buffer.Length)) > 0) {
                    ms.Write(buffer, 0, read);
                }
                Byte[] payload = ((MemoryStream) ms).ToArray();
                if (payload.Length > 0) {
                    coapResponse.Payload = payload;
                    coapResponse.ContentType = GetCoapMediaType(httpResponse.GetResponseHeader("content-type"));
                }
                dataStream.Close();
            }

            return coapResponse;
        }

        /// <summary>
        /// Gets the coap request. Creates the CoAP request from the HTTP method and
        /// mapping it through the properties file. The uri is translated using
        /// regular expressions, the uri format expected is either the embedded
        /// mapping (http://proxyname.domain:80/proxy/coapserver:5683/resource
        /// converted in coap://coapserver:5683/resource) or the standard uri to
        /// indicate a local request not to be forwarded. The method uses a decoder
        /// to translate the application/x-www-form-urlencoded format of the uri. The
        /// CoAP options are set translating the headers. If the HTTP message has an
        /// enclosing entity, it is converted to create the payload of the CoAP
        /// message; finally the content-type is set accordingly to the header and to
        /// the entity type.
        /// </summary>
        /// <param name="httpRequest">the http request</param>
        /// <param name="urlTemplate">Uri Template to match for parameters</param>
        /// <param name="proxyingEnabled">To be killed?</param>
        /// <returns>new CoAP request</returns>
        public static Request GetCoapRequest(IHttpRequest httpRequest, string urlTemplate, Boolean proxyingEnabled)
        {
            if (httpRequest == null) throw ThrowHelper.ArgumentNull("httpRequest");
            if (urlTemplate == null) throw ThrowHelper.ArgumentNull("proxyResource");

            Method coapMethod;
            if (!_Http2CoapMethod.TryGetValue(httpRequest.Method, out coapMethod)) throw ThrowHelper.TranslationException(httpRequest.Method + " method not mapped");

            // create the request
            Request coapRequest = new Request(coapMethod);

            // get the uri
            string uriString = httpRequest.Url;

            OO.UriTemplate template = new OO.UriTemplate(urlTemplate);
            OO.UriTemplateMatch matches = template.Match(new Uri("http://" + httpRequest.Host),  new Uri( uriString));
            if (matches == null) {
                throw ThrowHelper.TranslationException("Template Mismatch");
            }

            //  Check simple first

            if (matches.Bindings.ContainsKey("tu")) {
                uriString = (string) matches.Bindings["tu"].Value;

                // if the uri hasn't the indication of the scheme, add it
                if (!uriString.StartsWith("coap://") &&
                    !uriString.StartsWith("coaps://")) {
                    throw ThrowHelper.TranslationException("Schema is required");
                }


                // the uri will be set as a proxy-uri option
                // set the proxy-uri option to allow the lower layers to underes
                coapRequest.SetOption(Option.Create(OptionType.ProxyUri, uriString));

                // TODO set the proxy as the sender to receive the response correctly
                //coapRequest.PeerAddress = new EndpointAddress(IPAddress.Loopback);
            }
            else {
                throw ThrowHelper.TranslationException("Don't do the complete template patters yet");
            }

            // translate the http headers in coap options
            IEnumerable<Option> coapOptions = GetCoapOptions(httpRequest.Headers);
            coapRequest.SetOptions(coapOptions);

            // the payload
            if (httpRequest.InputStream != null) {
                Byte[] tmp = new Byte[4096];
                MemoryStream ms = new MemoryStream(tmp.Length);
                Int32 read;
                while ((read = httpRequest.InputStream.Read(tmp, 0, tmp.Length)) > 0) {
                    ms.Write(tmp, 0, read);
                }
                coapRequest.Payload = ms.ToArray();
                coapRequest.ContentType = GetCoapMediaType(httpRequest.Headers["content-type"]);
            }

            return coapRequest;
        }

        /// <summary>
        /// Gets the coap media type associated to the http content type. Firstly, it looks
        /// for a predefined mapping. If this step fails, then it
        /// tries to explicitly map/parse the declared mime/type by the http content type.
        /// If even this step fails, it sets application/octet-stream as
        /// content-type.
        /// </summary>
        /// <param name="httpContentTypeString"></param>
        /// <returns></returns>
        public static Int32 GetCoapMediaType(string httpContentTypeString)
        {
            Int32 coapContentType = MediaType.Undefined;

            // check if there is an associated content-type with the current contentType
            if (!String.IsNullOrEmpty(httpContentTypeString)) {
                // delete the last part (if any)
                httpContentTypeString = httpContentTypeString.Split(';')[0];

                // retrieve the mapping
                if (!_Http2CoapMediaType.TryGetValue(httpContentTypeString, out coapContentType))
                    // try to parse the media type
                    coapContentType = MediaType.Parse(httpContentTypeString);
            }

            // if not recognized, the content-type should be
            // application/octet-stream (draft-castellani-core-http-mapping 6.2)
            if (coapContentType == MediaType.Undefined) coapContentType = MediaType.ApplicationOctetStream;

            return coapContentType;
        }

        /// <summary>
        /// Gets the coap options starting from an array of http headers. The
        /// content-type is not handled by this method. The method iterates over an
        /// array of headers and for each of them tries to find a predefined mapping
        /// if the mapping does not exists it skips the header
        /// ignoring it. The method handles separately certain headers which are
        /// translated to options (such as accept or cache-control) whose content
        /// should be semantically checked or requires ad-hoc translation. Otherwise,
        /// the headers content is translated with the appropriate format required by
        /// the mapped option.
        /// </summary>
        /// <param name="headers"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IEnumerable<Option> GetCoapOptions(NameValueCollection headers)
        {
            if (headers == null) throw ThrowHelper.ArgumentNull("headers");

            List<Option> list = new List<Option>();
            foreach (string key in headers.AllKeys) {
                OptionType ot;
                if (!_Http2CoapOption.TryGetValue(key, out ot)) continue;

                // FIXME: CoAP does no longer support multiple accept-options.
                // If an HTTP request contains multiple accepts, this method
                // fails. Therefore, we currently skip accepts at the moment.
                if (ot == OptionType.Accept) continue;

                // ignore the content-type because it will be handled within the payload
                if (ot == OptionType.ContentType) continue;

                string headerValue = headers[key].Trim();
                if (ot == OptionType.Accept) {
                    // if it contains the */* wildcard, no CoAP Accept is set
                    if (!headerValue.Contains("*/*")) {
                        // remove the part where the client express the weight of each
                        // choice
                        headerValue = headerValue.Split(';')[0].Trim();

                        // iterate for each content-type indicated
                        foreach (string headerFragment in headerValue.Split(',')) {
                            // translate the content-type
                            IEnumerable<Int32> coapContentTypes;
                            if (headerFragment.Contains("*")) coapContentTypes = MediaType.ParseWildcard(headerFragment);
                            else coapContentTypes = new Int32[] {MediaType.Parse(headerFragment)};

                            // if is present a conversion for the content-type, then add
                            // a new option
                            foreach (int coapContentType in coapContentTypes) {
                                if (coapContentType != MediaType.Undefined) {
                                    // create the option
                                    Option option = Option.Create(ot, coapContentType);
                                    list.Add(option);
                                }
                            }
                        }
                    }
                }
                else if (ot == OptionType.MaxAge) {
                    int maxAge = 0;
                    if (!headerValue.Contains("no-cache")) {
                        headerValue = headerValue.Split(',')[0];
                        if (headerValue != null) {
                            Int32 index = headerValue.IndexOf('=');
                            if (!Int32.TryParse(headerValue.Substring(index + 1).Trim(), out maxAge)) continue;
                        }
                    }
                    // create the option
                    Option option = Option.Create(ot, maxAge);
                    list.Add(option);
                }
                else {
                    Option option = Option.Create(ot);
                    switch (Option.GetFormatByType(ot)) {
                        case OptionFormat.Integer:
                            option.IntValue = Int32.Parse(headerValue);
                            break;
                        case OptionFormat.Opaque:
                            option.RawValue = ByteArrayUtils.FromHexStream(headerValue);
                            break;
                        case OptionFormat.String:
                        default:
                            option.StringValue = headerValue;
                            break;
                    }
                    list.Add(option);
                }
            }

            return list;
        }

        /// <summary>
        /// Gets the http request starting from a CoAP request. The method creates
        /// the HTTP request through its request line. The request line is built with
        /// the uri coming from the string representing the CoAP method and the uri
        /// obtained from the proxy-uri option. If a payload is provided, the HTTP
        /// request encloses an HTTP entity and consequently the content-type is set.
        /// Finally, the CoAP options are mapped to the HTTP headers.
        /// </summary>
        /// <param name="coapRequest">the coap request</param>
        /// <returns>the http request</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="TranslationException"></exception> 
        public static WebRequest GetHttpRequest(Request coapRequest)
        {
            if (coapRequest == null) throw ThrowHelper.ArgumentNull("coapRequest");

            Uri proxyUri = null;
            try {
                proxyUri = coapRequest.ProxyUri;
            }
            catch (UriFormatException e) {
                throw new TranslationException("Cannot get the proxy-uri from the coap message", e);
            }

            if (proxyUri == null) throw new TranslationException("Cannot get the proxy-uri from the coap message");

            string coapMethod = coapRequest.Method.ToString();

            WebRequest httpRequest = WebRequest.Create(proxyUri);
            httpRequest.Method = coapMethod;

            Byte[] payload = coapRequest.Payload;
            if (payload != null && payload.Length > 0) {
                Int32 coapContentType = coapRequest.ContentType;
                string contentTypeString;

                if (coapContentType == MediaType.Undefined) contentTypeString = "application/octet-stream";
                else {
                    _Coap2HttpContentType.TryGetValue(coapContentType, out contentTypeString);
                    if (string.IsNullOrEmpty(contentTypeString)) {
                        contentTypeString = MediaType.ToString(coapContentType);
                    }
                }

                httpRequest.ContentType = contentTypeString;

                Stream dataStream = httpRequest.GetRequestStream();
                dataStream.Write(payload, 0, payload.Length);
                dataStream.Close();
            }

            NameValueCollection headers = GetHttpHeaders(coapRequest.GetOptions());
            foreach (string key in headers.AllKeys) {
                httpRequest.Headers[key] = headers[key];
            }

            return httpRequest;
        }

        /// <summary>
        /// Gets the http headers from a list of CoAP options. The method iterates
        /// over the list looking for a translation of each option in the predefined
        /// mapping. This process ignores the proxy-uri and the content-type because
        /// they are managed differently. If a mapping is present, the content of the
        /// option is mapped to a string accordingly to its original format and set
        /// as the content of the header.
        /// </summary>
        /// <param name="optionList"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static NameValueCollection GetHttpHeaders(IEnumerable<Option> optionList)
        {
            if (optionList == null) throw ThrowHelper.ArgumentNull("optionList");

            NameValueCollection headers = new NameValueCollection();

            foreach (Option opt in optionList) {
                // skip content-type because it should be translated while handling
                // the payload; skip proxy-uri because it has to be translated in a
                // different way
                if (opt.Type == OptionType.ContentType || opt.Type == OptionType.ProxyUri) continue;

                string headerName;
                if (!_Coap2HttpHeader.TryGetValue(opt.Type, out headerName)) continue;

                // format the value
                string headerValue = null;
                OptionFormat format = Option.GetFormatByType(opt.Type);
                if (format == OptionFormat.Integer) headerValue = opt.IntValue.ToString();
                else if (format == OptionFormat.String) headerValue = opt.StringValue;
                else if (format == OptionFormat.Opaque) headerValue = ByteArrayUtils.ToHexString(opt.RawValue);
                else continue;

                // custom handling for max-age
                // format: cache-control: max-age=60
                if (opt.Type == OptionType.MaxAge) headerValue = "max-age=" + headerValue;
                headers[headerName] = headerValue;
            }

            return headers;
        }

        /// <summary>
        /// Sets the parameters of the incoming http response from a CoAP response.
        /// The status code is mapped through the properties file and is set through
        /// the StatusLine. The options are translated to the corresponding headers
        /// and the max-age (in the header cache-control) is set to the default value
        /// (60 seconds) if not already present. If the request method was not HEAD
        /// and the coap response has a payload, the entity and the content-type are
        /// set in the http response.
        /// </summary>
        public static void GetHttpResponse(IHttpRequest httpRequest, Response coapResponse, IHttpResponse httpResponse)
        {
            if (httpRequest == null) throw ThrowHelper.ArgumentNull("httpRequest");
            if (coapResponse == null) throw ThrowHelper.ArgumentNull("coapResponse");
            if (httpResponse == null) throw ThrowHelper.ArgumentNull("httpResponse");

            HttpStatusCode httpCode;

            if (!_Coap2HttpCode.TryGetValue(coapResponse.StatusCode, out httpCode)) throw ThrowHelper.TranslationException("Cannot convert the coap code in http status code: " + coapResponse.StatusCode);

            httpResponse.StatusCode = (Int32) httpCode;

            NameValueCollection nvc = GetHttpHeaders(coapResponse.GetOptions());
            // set max-age if not already set
            if (nvc["cache-control"] == null) nvc.Set("cache-control", "max-age=" + CoapConstants.DefaultMaxAge);

            foreach (string key in nvc.Keys) {
                httpResponse.AppendHeader(key, nvc[key]);
            }

            Byte[] payload = coapResponse.Payload;
            if (payload != null) {
                httpResponse.OutputStream.Write(payload, 0, payload.Length);
                String contentType;
                if (_Coap2HttpContentType.TryGetValue(coapResponse.ContentType, out contentType)) httpResponse.AppendHeader("content-type", contentType);
            }
        }
    }
}
