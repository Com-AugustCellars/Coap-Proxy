using System;
using Com.AugustCellars.CoAP.Proxy;
using Com.AugustCellars.CoAP.Proxy.Resources;
using Com.AugustCellars.CoAP.Server;
using Com.AugustCellars.CoAP.Server.Resources;

namespace Com.AugustCellars.CoAP.Examples
{
    public class Program
    {
        static void Main(string[] args)
        {
            ProxyCoapClientResource coap2Coap = new ProxyCoapClientResource("coap2Coap");
            ProxyHttpClientResource coap2Http = new ProxyHttpClientResource("coap2Http");

            ProxyRootResource root = new ProxyRootResource(coap2Coap, coap2Http);

            // Create CoAP Server on PORT with proxy resources form CoAP to CoAP and HTTP
            CoapServer coapServer = new CoapServer(null, root, CoapConfig.Default.DefaultPort+2);
            coapServer.Add(new TargetResource("target"));
            coapServer.Start();

            ProxyHttpServer httpServer = new ProxyHttpServer(CoapConfig.Default.HttpPort) {
                ProxyCoapResolver = new DirectProxyCoAPResolver(coap2Coap)
            };

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        private class TargetResource : Resource
        {
            private int _counter;

            public TargetResource(string name)
                : base(name)
            { }

            protected override void DoGet(CoapExchange exchange)
            {
                exchange.Respond("Response " + (++_counter) + " from resource " + Name);
            }
        }
    }
}
