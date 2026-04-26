using System;
using System.Collections.Generic;

namespace ScrumTool.Model
{
    public class ProjectReport
    {
        public Project Project {
            get;
            set;
        }
        public List<UserStory> UserStories { get; set; } = new List<UserStory>();
        public double CompletionRate 
        { 
            get;
            set;
        }

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            int doneCount = UserStories.FindAll(s => s.State == UserStoryState.Done).Count;
            sb.AppendLine($"Project Report: {Project.Name}");
            sb.AppendLine($"  {Project.Description}");
            sb.AppendLine($"  Progress: {CompletionRate:P1}  ({doneCount}/{UserStories.Count} stories done)");
            sb.AppendLine();
            sb.AppendLine("Stories:");
            foreach (var s in UserStories)
                sb.AppendLine($"  {s}");
            return sb.ToString();
        }
    }

    public class SprintReport
    {
        public Project Project 
        { 
            get;
            set;
        }
        public List<UserStory> SprintStories { get; set; } = new List<UserStory>();
        public List<ScrumTask> AllSprintTasks { get; set; } = new List<ScrumTask>();
        public double RealCompletionRate 
        { 
            get;
            set;
        }
        public double PlannedCompletionRate 
        { 
            get;
            set;
        } = 0;
        public double TotalPlannedTime 
        { 
            get;
            set;
        }
        public double TotalActualTime 
        { 
            get;
            set;
        }

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Sprint Report: {Project.Name}");
            sb.AppendLine($"  Actual completion:  {RealCompletionRate:P1}");
            sb.AppendLine($"  Planned completion: {PlannedCompletionRate:P1}");
            sb.AppendLine($"  Time — planned: {TotalPlannedTime}h  actual: {TotalActualTime}h");
            sb.AppendLine();
            sb.AppendLine("Stories in sprint:");
            foreach (var s in SprintStories)
                sb.AppendLine($"  {s}");
            return sb.ToString();
        }
    }

    public class UserStoryReport
    {
        public UserStory Story { get; set; }
        public List<ScrumTask> Tasks { get; set; } = new List<ScrumTask>();
        public double RealCompletionRate { get; set; }
        public double PlannedCompletionRate { get; set; }
        public double TotalPlannedTime { get; set; }
        public double TotalActualTime { get; set; }

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"User Story: {Story.Title}");
            sb.AppendLine($"  State: {Story.State}   Priority: {Story.Priority}");
            sb.AppendLine($"  {Story.Content}");
            sb.AppendLine($"  Actual completion:  {RealCompletionRate:P1}");
            sb.AppendLine($"  Planned completion: {PlannedCompletionRate:P1}");
            sb.AppendLine($"  Time — planned: {TotalPlannedTime}h  actual: {TotalActualTime}h");
            sb.AppendLine();
            sb.AppendLine("Tasks:");
            foreach (var t in Tasks)
                sb.AppendLine($"  {t}");
            return sb.ToString();
        }
    }

    public class TaskReport
    {
        public ScrumTask Task { get; set; }
        public UserStory ParentStory { get; set; }
        public List<Person> AssignedPersons { get; set; } = new List<Person>();

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Task: {Task.Title}");
            sb.AppendLine($"  State: {Task.State}   Priority: {Task.Priority}   Difficulty: {Task.Difficulty}");
            sb.AppendLine($"  {Task.Description}");
            if (!string.IsNullOrEmpty(Task.CategoryLabels))
                sb.AppendLine($"  Labels: {Task.CategoryLabels}");
            sb.AppendLine($"  Time — planned: {Task.PlannedTime}h  actual: {Task.ActualTime}h");
            sb.AppendLine($"  Planned: {Task.PlannedStartDate?.ToShortDateString() ?? "?"} → {Task.PlannedEndDate?.ToShortDateString() ?? "?"}");
            sb.AppendLine($"  Actual:  {Task.ActualStartDate?.ToShortDateString() ?? "?"} → {Task.ActualEndDate?.ToShortDateString() ?? "?"}");
            sb.AppendLine($"  Story: {ParentStory?.Title ?? "N/A"}");
            sb.AppendLine("  Assigned to:");
            foreach (var p in AssignedPersons)
                sb.AppendLine($"    {p}");
            return sb.ToString();
        }
    }

    public class PersonReport
    {
        public Person Person { get; set; }
        public List<(Project project, List<ScrumTask> tasks)> ProjectTasks { get; set; }
            = new List<(Project, List<ScrumTask>)>();

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Person: {Person.Name} — {Person.Role}"); // I added stringbuilder here because it was easy and if someone wants to do it other way I'm okay with that
            foreach (var (project, tasks) in ProjectTasks)
            {
                sb.AppendLine($"  {project.Name}");
                foreach (var t in tasks)
                    sb.AppendLine($"    {t}");
            }
            return sb.ToString();
        }
    }
}