using Mercurio.Driver.Models.Zonitel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace Mercurio.Driver.Services
{
    public class ApiZonitelService
    {
        private readonly HttpClient _httpClient;
        private readonly ZonitelSettings _settings;

        public ApiZonitelService(HttpClient httpClient)
        {
            _httpClient = httpClient;
          
            _settings = new ZonitelSettings(); 
            _settings.BaseUrl = PrivateSettings.ApiZonitelBaseUrl;
            _settings.Version = PrivateSettings.ApiZonitelVersion;
            _settings.UserPrivateToken = PrivateSettings.ApiZonitelUserPrivateToken;
            _settings.ClientId = PrivateSettings.ApiZonitelClientId;
            _settings.MilanesTransportPhone = PrivateSettings.ApiZonitelMilanesTransportPhone;

            if (_settings == null)
                throw new Exception("Failed to load private data from Zonitel API");

            _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
            _httpClient.DefaultRequestHeaders.Add("X-Client-Id", _settings.ClientId);
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.UserPrivateToken);
        }
        public async Task<bool> SendSMSMessageDriverArrivesAtThePickupLocation(string clientPhone)
        {
            try
            {
                string endpoint = $"api/v{_settings.Version}/integrations/sms/send";

                string messageText = "Your driver is waiting.\n" +
                                    $"Proceed to meet your driver.\n" +
                                    "Visit www.etamilanes.com";

                string messageTextOld = $@"Your driver has arrived at the pickup location.
                                    Please proceed to meet your driver.
                                    For more details, visit www.etamilanes.com";

                var smsBody = new
                {
                    from = $"+1{_settings.MilanesTransportPhone}",
                    to = $"+1{clientPhone}",
                    text = messageText
                };

                // Escapado de caracteres Unicode: Por defecto, System.Text.Json convierte el símbolo + en \u002B. Aunque es JSON válido, muchas APIs no lo reconocen correctamente. (Zonitel no lo reconoce)
                // CONFIGURACIÓN CRUCIAL: Evitar que escape el símbolo '+'
                var options = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = false
                };

                string jsonPayload = JsonSerializer.Serialize(smsBody, options);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync(endpoint, content);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

    }
}
