namespace MW_Chat_ASP_NET_CORE_WEB_API_SignalR.Models
{
    public class User
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string? ConnectionId { get; set; }
        public bool Status { get; set; } = false;

        //public override bool Equals(object? obj)
        //{
        //    User user2 = obj as User ?? new User();
        //    return (user2.Name == this.Name && user2.Password == this.Password);
        //}
        //public static bool operator == (User user1, User user2)
        //{
        //    return (user1?.Name == user2?.Name && user1?.Password == user2?.Password);
        //}
        //public static bool operator !=(User user1, User user2)
        //{
        //    return !(user1 == user2);
        //}
    }
}
