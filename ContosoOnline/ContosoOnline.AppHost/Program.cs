var builder = DistributedApplication.CreateBuilder(args);

var ordersDb = builder.AddPostgres("ordersDb");

var products = builder.AddProject<Projects.Products>("products");

var ordersApi = builder.AddProject<Projects.Orders>("orders")
                         .WithReference(ordersDb);

builder.AddProject<Projects.Store>("store")
                   .WithReference(products)
                   .WithReference(ordersApi);

builder.AddProject<Projects.OrderProcessor>("orderprocessor")
       .WithReference(ordersApi)
       .WithReference(products);

builder.Build().Run();
