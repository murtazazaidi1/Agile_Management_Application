using System;
using System.Collections.Generic;

namespace ScrumTool.Model
{
    public enum TaskState // these will be the states in which a task can be
    {
        ToBeDone = 1,
        InProcess = 2,
        Done = 3
    }

    public class ScrumTask
    {
        public int Id 
        { 
            get;
            set;
        }
        public int UserStoryId {
            get; set;
        }
        public string Title {
            get; set;
        }
        public string Description {
            get; set;
        }
        public TaskState State {
            get; set;
        } = TaskState.ToBeDone;
        public int Priority {
            get; set;
        } = 0;
        public double PlannedTime {
            get; set;
        } = 0;
        public double ActualTime {
            get; set;
        } = 0;
        public DateTime? PlannedStartDate {
            get; set;
        }
        public DateTime? PlannedEndDate {
            get; set;
        }
        public DateTime? ActualStartDate {
            get; set;
        }
        public DateTime? ActualEndDate {
            get; set;
        }
        public int Difficulty {
            get; set;
        } = 0;
        public string CategoryLabels {
            get; set;
        } = "";

        public List<int> AssignedPersonIds { get; set; } = new List<int>();

        public ScrumTask() { }

        public ScrumTask(int id, int userStoryId, string title, string description = "")
        {
            Id = id;
            UserStoryId = userStoryId;
            Title = title;
            Description = description;
            State = TaskState.ToBeDone;
        }

        public bool CanTransitionTo(TaskState target, UserStoryState parentStoryState,
            List<ScrumTask> dependencyStoryTasks, out string reason)
        {
            reason = "";

            // tasks in backlog stories stay at ToBeDone
            if (parentStoryState == UserStoryState.ProjectBacklog)
            {
                reason = "This task's story hasn't been pulled into a sprint yet.";
                return target == TaskState.ToBeDone;
            }

            // moving backward is always allowed
            if ((int)target < (int)State)
                return true;

            if (target == TaskState.InProcess)
            {
                foreach (var depTask in dependencyStoryTasks)
                {
                    if (depTask.State != TaskState.Done)
                    {
                        reason = $"'{depTask.Title}' needs to be finished first.";
                        return false;
                    }
                }
                return true;
            }

            return true;
        }

        public override string ToString()
        {
            return $"[{Id}] {Title} ({State})"; // just for printing the id, title and state of task
        }
    }
}
