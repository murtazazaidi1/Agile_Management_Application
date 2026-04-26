using ScrumTool.Controller;
using ScrumTool.Model;
using System;
using System.Collections.Generic;

namespace ScrumTool.View
{
    public class ConsoleView
    {
        private readonly ScrumController _ctrl;
        private int? _currentProjectId = null;

        public ConsoleView(ScrumController controller)
        {
            _ctrl = controller ?? throw new ArgumentNullException(nameof(controller));
        }

        public void Run()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("⋆⁺｡˚⋆˙‧₊✩₊‧˙⋆˚｡⁺⋆ ⋆⁺｡˚⋆˙‧₊✩₊‧˙⋆˚｡⁺⋆");
            Console.WriteLine("Agile Project Management Tool - Group OK");

            Console.WriteLine("⠀⠀⢀⣀⡀⠘⢀⣀⠀⣀⠀⠀⠀⠀⣠⡀\r\n⠠⡪⠁⠄⢀⠟⠁⠀⠀⠀⠈⠢⠀⠀⠙⠁\r\n⠀⠑⠄⡑⢌⡀⠀⠀⠀⠀⠀⠀⡗⠠⡀⠀\r\n⠀⠀⠀⠈⠒⡬⢐⠢⠄⣀⠀⢠⠃⠱⡈⠢\r\n⠀⠀⠀⠀⠀⠈⠒⠨⠥⠶⠆⠩⠭⠥⠤⠐\r\n⠀⠀⠀⠀⠀⠀⠀⠀⠀⢰⡧⠀⠀⠀⠀");

            bool running = true;
            while (running)
            {
                ShowMainMenu();
                var input = Console.ReadLine()?.Trim().ToLower();

                switch (input)
                {
                    case "1": ManageProjects(); break;
                    case "2": ManagePersons(); break;
                    case "3":
                        if (!EnsureProjectSelected()) break;
                        ManageUserStories();
                        break;
                    case "4":
                        if (!EnsureProjectSelected()) break;
                        ManageTasks();
                        break;
                    case "5": ShowReports(); break;
                    case "6":
                        if (!EnsureProjectSelected()) break;
                        ManageTeams();
                        break;
                    case "q":
                        running = false;
                        break;
                    default:
                        Print("Unknown option, try again.");
                        break;
                }
            }

            Console.WriteLine("Kiitos for using the tool, Bye!");
        }

        private void ShowMainMenu()
        {
            string proj = _currentProjectId.HasValue
                ? $"(project: {_ctrl.GetProject(_currentProjectId.Value)?.Name})"
                : "(no project selected)";

            Console.WriteLine();
            Console.WriteLine($"⊹₊ ⋆ᯓ★ Main Menu {proj} ᯓ★⋆ ⊹₊");
            Console.WriteLine("1. ✧ Projects");
            Console.WriteLine("2. ✧ Persons");
            Console.WriteLine("3. ✧ User Stories (requires project)");
            Console.WriteLine("4. ✧ Tasks (requires project)");
            Console.WriteLine("5. ✧ Reports");
            Console.WriteLine("6. ✧ Teams (requires project)");
            Console.WriteLine("Q. ✧ Quit");
            Console.Write("> ");
        }

        private void ManageProjects()
        {
            bool back = false;
            while (!back)
            {
                Console.WriteLine("\n⋆⁺｡˚⋆˙‧₊☾ Projects ☽₊‧˙⋆˚｡⁺⋆ ");
                Console.WriteLine("1. ✧ List projects");
                Console.WriteLine("2. ✧ Create project");
                Console.WriteLine("3. ✧ Edit project");
                Console.WriteLine("4. ✧ Delete project");
                Console.WriteLine("5. ✧ Select active project");
                Console.WriteLine("6. ✧ Link person to project");
                Console.WriteLine("7. ✧ Unlink person from project");
                Console.WriteLine("B. ✧ Back");
                Console.Write("> ");

                switch (Console.ReadLine()?.Trim().ToLower())
                {
                    case "1":
                        var projs = _ctrl.ListProjects();
                        if (projs.Count == 0) { Print("No projects yet."); break; }
                        foreach (var p in projs)
                            Console.WriteLine($"  [{p.Id}] {p.Name} — {p.Description}");
                        break;

                    case "2":
                        string name = Prompt("Name: ");
                        string desc = Prompt("Description: ");
                        var (ok, msg, newId) = _ctrl.CreateProject(name, desc);
                        Print(msg);
                        if (ok) Print($"Created with ID {newId}");
                        break;

                    case "3":
                        int projectId = PromptInt("Project ID: ");
                        string newName = Prompt("New name: ");
                        string newDesc = Prompt("New description: ");
                        Print(_ctrl.EditProject(projectId, newName, newDesc).message);
                        break;

                    case "4":
                        // TODO: maybe ask for confirmation here someday
                        int deleteId = PromptInt("Project ID: ");
                        Print(_ctrl.RemoveProject(deleteId).message);
                        break;

                    case "5":
                        int activateId = PromptInt("Project ID: ");
                        var project = _ctrl.GetProject(activateId);
                        if (project == null) { Print("Couldn't find that project."); break; }
                        _currentProjectId = activateId;
                        Print($"Now working on: {project.Name}");
                        break;

                    case "6":
                        int personId = PromptInt("Person ID: ");
                        int linkProjectId = PromptInt("Project ID: ");
                        Print(_ctrl.LinkPersonToProject(personId, linkProjectId).message);
                        break;

                    case "7":
                        int unlinkPersonId = PromptInt("Person ID: ");
                        int unlinkProjectId = PromptInt("Project ID: ");
                        Print(_ctrl.UnlinkPersonFromProject(unlinkPersonId, unlinkProjectId).message);
                        break;

                    case "b": back = true; break;
                    default: Print("Invalid."); break;
                }
            }
        }

        private void ManagePersons()
        {
            bool back = false;
            while (!back)
            {
                Console.WriteLine("\n⋆⁺｡˚⋆˙‧₊☾ Persons ☽₊‧˙⋆˚｡⁺⋆");
                Console.WriteLine("1. ✧ List all persons");
                Console.WriteLine("2. ✧ Add person");
                Console.WriteLine("3. ✧ Edit person");
                Console.WriteLine("4. ✧ Delete person");
                Console.WriteLine("5. ✧ List persons in current project");
                Console.WriteLine("B. ✧ Back");
                Console.Write("> ");

                switch (Console.ReadLine()?.Trim().ToLower())
                {
                    case "1":
                        var all = _ctrl.ListPersons();
                        if (all.Count == 0) { Print("No persons yet."); break; }
                        foreach (var p in all)
                            Console.WriteLine($"  {p}");
                        break;

                    case "2":
                        string personName = Prompt("Name: ");
                        string role = Prompt("Role: ");
                        var (ok, msg, newId) = _ctrl.CreatePerson(personName, role);
                        Print(msg);
                        if (ok) Print($"ID: {newId}");
                        break;

                    case "3":
                        int editId = PromptInt("Person ID: ");
                        string updatedName = Prompt("New name: ");
                        string updatedRole = Prompt("New role: ");
                        Print(_ctrl.EditPerson(editId, updatedName, updatedRole).message);
                        break;

                    case "4":
                        int deleteId = PromptInt("Person ID: ");
                        Print(_ctrl.RemovePerson(deleteId).message);
                        break;

                    case "5":
                        if (!EnsureProjectSelected()) break;
                        var inProject = _ctrl.GetPersonsForProject(_currentProjectId.Value);
                        if (inProject.Count == 0) { Print("Nobody linked to this project yet."); break; }
                        foreach (var p in inProject)
                            Console.WriteLine($"  {p}");
                        break;

                    case "b": back = true; break;
                    default: Print("Invalid."); break;
                }
            }
        }

        private void ManageUserStories()
        {
            bool back = false;
            while (!back)
            {
                Console.WriteLine("\n⋆⁺｡˚⋆˙‧₊☾ User Stories ☽₊‧˙⋆˚｡⁺⋆");
                Console.WriteLine("1. ✧ List stories");
                Console.WriteLine("2. ✧ Add story");
                Console.WriteLine("3. ✧ Edit story");
                Console.WriteLine("4. ✧ Delete story");
                Console.WriteLine("5. ✧ Move story state");
                Console.WriteLine("6. ✧ Add dependency");
                Console.WriteLine("7. ✧ Remove dependency");
                Console.WriteLine("B. ✧ Back");
                Console.Write("> ");

                switch (Console.ReadLine()?.Trim().ToLower())
                {
                    case "1":
                        var stories = _ctrl.ListUserStories(_currentProjectId.Value);
                        if (stories.Count == 0) { Print("No stories yet."); break; }
                        foreach (var s in stories)
                        {
                            Console.WriteLine($"  {s}  (priority: {s.Priority})");
                            if (s.DependsOnIds.Count > 0)
                                Console.WriteLine($"    depends on: {string.Join(", ", s.DependsOnIds)}");
                        }
                        break;

                    case "2":
                        string title = Prompt("Title: ");
                        string content = Prompt("Content: ");
                        int priority = PromptInt("Priority (0 = lowest): ");
                        var (ok, msg, newId) = _ctrl.CreateUserStory(_currentProjectId.Value, title, content, priority);
                        Print(msg);
                        if (ok) Print($"Story ID: {newId}");
                        break;

                    case "3":
                        int storyId = PromptInt("Story ID: ");
                        string newTitle = Prompt("New title: ");
                        string newContent = Prompt("New content: ");
                        int newPriority = PromptInt("New priority: ");
                        Print(_ctrl.EditUserStory(storyId, newTitle, newContent, newPriority).message);
                        break;

                    case "4":
                        int deleteId = PromptInt("Story ID: ");
                        Print(_ctrl.RemoveUserStory(deleteId).message);
                        break;

                    case "5":
                        int moveId = PromptInt("Story ID: ");
                        Console.WriteLine("  1 = Project Backlog");
                        Console.WriteLine("  2 = In Sprint");
                        Console.WriteLine("  3 = Done");
                        int stateChoice = PromptInt("State: ");
                        if (stateChoice < 1 || stateChoice > 3) { Print("Invalid state."); break; }
                        Print(_ctrl.MoveUserStoryToState(moveId, (UserStoryState)stateChoice).message);
                        break;

                    case "6":
                        int sid = PromptInt("Story ID: ");
                        int depId = PromptInt("Depends on story ID: ");
                        Print(_ctrl.AddUserStoryDependency(sid, depId).message);
                        break;

                    case "7":
                        int removeSid = PromptInt("Story ID: ");
                        int removeDepId = PromptInt("Remove dependency on story ID: ");
                        Print(_ctrl.RemoveUserStoryDependency(removeSid, removeDepId).message);
                        break;

                    case "b": back = true; break;
                    default: Print("Invalid."); break;
                }
            }
        }

        private void ManageTasks()
        {
            bool back = false;
            while (!back)
            {
                Console.WriteLine("\n⋆⁺｡˚⋆˙‧₊☾ Tasks ☽₊‧˙⋆˚｡⁺⋆");
                Console.WriteLine("1. ✧ List tasks for a story");
                Console.WriteLine("2. ✧ Add task");
                Console.WriteLine("3. ✧ Edit task");
                Console.WriteLine("4. ✧ Delete task");
                Console.WriteLine("5. ✧ Move task state");
                Console.WriteLine("6. ✧ Assign person");
                Console.WriteLine("7. ✧ Remove person");
                Console.WriteLine("8. ✧ Change priority");
                Console.WriteLine("B. ✧ Back");
                Console.Write("> ");

                switch (Console.ReadLine()?.Trim().ToLower())
                {
                    case "1":
                        int storyId = PromptInt("User Story ID: ");
                        var tasks = _ctrl.ListTasks(storyId);
                        if (tasks.Count == 0) { Print("No tasks for that story."); break; }
                        foreach (var t in tasks)
                        {
                            Console.WriteLine($"  {t}  priority={t.Priority}  difficulty={t.Difficulty}");
                            Console.WriteLine($"    planned: {t.PlannedTime}h  actual: {t.ActualTime}h");
                            if (!string.IsNullOrEmpty(t.CategoryLabels))
                                Console.WriteLine($"    labels: {t.CategoryLabels}");
                        }
                        break;

                    case "2":
                        int usid = PromptInt("User Story ID: ");
                        string taskTitle = Prompt("Title: ");
                        string taskDesc = Prompt("Description: ");
                        int taskPriority = PromptInt("Priority: ");
                        double plannedHours = PromptDouble("Planned hours: ");
                        DateTime? startDate = PromptDate("Planned start (yyyy-MM-dd, blank = skip): ");
                        DateTime? endDate = PromptDate("Planned end (yyyy-MM-dd, blank = skip): ");
                        int difficulty = PromptInt("Difficulty (0-5): ");
                        string labels = Prompt("Labels (or blank): ");
                        var (ok, msg, taskId) = _ctrl.CreateTask(usid, taskTitle, taskDesc, taskPriority, plannedHours, startDate, endDate, difficulty, labels);
                        Print(msg);
                        if (ok) Print($"Task ID: {taskId}");
                        break;

                    case "3":
                        int editId = PromptInt("Task ID: ");
                        var existing = _ctrl.GetTask(editId);
                        if (existing == null) { Print("Task not found."); break; }

                        string updatedTitle = Prompt($"Title [{existing.Title}]: ");
                        if (string.IsNullOrWhiteSpace(updatedTitle)) updatedTitle = existing.Title;

                        string updatedDesc = Prompt($"Description [{existing.Description}]: ");
                        if (string.IsNullOrWhiteSpace(updatedDesc)) updatedDesc = existing.Description;

                        int updatedPriority = PromptIntDefault($"Priority [{existing.Priority}]: ", existing.Priority);
                        double updatedPlanned = PromptDoubleDefault($"Planned hours [{existing.PlannedTime}]: ", existing.PlannedTime);
                        double updatedActual = PromptDoubleDefault($"Actual hours [{existing.ActualTime}]: ", existing.ActualTime);
                        int updatedDifficulty = PromptIntDefault($"Difficulty [{existing.Difficulty}]: ", existing.Difficulty);

                        string updatedLabels = Prompt($"Labels [{existing.CategoryLabels}]: ");
                        if (string.IsNullOrWhiteSpace(updatedLabels)) updatedLabels = existing.CategoryLabels;

                        Print(_ctrl.EditTask(editId, updatedTitle, updatedDesc, updatedPriority,
                            updatedPlanned, updatedActual,
                            existing.PlannedStartDate, existing.PlannedEndDate,
                            existing.ActualStartDate, existing.ActualEndDate,
                            updatedDifficulty, updatedLabels).message);
                        break;

                    case "4":
                        int deleteId = PromptInt("Task ID: ");
                        Print(_ctrl.RemoveTask(deleteId).message);
                        break;

                    case "5":
                        int moveId = PromptInt("Task ID: ");
                        Console.WriteLine("  1 = To Be Done");
                        Console.WriteLine("  2 = In Process");
                        Console.WriteLine("  3 = Done");
                        int stateChoice = PromptInt("State: ");
                        if (stateChoice < 1 || stateChoice > 3) { Print("Invalid state."); break; }
                        Print(_ctrl.MoveTaskToState(moveId, (TaskState)stateChoice).message);
                        break;

                    case "6":
                        int assignTaskId = PromptInt("Task ID: ");
                        int assignPersonId = PromptInt("Person ID: ");
                        Print(_ctrl.AssignPersonToTask(assignTaskId, assignPersonId).message);
                        break;

                    case "7":
                        int removeTaskId = PromptInt("Task ID: ");
                        int removePersonId = PromptInt("Person ID: ");
                        Print(_ctrl.RemovePersonFromTask(removeTaskId, removePersonId).message);
                        break;

                    case "8":
                        int changePriorityTaskId = PromptInt("Task ID: ");
                        int newPriority = PromptInt("New priority: ");
                        Print(_ctrl.ChangeTaskPriority(changePriorityTaskId, newPriority).message);
                        break;

                    case "b": back = true; break;
                    default: Print("Invalid."); break;
                }
            }
        }

        private void ManageTeams()
        {
            bool back = false;
            while (!back)
            {
                Console.WriteLine("\n⋆⁺｡˚⋆˙‧₊☾ Teams ☽₊‧˙⋆˚｡⁺⋆");
                Console.WriteLine("1. ✧ List teams");
                Console.WriteLine("2. ✧ Create team");
                Console.WriteLine("3. ✧ Edit team");
                Console.WriteLine("4. ✧ Delete team");
                Console.WriteLine("5. ✧ Add person to team");
                Console.WriteLine("6. ✧ Remove person from team");
                Console.WriteLine("7. ✧ List persons in team");
                Console.WriteLine("B. ✧ Back");
                Console.Write("> ");

                switch (Console.ReadLine()?.Trim().ToLower())
                {
                    case "1":
                        var teams = _ctrl.ListTeams(_currentProjectId.Value);
                        if (teams.Count == 0) { Print("No teams yet."); break; }
                        foreach (var t in teams)
                            Console.WriteLine($"  {t}  ({t.MemberIds.Count} members)");
                        break;

                    case "2":
                        string teamName = Prompt("Team name: ");
                        var (ok, msg, newId) = _ctrl.CreateTeam(teamName, _currentProjectId.Value);
                        Print(msg);
                        if (ok) Print($"Team ID: {newId}");
                        break;

                    case "3":
                        int editId = PromptInt("Team ID: ");
                        string newTeamName = Prompt("New name: ");
                        Print(_ctrl.EditTeam(editId, newTeamName).message);
                        break;

                    case "4":
                        int deleteId = PromptInt("Team ID: ");
                        Print(_ctrl.RemoveTeam(deleteId).message);
                        break;

                    case "5":
                        int addTeamId = PromptInt("Team ID: ");
                        int addPersonId = PromptInt("Person ID: ");
                        Print(_ctrl.AddPersonToTeam(addTeamId, addPersonId).message);
                        break;

                    case "6":
                        int removeTeamId = PromptInt("Team ID: ");
                        int removePersonId = PromptInt("Person ID: ");
                        Print(_ctrl.RemovePersonFromTeam(removeTeamId, removePersonId).message);
                        break;

                    case "7":
                        int listTeamId = PromptInt("Team ID: ");
                        var members = _ctrl.GetPersonsForTeam(listTeamId);
                        if (members.Count == 0) { Print("No members in this team."); break; }
                        foreach (var p in members)
                            Console.WriteLine($"  {p}");
                        break;

                    case "b": back = true; break;
                    default: Print("Invalid."); break;
                }
            }
        }

        private void ShowReports()
        {
            bool back = false;
            while (!back)
            {
                Console.WriteLine("\n⋆⁺｡˚⋆˙‧₊☾ Reports ☽₊‧˙⋆˚｡⁺⋆");
                Console.WriteLine("1. ✧ Project report");
                Console.WriteLine("2. ✧ Sprint report");
                Console.WriteLine("3. ✧ User story report");
                Console.WriteLine("4. ✧ Task report");
                Console.WriteLine("5. ✧ Person report");
                Console.WriteLine("B. ✧ FBack");
                Console.Write("> ");

                switch (Console.ReadLine()?.Trim().ToLower())
                {
                    case "1":
                        int pid = PromptInt("Project ID: ");
                        var projectReport = _ctrl.GetProjectReport(pid);
                        if (projectReport == null) { Print("Not found."); break; }
                        Console.WriteLine(projectReport.ToString());
                        break;

                    case "2":
                        int sprintPid = PromptInt("Project ID: ");
                        var sprintReport = _ctrl.GetSprintReport(sprintPid);
                        if (sprintReport == null) { Print("Not found."); break; }
                        Console.WriteLine(sprintReport.ToString());
                        break;

                    case "3":
                        int usid = PromptInt("User Story ID: ");
                        var storyReport = _ctrl.GetUserStoryReport(usid);
                        if (storyReport == null) { Print("Not found."); break; }
                        Console.WriteLine(storyReport.ToString());
                        break;

                    case "4":
                        int tid = PromptInt("Task ID: ");
                        var taskReport = _ctrl.GetTaskReport(tid);
                        if (taskReport == null) { Print("Not found."); break; }
                        Console.WriteLine(taskReport.ToString());
                        break;

                    case "5":
                        int personId = PromptInt("Person ID: ");
                        var personReport = _ctrl.GetPersonReport(personId);
                        if (personReport == null) { Print("Not found."); break; }
                        Console.WriteLine(personReport.ToString());
                        break;

                    case "b": back = true; break;
                    default: Print("Invalid."); break;
                }
            }
        }

        // helpers to meet logic

        private bool EnsureProjectSelected()
        {
            if (_currentProjectId.HasValue) return true;
            Print("Select a project first."); //It will make sure user can't access options like userstories without selecting  project
            return false;
        }

        private void Print(string msg) => Console.WriteLine($"  >> {msg}");

        private string Prompt(string label)
        {
            Console.Write(label);
            return Console.ReadLine()?.Trim() ?? "";
        }

        private int PromptInt(string label)
        {
            Console.Write(label);
            return int.TryParse(Console.ReadLine(), out int v) ? v : 0;
        }

        private int PromptIntDefault(string label, int fallback)
        {
            Console.Write(label);
            string s = Console.ReadLine();
            return string.IsNullOrWhiteSpace(s) ? fallback : (int.TryParse(s, out int v) ? v : fallback);
        }

        private double PromptDouble(string label)
        {
            Console.Write(label);
            return double.TryParse(Console.ReadLine(), out double v) ? v : 0;
        }

        private double PromptDoubleDefault(string label, double fallback)
        {
            Console.Write(label);
            string s = Console.ReadLine();
            return string.IsNullOrWhiteSpace(s) ? fallback : (double.TryParse(s, out double v) ? v : fallback);
        }

        private DateTime? PromptDate(string label)
        {
            Console.Write(label);
            string s = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(s)) return null;
            return DateTime.TryParse(s, out DateTime d) ? d : (DateTime?)null;
        }
    }
}