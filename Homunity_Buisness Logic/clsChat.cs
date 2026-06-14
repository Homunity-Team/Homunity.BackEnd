using Homunity_Buisness_Logic;
using Homunity_Data_Access;
using Homunity_Shared_DTOs;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Homunity_Business_Logic
{
    public class clsChat
    {
        private readonly string _apiKey;
        private readonly string _apiUrl;
        private readonly IConfiguration _configuration;
        private List<clsUniversities> _cachedUniversities = null;

        public clsChat(IConfiguration configuration)
        {
            _configuration = configuration;
            _apiKey = _configuration["Gemini:ApiKey"];
            _apiUrl = _configuration["Gemini:ApiUrl"];

            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_apiUrl))
                throw new Exception("Gemini API key or URL is missing in appsettings.json");
        }

        public List<ChatMessageDTO> GetHistory(int studentId)
            => clsChatData.GetHistory(studentId, 20);

        public bool ClearHistory(int studentId)
            => clsChatData.ClearHistory(studentId);

        // ========================== Helper: Map DataTable to Properties ==========================
        private List<clsProperties> MapProperties(DataTable dt)
        {
            var list = new List<clsProperties>();
            if (dt == null) return list;

            foreach (DataRow row in dt.Rows)
            {
                var prop = new clsProperties();
                prop.LoadFromDataRow(row);
                list.Add(prop);
            }
            return list;
        }

        // ========================== Get Properties Context (default 10) ==========================
        private string GetPropertiesContext(int maxCount = 10)
        {
            try
            {
                var dt = clsPropertiesData.GetAllPropertiesV2();
                var props = MapProperties(dt);
                var sb = new StringBuilder();

                foreach (var p in props.Take(maxCount))
                {
                    sb.AppendLine(
                        $"- عقار: {p.Title} | السعر: {p.Price} جنيه/شهر | " +
                        $"الغرف: {p.Rooms} | النوع: {p.PropertyType} | " +
                        $"العنوان: {(string.IsNullOrEmpty(p.Address) ? "غير محدد" : p.Address)} | " +
                        $"الجامعة: {p.UniversityName ?? "غير مرتبط بجامعة"} | " +
                        $"ID: {p.PropertyID}"
                    );
                }
                return sb.Length > 0 ? sb.ToString() : "لا توجد عقارات متاحة حالياً.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetPropertiesContext error: {ex.Message}");
                return "لا توجد عقارات متاحة حالياً.";
            }
        }

        // ========================== Get All Universities from DB (cached) ==========================
        private List<clsUniversities> GetAllUniversities()
        {
            if (_cachedUniversities != null) return _cachedUniversities;
            _cachedUniversities = clsUniversities.GetAllUniversities();
            return _cachedUniversities;
        }

        // ========================== Extract University Name from User Message ==========================
        private string ExtractUniversityName(string userMessage)
        {
            var universities = GetAllUniversities();
            foreach (var uni in universities)
            {
                if (userMessage.Contains(uni.Name, StringComparison.OrdinalIgnoreCase))
                    return uni.Name;
            }
            return null;
        }

        // ========================== Get Properties by University Name ==========================
        private List<clsProperties> GetPropertiesByUniversityName(string universityName)
        {
            try
            {
                var universities = GetAllUniversities();
                var targetUni = universities.FirstOrDefault(u => u.Name.Equals(universityName, StringComparison.OrdinalIgnoreCase));
                if (targetUni == null) return new List<clsProperties>();

                var dt = clsPropertiesData.GetAllPropertiesV2();
                var allProps = MapProperties(dt);

                var filtered = allProps
                    .Where(p => p.UniversityId.HasValue && p.UniversityId.Value == targetUni.UniversityId)
                    .ToList();

                return filtered;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetPropertiesByUniversityName error: {ex.Message}");
                return new List<clsProperties>();
            }
        }

        // ========================== Build Context String for Specific University ==========================
        private string BuildUniversityContext(string universityName, List<clsProperties> properties)
        {
            if (properties == null || properties.Count == 0)
                return $"⚠️ لا توجد عقارات مرتبطة بجامعة {universityName} حالياً.";

            var sb = new StringBuilder();
            sb.AppendLine($"📚 **العقارات المتاحة بجامعة {universityName}:**");
            foreach (var p in properties)
            {
                sb.AppendLine(
                    $"- 🏠 {p.Title} | السعر: {p.Price} جنيه/شهر | الغرف: {p.Rooms} | النوع: {p.PropertyType} | " +
                    $"العنوان: {(string.IsNullOrEmpty(p.Address) ? "غير محدد" : p.Address)} | ID: {p.PropertyID}"
                );
            }
            return sb.ToString();
        }

        // ========================== Get Team & Supervisor Info (منفصلة وقابلة للتجميع حسب الطلب) ==========================
        private string GetTeamAndSupervisorInfo()
        {
            // هذه المعلومات تُستخدم فقط لبناء system prompt، وسيتم إعطاء تعليمات للـ Gemini باستخدامها بشكل منفصل حسب السؤال
            return @"
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
        }

        // ========================== Main SendMessage (مع تعليمات دقيقة للرد حسب السؤال) ==========================
        public async Task<ChatResponse> SendMessage(int studentId, string userMessage)
        {
            clsChatData.SaveMessage(studentId, "user", userMessage);

            string targetUniversity = ExtractUniversityName(userMessage);
            List<clsProperties> universityProperties = null;
            string universityContext = "";

            if (!string.IsNullOrEmpty(targetUniversity))
            {
                universityProperties = GetPropertiesByUniversityName(targetUniversity);
                universityContext = BuildUniversityContext(targetUniversity, universityProperties);
            }

            string generalPropertiesContext = GetPropertiesContext(10);
            var history = clsChatData.GetHistory(studentId, 10);
            string teamInfo = GetTeamAndSupervisorInfo();

            // تعليمات دقيقة للـ Gemini: الإجابة حسب السؤال وعدم خلط المعلومات
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

            string reply = await CallGeminiAPI(systemPrompt, history);
            clsChatData.SaveMessage(studentId, "assistant", reply);
            var suggestions = ExtractSuggestions(reply, userMessage, universityProperties);

            return new ChatResponse { Reply = reply, Suggestions = suggestions };
        }

        // ========================== Call Gemini API ==========================
        private async Task<string> CallGeminiAPI(string systemPrompt, List<ChatMessageDTO> history)
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(30);

                var contents = new List<object>();
                foreach (var msg in history)
                {
                    string geminiRole = msg.Role == "user" ? "user" : "model";
                    contents.Add(new
                    {
                        role = geminiRole,
                        parts = new[] { new { text = msg.Content } }
                    });
                }

                var requestBody = new
                {
                    system_instruction = new
                    {
                        parts = new[] { new { text = systemPrompt } }
                    },
                    contents = contents
                };

                var json = JsonSerializer.Serialize(requestBody);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{_apiUrl}?key={_apiKey}", httpContent);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Gemini error: {response.StatusCode} - {responseJson}");
                    return "عذراً، يوجد مشكلة في الاتصال حالياً. حاول مرة أخرى.";
                }

                using var doc = JsonDocument.Parse(responseJson);
                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();
                return text ?? "لم أستطع فهم السؤال.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in Gemini: {ex.Message}");
                return "عذراً، حدث خطأ داخلي.";
            }
        }

        // ========================== Extract Property Suggestions ==========================
        private List<PropertySuggestion> ExtractSuggestions(string reply, string userMessage, List<clsProperties> universitySpecificProperties = null)
        {
            try
            {
                List<clsProperties> props;
                if (universitySpecificProperties != null && universitySpecificProperties.Count > 0)
                {
                    props = universitySpecificProperties;
                }
                else
                {
                    var dt = clsPropertiesData.GetAllPropertiesV2();
                    props = MapProperties(dt);
                }

                var keywords = userMessage.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                var filtered = props
                    .Where(p => p.PropertyStatusID == 2)
                    .Where(p => keywords.Any(k =>
                        k.Length > 2 &&
                        ((p.Title?.ToLower().Contains(k) ?? false) ||
                         (p.Address?.ToLower().Contains(k) ?? false) ||
                         (p.UniversityName?.ToLower().Contains(k) ?? false))
                    ))
                    .Take(3)
                    .Select(p =>
                    {
                        var images = clsPropertyImages.GetImagesByPropertyID(p.PropertyID);
                        return new PropertySuggestion
                        {
                            PropertyID = p.PropertyID,
                            Title = p.Title,
                            Price = p.Price,
                            Address = p.Address ?? "",
                            ImageUrl = images?.FirstOrDefault()?.ImagePath ?? ""
                        };
                    })
                    .ToList();

                return filtered;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ExtractSuggestions error: {ex.Message}");
                return new List<PropertySuggestion>();
            }
        }
    }
}