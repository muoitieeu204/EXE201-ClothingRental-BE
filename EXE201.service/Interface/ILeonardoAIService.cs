using EXE201.Service.DTOs.LeonardoAIDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.Interface
{
    public interface ILeonardoAIService
    {
        Task<string?> GenerateVideoAsync(GenerateRequestVideoDTO requestVideoDTO);
        Task<(bool isComplete, string? videoUrl, string status)> GetVideoStatusAsync(string generationId);
        Task<string?> WaitForVideoCompletionAsync(string generationId, int maxWaitSeconds = 180);
    }
}
