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
                string input = Console.ReadLine()?.Trim().ToLower();
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
                    case "q": running = false; break;
                    default: Print("Invalid option."); break;
                }
            }
            Console.WriteLine(".𖥔 ݁ ˖ִ ࣪⚝₊ ⊹˚. ݁₊Goodbye! ⊹ . ݁˖ . ݁.𖥔 ݁ ˖");
        }

        private void ShowMainMenu()
        {
            string proj = _currentProjectId.HasValue
                ? $"[Project: {_ctrl.GetProject(_currentProjectId.Value)?.Name}]"    // Ai
                : "[No project selected]";
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
                            Console.WriteLine($"  {p} - {p.Description}");
                        break;

                    case "2":
                        string name = Prompt("Project name: ");
                        string desc = Prompt("Description: ");
                        var (ok, msg, id) = _ctrl.CreateProject(name, desc);
                        Print(msg);
                        if (ok) Print($"ID: {id}");
                        break;

                    case "3":
                        int eid = PromptInt("Project ID: ");
                        string en = Prompt("New name: ");
                        string ed = Prompt("New description: ");
                        Print(_ctrl.EditProject(eid, en, ed).message);
                        break;

                    case "4":
                        int did = PromptInt("Project ID to delete: ");
                        Print(_ctrl.RemoveProject(did).message);
                        break;

                    case "5":
                        int sid = PromptInt("Project ID: ");
                        var sp = _ctrl.GetProject(sid);
                        if (sp == null) { Print("Not found."); break; }
                        _currentProjectId = sid;
                        Print($"Active project set to: {sp.Name}");
                        break;

                    case "6":
                        int linkPersonId = PromptInt("Person ID: ");
                        int linkProjectId = PromptInt("Project ID: ");
                        Print(_ctrl.LinkPersonToProject(linkPersonId, linkProjectId).message);
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
                        var persons = _ctrl.ListPersons();
                        if (persons.Count == 0) { Print("No persons found."); break; }
                        foreach (var p in persons)
                            Console.WriteLine($"  {p}");
                        break;

                    case "2":
                        string n = Prompt("Name: ");
                        string r = Prompt("Role: ");
                        var (ok, msg, id) = _ctrl.CreatePerson(n, r);
                        Print(msg);
                        if (ok) Print($"ID: {id}");
                        break;

                    case "3":
                        int eid = PromptInt("Person ID: ");
                        string en = Prompt("New name: ");
                        string er = Prompt("New role: ");
                        Print(_ctrl.EditPerson(eid, en, er).message);
                        break;

                    case "4":
                        int did = PromptInt("Person ID: ");
                        Print(_ctrl.RemovePerson(did).message);
                        break;

                    case "5":
                        if (!EnsureProjectSelected()) break;
                        var pp = _ctrl.GetPersonsForProject(_currentProjectId.Value);
                        if (pp.Count == 0) { Print("Nobody linked to this project yet."); break; }
                        foreach (var p in pp)
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
                Console.WriteLine("\n⋆⁺｡˚⋆˙‧₊☾ USer Stories ☽₊‧˙⋆˚｡⁺⋆");
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
                            Console.WriteLine($"  {s}  priority={s.Priority}");
                            if (s.DependsOnIds.Count > 0)
                                Console.WriteLine($"    depends on: {string.Join(", ", s.DependsOnIds)}");
                        }
                        break;

                    case "2":
                        string t = Prompt("Title: ");
                        string c = Prompt("Content: ");
                        int pr = PromptInt("Priority (0=low): ");
                        var (ok, msg, id) = _ctrl.CreateUserStory(_currentProjectId.Value, t, c, pr);
                        Print(msg);
                        if (ok) Print($"ID: {id}");
                        break;

                    case "3":
                        int eid = PromptInt("Story ID: ");
                        string et = Prompt("New title: ");
                        string ec = Prompt("New content: ");
                        int ep = PromptInt("New priority: ");
                        Print(_ctrl.EditUserStory(eid, et, ec, ep).message);
                        break;

                    case "4":
                        int did = PromptInt("Story ID: ");
                        Print(_ctrl.RemoveUserStory(did).message);
                        break;

                    case "5":
                        int mid = PromptInt("Story ID: ");
                        Console.WriteLine("1=ProjectBacklog  2=InSprint  3=Done");
                        int stateNum = PromptInt("Target state: ");
                        if (stateNum < 1 || stateNum > 3) { Print("Invalid state."); break; }
                        Print(_ctrl.MoveUserStoryToState(mid, (UserStoryState)stateNum).message);
                        break;

                    case "6":
                        int sid = PromptInt("Story ID: ");
                        int depId = PromptInt("Depends on story ID: ");
                        Print(_ctrl.AddUserStoryDependency(sid, depId).message);
                        break;

                    case "7":
                        int rsid = PromptInt("Story ID: ");
                        int rdepId = PromptInt("Remove dependency on story ID: ");
                        Print(_ctrl.RemoveUserStoryDependency(rsid, rdepId).message);
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
                        int sid = PromptInt("User Story ID: ");
                        var tasks = _ctrl.ListTasks(sid);
                        if (tasks.Count == 0) { Print("No tasks."); break; }
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
                        string tt = Prompt("Title: ");
                        string td = Prompt("Description: ");
                        int tp = PromptInt("Priority: ");
                        double tpt = PromptDouble("Planned time (hours): ");
                        DateTime? psd = PromptDate("Planned start (yyyy-MM-dd, blank=none): ");
                        DateTime? ped = PromptDate("Planned end (yyyy-MM-dd, blank=none): ");
                        int diff = PromptInt("Difficulty (0-5): ");
                        string cat = Prompt("Category labels: ");
                        var (ok, msg, tid) = _ctrl.CreateTask(usid, tt, td, tp, tpt, psd, ped, diff, cat);
                        Print(msg);
                        if (ok) Print($"ID: {tid}");
                        break;

                    case "3":
                        int eid = PromptInt("Task ID: ");
                        var et = _ctrl.GetTask(eid);
                        if (et == null) { Print("Not found."); break; }
                        string etn = Prompt($"Title [{et.Title}]: ");
                        if (string.IsNullOrWhiteSpace(etn)) etn = et.Title;
                        string etd = Prompt($"Description [{et.Description}]: ");
                        if (string.IsNullOrWhiteSpace(etd)) etd = et.Description;
                        int epr = PromptIntDefault($"Priority [{et.Priority}]: ", et.Priority);
                        double ept = PromptDoubleDefault($"Planned time [{et.PlannedTime}h]: ", et.PlannedTime);
                        double eat = PromptDoubleDefault($"Actual time [{et.ActualTime}h]: ", et.ActualTime);
                        int ediff = PromptIntDefault($"Difficulty [{et.Difficulty}]: ", et.Difficulty);
                        string ecat = Prompt($"Labels [{et.CategoryLabels}]: ");
                        if (string.IsNullOrWhiteSpace(ecat)) ecat = et.CategoryLabels;
                        Print(_ctrl.EditTask(eid, etn, etd, epr, ept, eat,
                            et.PlannedStartDate, et.PlannedEndDate,
                            et.ActualStartDate, et.ActualEndDate,
                            ediff, ecat).message);
                        break;

                    case "4":
                        int did = PromptInt("Task ID: ");
                        Print(_ctrl.RemoveTask(did).message);
                        break;

                    case "5":
                        int mid = PromptInt("Task ID: ");
                        Console.WriteLine("1=ToBeDone  2=InProcess  3=Done");
                        int stateNum = PromptInt("Target state: ");
                        if (stateNum < 1 || stateNum > 3) { Print("Invalid."); break; }
                        Print(_ctrl.MoveTaskToState(mid, (TaskState)stateNum).message);
                        break;

                    case "6":
                        int atid = PromptInt("Task ID: ");
                        int apid = PromptInt("Person ID: ");
                        Print(_ctrl.AssignPersonToTask(atid, apid).message);
                        break;

                    case "7":
                        int rtid = PromptInt("Task ID: ");
                        int rpid = PromptInt("Person ID: ");
                        Print(_ctrl.RemovePersonFromTask(rtid, rpid).message);
                        break;

                    case "8":
                        int cpid = PromptInt("Task ID: ");
                        int newPri = PromptInt("New priority: ");
                        Print(_ctrl.ChangeTaskPriority(cpid, newPri).message);
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
                        {
                            Console.WriteLine($"  {t}  members: {t.MemberIds.Count}");
                        }
                        break;

                    case "2":
                        string name = Prompt("Team name: ");
                        var (ok, msg, id) = _ctrl.CreateTeam(name, _currentProjectId.Value);
                        Print(msg);
                        if (ok) Print($"ID: {id}");
                        break;

                    case "3":
                        int eid = PromptInt("Team ID: ");
                        string en = Prompt("New name: ");
                        Print(_ctrl.EditTeam(eid, en).message);
                        break;

                    case "4":
                        int did = PromptInt("Team ID: ");
                        Print(_ctrl.RemoveTeam(did).message);
                        break;

                    case "5":
                        int atid = PromptInt("Team ID: ");
                        int apid = PromptInt("Person ID: ");
                        Print(_ctrl.AddPersonToTeam(atid, apid).message);
                        break;

                    case "6":
                        int rtid = PromptInt("Team ID: ");
                        int rpid = PromptInt("Person ID: ");
                        Print(_ctrl.RemovePersonFromTeam(rtid, rpid).message);
                        break;

                    case "7":
                        int ltid = PromptInt("Team ID: ");
                        var persons = _ctrl.GetPersonsForTeam(ltid);
                        if (persons.Count == 0) { Print("No members in this team."); break; }
                        foreach (var p in persons)
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
                        var pr = _ctrl.GetProjectReport(pid);
                        if (pr == null) { Print("Not found."); break; }
                        Console.WriteLine(pr.ToString());
                        break;

                    case "2":
                        int spid = PromptInt("Project ID: ");
                        var sr = _ctrl.GetSprintReport(spid);
                        if (sr == null) { Print("Not found."); break; }
                        Console.WriteLine(sr.ToString());
                        break;

                    case "3":
                        int usid = PromptInt("User Story ID: ");
                        var usr = _ctrl.GetUserStoryReport(usid);
                        if (usr == null) { Print("Not found."); break; }
                        Console.WriteLine(usr.ToString());
                        break;

                    case "4":
                        int tid = PromptInt("Task ID: ");
                        var tr = _ctrl.GetTaskReport(tid);
                        if (tr == null) { Print("Not found."); break; }
                        Console.WriteLine(tr.ToString());
                        break;

                    case "5":
                        int perid = PromptInt("Person ID: ");
                        var perr = _ctrl.GetPersonReport(perid);
                        if (perr == null) { Print("Not found."); break; }
                        Console.WriteLine(perr.ToString());
                        break;

                    case "b": back = true; break;
                    default: Print("Invalid."); break;
                }
            }
        }

        // helpers

        private bool EnsureProjectSelected()
        {
            if (_currentProjectId.HasValue) return true;
            Print("Select a project first (Projects > option 5).");
            return false;
        }

        private void Print(string msg) => Console.WriteLine($"  >> {msg}");

        private string Prompt(string msg)
        {
            Console.Write(msg);
            return Console.ReadLine()?.Trim() ?? "";
        }

        private int PromptInt(string msg)
        {
            Console.Write(msg);
            return int.TryParse(Console.ReadLine(), out int v) ? v : 0;
        }

        private int PromptIntDefault(string msg, int def)
        {
            Console.Write(msg);
            string s = Console.ReadLine();
            return string.IsNullOrWhiteSpace(s) ? def : (int.TryParse(s, out int v) ? v : def);
        }

        private double PromptDouble(string msg)
        {
            Console.Write(msg);
            return double.TryParse(Console.ReadLine(), out double v) ? v : 0;
        }

        private double PromptDoubleDefault(string msg, double def)
        {
            Console.Write(msg);
            string s = Console.ReadLine();
            return string.IsNullOrWhiteSpace(s) ? def : (double.TryParse(s, out double v) ? v : def);
        }

        private DateTime? PromptDate(string msg)
        {
            Console.Write(msg);
            string s = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(s)) return null;
            return DateTime.TryParse(s, out DateTime d) ? d : (DateTime?)null;
        }
    }
}