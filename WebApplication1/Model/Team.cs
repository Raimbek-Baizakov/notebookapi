namespace WebApplication1
{
    public class Team
    {
        public Team()
        {

        }
        public Team(string FullName, string PathImage, string Description, string PositionName)
        {
            this.FullName = FullName;
            this.PathImage = PathImage;
            this.Description = Description;
            this.PositionName = PositionName;
        }
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public string FullName { get; set; }
        public string PathImage { get; set; }
        public string Description { get; set; }
        public string PositionName { get; set; }
    }
}