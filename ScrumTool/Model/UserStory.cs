using System.Collections.Generic;

namespace ScrumTool.Model
{
    public enum UserStoryState // I have added this enum to make it easier to manage the state of user stories
    {
        ProjectBacklog = 1,
        InSprint = 2,
        Done = 3
    }

    public class UserStory
    {
        public int Id {
            get; 
            set;
        }
        public int ProjectId {
            get; 
            set; }
        public string Title {
            get; 
            set; }
        public string Content {
            get; 
            set; }
        public UserStoryState State {
            get; 
            set; } = UserStoryState.ProjectBacklog; // I have added so the default will be project backlog
        public int Priority {
            get; 
            set; } = 0;

        // ids of stories this one depends on
        public List<int> DependsOnIds { get; set; } = new List<int>();

        public UserStory() { }

        public UserStory(int id, int projectId, string title, string content = "", int priority = 0)
        {
            Id = id;
            ProjectId = projectId;
            Title = title;
            Content = content;
            Priority = priority;
            State = UserStoryState.ProjectBacklog; // default state will be backlog
        }

        // checks whether transitioning to the given state is allowed
        public bool CanTransitionTo(UserStoryState target, List<UserStory> dependencies, List<ScrumTask> linkedTasks, out string reason)
        {
            reason = "";

            // going backward is always fine
            if ((int)target < (int)State)
                return true;

            if (State == UserStoryState.ProjectBacklog && target == UserStoryState.InSprint)
            {
                foreach (var dep in dependencies)
                {
                    if (dep.State == UserStoryState.ProjectBacklog)
                    {
                        reason = $"'{dep.Title}' must be in sprint or done before this story can start.";
                        return false;
                    }
                }
                return true;
            }

            if (target == UserStoryState.Done)
            {
                foreach (var task in linkedTasks)
                {
                    if (task.State != TaskState.Done)
                    {
                        reason = $"Task '{task.Title}' still isn't done.";
                        return false;
                    }
                }
                return true;
            }

            return true;
        }

        public override string ToString()
        {
            return $"[{Id}] {Title} ({State})"; // it will just return the id, title and state of the user story
        }
    }
}