using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.Web.Http;
using Newtonsoft.Json.Linq;
using VK_Miner.DataModel;
using UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding;

namespace VK_Miner.VK
{
    public static class Api
    {
        internal const string Version = "5.35";
        internal const string ApiRoot = "https://api.vk.com/method/";

        private const int CallDelay = 350;

        private static readonly Uri AuthStartUri;
        private static readonly Uri AuthEndUri;

        public static long UserId { get; private set; }
        internal static string Token;

        public static LoggedUserView LoggedUser { get; private set; }

        public static Func<CaptchaException, string> CaptchaResolver;

        private static readonly ApplicationDataContainer CredentialContainer;
        private static readonly Stopwatch RequestStopwatch;

        private static volatile Task<HttpResponseMessage> _currentExecutingRequestTask;

        private static readonly Func<RequestData> CheckTokenRequestCreator;

        private static int runningRequestsCount;
        public static bool IsBusy => runningRequestsCount > 0;

        static Api()
        {
            CredentialContainer = ApplicationData.Current.LocalSettings.CreateContainer(
                "[VK Miner] Entry data",
                ApplicationDataCreateDisposition.Always);

            const string scope = "friends,photos,audio,video,status,wall,groups,offline";
            const string blank = "https://oauth.vk.com/blank.html";
            const string url = "https://oauth.vk.com/authorize"
                               + "?client_id=4989758"
                               + "&scope=" + scope
                               + "&redirect_uri=" + blank
                               + "&display=popup"
                               + "&response_type=token";
            AuthStartUri = new Uri(url);
            AuthEndUri = new Uri(blank);

            RequestStopwatch = Stopwatch.StartNew();

            CheckTokenRequestCreator = RequestData.Creator<Func<RequestData>>("users.get",
                new RequestArgs()
                {
                    ["fields"] = "photo_50"
                });
        }

        public static async Task Initialize()
        {
            if (TryReadSavedCredentials())
                LoggedUser = await CheckTokenAsync();

            if (LoggedUser == null)
                LoggedUser = await AuthAsync();
        }

        private static bool TryReadSavedCredentials()
        {
            Token = (string)CredentialContainer.Values["Token"];
            UserId = ((long?)CredentialContainer.Values["Id"]).GetValueOrDefault();

            return Token != null;
        }
        private static void SaveCurrentCredentials()
        {
            CredentialContainer.Values["Token"] = Token;
            CredentialContainer.Values["Id"] = UserId;
        }
        private static void DeleteCredentials()
        {
            CredentialContainer.Values["Token"] = null;
            CredentialContainer.Values["Id"] = null;
        }

        private static async Task<LoggedUserView> CheckTokenAsync()
        {
            try
            {
                var array = await Execute(CheckTokenRequestCreator()) as JArray;
                if (array != null && array.Count > 0)
                    return new LoggedUserView(array.First);
            }
            catch (ApiException) { }

            return null;
        }

        public static async Task<LoggedUserView> AuthAsync()
        {
            while (true)
            {
                var errorMessage = "Для работы приложения необходимо авторизоваться.";
                try
                {
                    var result = await WebAuthenticationBroker.AuthenticateAsync(
                        WebAuthenticationOptions.None,
                        AuthStartUri, AuthEndUri);

                    if (result.ResponseStatus == WebAuthenticationStatus.Success)
                    {
                        var parts = result.ResponseData.Split(new[] { '#', '=', '&' });
                        if (parts.Length >= 7 && parts[1] == "access_token" && parts[5] == "user_id")
                        {
                            Token = parts[2];
                            UserId = long.Parse(parts[6]);

                            SaveCurrentCredentials();
                            var user = await CheckTokenAsync();
                            if (user != null)
                                return user;
                        }
                    }
                    else if (result.ResponseStatus == WebAuthenticationStatus.UserCancel)
                    {
                        Application.Current.Exit();
                    }
                    else if (result.ResponseStatus == WebAuthenticationStatus.ErrorHttp)
                    {
                        errorMessage = "Ошибка подключения.";
                    }
                }
                catch (Exception)
                {
                    errorMessage = "Неизвестная ошибка";
                }

                await (new MessageDialog(errorMessage)).ShowAsync();
            }
        }

        public static async Task LogoutAsync()
        {
            DeleteCredentials();
            LoggedUser = await AuthAsync();
        }

        private static async Task<HttpResponseMessage> ExecuteRaw(RequestData request)
        {
            const string contentType = "application/x-www-form-urlencoded";
            var content = new HttpStringContent(request.Content, UnicodeEncoding.Utf8, contentType);

            var delay = CallDelay - RequestStopwatch.ElapsedMilliseconds;
            if (delay > 0) await Task.Delay((int)delay);
            RequestStopwatch.Restart();

            HttpResponseMessage response;
            using (var client = new HttpClient())
                response = await client.PostAsync(request.Uri, content);

            _currentExecutingRequestTask = null;

            return response;
        }
        private static async Task<JObject> ExecuteWithDelay(RequestData request)
        {
            Interlocked.Increment(ref runningRequestsCount);

            var isResult = false;
            Task<HttpResponseMessage> task = null;

            while (!isResult)
            {
                lock (RequestStopwatch)
                {
                    task = _currentExecutingRequestTask;
                    if (task == null)
                    {
                        task = (_currentExecutingRequestTask = ExecuteRaw(request));
                        isResult = true;
                    }
                }

                await task;
            }

            Interlocked.Decrement(ref runningRequestsCount);

            var responseString = await task.Result.Content.ReadAsStringAsync();
            return JObject.Parse(responseString);
        }
        public static async Task<JToken> Execute(RequestData request)
        {
            var response = await ExecuteWithDelay(request);

            JToken error;
            if (response.TryGetValue("error", out error))
            {
                var newRequest = new RequestData(request.Uri, null);
                do
                {
                    var errorCode = (ApiErrorType)error.Value<int>("error_code");
                    if (errorCode != ApiErrorType.CaptchaNeeded)
                        throw new ApiException(error);

                    var key = CaptchaResolver(new CaptchaException(error));
                    newRequest.Content = $"{request.Content}&captcha_sid={error["captcha_sid"]}&captcha_key={key}";

                    response = await ExecuteWithDelay(newRequest);
                    error = response["error"];
                } while (error != null);
            }

            return response["response"].Value<JToken>();
        }
    }
}