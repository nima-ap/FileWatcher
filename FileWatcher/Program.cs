using System.Text;
using FileWatcher;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

string dir = Environment.CurrentDirectory;
FileInfo file = new(Path.Combine(dir, "test.json"));
if (!file.Exists)
{
    string json = JsonConvert.SerializeObject(new Person() { Name = "Jane Doe" });
    File.WriteAllText(file.FullName, json);
}

PhysicalFileProvider fileProvider = new(dir);

ChangeToken.OnChange(() => fileProvider.Watch("test.json"), () =>
{
    int maxRetries = 5;
    int retryTimeoutMs = 50;

    while (maxRetries > 0)
    {
        try
        {
            using Stream stream = file.OpenRead();
            using StreamReader reader = new(
                stream,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true,
                bufferSize: 1024,
                leaveOpen: false);

            Person person = JsonConvert.DeserializeObject<Person>(reader.ReadToEnd());
            Console.WriteLine($"Parsed person name: {person.Name}");
            
            maxRetries = 0;
        }
        catch (IOException)
        {
            maxRetries--;
            if (maxRetries == 0)
            {
                throw;
            }

            Thread.Sleep(retryTimeoutMs);
            retryTimeoutMs *= 2;
        }
    }
});

// To stall and prevent immediate exit
Console.ReadLine();