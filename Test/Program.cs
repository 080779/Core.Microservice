using RestTemplateCore;
using System;
using System.Net.Http;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                RestTemplate rest = new RestTemplate(httpClient);
            }
        }
    }
}
