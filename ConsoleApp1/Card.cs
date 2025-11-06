using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyNamespace
{
    public class Card
    {
        [JsonPropertyName("team")]
        public string Team { get; set; }
        [JsonPropertyName("word")]
        public string Word { get; set; } = string.Empty;

        public Card(string team, string word)
        {
            this.Team = team;
            this.Word = word;
        }

        public string ToJson()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            return JsonSerializer.Serialize(this, options);
        }

        
        public static Card FromJson(string json)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.IncludeFields = true;

            Card? card = JsonSerializer.Deserialize<Card>(json, options);
           
            if (card == null)
                throw new InvalidOperationException("Failed to deserialize Card from JSON.");
            else
                return card;

        }
        public override string ToString()
        {
            return $"Word: {Word}";
        }

        public string teamReveal()
        {
            return $"Team: {Team}\nWord: {Word}";
        }
    }
}