using System;
using System.Linq;
using System.Text;

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
                var responce = TwitService.StealTwits(username);
                if (responce != null)
                {
                    TwitService.AnalyseFrequency(responce, username);
                }
                else
                {
                    Console.WriteLine("Твитов нет(");
                }
                Console.WriteLine("Посмотрим еще? \nВведите +, если хотите еше посмотреть");
                var selector = Console.ReadLine();
                isGame = selector == "+";
            }

        }

       
    }
    

}
