using System;
using image_recognizer;
using System.Threading;
using System.Collections.Generic;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Classificator cl = new Classificator(@"C:\Users\vdtri\Pictures\Image_for_c#");
            cl.recognize();
            foreach(var k in cl.answers())
            {
                Console.WriteLine(k);
            }
        }
    }
}
