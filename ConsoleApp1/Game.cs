using System.Data;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace MyNamespace
{
    public class Game
    {
        //Private 
        Deck deck;
        Deck llmDeck;
        string tour;
        Hint lastHint;
        bool gameIsOn;

        //Words in llmPools 
        public int RedTeamLLMWordsCount { get
            {
                int result = 0;
                foreach (Card item in llmDeck.cards)
                {
                    if (item.Team == "Red")
                        result++;
                }

                return result;
            } }
        public int BlueTeamLLMWordsCount { get
            {
                int result = 0;
                foreach (Card item in llmDeck.cards)
                {
                    if (item.Team == "Blue")
                        result++;
                }

                return result;
            } }

        //Messages
        string msg;
        string systemMsg;

        //Points
        int redPoints;
        int bluePoints;

        //Chances
        int redChances;
        int blueChances;

        //Additional
        bool AssasinNotChosen;
        StringBuilder listOfHints;

        //Properties
        public bool GameIsOn { get { return gameIsOn; } }
        public Deck Deck { get { return deck; } }

        public Deck LLMDECK { get { return llmDeck; } }
        public string Tour { get { return tour; } }

        // [####]---> Constructor <---[###]
        public Game(Deck deck)
        {
            this.deck = deck;
            tour = deck.Whostarts();

        }



        // [####]---> SetUp <---[###]
        public void Start()
        {
            FileOp.Write("Decsk/PreviousDeck.txt", deck.ToJson());
            llmDeck = deck.Clone() as Deck;
            if (llmDeck == null)
                throw new GameException("Problem with cloning the deck for LLM");
            
            listOfHints = new StringBuilder();
            listOfHints.AppendLine("####### List of hints #######");
            



            GenerateHint();
            //lastHint = new Hint("Example", 3);
            
            
            gameIsOn = true;
            AssasinNotChosen = true;



            redPoints = 0;
            bluePoints = 0;

            //msg set up
            msg = string.Empty;
            systemMsg = string.Empty;


            Console.WriteLine("\nPress any button to start...\n");
            Console.ReadKey();
            Console.Clear();

            SaveDeckInfo();



        }



        // [####]---> Settings <---[###]
        public void GenerateHint()
        {
            int actualTeamWordsCount = (tour == "Red") ? RedTeamLLMWordsCount : BlueTeamLLMWordsCount;

            if (actualTeamWordsCount > 0)
            {
                if (gameIsOn == true)
                    Console.Clear();
                Console.Write("Generating Hint:   ");



                //SETUP 
                string DeepSeek_api = FileOp.Read("../../API_KEY_DEEPSEEK_IO.txt");
                LLM llm = new LLM(DeepSeek_api);
                string _hintExample = FileOp.Read("HintPrompts/HintJsonExample.txt");
                string _tour = this.Tour;
                string _actualDeckJson = llmDeck.ToJson();

            
                //PROMPT

                string _SystemPrompt = FileOp.Read("HintPrompts/HintSystemPrompt.txt");
                string _UserPromptPrompt = $"_NowTour = {_tour}, _actualDeck = {_actualDeckJson}, _hintExample = {_hintExample}";
                string? response = llm.SendRequestAsync(_SystemPrompt, _UserPromptPrompt).Result;

                if (response == null)
                    throw new GameException("Problem with generating a hint by LLM");
                else
                {
                    Console.Write("Done\n");
                    lastHint = Hint.FromJson(response);
                    listOfHints.AppendLine($"-- {tour} | {lastHint}");

                    DeleteCardsFromLLMDeck(lastHint.usesCards!);


                }

            }
            else
            {
                lastHint = new Hint("All cards have a hint!", 1);
            }
                




        }

        public void DeleteCardsFromLLMDeck(List<Card> usedCards)
        {
            if (llmDeck == null)
                throw new GameException("LLM deck is null.");

            if (llmDeck.cards == null || llmDeck.cards.Count == 0)
                return;

            if (usedCards == null || usedCards.Count == 0)
                return;

            foreach (var used in usedCards)
            {
                if (used == null)
                    continue;

                var usedWord = used.Word?.Trim();
                if (string.IsNullOrEmpty(usedWord))
                    continue;

                var usedTeam = used.Team?.Trim();

                
                llmDeck.cards.RemoveAll(c =>
                    !string.IsNullOrEmpty(c.Word) &&
                    string.Equals(c.Word.Trim(), usedWord, StringComparison.OrdinalIgnoreCase) &&
                    (string.IsNullOrEmpty(usedTeam) || string.Equals(c.Team?.Trim(), usedTeam, StringComparison.OrdinalIgnoreCase))
                );
            }

            systemMsg = llmDeck.cards.Count().ToString();

            
            try
            {
                FileOp.Write("Decsk/LLMDeck.txt", llmDeck.ToJson(), false);
            }
            catch (Exception ex)
            {
                
                throw new GameException("Failed to write updated LLM deck to disk.", ex);
            }
        }

        public void GameKeepGoing()
        {
            int red = 0, blue = 0;

            foreach (Card item in deck.cards)
            {
                if (item.Team == "Red")
                    red++;
                if (item.Team == "Blue")
                    blue++;

            }

            gameIsOn = red != 0 && blue != 0 && AssasinNotChosen;
            //Console.WriteLine($"red != 0 -> {red != 0}");
            //Console.WriteLine($"blue != 0 -> {blue != 0}");
            //Console.WriteLine($"AssasinIsNotChosen -> {AssasinNotChosen}");
            //Console.WriteLine(gameIsOn);
        }

        public void SaveDeckInfo()
        {
            List<Card> RedCards = new List<Card>();
            List<Card> BlueCards = new List<Card>();
            List<Card> AssassinCards = new List<Card>();
            List<Card> NeutralCards = new List<Card>();

            foreach (var card in deck.cards)
            {
                if (card.Team == "Red")
                    RedCards.Add(card);
                if (card.Team == "Blue")
                    BlueCards.Add(card);
                if (card.Team == "Assasin")
                    AssassinCards.Add(card);
                if (card.Team == "Neutral")
                    NeutralCards.Add(card);
            }

            FileOp.Write("Decsk/RedCards.txt", JsonSerializer.Serialize(RedCards, new JsonSerializerOptions { WriteIndented = true }), false);
            FileOp.Write("Decsk/BlueCards.txt", JsonSerializer.Serialize(BlueCards, new JsonSerializerOptions { WriteIndented = true }), false);
            FileOp.Write("Decsk/AssassinCards.txt", JsonSerializer.Serialize(AssassinCards, new JsonSerializerOptions { WriteIndented = true }), false);
            FileOp.Write("Decsk/NeutralCards.txt", JsonSerializer.Serialize(NeutralCards, new JsonSerializerOptions { WriteIndented = true }), false);
            FileOp.Write("Deck.json", deck.ToJson(), false);
            FileOp.Write("LLMDeck.json", llmDeck.ToJson(), false);
            FileOp.Write("Decsk/FullDeck.txt", deck.ToJson(), false);
        }

        public void GameInfo()
        {
            //Setup
            string tabInterface = TabInterface();
            string listOfHintsInterface = listOfHints.ToString();

            Console.WriteLine(deck);
            Console.WriteLine(tabInterface);
            Console.WriteLine(listOfHintsInterface);
            

            Console.WriteLine($"" +
                $"######## Game info ##########\n" +
                $"Tour:\t\t{tour}\n" +
                $"Last hint:\t{lastHint}\n\n");

            Console.WriteLine();
            Console.WriteLine(systemMsg);
            Console.WriteLine(msg);
        }

        public bool WordExistsInDeck(string userGuess)
        {
            foreach (Card item in deck.cards)
            {
                if (item.Word == userGuess)
                    return true;
            }
            return false;
        }

        public string DeleteCard(string userGuess)
        {
            string? cardTeam = string.Empty;
            int cardIndex = -1;
            int cardIndexLLM = -1;

            foreach (Card item in llmDeck.cards)
            {
                if (item.Word == userGuess)
                {
                    cardIndexLLM = deck.cards.IndexOf(item);
                }

            }

            foreach (Card item in deck.cards)
            {
                if (item.Word == userGuess)
                {
                    cardTeam = item.Team;
                    cardIndex = deck.cards.IndexOf(item);
                }
            }

            if (cardIndex >= 0)
                deck.cards.RemoveAt(cardIndex);
            else
                throw new GameException("Problem with deleting card");

            if (cardIndexLLM >= 0)
            {
                FileOp.Write("LLMDeck.txt", llmDeck.ToJson(), false);
                llmDeck.cards.RemoveAt(cardIndexLLM);

            }

            if (cardTeam == null || cardTeam == string.Empty)
                throw new ArgumentNullException("Card not found!");
            else
                return cardTeam;

        }

        public void AddPoint(string team)
        {
            if (team == "Red")
                redPoints++;
            if (team == "Blue")
                bluePoints++;

        }

        public void ClearMsg()
        {
            msg = string.Empty;
        }



        // [####]---> Gameplay <---[###]

        public void UserGiveAnswer()
        {
            int teamChances = (tour == "Red") ? redChances : blueChances;


            int tourChances = lastHint.NoumberOfSimilarWords + teamChances;
            bool answering = true;

            while (answering && tourChances > 0)
            {
                Console.Clear();
                GameInfo();
                Console.WriteLine($"Chances: {tourChances}");


                Console.Write("User guess: ");
                string? userGuess = Console.ReadLine();

                if (userGuess == null || userGuess == string.Empty)
                    msg = "You need to type an answer!\n";
                else if (!WordExistsInDeck(userGuess))
                    msg = "Word does not exists in the Deck!\n";
                else
                {
                    string teamOfChosenCard = DeleteCard(userGuess);
                    AddPoint(teamOfChosenCard);

                    if (teamOfChosenCard == "Assasin")
                    {
                        msg = $"You chose the Assassin card! Game Over!\n";
                        THE_END();
                        answering = false;
                    }
                    

                    if (tour == teamOfChosenCard)
                    {
                        tourChances--;
                        msg = $"Good choice! The card was of team: {teamOfChosenCard}\nYou have {tourChances} chances left.\n";
                        Console.Clear();
                        GameInfo();

                    }
                    else
                    {
                        msg = $"You chose a card of the other team! The card was of team: {teamOfChosenCard}\n";
                        Console.Clear();
                        GameInfo();
                        Console.ReadKey();
                        answering = false;

                    }

                }



            }

            if (gameIsOn)
            {
                if (tour == "Red")
                    redChances = tourChances;
                else if (tour == "Blue")
                    blueChances = tourChances;

                Console.Clear();
                SaveDeckInfo();
                
                ClearMsg();
                nextTour();
                GenerateHint();


                GameInfo();

            }


        }

        public void nextTour()
        {
            if (tour == "Red")
                tour = "Blue";
            else if (tour == "Blue")
                tour = "Red";
        }



        //[####]---> Interfaces <---[###]

        public string TabInterface()
        {
            string tabDuringGame = $"" +
                $"\t ------------ TAB ------------\n" +
                $"\t Red: {redPoints}\n" +
                $"\t Blue: {bluePoints}\n" +
                $"\t -----------------------------\n\n";

            return tabDuringGame;
        }

        

        




        // [####]---> Else <---[###]
        public void Test()
        {
            
        }

        public void THE_END()
        {
            Console.WriteLine("THE END");
            gameIsOn = false;
            AssasinNotChosen = false;

        }

        public void Tabble()
        {
            Console.WriteLine("\n\n\n\n\t\tTHE END OF THE GAME\n");
            Console.WriteLine($"" +
                $"\t\t ------------ TAB ------------\n" +
                $"\t\t Red: {redPoints}\n" +
                $"\t\t Blue: {bluePoints}\n" +
                $"\t\t -----------------------------\n\n");
        }

        




        [Serializable]
        public class GameException : Exception
        {
            public GameException() { }
            public GameException(string message) : base(message) { }
            public GameException(string message, Exception inner) : base(message, inner) { }
            protected GameException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }


    }
}