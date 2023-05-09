using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Products
{
    public class ProductsGrpcService : Products.ProductsBase
    {
        private readonly IProductService _productService;

        public ProductsGrpcService(IProductService productService, ILogger<ProductsGrpcService> logger)
        {
            _productService = productService;
        }

        public override async Task<CheckProductInventoryResponse> CheckProductInventory(CheckProductInventoryRequest request, ServerCallContext context)
            => await _productService.CheckProductInventoryAsync(request.ProductId, request.ItemsRequested)
                .ContinueWith(task => new CheckProductInventoryResponse { IsEnoughAvailable = task.Result });

        public override async Task<GetProductsResponse> GetProducts(Empty request, ServerCallContext context)
            => await _productService.GetProductsAsync()
                .ContinueWith(task => new GetProductsResponse { Products = { task.Result } });

        public override async Task<UpdateProductInventoryResponse> UpdateProductInventory(InventoryUpdateRequest request, ServerCallContext context)
            => await _productService.SubtractInventory(request.ProductId, request.ItemsRequested)
                .ContinueWith(task => new UpdateProductInventoryResponse { InventoryUpdated = task.Result });
    }
}
