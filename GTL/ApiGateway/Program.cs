using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromMemory(
        new[]
        {
            new RouteConfig
            {
                RouteId = "catalog",
                ClusterId = "catalog-cluster",
                Match = new RouteMatch { Path = "/api/catalog/{**catch-all}" }
            },
            new RouteConfig
            {
                RouteId = "search",
                ClusterId = "search-cluster",
                Match = new RouteMatch { Path = "/api/search/{**catch-all}" }
            }
        },
        new[]
        {
            new ClusterConfig
            {
                ClusterId = "catalog-cluster",
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    ["catalog"] = new() { Address = "http://localhost:5001" } // CatalogService URL
                }
            },
            new ClusterConfig
            {
                ClusterId = "search-cluster",
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    ["search"] = new() { Address = "http://localhost:5002" } // SearchService URL
                }
            }
        });

var app = builder.Build();
app.MapReverseProxy();
app.Run();