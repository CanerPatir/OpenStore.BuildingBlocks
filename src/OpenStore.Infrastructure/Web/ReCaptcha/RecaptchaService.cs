using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace OpenStore.Infrastructure.Web.ReCaptcha
{
    public class RecaptchaService : IRecaptchaService
    {
        public readonly RecaptchaSettings RecaptchaSettings;
        private HttpClient _httpClient;

        public RecaptchaService( IHttpClientFactory httpClientFactory, IOptions<RecaptchaSettings> options)
        {
            _httpClient = httpClientFactory.CreateClient(RecaptchaServiceCollection.ReCaptchaHttpClientKey);
            RecaptchaSettings = options.Value;
        }

        public async Task<RecaptchaResponse> Validate(HttpRequest request, bool antiForgery = true)
        {
            if (!request.Form.ContainsKey("g-recaptcha-response")) // error if no reason to do anything, this is to alert developers they are calling it without reason.
                throw new System.ComponentModel.DataAnnotations.ValidationException("Google recaptcha response not found in form. Did you forget to include it?");

            var response = request.Form["g-recaptcha-response"];
            var result = await _httpClient.GetStringAsync($"https://{RecaptchaSettings.Site}/recaptcha/api/siteverify?secret={RecaptchaSettings.SecretKey}&response={response}");
            var captchaResponse = JsonSerializer.Deserialize<RecaptchaResponse>(result);

            if (captchaResponse.success && antiForgery)
                if (captchaResponse.hostname?.ToLower() != request.Host.Host?.ToLower() && captchaResponse.hostname != "testkey.google.com")
                    throw new System.ComponentModel.DataAnnotations.ValidationException("Recaptcha host, and request host do not match. Forgery attempt?");

            return captchaResponse;
        }

        public async Task<RecaptchaResponse> Validate(string responseCode)
        {
            if (string.IsNullOrEmpty(responseCode))
                throw new System.ComponentModel.DataAnnotations.ValidationException("Google recaptcha response is empty?");

            var result = await _httpClient.GetStringAsync($"https://{RecaptchaSettings.Site}/recaptcha/api/siteverify?secret={RecaptchaSettings.SecretKey}&response={responseCode}");
            var captchaResponse = JsonSerializer.Deserialize<RecaptchaResponse>(result);

            return captchaResponse;
        }
    }
}