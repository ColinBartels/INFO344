using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LINQ
{
    class Program
    {
        static void Main(string[] args)
        {
            int[] temp = new int[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            var results = temp.Select(x => x * x)
                .OrderByDescending(x => x)
                .Select(x => x.ToString())
                .ToList();

            var numbers = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 2, 4, 6, 8, 12, 2, 4, 6 };
            var evenNumberHistogram = numbers
                .Where(x => x % 2 == 0)
                .GroupBy(x => x)
                .Select(x => new Tuple<int, int>(x.Key, x.ToList().Count))
                .OrderByDescending(x => x.Item1)
                .ToList();

            for (int i = 0; i < evenNumberHistogram.Count; i++)
            {
                Console.WriteLine(evenNumberHistogram[i]);
            }

            var primes = numbers
                .Where(x => isPrime(x))
                .Select(x => x.ToString())
                .ToList();

            for (int i = 0; i < primes.Count; i++)
            {
                Console.WriteLine(primes[i]);
            }

            var input = new string[] { "hello", "info" };
            var result = input.SelectMany(x => x);

            var numbers2 = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 2, 4, 6, 8, 12, 2, 4, 6 };
            var result2 = numbers2.Skip(5).Take(3);

            WebClient web = new WebClient();

            string file = web.DownloadString("http://uwinfo344.chunkaiw.com/obama.txt").ToLower();
            var fixedInput = Regex.Replace(file, "[^a-zA-Z0-9% ._]", string.Empty);

            var mentions = fixedInput
                .Split(' ')
                .Where(x => x.Contains("Obama"))
                .Where(x => x.Length <= 7)
                .ToList();
            Console.WriteLine("# of obamas:" + mentions.Count);

            var president = file
                .Split(new char[] { '.', '!', '?', '\n'})
                .Where(x => (x.Contains("obama") || x.Contains("obama ") || x.Contains("obama's")) && x.Contains("president"))
                .ToList();
            Console.WriteLine("# of obamas + president: " + president.Count);

            Console.Read();

        }

        public static bool isPrime(int number)
        {
            int boundary = (int)Math.Floor(Math.Sqrt(number));

            if (number == 1) return false;
            if (number == 2) return true;

            for (int i = 2; i <= boundary; ++i)
            {
                if (number % i == 0) return false;
            }

            return true;
        }
    }
}
