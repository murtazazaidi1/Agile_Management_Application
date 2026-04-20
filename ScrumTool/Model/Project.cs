namespace ScrumTool.Model
{
    public class Project
    {
        public int Id {
            get;
            set;
        }
        public string Name {
            get;
            set;
        }
        public string Description {
            get;
            set;
        }

        public Project() 
        {
        }

        public Project(int id, string name, string description = "")
        {
            Id = id;
            Name = name;
            Description = description;
        }

        public override string ToString()
        {
            return $"[{Id}] {Name}"; // to return the project's name
        }
    }
}