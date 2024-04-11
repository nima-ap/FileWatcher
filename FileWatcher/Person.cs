using Newtonsoft.Json;

namespace FileWatcher
{
	public class Person
    {
        [JsonProperty("name")]
        public string? Name { get; set; }
	}
}

