using System;
using System.IO;
using WebApiUtils;

namespace TestConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var text = File.ReadAllText(@"C:\Users\Admin\Desktop\123.txt");
            var tp = new JsonParser();
            tp.GetStructure(text);

        }
    }
}
