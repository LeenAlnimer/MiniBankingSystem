using MiniBankingSystem.Application.DTOs;

namespace MiniBankingSystem.Application.Interfaces;

public interface ICustomerService
{
    Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto);

    Task<List<CustomerDto>> GetCustomersAsync();

    Task<CustomerDto?> GetCustomerByIdAsync(Guid id);

    Task<bool> UpdateCustomerAsync(Guid id, CreateCustomerDto dto);

    Task<bool> DeleteCustomerAsync(Guid id);
}