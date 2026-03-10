using EXE201.Repository.Interfaces;
using EXE201.Service.DTOs.ChatbotDTOs;
using EXE201.Service.Interface;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace EXE201.Service.Implementation
{
    public class ChatbotService : IChatbotService
    {
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;

        public ChatbotService(IConfiguration configuration, IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;
        }

        public async Task<ChatMessageResponseDto> GetChatResponseAsync(string userMessage, List<ChatHistoryItemDto>? history = null)
        {
            var apiKey = _configuration["GeminiAPI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_GOOGLE_AI_API_KEY")
            {
                return new ChatMessageResponseDto
                {
                    Response = "Xin lỗi, chức năng chatbot chưa được cấu hình. Vui lòng liên hệ quản trị viên."
                };
            }

            var client = new Client(apiKey: apiKey);

            // Bước 1: Dùng Gemini trích xuất tiêu chí tìm kiếm từ tin nhắn
            var criteria = await ExtractSearchCriteriaAsync(client, userMessage);

            // Bước 2: Tìm outfit phù hợp trong DB
            var outfits = await SearchOutfitsAsync(criteria);

            // Bước 3: Dùng Gemini tạo câu trả lời tư vấn (có kèm history)
            var response = await GenerateConsultationAsync(client, userMessage, outfits, history);

            return new ChatMessageResponseDto
            {
                Response = response,
                OutfitSuggestions = outfits.Select(o => new OutfitSuggestionDto
                {
                    OutfitId = o.OutfitId,
                    Name = o.Name,
                    Description = null, // OutfitAttribute không được load cùng query này
                    RentalPrice = o.BaseRentalPrice,
                    Type = o.Type,
                    Gender = o.Gender,
                    Region = o.Region,
                    ImageUrl = o.OutfitImages
                        .OrderBy(img => img.SortOrder ?? int.MaxValue)
                        .FirstOrDefault()?.ImageUrl
                }).ToList()
            };
        }

        private async Task<OutfitSearchCriteria> ExtractSearchCriteriaAsync(Client client, string userMessage)
        {
            var systemPrompt = """
Bạn là AI chuyên phân tích yêu cầu của khách hàng về thuê trang phục.

Nhiệm vụ: Đọc tin nhắn và trích xuất thông tin dưới dạng JSON:
{
  "queryType": "loại yêu cầu: 'greeting' nếu chỉ chào hỏi, 'best_sellers' nếu hỏi về bán chạy/phổ biến/hot/trending/được thuê nhiều, 'cheapest' nếu hỏi về rẻ nhất/giá thấp nhất/đời rẻ/giá rẻ, 'most_expensive' nếu hỏi về đắt nhất/giá cao nhất/cao cấp nhất, 'search' cho các trường hợp còn lại",
  "keywords": ["từ khoá về loại trang phục: áo dài, vest, váy cưới, hanbok, kimono..."],
  "type": "loại trang phục (traditional, modern, wedding, casual...)",
  "gender": "giới tính (male, female, unisex...)",
  "region": "vùng/quốc gia xuất xứ (Vietnamese, Korean, Japanese, Chinese...)",
  "occasion": "dịp mặc (wedding, graduation, festival, party...)",
  "priceRange": "giá thuê tối đa dạng số (VD: 500000)"
}

Nếu không có thông tin thì để rỗng [] hoặc "".
Chỉ trả về JSON hợp lệ, không thêm văn bản nào khác.
""";

            var config = new GenerateContentConfig
            {
                SystemInstruction = new Content
                {
                    Parts = new List<Part> { new Part { Text = systemPrompt } }
                },
                Temperature = 0.1f
            };

            try
            {
                var response = await client.Models.GenerateContentAsync(
                    model: "gemini-2.5-flash-lite",
                    contents: userMessage,
                    config: config);

                var jsonText = response?.Candidates?[0]?.Content?.Parts?[0]?.Text?.Trim();
                if (string.IsNullOrEmpty(jsonText))
                    return new OutfitSearchCriteria();

                // Xử lý trường hợp Gemini bọc JSON trong markdown code block
                if (jsonText.StartsWith("```"))
                {
                    var start = jsonText.IndexOf('{');
                    var end = jsonText.LastIndexOf('}') + 1;
                    if (start >= 0 && end > start)
                        jsonText = jsonText[start..end];
                }

                return JsonSerializer.Deserialize<OutfitSearchCriteria>(jsonText)
                    ?? new OutfitSearchCriteria();
            }
            catch
            {
                return new OutfitSearchCriteria();
            }
        }

        private async Task<List<Repository.Models.Outfit>> SearchOutfitsAsync(OutfitSearchCriteria criteria)
        {
            // Dùng GetAvailableOutfitsAsync vì nó đã include OutfitImages
            var allOutfits = await _unitOfWork.Outfits.GetAvailableOutfitsAsync();

            // Nếu hỏi bán chạy: trả về top 5 theo số lần booking
            if (criteria.QueryType == "best_sellers")
                return await GetBestSellersAsync(allOutfits);

            // Nếu hỏi rẻ nhất: sắp xếp theo giá tăng dần
            if (criteria.QueryType == "cheapest")
                return allOutfits.OrderBy(o => o.BaseRentalPrice).Take(5).ToList();

            // Nếu hỏi đắt nhất: sắp xếp theo giá giảm dần
            if (criteria.QueryType == "most_expensive")
                return allOutfits.OrderByDescending(o => o.BaseRentalPrice).Take(5).ToList();

            var query = allOutfits.AsEnumerable();

            // Lọc theo keywords (tìm trong Name)
            if (criteria.Keywords.Count > 0)
            {
                var keywords = criteria.Keywords
                    .Select(k => k.Trim().ToLowerInvariant())
                    .Where(k => !string.IsNullOrEmpty(k))
                    .ToList();

                if (keywords.Count > 0)
                    query = query.Where(o =>
                        keywords.Any(kw => o.Name.ToLowerInvariant().Contains(kw)));
            }

            // Lọc theo giới tính
            if (!string.IsNullOrWhiteSpace(criteria.Gender))
            {
                var gender = criteria.Gender.Trim().ToLowerInvariant();
                query = query.Where(o =>
                    o.Gender != null && o.Gender.ToLowerInvariant().Contains(gender));
            }

            // Lọc theo vùng/xuất xứ
            if (!string.IsNullOrWhiteSpace(criteria.Region))
            {
                var region = criteria.Region.Trim().ToLowerInvariant();
                query = query.Where(o =>
                    o.Region != null && o.Region.ToLowerInvariant().Contains(region));
            }

            // Lọc theo loại trang phục
            if (!string.IsNullOrWhiteSpace(criteria.Type))
            {
                var type = criteria.Type.Trim().ToLowerInvariant();
                query = query.Where(o =>
                    o.Type != null && o.Type.ToLowerInvariant().Contains(type));
            }

            // Lọc theo giá thuê
            if (!string.IsNullOrWhiteSpace(criteria.PriceRange))
            {
                var priceStr = criteria.PriceRange
                    .Replace("k", "000").Replace("K", "000")
                    .Replace("triệu", "000000").Replace("tr", "000000");
                if (decimal.TryParse(priceStr.Replace(",", "").Replace(".", ""), out var maxPrice) && maxPrice > 0)
                    query = query.Where(o => o.BaseRentalPrice <= maxPrice);
            }

            return query.Take(5).ToList();
        }

        private async Task<List<Repository.Models.Outfit>> GetBestSellersAsync(
            IEnumerable<Repository.Models.Outfit> availableOutfits)
        {
            // Đếm số lần booking theo OutfitSizeId → OutfitId
            var bookingDetails = await _unitOfWork.BookingDetails.GetAllAsync();
            var outfitSizes    = await _unitOfWork.OutfitSizes.GetAllAsync();

            // Map SizeId → OutfitId
            var sizeToOutfit = outfitSizes.ToDictionary(s => s.SizeId, s => s.OutfitId);

            // Count bookings per OutfitId
            var bookingCount = bookingDetails
                .Where(bd => sizeToOutfit.ContainsKey(bd.OutfitSizeId))
                .GroupBy(bd => sizeToOutfit[bd.OutfitSizeId])
                .ToDictionary(g => g.Key, g => g.Count());

            // Sort available outfits by booking count desc
            return availableOutfits
                .OrderByDescending(o => bookingCount.GetValueOrDefault(o.OutfitId, 0))
                .Take(5)
                .ToList();
        }

        private async Task<string> GenerateConsultationAsync(
            Client client,
            string userMessage,
            List<Repository.Models.Outfit> outfits,
            List<ChatHistoryItemDto>? history = null)
        {
            var outfitsContext = outfits.Count > 0
                ? string.Join("\n", outfits.Select((o, i) =>
                    $"- [{i + 1}] {o.Name} | Loại: {o.Type ?? "N/A"} | Giới tính: {o.Gender ?? "N/A"} | Vùng: {o.Region ?? "N/A"} | Giá thuê: {o.BaseRentalPrice:N0} VNĐ"))
                : "Không tìm thấy trang phục phù hợp trong kho.";

            var systemPrompt = """
Bạn là nhân viên tư vấn thân thiện của Clothing Rental - dịch vụ cho thuê trang phục.

Nhiệm vụ: Dựa trên yêu cầu của khách hàng và danh sách trang phục tìm được, hãy phản hồi phù hợp.

Quy tắc:
- Trả lời bằng tiếng Việt, thân thiện và nhiệt tình.
- Nếu khách hàng chỉ chào hỏi (hello, xin chào, hi...): hãy chào lại và giới thiệu bạn có thể giúp gì (tư vấn trang phục thuê).
- Nếu khách hỏi về trang phục và có kết quả: giới thiệu 1-3 trang phục nổi bật và giải thích lý do phù hợp.
- Nếu khách hỏi về trang phục nhưng không có kết quả: gợi ý khách mô tả chi tiết hơn hoặc liên hệ hotline.
- Giữ câu trả lời ngắn gọn, dễ đọc (không quá 4 câu).
""";

            var currentTurnContent = $"""
Yêu cầu khách hàng: "{userMessage}"

Danh sách trang phục tìm được:
{outfitsContext}

Hãy tư vấn cho khách hàng.
""";

            // Xây dựng danh sách nội dung: history + tin nhắn hiện tại
            var contents = new List<Content>();
            if (history != null)
            {
                foreach (var item in history)
                {
                    var role = item.Role == "model" ? "model" : "user";
                    contents.Add(new Content
                    {
                        Role = role,
                        Parts = new List<Part> { new Part { Text = item.Content } }
                    });
                }
            }
            contents.Add(new Content
            {
                Role = "user",
                Parts = new List<Part> { new Part { Text = currentTurnContent } }
            });

            var config = new GenerateContentConfig
            {
                SystemInstruction = new Content
                {
                    Parts = new List<Part> { new Part { Text = systemPrompt } }
                },
                Temperature = 0.7f,
                MaxOutputTokens = 512
            };

            try
            {
                var response = await client.Models.GenerateContentAsync(
                    model: "gemini-2.5-flash-lite",
                    contents: contents,
                    config: config);

                var text = response?.Candidates?[0]?.Content?.Parts?[0]?.Text?.Trim();
                return text ?? "Xin lỗi, tôi không thể xử lý được. Vui lòng thử lại.";
            }
            catch (Exception ex)
            {
                return $"Xin lỗi, đã có lỗi xảy ra: {ex.Message}";
            }
        }
    }
}
