using System.Collections.Generic;

namespace ScrumTool.Model
{
    public class Team
    {
        public int Id 
        { 
            get;
            set;
        }
        public string Name {
            get;
            set;
        }
        public int ProjectId {
            get;
            set;
        }

        public List<int> MemberIds { get; set; } = new List<int>();

        public Team() 
        { 
        }

        public Team(int id, string name, int projectId) // to set the id, name and project id when creating a team
        {
            Id = id;
            Name = name;
            ProjectId = projectId; 
        }

        public override string ToString()
        {
            return $"[{Id}] {Name}"; // to return the team's name
        }
    }
}