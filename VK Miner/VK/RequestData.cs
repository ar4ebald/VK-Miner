using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.Web.Http;
using VK_Miner.VK.Model;
using UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding;

namespace VK_Miner.VK
{
    public class RequestData
    {
        public Uri Uri;
        public string Content;

        public RequestData(Uri uri, string content)
        {
            Uri = uri;
            Content = content;
        }

        private static readonly MethodInfo EncodeMethod = typeof(WebUtility).GetMethod("UrlEncode");
        private static readonly MethodInfo ToStringMethod = typeof(object).GetMethod("ToString");
        private static readonly MethodInfo StringFormatMethod = typeof(string).GetMethod("Format", new[] { typeof(string), typeof(object[]) });
        private static readonly MemberInfo AccessTokenMember = typeof(Api).GetField("Token", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly ConstructorInfo Constructor = typeof(RequestData).GetConstructor(new[] { typeof(Uri), typeof(string) });

        public static T Creator<T>(string method, RequestArgs args)
        {
            var tType = typeof(T);
            if (!typeof(Delegate).IsAssignableFrom(tType))
                throw new ArgumentException(nameof(T));

            var methodInfo = tType.GetMethod("Invoke");
            if (methodInfo.ReturnType != typeof(RequestData))
                throw new ArgumentException(nameof(T));

            var methodArgs = methodInfo.GetParameters();

            var formatParts = new List<string>();
            formatParts.AddRange(
                Enumerable.Range(0, methodArgs.Length)
                    .Select(i => $"{methodArgs[i].Name.CamelCaseToSnakeCase()}={{{i}}}"));
            formatParts.AddRange(args.Select(i => $"{i.Key}={WebUtility.UrlEncode(i.Value.ToString())}"));
            formatParts.Add("v=" + Api.Version);
            formatParts.Add($"access_token={{{methodArgs.Length}}}");

            var paramExpList = methodArgs.Select(i => Expression.Parameter(i.ParameterType, i.Name)).ToList();

            var formatExp = Expression.Constant(string.Join("&", formatParts));

            var encodedParams = paramExpList.Select<ParameterExpression, Expression>(
                i => Expression.Call(EncodeMethod, Expression.Call(i, ToStringMethod)));
            var argsValues = encodedParams.Concat(new[] { Expression.MakeMemberAccess(null, AccessTokenMember) });
            var argsExp = Expression.NewArrayInit(typeof(object), argsValues);

            var uriExp = Expression.Constant(new Uri(Api.ApiRoot + method));
            var contentExp = Expression.Call(StringFormatMethod, formatExp, argsExp);

            var resultExp = Expression.New(Constructor, uriExp, contentExp);

            return Expression.Lambda<T>(resultExp, paramExpList).Compile();
        }

        public static RequestData Empty(string uri) => new RequestData(new Uri(uri), string.Empty);

        public static RequestData Default(string uri)
            => new RequestData(new Uri(uri), $"access_token={Api.Token}&v={Api.Version}");
    }
}
