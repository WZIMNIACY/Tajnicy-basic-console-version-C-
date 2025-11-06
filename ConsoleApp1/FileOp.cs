using System.Text;

namespace MyNamespace
{
    static public class FileOp
    {

        static public string basic = "D:\\OneDrive\\Studia\\Rok 2\\Semestr 3\\Inzynieria Oprogramowania\\Kodowanie\\Testowanie AI\\Tajniacy\\Notatki\\";
        public static string Read(string path)
        {
            path = basic + path;
            
            using (StreamReader SR = new(path))
            {
                StringBuilder sb = new StringBuilder();
                string? Line = SR.ReadLine();

                while(Line != null)
                {
                    sb.Append(Line);
                    Line = SR.ReadLine();
                }

                return sb.ToString();

            }
        }

        public static void Write(string path, string text, bool ov = false)
        {
            path = basic + path;

            //ov = false -> override on
            //ov = true -> ovveride off

            using (StreamWriter SW = new StreamWriter(path, ov))
            {
                if (!File.Exists(path))
                    File.Create(path);
                SW.Write(text);
            }
        }


        [Serializable]
        public class FileOpException : Exception
        {
            public FileOpException() { }
            public FileOpException(string message) : base(message) { }
            public FileOpException(string message, Exception inner) : base(message, inner) { }
            protected FileOpException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }
    }
}