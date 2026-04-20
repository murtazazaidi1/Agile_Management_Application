using ScrumTool.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScrumTool.Controller
{
    public class ScrumController
    {
        private readonly SqliteDatabase _db;

        public ScrumController(SqliteDatabase database)
        {
            _db = database ?? throw new ArgumentNullException(nameof(database));
        }

        // ---- projects ----

        public (bool success, string message, int id) CreateProject(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                return (false, "Project name can't be empty.", 0);
            int id = _db.AddProject(name.Trim(), description?.Trim() ?? "");
            return (true, "Project created.", id);
        }

        public (bool success, string message) EditProject(int id, string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                return (false, "Name can't be empty.");
            if (_db.GetProject(id) == null)
                return (false, $"No project found with id {id}.");
            _db.UpdateProject(id, name.Trim(), description?.Trim() ?? "");
            return (true, "Project updated.");
        }

        public (bool success, string message) RemoveProject(int id)
        {
            if (_db.GetProject(id) == null)
                return (false, $"No project found with id {id}.");
            _db.DeleteProject(id);
            return (true, "Project deleted.");
        }

        public List<Project> ListProjects() => _db.GetAllProjects();
        public Project GetProject(int id) => _db.GetProject(id);

        // ---- persons ----

        public (bool success, string message, int id) CreatePerson(string name, string role)
        {
            if (string.IsNullOrWhiteSpace(name)) return (false, "Name can't be empty.", 0);
            if (string.IsNullOrWhiteSpace(role)) return (false, "Role can't be empty.", 0);
            int id = _db.AddPerson(name.Trim(), role.Trim());
            return (true, "Person added.", id);
        }

        public (bool success, string message) EditPerson(int id, string name, string role)
        {
            if (string.IsNullOrWhiteSpace(name)) return (false, "Name can't be empty.");
            if (string.IsNullOrWhiteSpace(role)) return (false, "Role can't be empty.");
            if (_db.GetPerson(id) == null) return (false, $"Person {id} not found.");
            _db.UpdatePerson(id, name.Trim(), role.Trim());
            return (true, "Person updated.");
        }

        public (bool success, string message) RemovePerson(int id)
        {
            if (_db.GetPerson(id) == null) return (false, $"Person {id} not found.");
            _db.DeletePerson(id);
            return (true, "Person removed.");
        }

        public List<Person> ListPersons() => _db.GetAllPersons();
        public Person GetPerson(int id) => _db.GetPerson(id);

        public (bool success, string message) LinkPersonToProject(int personId, int projectId)
        {
            if (_db.GetPerson(personId) == null) return (false, "Person not found.");
            if (_db.GetProject(projectId) == null) return (false, "Project not found.");
            _db.LinkPersonToProject(personId, projectId);
            return (true, "Person linked to project.");
        }

        public (bool success, string message) UnlinkPersonFromProject(int personId, int projectId)
        {
            _db.UnlinkPersonFromProject(personId, projectId);
            return (true, "Person removed from project.");
        }

        public List<Person> GetPersonsForProject(int projectId) => _db.GetPersonsForProject(projectId);

        // ---- user stories ----

        public (bool success, string message, int id) CreateUserStory(int projectId, string title, string content, int priority)
        {
            if (_db.GetProject(projectId) == null) return (false, "Project not found.", 0);
            if (string.IsNullOrWhiteSpace(title)) return (false, "Title can't be empty.", 0);
            int id = _db.AddUserStory(projectId, title.Trim(), content?.Trim() ?? "", priority);
            return (true, "User story created.", id);
        }

        public (bool success, string message) EditUserStory(int id, string title, string content, int priority)
        {
            var story = _db.GetUserStory(id);
            if (story == null) return (false, "User story not found.");
            if (story.State == UserStoryState.Done) return (false, "Can't edit a story that's already done.");
            if (string.IsNullOrWhiteSpace(title)) return (false, "Title can't be empty.");
            _db.UpdateUserStory(id, title.Trim(), content?.Trim() ?? "", priority, story.State);
            return (true, "User story updated.");
        }

        public (bool success, string message) RemoveUserStory(int id)
        {
            if (_db.GetUserStory(id) == null) return (false, "User story not found.");
            _db.DeleteUserStory(id); // cascades to tasks and dependencies
            return (true, "User story deleted along with its tasks.");
        }

        public List<UserStory> ListUserStories(int projectId) => _db.GetUserStoriesForProject(projectId);
        public UserStory GetUserStory(int id) => _db.GetUserStory(id);

        public (bool success, string message) MoveUserStoryToState(int id, UserStoryState targetState)
        {
            var story = _db.GetUserStory(id);
            if (story == null) return (false, "User story not found.");

            var deps = _db.GetDependenciesForUserStory(id);
            var tasks = _db.GetTasksForUserStory(id);

            if (!story.CanTransitionTo(targetState, deps, tasks, out string reason))
                return (false, reason);

            _db.UpdateUserStoryState(id, targetState);

            // reset all tasks whenever the story moves in or out of sprint
            if (targetState == UserStoryState.InSprint || targetState == UserStoryState.ProjectBacklog)
            {
                foreach (var task in tasks)
                    _db.UpdateTaskState(task.Id, TaskState.ToBeDone);
            }

            return (true, $"Story moved to {targetState}.");
        }

        public (bool success, string message) AddUserStoryDependency(int storyId, int dependsOnId)
        {
            if (storyId == dependsOnId) return (false, "A story can't depend on itself.");
            if (_db.GetUserStory(storyId) == null) return (false, "User story not found.");
            if (_db.GetUserStory(dependsOnId) == null) return (false, "Dependency story not found.");
            _db.AddUserStoryDependency(storyId, dependsOnId);
            return (true, "Dependency added.");
        }

        public (bool success, string message) RemoveUserStoryDependency(int storyId, int dependsOnId)
        {
            _db.RemoveUserStoryDependency(storyId, dependsOnId);
            return (true, "Dependency removed.");
        }

        // ---- tasks ----

        public (bool success, string message, int id) CreateTask(int userStoryId, string title, string description,
            int priority, double plannedTime, DateTime? plannedStart, DateTime? plannedEnd,
            int difficulty, string categoryLabels)
        {
            var story = _db.GetUserStory(userStoryId);
            if (story == null) return (false, "User story not found.", 0);
            if (story.State == UserStoryState.Done) return (false, "Can't add tasks to a story that's already done.", 0);
            if (string.IsNullOrWhiteSpace(title)) return (false, "Task title can't be empty.", 0);
            if (plannedTime < 0) return (false, "Planned time can't be negative.", 0);

            int id = _db.AddTask(userStoryId, title.Trim(), description?.Trim() ?? "",
                priority, plannedTime, plannedStart, plannedEnd, difficulty, categoryLabels?.Trim() ?? "");
            return (true, "Task created.", id);
        }

        public (bool success, string message) EditTask(int id, string title, string description,
            int priority, double plannedTime, double actualTime,
            DateTime? plannedStart, DateTime? plannedEnd,
            DateTime? actualStart, DateTime? actualEnd,
            int difficulty, string categoryLabels)
        {
            var task = _db.GetTask(id);
            if (task == null) return (false, "Task not found.");
            if (string.IsNullOrWhiteSpace(title)) return (false, "Title can't be empty.");
            if (plannedTime < 0 || actualTime < 0) return (false, "Time values can't be negative.");

            _db.UpdateTask(id, title.Trim(), description?.Trim() ?? "", priority,
                plannedTime, actualTime, plannedStart, plannedEnd, actualStart, actualEnd,
                difficulty, categoryLabels?.Trim() ?? "", task.State);
            return (true, "Task updated.");
        }

        public (bool success, string message) RemoveTask(int id)
        {
            if (_db.GetTask(id) == null) return (false, "Task not found.");
            _db.DeleteTask(id);
            return (true, "Task deleted.");
        }

        public List<ScrumTask> ListTasks(int userStoryId) => _db.GetTasksForUserStory(userStoryId);
        public ScrumTask GetTask(int id) => _db.GetTask(id);

        public (bool success, string message) MoveTaskToState(int taskId, TaskState targetState)
        {
            var task = _db.GetTask(taskId);
            if (task == null) return (false, "Task not found.");

            var story = _db.GetUserStory(task.UserStoryId);
            if (story == null) return (false, "Couldn't find the parent user story.");

            var depStoryTasks = new List<ScrumTask>();
            foreach (int depId in story.DependsOnIds)
                depStoryTasks.AddRange(_db.GetTasksForUserStory(depId));

            if (!task.CanTransitionTo(targetState, story.State, depStoryTasks, out string reason))
                return (false, reason);

            _db.UpdateTaskState(taskId, targetState);
            return (true, $"Task moved to {targetState}.");
        }

        public (bool success, string message) AssignPersonToTask(int taskId, int personId)
        {
            var task = _db.GetTask(taskId);
            if (task == null) return (false, "Task not found.");
            if (_db.GetPerson(personId) == null) return (false, "Person not found.");

            var story = _db.GetUserStory(task.UserStoryId);
            var projectPersons = _db.GetPersonsForProject(story.ProjectId);
            if (!projectPersons.Exists(p => p.Id == personId))
                return (false, "That person isn't part of this project.");

            _db.AssignPersonToTask(taskId, personId);
            return (true, "Person assigned.");
        }

        public (bool success, string message) RemovePersonFromTask(int taskId, int personId)
        {
            if (_db.GetTask(taskId) == null) return (false, "Task not found.");
            _db.RemovePersonFromTask(taskId, personId);
            return (true, "Person removed from task.");
        }

        public (bool success, string message) ChangeTaskPriority(int taskId, int priority)
        {
            var task = _db.GetTask(taskId);
            if (task == null) return (false, "Task not found.");
            _db.UpdateTask(task.Id, task.Title, task.Description, priority,
                task.PlannedTime, task.ActualTime,
                task.PlannedStartDate, task.PlannedEndDate,
                task.ActualStartDate, task.ActualEndDate,
                task.Difficulty, task.CategoryLabels, task.State);
            return (true, "Priority updated.");
        }

        // ---- teams ----

        public (bool success, string message, int id) CreateTeam(string name, int projectId)
        {
            if (string.IsNullOrWhiteSpace(name)) return (false, "Team name can't be empty.", 0);
            if (_db.GetProject(projectId) == null) return (false, "Project not found.", 0);
            int id = _db.AddTeam(name.Trim(), projectId);
            return (true, "Team created.", id);
        }

        public (bool success, string message) EditTeam(int id, string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return (false, "Team name can't be empty.");
            if (_db.GetTeam(id) == null) return (false, "Team not found.");
            _db.UpdateTeam(id, name.Trim());
            return (true, "Team updated.");
        }

        public (bool success, string message) RemoveTeam(int id)
        {
            if (_db.GetTeam(id) == null) return (false, "Team not found.");
            _db.DeleteTeam(id);
            return (true, "Team deleted.");
        }

        public List<Team> ListTeams(int projectId) => _db.GetTeamsForProject(projectId);

        public (bool success, string message) AddPersonToTeam(int teamId, int personId)
        {
            var team = _db.GetTeam(teamId);
            if (team == null) return (false, "Team not found.");
            if (_db.GetPerson(personId) == null) return (false, "Person not found.");

            var projectPersons = _db.GetPersonsForProject(team.ProjectId);
            if (!projectPersons.Exists(p => p.Id == personId))
                return (false, "Person isn't part of this project yet.");

            _db.AddPersonToTeam(teamId, personId);
            return (true, "Person added to team.");
        }

        public (bool success, string message) RemovePersonFromTeam(int teamId, int personId)
        {
            if (_db.GetTeam(teamId) == null) return (false, "Team not found.");
            _db.RemovePersonFromTeam(teamId, personId);
            return (true, "Person removed from team.");
        }

        public List<Person> GetPersonsForTeam(int teamId) => _db.GetPersonsForTeam(teamId);

        // ---- reports ----

        public ProjectReport GetProjectReport(int projectId)
        {
            var project = _db.GetProject(projectId);
            if (project == null) return null;
            var stories = _db.GetUserStoriesForProject(projectId);
            int done = stories.Count(s => s.State == UserStoryState.Done);
            double rate = stories.Count > 0 ? (double)done / stories.Count : 0;
            return new ProjectReport { Project = project, UserStories = stories, CompletionRate = rate };
        }

        public SprintReport GetSprintReport(int projectId)
        {
            var project = _db.GetProject(projectId);
            if (project == null) return null;

            var sprintStories = _db.GetUserStoriesForProject(projectId)
                                   .Where(s => s.State == UserStoryState.InSprint).ToList();

            var allTasks = new List<ScrumTask>();
            foreach (var s in sprintStories)
                allTasks.AddRange(_db.GetTasksForUserStory(s.Id));

            int doneTasks = allTasks.Count(t => t.State == TaskState.Done);
            double real = allTasks.Count > 0 ? (double)doneTasks / allTasks.Count : 0;

            var today = DateTime.Today;
            int plannedDone = allTasks.Count(t => t.PlannedEndDate.HasValue && t.PlannedEndDate.Value.Date <= today);
            double plannedRate = allTasks.Count > 0 ? (double)plannedDone / allTasks.Count : 0;

            return new SprintReport
            {
                Project = project,
                SprintStories = sprintStories,
                AllSprintTasks = allTasks,
                RealCompletionRate = real,
                PlannedCompletionRate = plannedRate,
                TotalPlannedTime = allTasks.Sum(t => t.PlannedTime),
                TotalActualTime = allTasks.Sum(t => t.ActualTime)
            };
        }

        public UserStoryReport GetUserStoryReport(int userStoryId)
        {
            var story = _db.GetUserStory(userStoryId);
            if (story == null) return null;
            var tasks = _db.GetTasksForUserStory(userStoryId);

            int done = tasks.Count(t => t.State == TaskState.Done);
            double real = tasks.Count > 0 ? (double)done / tasks.Count : 0;

            var today = DateTime.Today;
            int pastDeadline = tasks.Count(t => t.PlannedEndDate.HasValue && t.PlannedEndDate.Value.Date <= today);
            double planned = tasks.Count > 0 ? (double)pastDeadline / tasks.Count : 0;

            return new UserStoryReport
            {
                Story = story,
                Tasks = tasks,
                RealCompletionRate = real,
                PlannedCompletionRate = planned,
                TotalPlannedTime = tasks.Sum(t => t.PlannedTime),
                TotalActualTime = tasks.Sum(t => t.ActualTime)
            };
        }

        public TaskReport GetTaskReport(int taskId)
        {
            var task = _db.GetTask(taskId);
            if (task == null) return null;
            return new TaskReport
            {
                Task = task,
                ParentStory = _db.GetUserStory(task.UserStoryId),
                AssignedPersons = _db.GetPersonsForTask(taskId)
            };
        }

        public PersonReport GetPersonReport(int personId)
        {
            var person = _db.GetPerson(personId);
            if (person == null) return null;
            var projects = _db.GetProjectsForPerson(personId);
            var report = new PersonReport { Person = person };
            foreach (var p in projects)
            {
                var tasks = _db.GetTasksForPersonInProject(personId, p.Id);
                report.ProjectTasks.Add((p, tasks));
            }
            return report;
        }
    }
}