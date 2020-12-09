namespace OpenStore.Infrastructure.Web.ReCaptcha.Versions
{
    public class RecaptchaV3HiddenInput : RecaptchaVersion
    {
        public string Id { get; set; } = "g-recaptcha-response";
        public string Action { get; set; } = "homepage";
        public bool IsAsync { get; set; } = true;
    }
}
