using System;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        string filePath = "Sample.txt";

        // Create a big file
        FileStream fs = new FileStream(filePath, FileMode.CreateNew);
        fs.Seek(1024 * 1024, SeekOrigin.Begin);
        fs.WriteByte(0);
        fs.Close();

        var task = ReadFileAsync(filePath);

        Console.WriteLine("A synchronous message");

        int length = await task;

        Console.WriteLine("Total file length: " + length);
        Console.WriteLine("After reading message");
        Console.ReadLine();
    }

    static async Task<int> ReadFileAsync(string file)
    {
        Console.WriteLine("Start reading file");

        int length = 0;

        using (StreamReader reader = new StreamReader(file))
        {
            string fileContent = await reader.ReadToEndAsync();
            length = fileContent.Length;
        }

        Console.WriteLine("Finished reading file");

        return length;
    }
}