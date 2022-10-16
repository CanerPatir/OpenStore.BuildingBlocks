using Microsoft.AspNetCore.Http;

namespace OpenStore.Infrastructure.Web.ReCaptcha
{
    public interface IRecaptchaService
    {
        Task<RecaptchaResponse> Validate(HttpRequest request, bool antiForgery = true);

        Task<RecaptchaResponse> Validate(string responseCode);
    }
}