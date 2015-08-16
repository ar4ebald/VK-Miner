using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace VK_Miner.VK
{
    public class ApiException : Exception
    {
        public ApiErrorType ApiErrorCode { get; private set; }
        public string ErrorMsg { get; private set; }

        public KeyValuePair<string, string>[] RequestParams { get; private set; }

        public ApiException(JToken json)
        {
            ApiErrorCode = (ApiErrorType)json["error_code"].Value<int>();
            ErrorMsg = json["error_msg"].Value<string>();
            RequestParams = json["request_params"].Value<JArray>()
                .AsEnumerable()
                .Select(i => new KeyValuePair<string, string>(i["key"].Value<string>(), i["value"].Value<string>()))
                .ToArray();
        }
    }
}
