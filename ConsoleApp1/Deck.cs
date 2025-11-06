using System.Data;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyNamespace
{
    public class Deck : ICloneable
    {

        [JsonPropertyName("cards")]
        public List<Card> cards { get; set; }
        
        [JsonIgnore]
        public int Length { get { return cards.Count; } }

        [JsonIgnore]

        public string WordsInDeck
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (var card in cards)
                {
                    sb.Append(card.Word + ",");
                }
                return sb.ToString().Trim();
            }
        }

        public Deck(List<Card> cards)
        {
            if (cards.Count != 25)
                throw new InvalidOperationException("A deck must contain exactly 25 cards.");
            this.cards = cards;
        }
        public Deck()
        {
            Console.Write("Generating deck:   ");
            // allocate internal array with exactly 25 slots

            cards = new List<Card>();
            
            //PREVIOUS DECK
            //string previousDeckJson = FileOp.Read("Decsk/PreviousDeck.txt");
            //Deck previousDeck = Deck.FromJson(previousDeckJson);




            //######### Create instance of LLM and creating pormpts #########
            string DeepSeek_api = FileOp.Read("../../API_KEY_DEEPSEEK_IO.txt");
            LLM lLM = new LLM(DeepSeek_api);
            string systemPrompt = FileOp.Read("DeckPrompts/DeckSystemPrompt.txt");
            string userPrompt = "Generate words";

            //######### Calling LLM #########

            
            //example response
            //string response = "apple-bridge-cloud-dream-eagle-flower-garden-hill-ice-jungle-kite-lake-mountain-nest-ocean-pebble-quiet-river-sun-tree-umbrella-valley-wind-xylophone-yellow";
            
            
            string response = lLM.SendRequestAsync(systemPrompt, userPrompt, maxTokens: 256).Result;
            List<string> nounPool = response.Split('-').ToList();




            var rnd = new Random();

            // select 25 unique nouns (shuffle nounPool and take first 25)
            var selectedNouns = nounPool
                .OrderBy(n => rnd.Next())
                .Distinct()
                .Take(25)
                .ToArray();

            // ensure we have 25 unique words; if pool was smaller this would be an issue
            if (selectedNouns.Length < 25)
                throw new InvalidOperationException("Not enough unique nouns to build the deck.");

            // create team assignment list with exact counts using enum values
            var teamAssignments = new List<String>();

            // 1 Assassin
            for (int i = 0; i < 1; i++) teamAssignments.Add("Assasin");
            // 9 Neutral
            for (int i = 0; i < 9; i++) teamAssignments.Add("Neutral");

            Random random = new Random();
            int rndNumber = random.Next(0, 10);

            if(rndNumber < 5)
            {
                // 8 Blue
                for (int i = 0; i < 8; i++) teamAssignments.Add("Blue");
                // 7 Red
                for (int i = 0; i < 7; i++) teamAssignments.Add("Red");
            }
            else
            {
                // 8 Red
                for (int i = 0; i < 8; i++) teamAssignments.Add("Red");
                // 7 Blue
                for (int i = 0; i < 7; i++) teamAssignments.Add("Blue");
            }




            // sanity check: should be 25
            if (teamAssignments.Count != 25)
                throw new InvalidOperationException("Team assignment count mismatch.");

            // shuffle team assignments so positions are randomized independently of words
            teamAssignments = teamAssignments.OrderBy(t => rnd.Next()).ToList();

            // populate cards array
            for (int i = 0; i < 25; i++)
            {
                cards.Add(new Card(teamAssignments[i], selectedNouns[i]));
            }

            Console.Write("Done\n");
        }

        
        public string ToJson()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            return JsonSerializer.Serialize(this, options);
        }
        
        public static Deck FromJson(string json)
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter());
            var deck = JsonSerializer.Deserialize<Deck>(json, options);
            if (deck == null)
                throw new InvalidOperationException("Failed to deserialize Deck from JSON.");
            // ensure we have exactly 25 cards after deserialization
            if (deck.cards == null || deck.cards.Count != 25)
                throw new InvalidOperationException("Deserialized deck must contain exactly 25 cards.");
            return deck;
        }

        public override bool Equals(object? obj)
        {
            Deck? deck = obj as Deck;

            if(deck != null)
                return cards.GetHashCode() == deck.cards.GetHashCode();
            else 
                throw new InvalidOperationException("Object is not a Deck");
        }

        public override int GetHashCode()
        {
            return cards.GetHashCode();
        }

        public string Whostarts()
        {
            int counter = 0;

            foreach (Card item in cards)
            {
                if (item.Team == "Red")
                    counter += 1;
            }

            if (counter == 8)
                return "Red";
            else
                return "Blue";
        }

        public override string ToString()
        {
            StringBuilder SB = new StringBuilder();
            int i = 1;
            foreach (var card in cards)
            {
                SB.AppendLine($"### {i} ###");
                SB.AppendLine(card.Word);
                SB.AppendLine();
                i++;
            }

            return SB.ToString();
        }

        public object Clone()
        {
            List<Card> cards = new List<Card>();
            foreach (var card in this.cards)
            {
                cards.Add(new Card(card.Team, card.Word));
            }
            return new Deck(cards);
        }
    }
}