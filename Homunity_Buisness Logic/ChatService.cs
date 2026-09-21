using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Homunity_Data_Access.Entities;
using Homunity_Data_Access.Repositories;
using Homunity_Data_Access.Repositories.Models;
using Homunity_Shared_DTOs;
using Microsoft.Extensions.Logging;

namespace Homunity_Buisness_Logic
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepo;
        private readonly IPropertyRepository _propertyRepo;
        private readonly IUniversityRepository _universityRepo;
        private readonly IGeminiClient _geminiClient;
        private readonly ILogger<ChatService> _logger;
        private List<UniversityEntity>? _cachedUniversities;

        // ملاحظة (Sprint 5): تم إزالة الاعتماد المباشر على IConfiguration بالكامل من هنا —
        // معرفة مفتاح/رابط Gemini أصبحت حصرًا داخل GeminiClient، حيث يجب أن تكون.
        public ChatService(
            IChatRepository chatRepo,
            IPropertyRepository propertyRepo,
            IUniversityRepository universityRepo,
            IGeminiClient geminiClient,
            ILogger<ChatService> logger)
        {
            _chatRepo = chatRepo;
            _propertyRepo = propertyRepo;
            _universityRepo = universityRepo;
            _geminiClient = geminiClient;
            _logger = logger;
        }

        public async Task<List<ChatMessageResponse>> GetHistoryAsync(int studentId)
        {
            var messages = await _chatRepo.GetHistoryAsync(studentId, 20);
            return messages.Select(m => new ChatMessageResponse
            {
                MessageId = m.MessageId,
                Role = m.Role,
                Content = m.Content,
                CreatedAt = m.CreatedAt ?? DateTime.Now
            }).ToList();
        }

        public Task<bool> ClearHistoryAsync(int studentId) => _chatRepo.ClearHistoryAsync(studentId);

        private async Task<string> GetPropertiesContextAsync(int maxCount = 10)
        {
            try
            {
                var props = await _propertyRepo.GetActiveForChatAsync(maxCount);
                var sb = new StringBuilder();
                foreach (var p in props)
                {
                    sb.AppendLine(
                        $"- عقار: {p.Title} | السعر: {p.Price} جنيه/شهر | الغرف: {p.Rooms} | النوع: {p.PropertyType} | " +
                        $"العنوان: {(string.IsNullOrEmpty(p.Address) ? "غير محدد" : p.Address)} | " +
                        $"الجامعة: {p.UniversityName ?? "غير مرتبط بجامعة"} | ID: {p.PropertyId}");
                }
                return sb.Length > 0 ? sb.ToString() : "لا توجد عقارات متاحة حالياً.";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "GetPropertiesContext failed; falling back to empty context.");
                return "لا توجد عقارات متاحة حالياً.";
            }
        }

        private async Task<List<UniversityEntity>> GetAllUniversitiesAsync()
        {
            if (_cachedUniversities != null) return _cachedUniversities;
            _cachedUniversities = await _universityRepo.GetAllAsync();
            return _cachedUniversities;
        }

        private async Task<string?> ExtractUniversityNameAsync(string userMessage)
        {
            var universities = await GetAllUniversitiesAsync();
            return universities.FirstOrDefault(u => userMessage.Contains(u.Name, StringComparison.OrdinalIgnoreCase))?.Name;
        }

        private async Task<List<PropertyChatProjection>> GetPropertiesByUniversityNameAsync(string universityName)
        {
            try
            {
                var universities = await GetAllUniversitiesAsync();
                var targetUni = universities.FirstOrDefault(u => u.Name.Equals(universityName, StringComparison.OrdinalIgnoreCase));
                if (targetUni == null) return new List<PropertyChatProjection>();

                var all = await _propertyRepo.GetActiveForChatAsync(1000);
                return all.Where(p => p.UniversityId.HasValue && p.UniversityId.Value == targetUni.UniversityId).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "GetPropertiesByUniversityName failed for {UniversityName}.", universityName);
                return new List<PropertyChatProjection>();
            }
        }

        private string BuildUniversityContext(string universityName, List<PropertyChatProjection> properties)
        {
            if (properties.Count == 0) return $"⚠️ لا توجد عقارات مرتبطة بجامعة {universityName} حالياً.";

            var sb = new StringBuilder();
            sb.AppendLine($"📚 **العقارات المتاحة بجامعة {universityName}:**");
            foreach (var p in properties)
                sb.AppendLine($"- 🏠 {p.Title} | السعر: {p.Price} جنيه/شهر | الغرف: {p.Rooms} | النوع: {p.PropertyType} | " +
                              $"العنوان: {(string.IsNullOrEmpty(p.Address) ? "غير محدد" : p.Address)} | ID: {p.PropertyId}");
            return sb.ToString();
        }

        private static string GetTeamAndSupervisorInfo() => @"
🔹 **معلومات فريق العمل (بدون أدوار مكررة):**

**Back-end Team:**
- شنوده محسن (Team Leader)
- إبراهيم محمد
- عبد الله سيد
- محمد عبد الله
- مهرائيل عاصم

**Front-end Team:**
- محمد زكريا
- إسراء علاء
- مريم صبحي
- منة الله إمام
- هاجر قاسم
- ندى فؤاد

**UI/UX Team:**
- بسملة يحيى

🔹 **المشرفون الأكاديميون:**
- د. محمد أحمد محفوظ (المشرف الرئيسي)
- د. أحمد أمين

🔹 **التقنيات المستخدمة:**
- Backend: ASP.NET Core, ADO.NET, SQL Server (3-Tier Architecture)
- Frontend: HTML, CSS, JavaScript, Bootstrap
- Design: Figma
- Tools: Notion, GitHub, Swagger, Postman

🔹 **جهة التقديم:**
أكاديمية الطيبة التعليمية - قسم نظم ومعلومات الأعمال 2026
";

        public async Task<ChatResponse> SendMessageAsync(int studentId, string userMessage)
        {
            await _chatRepo.SaveMessageAsync(studentId, "user", userMessage);

            string? targetUniversity = await ExtractUniversityNameAsync(userMessage);
            List<PropertyChatProjection>? universityProperties = null;
            string universityContext = "";

            if (!string.IsNullOrEmpty(targetUniversity))
            {
                universityProperties = await GetPropertiesByUniversityNameAsync(targetUniversity);
                universityContext = BuildUniversityContext(targetUniversity, universityProperties);
            }

            string generalPropertiesContext = await GetPropertiesContextAsync(10);
            var history = await _chatRepo.GetHistoryAsync(studentId, 10);
            string teamInfo = GetTeamAndSupervisorInfo();

            string systemPrompt = $@"أنت مساعد ذكي لمنصة Homunity - منصة إيجار السكن الطلابي في مصر.
مهمتك مساعدة الطلاب في إيجاد أفضل سكن مناسب لهم والإجابة عن أسئلتهم بدقة.

**معلومات عن المشروع والعقارات والفريق:**
{teamInfo}

**العقارات المتاحة حالياً (عامة):**
{generalPropertiesContext}

**إذا ذكر المستخدم جامعة معينة، فهذه عقاراتها:**
{universityContext}

**إرشادات صارمة للرد (اتبعها بدقة):**
1. إذا سأل المستخدم عن ""فريق العمل"" أو ""المطورين"" أو ""الفريق المنفذ""، أجب فقط بأسماء الفريق (Back-end, Front-end, UI/UX) كما هي مذكورة أعلاه، ولا تذكر المشرفين أو التقنيات.
2. إذا سأل عن ""المشرف"" أو ""الدكتور المشرف""، أجب فقط بأسماء المشرفين.
3. إذا سأل عن ""التقنيات"" أو ""التكنولوجيا المستخدمة""، أجب فقط بقائمة التقنيات.
4. إذا سأل عن ""المشروع"" أو ""كل شيء"" أو ""معلومات كاملة""، أجب بكل المعلومات (الفريق + المشرفين + التقنيات).
5. بخلاف ذلك، ركز على مساعدة الطالب في إيجاد سكن مناسب باستخدام قائمة العقارات المقدمة.
6. رد دائماً بالعربية ما لم يكتب الطالب بالإنجليزية.
7. لا تخترع معلومات غير موجودة.
8. كن مختصراً ومفيداً.

**ملاحظة مهمة:**
- إذا طلب المستخدم حجز عقار، أخبره بالضغط على زر Book Now في صفحة العقار.
- إذا لم توجد عقارات للجامعة المطلوبة، أخبره بذلك بوضوح.";

            var geminiHistory = history.Select(m => new GeminiHistoryMessage { Role = m.Role, Content = m.Content }).ToList();
            var geminiResult = await _geminiClient.GenerateReplyAsync(systemPrompt, geminiHistory);

            string reply;
            if (geminiResult.Success)
            {
                reply = geminiResult.ReplyText!;
            }
            else
            {
                // لا يتم تسجيل أي جزء من الـsystemPrompt أو رسالة المستخدم — فقط سبب الفشل التقني.
                _logger.LogWarning("Gemini integration failed for StudentId {StudentId}: {Reason}", studentId, geminiResult.FailureReason);

                // نفس رسائل الفشل الأصلية بالضبط، محافظة على السلوك السابق لكل حالة فشل.
                reply = geminiResult.FailureReason switch
                {
                    "empty_response" => "لم أستطع فهم السؤال.",
                    "provider_error" => "عذراً، يوجد مشكلة في الاتصال حالياً. حاول مرة أخرى.",
                    _ => "عذراً، حدث خطأ داخلي."
                };
            }

            await _chatRepo.SaveMessageAsync(studentId, "assistant", reply);
            var suggestions = await ExtractSuggestionsAsync(userMessage, universityProperties);

            return new ChatResponse { Reply = reply, Suggestions = suggestions };
        }

        private async Task<List<PropertySuggestion>> ExtractSuggestionsAsync(string userMessage, List<PropertyChatProjection>? universitySpecificProperties)
        {
            try
            {
                var props = (universitySpecificProperties != null && universitySpecificProperties.Count > 0)
                    ? universitySpecificProperties
                    : await _propertyRepo.GetActiveForChatAsync(50);

                var keywords = userMessage.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                return props
                    .Where(p => keywords.Any(k => k.Length > 2 &&
                        ((p.Title?.ToLower().Contains(k) ?? false) ||
                         (p.Address?.ToLower().Contains(k) ?? false) ||
                         (p.UniversityName?.ToLower().Contains(k) ?? false))))
                    .Take(3)
                    .Select(p => new PropertySuggestion
                    {
                        PropertyID = p.PropertyId,
                        Title = p.Title,
                        Price = p.Price,
                        Address = p.Address ?? "",
                        ImageUrl = p.MainImagePath ?? ""
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "ExtractSuggestions failed.");
                return new List<PropertySuggestion>();
            }
        }
    }
}