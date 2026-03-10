using EXE201.Service.DTOs.ChatbotDTOs;

namespace EXE201.Service.Interface
{
    public interface IChatbotService
    {
        Task<ChatMessageResponseDto> GetChatResponseAsync(string userMessage, List<ChatHistoryItemDto>? history = null);
    }
}
