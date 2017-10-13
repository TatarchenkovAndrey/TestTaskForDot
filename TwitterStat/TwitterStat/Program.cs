using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using Newtonsoft.Json;

namespace TwitterStat
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            var isGame = true;
            while (isGame)
            {
                Console.Write("Введите логин пользователя: ");
                var username = Console.ReadLine();
                while (string.IsNullOrEmpty(username))
                {
                    Console.Write("Введи еще раз: ");
                    username = Console.ReadLine();
                }
                username = username.Split().First();
                username = username.Replace("@","");
                var responce = StealTwits(username);
                if (responce != null)
                {
                    FrequencyAnalysis(responce);
                }
                else
                {
                    Console.WriteLine("Твитов нет(");
                }
                Console.WriteLine("Посмотрим еще? \nВведите +, если хотите еше посмотреть");
                var selector = Console.ReadLine();
                isGame = selector == "+" ? true : false;
            }

        }
        /// <summary>
        /// Производим частотный анализ
        /// </summary>
        /// <param name="text"></param>
        public static void FrequencyAnalysis(string text)
        {
            text = text.ToLower();
            var regex = new Regex(@"[^а-яa-z]");
            var res = "";
            var correctText = regex.Replace(text, res);
            var freqChars = correctText.GroupBy(l => l).Select(s => new AnswerMsg { Letter = s.Key, Count = s.Count() }).OrderBy(o => o.Letter).ToList();
            Console.WriteLine("{");
            foreach (var symbal in freqChars)
            {
                Console.WriteLine($"    {symbal.Letter} : '{symbal.Rate(text.Length)}',");
            }
            Console.WriteLine("}");
        }

        /// <summary>
        /// Метод получения твитов
        /// </summary>
        /// <param name="screenName"></param>
        /// <returns></returns>
        public static string StealTwits(string screenName)
        {
            #region Данные для авторизации
            var oAuthConsumerKey = "QVgMbIN2rLNk2VyPnlWscpvkc";
            var oAuthConsumerSecret = "iGOfvqF6kGjdZYKi4c8Jl6xCh9rlnqVwYIdD6oWWsceMu1wDDW";
            var oAuthUrl = "https://api.twitter.com/oauth2/token";
            #endregion

            #region Процесс авторизации
            var authHeaderFormat = "Basic {0}";

            var authHeader = string.Format(authHeaderFormat,
                Convert.ToBase64String(Encoding.UTF8.GetBytes(Uri.EscapeDataString(oAuthConsumerKey) + ":" +
                                                              Uri.EscapeDataString((oAuthConsumerSecret)))
                ));

            var postBody = "grant_type=client_credentials";

            HttpWebRequest authRequest = (HttpWebRequest)WebRequest.Create(oAuthUrl);
            authRequest.Headers.Add("Authorization", authHeader);
            authRequest.Method = "POST";
            authRequest.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
            authRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (Stream stream = authRequest.GetRequestStream())
            {
                byte[] content = ASCIIEncoding.ASCII.GetBytes(postBody);
                stream.Write(content, 0, content.Length);
            }

            authRequest.Headers.Add("Accept-Encoding", "gzip");

            WebResponse authResponse = authRequest.GetResponse();
            // deserialize into an object
            TwitAuthenticateResponse twitAuthResponse;
            using (authResponse)
            {
                using (var reader = new StreamReader(authResponse.GetResponseStream()))
                {
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    var objectText = reader.ReadToEnd();
                    twitAuthResponse = JsonConvert.DeserializeObject<TwitAuthenticateResponse>(objectText);
                }
            }
            #endregion

            #region Получаем тексты твитов(если они есть), если пользователен существует или он не закрыт

            var timelineUrl = $"https://api.twitter.com/1.1/statuses/user_timeline.json?screen_name={screenName}&include_rts=1&exclude_replies=1&count=5";
            HttpWebRequest timeLineRequest = (HttpWebRequest)WebRequest.Create(timelineUrl);
            timeLineRequest.Headers.Add("Authorization", $"{twitAuthResponse.TokenType} {twitAuthResponse.AccessToken}");
            timeLineRequest.Method = "Get";

            WebResponse twitWebResponse;
            try
            {
                twitWebResponse = timeLineRequest.GetResponse();
            }
            catch (Exception)
            {
                Console.WriteLine("Ой, видимо аккаунт пользователя скрыт, либо его не существует");
                return null;
            }
            var jsonResponce = string.Empty;
            using (twitWebResponse)
            {
                using (var reader = new StreamReader(twitWebResponse.GetResponseStream()))
                {
                    jsonResponce = reader.ReadToEnd();
                }
            }


            #endregion

            #region Десериализуем  полученный ответ и возвращаем результат

            var twits = JsonConvert.DeserializeObject<Twit[]>(jsonResponce);
            var response = string.Empty;
            if (twits.Length > 0)
            {
                foreach (var twit in twits)
                {
                    response = $"{response}{twit.Text}";
                }
                return response;
            }
            return null;

            #endregion

        }
       
    }
    public class TwitAuthenticateResponse
    {
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }

}
