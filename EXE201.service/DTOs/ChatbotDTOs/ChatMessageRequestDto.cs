namespace EXE201.Service.DTOs.ChatbotDTOs
{
    public class ChatMessageRequestDto
    {
        public string Message { get; set; } = string.Empty;
        /// <summary>
        /// Lịch sử chat trước đó. FE tự lưu và gửi kèm mỗi request.
        /// Role: "user" hoặc "model"
        /// </summary>
        public List<ChatHistoryItemDto>? History { get; set; }
    }

    public class ChatHistoryItemDto
    {
        public string Role { get; set; } = string.Empty;    // "user" | "model"
        public string Content { get; set; } = string.Empty;
    }
}
