# Coap-Proxy

[![NuGet Status](https://img.shields.io/nuget/v/Com.AugustCellars.CoAP.Proxy.png)](https://www.nuget.org/packages/Com.AugustCellars.CoAP.Proxy)
[![Build Status](https://api.travis-ci.org/jimsch/CoAP-Proxy.png)](https://travis-ci.org/jimsch/CoAP-Proxy)

The Constrained Application Protocol (CoAP) (https://datatracker.ietf.org/doc/draft-ietf-core-coap/)
is a RESTful web transfer protocol for resource-constrained networks and nodes.
CoAP.NET is an implementation in C# providing CoAP-based services to .NET applications. 
Reviews and suggestions would be appreciated.

Proxy code for CoAP to deal with CoAP-CoAP, CoAP-HTTP and HTTP-CoAP boundaries

## Copyright

Original Code:
Copyright (c) 2011-2015, Longxiang He <longxianghe@gmail.com>,
SmeshLink Technology Co.

Deltas since then:
Yeah, I really ought to.

## Content

- [Quick Start](#quick-start)
- [Build](#build)
- [License](#license)
- [Acknowledgements](#acknowledgements)

## How to Install

The C# implementation is available in the NuGet Package Gallery under the name [Com.AugustCellars.CoAP](https://www.nuget.org/packages/Com.AugustCellars.CoAP).
To install this library as a NuGet package, enter 'Install-Package Com.AugustCellars.CoAP' in the NuGet Package Manager Console.

## Documentation

Documentation can be found in two places.
First an XML file is installed as part of the package for inline documentation.
Additionally, I have started working on the [Wiki](https://github.com/jimsch/CoAP-CSharp/wiki) associated with this project.

## Quick Start

There may not really be one.

## Building the sources

I am currently sync-ed up to Visual Studio 2017 and have started using language features of C# v7.0 that are supported both in Visual Studio and in the latest version of mono.

## License

See [LICENSE](LICENSE) for more info.

## Acknowledgements

This is a copy of the CoAP.NET project hosted at (https://http://coap.codeplex.com/).
As this project does not seem to be maintained anymore, and I am doing active updates to it, I have made a local copy that things are going to move forward on.

Current projects are:

- [HTTP->CoAP Proxy]{https://tools.ietf.org/html/rfc8075} - Guidelines for an HTTP->CoAP Proxy


This code base is derived from the CoAP.NET project done by Longxiang He at SmeshLink Technology Co.
CoAP.NET was based on [**Californium**](https://github.com/mkovatsc/Californium),
a CoAP framework in Java by Matthias Kovatsch, Dominique Im Obersteg,
and Daniel Pauli, ETH Zurich. See <http://people.inf.ethz.ch/mkovatsc/californium.php>.
Thanks to the authors and their great job.

