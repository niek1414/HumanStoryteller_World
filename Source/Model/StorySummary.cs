namespace HumanStoryteller.Model {
    public class StorySummary {
        public StorySummary(long id, string name, string description, string creator, string avatar, float rating) {
            Id = id;
            Name = name;
            Description = description;
            Creator = creator;
            Avatar = avatar;
            Rating = rating;
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Creator { get; set; }
        public string Avatar { get; set; }
        public float Rating { get; set; }
    }
}