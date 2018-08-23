using Polly;
using RestTemplateCore;
using System;
using System.Net.Http;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            Policy policy = Policy.Handle<ArgumentException>().Fallback(()=> {
                Console.WriteLine("执行出错");
            }, ex => {
                Console.WriteLine(ex);
            });
            policy.Execute(()=> {
                Console.WriteLine("开始任务");
                throw new ArgumentException("hello world!");
                Console.WriteLine("完成任务");
            });*/
            Policy<string> policy = Policy<string>.Handle<ArgumentException>().Fallback(()=> {
                Console.WriteLine("执行出错");
                return "降级的值";
            });

            string res = policy.Execute(()=> {
                Console.WriteLine("开始任务");
                throw new ArgumentException("hello world!");
                Console.WriteLine("完成任务");
                return "正常的值";
            });
            Console.WriteLine($"返回值：{res}");
            Console.ReadKey();
        }
    }
}
