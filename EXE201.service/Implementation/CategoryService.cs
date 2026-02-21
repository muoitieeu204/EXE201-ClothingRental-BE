using AutoMapper;
using EXE201.Repository.Interfaces;
using EXE201.Repository.Models;
using EXE201.Service.DTOs.CategoryDTOs;
using EXE201.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.Implementation
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public CategoryService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllAsync()
        {
            var categories = await _uow.Categories.GetAllAsync();
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<CategoryDto?> GetByIdAsync(int id)
        {
            if (id <= 0) return null;

            var category = await _uow.Categories.GetByIdAsync(id);
            if (category == null) return null;

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<CategoryDto?> GetByNameAsync(string categoryName)
        {
            var name = categoryName?.Trim();
            if (string.IsNullOrWhiteSpace(name)) return null;

            var category = await _uow.Categories.GetCategoryByNameAsync(name);
            if (category == null) return null;

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<CategoryDto?> CreateAsync(CreateCategoryDto dto)
        {
            if (dto == null) return null;

            var name = dto.CategoryName?.Trim();
            if (string.IsNullOrWhiteSpace(name)) return null;

            // Check trùng tên
            var existed = await _uow.Categories.GetCategoryByNameAsync(name);
            if (existed != null) return null;

            var entity = _mapper.Map<Category>(dto);
            entity.CategoryName = name;

            await _uow.Categories.AddAsync(entity);
            await _uow.SaveChangesAsync();

            return _mapper.Map<CategoryDto>(entity);
        }

        public async Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryDto dto)
        {
            if (id <= 0) return null;
            if (dto == null) return null;

            var category = await _uow.Categories.GetByIdAsync(id);
            if (category == null) return null;

            // Update Name (nếu có truyền)
            if (dto.CategoryName != null)
            {
                var newName = dto.CategoryName.Trim();
                if (string.IsNullOrWhiteSpace(newName)) return null;

                // Nếu đổi tên thì check trùng
                if (!string.Equals(category.CategoryName, newName, StringComparison.OrdinalIgnoreCase))
                {
                    var existed = await _uow.Categories.GetCategoryByNameAsync(newName);
                    if (existed != null) return null;
                }

                category.CategoryName = newName;
            }

            // Update Description (nếu có truyền)
            if (dto.Description != null)
            {
                category.Description = dto.Description;
            }

            await _uow.Categories.UpdateAsync(category);
            await _uow.SaveChangesAsync();

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0) return false;

            var category = await _uow.Categories.GetByIdAsync(id);
            if (category == null) return false;

            await _uow.Categories.DeleteAsync(category); // nếu repo bạn tên khác (Remove/DeleteById) thì đổi đúng 1 dòng này
            await _uow.SaveChangesAsync();
            return true;
        }
    }

}
