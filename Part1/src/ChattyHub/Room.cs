namespace ChattyHub
{
    public class Room
    {
        public Room(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; }
        public string Name { get; }
    }

    public class Message
    {
        public Message(string userName, string text)
        {
            UserName = userName;
            Text = text;
        }

        public string UserName { get; }
        public string Text { get; }
    }
}