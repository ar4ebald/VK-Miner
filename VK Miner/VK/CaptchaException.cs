using Newtonsoft.Json.Linq;

namespace VK_Miner.VK
{
    public class CaptchaException : ApiException
    {
        public string CaptchaSid { get; private set; }
        public string CaptchaImg { get; private set; }

        public CaptchaException(JToken json) : base(json)
        {
            CaptchaSid = json["captcha_sid"].Value<string>();
            CaptchaImg = json["captcha_img"].Value<string>();
        }
    }
}