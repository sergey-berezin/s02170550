using System;
using image_recognizer;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Classificator cl = new Classificator(@"C:\Users\vdtri\Pictures\Image_for_c#");
            cl.recognize();
        }
    }
}
