using System;
using Com.AugustCellars.CoAP.Net;
using Com.AugustCellars.CoAP.Server.Resources;
using Com.AugustCellars.CoAP.Proxy.Resources;

namespace Com.AugustCellars.CoAP.Proxy
{
    public class ProxyRootResource : Resource
    {
        private readonly ProxyCoapClientResource _coapProxy;
        private readonly ProxyHttpClientResource _httpProxy;


        public ProxyRootResource(ProxyCoapClientResource coapProxy, ProxyHttpClientResource httpProxy) : base("proxyRoot")
        {
            _coapProxy = coapProxy;
            _httpProxy = httpProxy;
        }

        public override void HandleRequest(Exchange exchange)
        {
            Request req = exchange.Request;

            if (!req.HasOption(OptionType.ProxyUri)) {
                exchange.SendResponse(new Response(StatusCode.BadOption));
                return;
            }

            Uri uri = req.ProxyUri;
            switch (uri.Scheme) {
                case "coap":
                case "coaps":
                    if (_coapProxy == null) {
                        exchange.SendResponse(new Response(StatusCode.BadGateway));
                    }
                    else _coapProxy.HandleRequest(exchange);
                    return;

                case "http":
                case "https":
                    if (_httpProxy == null) {
                        exchange.SendResponse(new Response(StatusCode.BadGateway));
                    }
                    else _httpProxy.HandleRequest(exchange);
                    return;
            }

            exchange.SendResponse(new Response(StatusCode.BadOption));
        }
    }
}
