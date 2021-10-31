namespace MeiyounaiseSlash.Data.Models
{
    public class Message
    {
        public ulong Id { get; set; }
        public ulong IdInBoard { get; set; }
        public bool HasBeenSent { get; set; }
    }
}