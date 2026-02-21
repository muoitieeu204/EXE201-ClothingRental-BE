using AutoMapper;
using EXE201.Repository.Interfaces;
using EXE201.Repository.Models;
using EXE201.Service.DTOs.LoyaltyTransactionDTOs;
using EXE201.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.Implementation
{
    public class LoyaltyTransactionService : ILoyaltyTransactionService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public LoyaltyTransactionService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        // GET ALL (admin) - trả FullName
        public async Task<IEnumerable<LoyaltyTransactionDto>> GetAllAsync()
        {
            var entities = await _uow.LoyaltyTransactions.GetAllAsync();
            return await MapWithFullNameAsync(entities);
        }

        // GET ALL /me - chỉ theo userId từ token (controller lấy token -> userId -> gọi service)
        public async Task<IEnumerable<LoyaltyTransactionDto>> GetAllMyAsync(int userId)
        {
            if (userId <= 0) return Enumerable.Empty<LoyaltyTransactionDto>();

            var entities = await _uow.LoyaltyTransactions.FindAsync(t => t.UserId == userId);
            return await MapWithFullNameAsync(entities);
        }

        // GET BY ID
        public async Task<LoyaltyTransactionDto?> GetByIdAsync(int transactionId)
        {
            if (transactionId <= 0) return null;

            var entity = await _uow.LoyaltyTransactions.GetByIdAsync(transactionId);
            if (entity == null) return null;

            var dto = _mapper.Map<LoyaltyTransactionDto>(entity);

            // Fill FullName (vì entity.User có thể null nếu không Include)
            var user = await _uow.Users.GetByIdAsync(entity.UserId);
            dto.FullName = user?.FullName;

            return dto;
        }

        // CREATE
        public async Task<LoyaltyTransactionDto?> CreateAsync(CreateLoyaltyTransactionDto dto)
        {
            if (dto == null) return null;
            if (dto.UserId <= 0) return null;

            var type = dto.TransactionType?.Trim();
            if (string.IsNullOrWhiteSpace(type)) return null;

            // Check user tồn tại
            var user = await _uow.Users.GetByIdAsync(dto.UserId);
            if (user == null) return null;

            var entity = _mapper.Map<LoyaltyTransaction>(dto);
            entity.TransactionId = 0;                 // identity
            entity.CreatedAt = DateTime.UtcNow;       // service tự set
            entity.TransactionType = type;

            await _uow.LoyaltyTransactions.AddAsync(entity);
            await _uow.SaveChangesAsync();

            // Return DTO (kèm FullName)
            var result = _mapper.Map<LoyaltyTransactionDto>(entity);
            result.FullName = user.FullName;
            return result;
        }

        // UPDATE
        public async Task<LoyaltyTransactionDto?> UpdateAsync(int transactionId, UpdateLoyaltyTransactionDto dto)
        {
            if (transactionId <= 0) return null;
            if (dto == null) return null;

            var entity = await _uow.LoyaltyTransactions.GetByIdAsync(transactionId);
            if (entity == null) return null;

            // Không cho update kiểu gửi rỗng
            var hasAnyField =
                dto.PointsAmount.HasValue ||
                dto.TransactionType != null ||
                dto.Description != null;

            if (!hasAnyField) return null;

            if (dto.PointsAmount.HasValue)
                entity.PointsAmount = dto.PointsAmount.Value;

            if (dto.TransactionType != null)
            {
                var type = dto.TransactionType.Trim();
                if (string.IsNullOrWhiteSpace(type)) return null;
                entity.TransactionType = type;
            }

            if (dto.Description != null)
                entity.Description = dto.Description;

            await _uow.LoyaltyTransactions.UpdateAsync(entity);
            await _uow.SaveChangesAsync();

            var result = _mapper.Map<LoyaltyTransactionDto>(entity);

            // Fill FullName
            var user = await _uow.Users.GetByIdAsync(entity.UserId);
            result.FullName = user?.FullName;

            return result;
        }

        // DELETE
        public async Task<bool> DeleteAsync(int transactionId)
        {
            if (transactionId <= 0) return false;

            var entity = await _uow.LoyaltyTransactions.GetByIdAsync(transactionId);
            if (entity == null) return false;

            await _uow.LoyaltyTransactions.DeleteAsync(entity);
            await _uow.SaveChangesAsync();
            return true;
        }

        // ===== Helper: map list + fill FullName theo batch (tránh N+1) =====
        private async Task<IEnumerable<LoyaltyTransactionDto>> MapWithFullNameAsync(IEnumerable<LoyaltyTransaction> entities)
        {
            var list = entities?.ToList() ?? new List<LoyaltyTransaction>();
            if (list.Count == 0) return Enumerable.Empty<LoyaltyTransactionDto>();

            var dtos = _mapper.Map<List<LoyaltyTransactionDto>>(list);

            var userIds = list.Select(x => x.UserId).Distinct().ToList();
            var users = await _uow.Users.FindAsync(u => userIds.Contains(u.UserId));
            var dict = users.ToDictionary(u => u.UserId, u => u.FullName);

            for (int i = 0; i < list.Count; i++)
            {
                var uid = list[i].UserId;
                dtos[i].FullName = dict.TryGetValue(uid, out var name) ? name : null;
            }

            return dtos;
        }
    }

}
