namespace ScrumTool.Model
{
    public class Person
    {
        public int Id {
            get;
            set;
        }
        public string Name {
            get;
            set;
        }
        public string Role {
            get;
            set;
        }

        public Person() 
        {
        
        }

        public Person(int id, string name, string role)
        {
            Id = id;
            Name = name;
            Role = role;
        }

        public override string ToString()
        {
            return $"[{Id}] {Name} ({Role})"; // to return the person's name and role 
        }
    }
}