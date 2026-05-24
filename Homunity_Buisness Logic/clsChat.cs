using Homunity_Buisness_Logic;
using Homunity_Data_Access;
using Homunity_Shared_DTOs;
using System;
using System.Configuration;
using System.Data;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
public class clsChat
{
      private readonly string _apiKey;
      private readonly string _apiUrl;
      private readonly IConfiguration _configuration;

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

    private List<clsProperties> MapProperties(DataTable dt)
    {
        var list = new List<clsProperties>();
        if (dt == null) return list;

        foreach (DataRow row in dt.Rows)
        {
            list.Add(new clsProperties
            {
                PropertyID = Convert.ToInt32(row["PropertyID"]),
                OwnerID = Convert.ToInt32(row["OwnerID"]),
                Title = row["Title"].ToString(),
                Description = row["Description"].ToString(),
                Price = Convert.ToDecimal(row["Price"]),
                Rooms = Convert.ToInt32(row["Rooms"]),
                PropertyType = row["PropertyType"].ToString(),
                LocationID = Convert.ToInt32(row["LocationID"]),
                PropertyStatusID = Convert.ToInt32(row["StatusID"]),   // ✅ الإصلاح
                RejectReason = row["RejectReason"]?.ToString(),
                CreatedAt = Convert.ToDateTime(row["CreatedAt"]),
                UniversityId = row["UniversityId"] != DBNull.Value
                                       ? Convert.ToInt32(row["UniversityId"]) : 0,

                // ✅ نبني العنوان من City + Area + Street لأن Street ممكن يكون null
                Address = BuildAddress(row)
            });
        }
        return list;
    }

    private string BuildAddress(DataRow row)
    {
        // لو FullAddress موجود استخدمه (لو بترجعه في query تاني)
        string street = row.Table.Columns.Contains("Address") && row["Address"] != DBNull.Value
                        ? row["Address"].ToString().Trim() : "";
        string city = row.Table.Columns.Contains("City") && row["City"] != DBNull.Value
                        ? row["City"].ToString().Trim() : "";
        string area = row.Table.Columns.Contains("Area") && row["Area"] != DBNull.Value
                        ? row["Area"].ToString().Trim() : "";

        if (!string.IsNullOrEmpty(street))
            return $"{street}, {area}, {city}";
        if (!string.IsNullOrEmpty(area))
            return $"{area}, {city}";
        return city;
    }

    public async Task<ChatResponse> SendMessage(int studentId, string userMessage)
      {
          // 1. Save user message
          clsChatData.SaveMessage(studentId, "user", userMessage);

          // 2. Get available properties from DB for context (now only 10)
          string propertiesContext = GetPropertiesContext(10);

          // 3. Get conversation history (last 10)
          var history = clsChatData.GetHistory(studentId, 10);

          // 4. System prompt
          string systemPrompt = $@"أنت مساعد ذكي لمنصة Homunity - منصة إيجار السكن الطلابي في مصر.
مهمتك مساعدة الطلاب في إيجاد أفضل سكن مناسب لهم.

معلومات العقارات المتاحة حالياً:
{propertiesContext}

إرشادات مهمة:
- رد دائماً بالعربية ما لم يكتب الطالب بالإنجليزية
- كن ودوداً ومفيداً
- إذا سأل الطالب عن عقارات، اقترح المناسب منها بناءً على السياق
- إذا ذكر ميزانية أو جامعة معينة، صفّ العقارات المناسبة
- إذا أراد حجز عقار، أخبره بالضغط على زرار Book Now في صفحة العقار
- لا تخترع معلومات غير موجودة في البيانات
- كن مختصراً ومفيداً في ردودك";

         // 5. Call Gemini API (history already contains the user message, no duplication)
         string reply = await CallGeminiAPI(systemPrompt, history);
         // 6. Save assistant reply
         clsChatData.SaveMessage(studentId, "assistant", reply);
         // 7. Extract property suggestions
         var suggestions = ExtractSuggestions(reply, userMessage);
         return new ChatResponse { Reply = reply, Suggestions = suggestions };
     }
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
    private async Task<string> CallGeminiAPI(string systemPrompt, List<ChatMessageDTO> history)
     {
         try
         {
             using var client = new HttpClient();
             client.Timeout = TimeSpan.FromSeconds(30);
             // Build contents from history only (no duplicate currentMessage)
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
     private List<PropertySuggestion> ExtractSuggestions(string reply, string userMessage)
     {
         try
         {
             var dt = clsPropertiesData.GetAllPropertiesV2();
             var props = MapProperties(dt);
             var keywords = userMessage.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
             return props
                 .Where(p => p.PropertyStatusID == 2)
                 .Where(p => keywords.Any(k =>
                     k.Length > 2 &&
                     (
                         (p.Title?.ToLower().Contains(k) ?? false) ||
                         (p.Address?.ToLower().Contains(k) ?? false)
                     )
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
         }
         catch
         {
             return new List<PropertySuggestion>();
         }
     }
}