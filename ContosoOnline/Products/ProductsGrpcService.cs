using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Products;

public class ProductsGrpcService(IProductService productService) : Products.ProductsBase
{
    public override async Task<GetProductsResponse> GetProducts(Empty request, ServerCallContext context)
    {
        var products = await productService.GetProductsAsync();
        return new GetProductsResponse { Products = { products } };
    }

    public override async Task<CheckProductInventoryResponse> CheckProductInventory(CheckProductInventoryRequest request, ServerCallContext context)
    {
        var inventory = await productService.CheckProductInventoryAsync(request.ProductId, request.ItemsRequested);
        return new CheckProductInventoryResponse { IsEnoughAvailable = inventory };
    }

    public override async Task<InventorySubtractionResponse> SubtractInventory(InventorySubtractionRequest request, ServerCallContext context)
    {
        var result = await productService.SubtractInventory(request.ProductId, request.ItemsRequested);
        return new InventorySubtractionResponse { InventoryUpdated = result };
    }
}
