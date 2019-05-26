namespace HumanStoryteller.Model {
    public class StorySummary {
        public StorySummary(long id, string name, string description, string username, string badges, long creator, string avatar, float rating, long votes, bool featured) {
            Id = id;
            Name = name;
            Description = description;
            Creator = creator;
            Badges = badges;
            Username = username;
            Avatar = avatar;
            Rating = rating;
            Votes = votes;
            Featured = featured;
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public long Creator { get; set; }
        public string Badges { get; set; }
        public string Username { get; set; }
        public string Avatar { get; set; }
        public float Rating { get; set; }
        public long Votes { get; set; }
        public bool Featured { get; set; }
    }
}