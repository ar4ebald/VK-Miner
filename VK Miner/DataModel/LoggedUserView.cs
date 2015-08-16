using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace VK_Miner.DataModel
{
    public class LoggedUserView
    {
        public long Id { get; set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string FullName { get; private set; }
        public string Photo50 { get; private set; }

        public LoggedUserView() : this(JToken.Parse("{\"id\":17234229,\"first_name\":\"Артур\",\"last_name\":\"Булатов\",\"photo_50\":\"https://pp.vk.me/c624916/v624916229/365cb/9hLRQnlJRZI.jpg\"}")) { }

        public LoggedUserView(JToken json)
        {
            Id = json["id"].Value<long>();
            FirstName = json["first_name"].Value<string>();
            LastName = json["last_name"].Value<string>();
            FullName = string.Join(" ", FirstName, LastName);
            Photo50 = json["photo_50"].Value<string>();
        }
    }
}
