using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyNamespace
{
    [Serializable]
    public class Hint
    {
        [JsonPropertyName("Word")]
        public string Word { get; set; }
        [JsonPropertyName("NOSW")]
        public int NoumberOfSimilarWords { get; set; }
        
        [JsonPropertyName("Cards")]
        public List<Card>? usesCards { get; set; }

        
        public Hint(string word, int noumberOfSimilarWords)
        {
            this.Word = word;
            this.NoumberOfSimilarWords = noumberOfSimilarWords;
            usesCards = new List<Card>();
            
        }

        
        public string toJson()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.WriteIndented = true;

            string json = JsonSerializer.Serialize(this, options);

            if (json == "" || json == " " || json == null)
                throw new HintException("HITN Json serialization ERROR!");
            else
                return json;
        }
        public static Hint FromJson(string jsonFormat)
        {
            Hint? hint = JsonSerializer.Deserialize<Hint>(jsonFormat);

            if (hint == null)
                throw new HintException("Hint did not deserialized properly!");
            else
                return hint;
        }

        public override string ToString()
        {
            return $"{Word} | {NoumberOfSimilarWords}";
        }


        [Serializable]
        public class HintException : Exception
        {
            public HintException() { }
            public HintException(string message) : base(message) { }
            public HintException(string message, Exception inner) : base(message, inner) { }
            protected HintException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }
    }
}