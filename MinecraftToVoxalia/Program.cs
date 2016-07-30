using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace MinecraftToVoxalia
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] files = Directory.GetFiles(Environment.CurrentDirectory, "*.mca", SearchOption.TopDirectoryOnly);
            if (files.Length == 0)
            {
                Console.WriteLine("Please place the program directly amongst the region data (.mca files) you want to copy over.");
            }
        }
    }
}
