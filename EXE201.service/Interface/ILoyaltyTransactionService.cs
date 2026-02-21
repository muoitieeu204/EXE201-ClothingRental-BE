using EXE201.Service.DTOs.LoyaltyTransactionDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.Interface
{
    public interface ILoyaltyTransactionService
    {
        Task<IEnumerable<LoyaltyTransactionDto>> GetAllAsync();
        Task<IEnumerable<LoyaltyTransactionDto>> GetAllMyAsync(int userId);
        Task<LoyaltyTransactionDto?> GetByIdAsync(int transactionId);

        Task<LoyaltyTransactionDto?> CreateAsync(CreateLoyaltyTransactionDto dto);
        Task<LoyaltyTransactionDto?> UpdateAsync(int transactionId, UpdateLoyaltyTransactionDto dto);
        Task<bool> DeleteAsync(int transactionId);
    }
}
