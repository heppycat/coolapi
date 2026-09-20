using Application.DTOs;
using Application.Interfaces;
using Core.Entities;
using FluentValidation;

namespace Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly IValidator<CreateProductDto> _createValidator;

    public ProductService(
        IProductRepository repository,
        IValidator<CreateProductDto> createValidator
    )
    {
        _repository = repository;
        _createValidator = createValidator;
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        //fetch data from db (repo)
        var products = await _repository.GetAllAsync();

        //business logic later ...

        return products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
        });
    }

    public async Task<ProductDto> GetProductByIdAsync(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        return product == null
            ? null!
            : new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
            };
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var product = new Product { Name = dto.Name, Price = dto.Price };

        await _repository.AddAsync(product);

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
        };
    }

    public async Task UpdateProductAsync(int id, UpdateProductDto dto)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null)
            throw new KeyNotFoundException("Product not found.");

        product.Name = dto.Name;
        product.Price = dto.Price;
        await _repository.UpdateAsync(product);
    }

    public async Task DeleteProductAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }
}
