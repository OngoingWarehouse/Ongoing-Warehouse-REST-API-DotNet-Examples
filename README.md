<a href="https://www.ongoingwarehouse.com">![Logo](https://ongoingwarehouse.com/images/ongoing_logo_10k_blue.png)</a>
# Ongoing Warehouse .NET integration examples using the REST API
[Ongoing Warehouse](https://www.ongoingwarehouse.com/) is a Warehouse Management System (WMS).

We provide several [Application Programming Interfaces (APIs)](https://developer.ongoingwarehouse.com/). One of them is [based on REST](https://developer.ongoingwarehouse.com/REST/v1/index.html). In this repository, we provide examples on how you can access our REST API using C#.

The project is a small command-line program which uses our REST API to do the following:
* Create and update articles (including attaching files to articles).
* Create and update orders (including attaching files to orders).
* Create and update purchase orders.
* Read inventory adjustments.
* Read transporter contracts (shipping methods).

We provide an [OpenAPI specification file](https://developer.ongoingwarehouse.com/REST/v1/openapi.json) for our API. This is a machine-readable specification of the API, which allows you to automatically generate client code for your own programming language.  In this example, we used [NSwagStudio](https://github.com/RicoSuter/NSwag/wiki/NSwagStudio) to generate a client for C#.

If you want to use our SOAP API instead, [please take a look at these examples instead](https://github.com/HenrikOngoing/Ongoing-Warehouse-SDK).