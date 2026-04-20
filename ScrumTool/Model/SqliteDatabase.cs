using System;
using System.Collections.Generic;
using System.IO;
using System.Data.SQLite;

namespace ScrumTool.Model
{
    public class SqliteDatabase
    {
        private readonly string _connectionString;
        private SQLiteConnection _connection;

        public SqliteDatabase(string dbPath = "scrum.db")
        {
            _connectionString = "Data Source=" + dbPath + ";Version=3;";
        }

        // keep a single open connection for the lifetime of the app
        private SQLiteConnection GetConnection()
        {
            if (_connection == null)
            {
                _connection = new SQLiteConnection(_connectionString);
                _connection.Open();
                var fk = new SQLiteCommand("PRAGMA foreign_keys = ON;", _connection);
                fk.ExecuteNonQuery();
                fk.Dispose();
            }
            return _connection;
        }

        private SQLiteCommand CreateCommand(string sql)
        {
            return new SQLiteCommand(sql, GetConnection());
        }

        public void Initialize()
        {
            string schema = File.ReadAllText("Database/schema.sql");
            foreach (string rawStatement in schema.Split(';'))
            {
                string[] lines = rawStatement.Split('\n');
                var filtered = new List<string>();
                foreach (string line in lines)
                {
                    if (!line.TrimStart().StartsWith("--"))
                        filtered.Add(line);
                }
                string statement = string.Join("\n", filtered).Trim();
                if (string.IsNullOrWhiteSpace(statement))
                    continue;

                var cmd = CreateCommand(statement);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }

        // ---- projects ----

        public int AddProject(string name, string description)
        {
            var cmd = CreateCommand("INSERT INTO Projects (Name, Description) VALUES (@n, @d); SELECT last_insert_rowid();");
            cmd.Parameters.AddWithValue("@n", name);
            cmd.Parameters.AddWithValue("@d", description ?? "");
            int newId = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.Dispose();
            return newId;
        }

        public bool UpdateProject(int id, string name, string description)
        {
            var cmd = CreateCommand("UPDATE Projects SET Name=@n, Description=@d WHERE Id=@id");
            cmd.Parameters.AddWithValue("@n", name);
            cmd.Parameters.AddWithValue("@d", description ?? "");
            cmd.Parameters.AddWithValue("@id", id);
            bool changed = cmd.ExecuteNonQuery() > 0;
            cmd.Dispose();
            return changed;
        }

        public bool DeleteProject(int id)
        {
            var cmd = CreateCommand("DELETE FROM Projects WHERE Id=@id");
            cmd.Parameters.AddWithValue("@id", id);
            bool changed = cmd.ExecuteNonQuery() > 0;
            cmd.Dispose();
            return changed;
        }

        public Project GetProject(int id)
        {
            var cmd = CreateCommand("SELECT Id, Name, Description FROM Projects WHERE Id=@id");
            cmd.Parameters.AddWithValue("@id", id);
            var r = cmd.ExecuteReader();
            Project project = null;
            if (r.Read())
                project = new Project(r.GetInt32(0), r.GetString(1), r.IsDBNull(2) ? "" : r.GetString(2));
            r.Dispose();
            cmd.Dispose();
            return project;
        }

        public List<Project> GetAllProjects()
        {
            var list = new List<Project>();
            var cmd = CreateCommand("SELECT Id, Name, Description FROM Projects");
            var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new Project(r.GetInt32(0), r.GetString(1), r.IsDBNull(2) ? "" : r.GetString(2)));
            r.Dispose();
            cmd.Dispose();
            return list;
        }

        // ---- persons ----

        public int AddPerson(string name, string role)
        {
            var cmd = CreateCommand("INSERT INTO Persons (Name, Role) VALUES (@n, @r); SELECT last_insert_rowid();");
            cmd.Parameters.AddWithValue("@n", name);
            cmd.Parameters.AddWithValue("@r", role);
            int newId = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.Dispose();
            return newId;
        }

        public bool UpdatePerson(int id, string name, string role)
        {
            var cmd = CreateCommand("UPDATE Persons SET Name=@n, Role=@r WHERE Id=@id");
            cmd.Parameters.AddWithValue("@n", name);
            cmd.Parameters.AddWithValue("@r", role);
            cmd.Parameters.AddWithValue("@id", id);
            bool changed = cmd.ExecuteNonQuery() > 0;
            cmd.Dispose();
            return changed;
        }

        public bool DeletePerson(int id)
        {
            var cmd = CreateCommand("DELETE FROM Persons WHERE Id=@id");
            cmd.Parameters.AddWithValue("@id", id);
            bool changed = cmd.ExecuteNonQuery() > 0;
            cmd.Dispose();
            return changed;
        }

        public Person GetPerson(int id)
        {
            var cmd = CreateCommand("SELECT Id, Name, Role FROM Persons WHERE Id=@id");
            cmd.Parameters.AddWithValue("@id", id);
            var r = cmd.ExecuteReader();
            Person person = null;
            if (r.Read())
                person = new Person(r.GetInt32(0), r.GetString(1), r.GetString(2));
            r.Dispose();
            cmd.Dispose();
            return person;
        }

        public List<Person> GetAllPersons()
        {
            var list = new List<Person>();
            var cmd = CreateCommand("SELECT Id, Name, Role FROM Persons");
            var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new Person(r.GetInt32(0), r.GetString(1), r.GetString(2)));
            r.Dispose();
            cmd.Dispose();
            return list;
        }

        public bool LinkPersonToProject(int personId, int projectId)
        {
            var cmd = CreateCommand("INSERT OR IGNORE INTO ProjectPersons (ProjectId, PersonId) VALUES (@p, @pe)");
            cmd.Parameters.AddWithValue("@p", projectId);
            cmd.Parameters.AddWithValue("@pe", personId);
            bool changed = cmd.ExecuteNonQuery() > 0;
            cmd.Dispose();
            return changed;
        }

        public bool UnlinkPersonFromProject(int personId, int projectId)
        {
            var cmd = CreateCommand("DELETE FROM ProjectPersons WHERE ProjectId=@p AND PersonId=@pe");
            cmd.Parameters.AddWithValue("@p", projectId);
            cmd.Parameters.AddWithValue("@pe", personId);
            bool changed = cmd.ExecuteNonQuery() > 0;
            cmd.Dispose();
            return changed;
        }

        public List<Person> GetPersonsForProject(int projectId)
        {
            var list = new List<Person>();
            var cmd = CreateCommand("SELECT p.Id, p.Name, p.Role FROM Persons p JOIN ProjectPersons pp ON p.Id=pp.PersonId WHERE pp.ProjectId=@pid");
            cmd.Parameters.AddWithValue("@pid", projectId);
            var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new Person(r.GetInt32(0), r.GetString(1), r.GetString(2)));
            r.Dispose();
            cmd.Dispose();
            return list;
        }

        public List<Project> GetProjectsForPerson(int personId)
        {
            var list = new List<Project>();
            var cmd = CreateCommand("SELECT pr.Id, pr.Name, pr.Description FROM Projects pr JOIN ProjectPersons pp ON pr.Id=pp.ProjectId WHERE pp.PersonId=@pid");
            cmd.Parameters.AddWithValue("@pid", personId);
            var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new Project(r.GetInt32(0), r.GetString(1), r.IsDBNull(2) ? "" : r.GetString(2)));
            r.Dispose();
            cmd.Dispose();
            return list;
        }

        // ---- user stories ----

        public int AddUserStory(int projectId, string title, string content, int priority)
        {
            var cmd = CreateCommand("INSERT INTO UserStories (ProjectId, Title, Content, State, Priority) VALUES (@pid, @t, @c, 'ProjectBacklog', @pr); SELECT last_insert_rowid();");
            cmd.Parameters.AddWithValue("@pid", projectId);
            cmd.Parameters.AddWithValue("@t", title);
            cmd.Parameters.AddWithValue("@c", content ?? "");
            cmd.Parameters.AddWithValue("@pr", priority);
            int newId = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.Dispose();
            return newId;
        }

        public bool UpdateUserStory(int id, string title, string content, int priority, UserStoryState state)
        {
            var cmd = CreateCommand("UPDATE UserStories SET Title=@t, Content=@c, Priority=@pr, State=@s WHERE Id=@id");
            cmd.Parameters.AddWithValue("@t", title);
            cmd.Parameters.AddWithValue("@c", content ?? "");
            cmd.Parameters.AddWithValue("@pr", priority);
            cmd.Parameters.AddWithValue("@s", state.ToString());
            cmd.Parameters.AddWithValue("@id", id);
            bool changed = cmd.ExecuteNonQuery() > 0;
            cmd.Dispose();
            return changed;
        }

        public bool DeleteUserStory(int id)
        {
            var cmd = CreateCommand("DELETE FROM UserStories WHERE Id=@id");
            cmd.Parameters.AddWithValue("@id", id);
            bool changed = cmd.ExecuteNonQuery() > 0;
            cmd.Dispose();
            return changed;
        }

        public UserStory GetUserStory(int id)
        {
            var cmd = CreateCommand("SELECT Id, ProjectId, Title, Content, State, Priority FROM UserStories WHERE Id=@id");
            cmd.Parameters.AddWithValue("@id", id);
            var r = cmd.ExecuteReader();
            UserStory us = null;
            if (r.Read())
                us = MapUserStory(r);
            r.Dispose();
            cmd.Dispose();
            if (us != null)
                us.DependsOnIds = GetDependencyIds(id);
            return us;
        }

        public List<UserStory> GetUserStoriesForProject(int projectId)
        {
            var cmd = CreateCommand("SELECT Id, ProjectId, Title, Content, State, Priority FROM UserStories WHERE ProjectId=@pid");
            cmd.Parameters.AddWithValue("@pid", projectId);
            var r = cmd.ExecuteReader();
            var raw = new List<UserStory>();
            while (r.Read())
                raw.Add(MapUserStory(r));
            r.Dispose();
            cmd.Dispose();

            var list = new List<UserStory>();
            foreach (var us in raw)
            {
                us.DependsOnIds = GetDependencyIds(us.Id);
                list.Add(us);
            }
            return list;
        }

        private UserStory MapUserStory(SQLiteDataReader r)
        {
            return new UserStory
            {
                Id = r.GetInt32(0),
                ProjectId = r.GetInt32(1),
                Title = r.GetString(2),
                Content = r.IsDBNull(3) ? "" : r.GetString(3),
                State = (UserStoryState)Enum.Parse(typeof(UserStoryState), r.GetString(4)),
                Priority = r.GetInt32(5)
            };
        }

        private List<int> GetDependencyIds(int storyId)
        {
            var ids = new List<int>();
            var cmd = CreateCommand("SELECT DependsOnId FROM UserStoryDependencies WHERE UserStoryId=@id");
            cmd.Parameters.AddWithValue("@id", storyId);
            var r = cmd.ExecuteReader();
            while (r.Read())
                ids.Add(r.GetInt32(0));
            r.Dispose();
            cmd.Dispose();
            return ids;
        }

        public bool AddUserStoryDependency(int userStoryId, int dependsOnId)
        {
            var cmd = CreateCommand("INSERT OR IGNORE INTO UserStoryDependencies (UserStoryId, DependsOnId) VALUES (@s, @d)");
            cmd.Parameters.AddWithValue("@s", userStoryId);
            cmd.Parameters.AddWithValue("@d", dependsOnId);
            bool changed = cmd.ExecuteNonQuery() > 0;
            cmd.Dispose();
            return changed;
        }

        public bool RemoveUserStoryDependency(int userStoryId, int dependsOnId)
        {
            var cmd = CreateCommand("DELETE FROM UserStoryDependencies WHERE UserStoryId=@s AND DependsOnId=@d");
            cmd.Parameters.AddWithValue("@s", userStoryId);
            cmd.Parameters.AddWithValue("@d", dependsOnId);
            bool changed = cmd.ExecuteNonQuery() > 0;
            cmd.Dispose();
            return changed;
        }

        public List<UserStory> GetDependenciesForUserStory(int userStoryId)
        {
            var list = new List<UserStory>();
            var cmd = CreateCommand("SELECT us.Id, us.ProjectId, us.Title, us.Content, us.State, us.Priority FROM UserStories us JOIN UserStoryDependencies d ON us.Id=d.DependsOnId WHERE d.UserStoryId=@id");
            cmd.Parameters.AddWithValue("@id", userStoryId);
            var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(MapUserStory(r));
            r.Dispose();
            cmd.Dispose();
            return list;
        }

        public bool UpdateUserStoryState(int userStoryId, UserStoryState state)
        {
            var cmd = CreateCommand("UPDATE UserStories SET State=@s WHERE Id=@id");
            cmd.Parameters.AddWithValue("@s", state.ToString());
            cmd.Parameters.AddWithValue("@id", userStoryId);
            bool changed = cmd.ExecuteNonQuery() > 0;
            cmd.Dispose();
            return changed;
        }

        // ---- tasks ----

        public int AddTask(int userStoryId, string title, string description, int priority,
            double plannedTime, DateTime? plannedStartDate, DateTime? plannedEndDate,
            int difficulty, string categoryLabels)
        {
            var cmd = CreateCommand(@"INSERT INTO Tasks
                (UserStoryId, Title, Description, State, Priority, PlannedTime, PlannedStartDate, PlannedEndDate, Difficulty, CategoryLabels)
                VALUES (@uid, @t, @d, 'ToBeDone', @pr, @pt, @psd, @ped, @diff, @cat);
                SELECT last_insert_rowid();");
            cmd.Parameters.AddWithValue("@uid", userStoryId);
            cmd.Parameters.AddWithValue("@t", title);
            cmd.Parameters.AddWithValue("@d", description ?? "");
            cmd.Parameters.AddWithValue("@pr", priority);
            cmd.Parameters.AddWithValue("@pt", plannedTime);
            cmd.Parameters.AddWithValue("@psd", plannedStartDate.HasValue ? (object)plannedStartDate.Value.ToString("yyyy-MM-dd") : DBNull.Value);
            cmd.Parameters.AddWithValue("@ped", plannedEndDate.HasValue ? (object)plannedEndDate.Value.ToString("yyyy-MM-dd") : DBNull.Value);
            cmd.Parameters.AddWithValue("@diff", difficulty);
            cmd.Parameters.AddWithValue("@cat", categoryLabels ?? "");
            int newId = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.Dispose();
            return newId;
        }

        public bool UpdateTask(int id, string title, string description, int priority,
            double plannedTime, double actualTime,
            DateTime? plannedStartDate, DateTime? plannedEndDate,
            DateTime? actualStartDate, DateTime? actualEndDate,
            int difficulty, string categoryLabels, TaskState state)
        {
            var cmd = CreateCommand(@"UPDATE Tasks SET
                Title=@t, Description=@d, Priority=@pr,
                PlannedTime=@pt, ActualTime=@at,
                PlannedStartDate=@psd, PlannedEndDate=@ped,
                ActualStartDate=@asd, ActualEndDate=@aed,
                Difficulty=@diff, CategoryLabels=@cat, State=@s
                WHERE Id=@id");
            cmd.Parameters.AddWithValue("@t", title);
            cmd.Parameters.AddWithValue("@d", description ?? "");
            cmd.Parameters.AddWithValue("@pr", priority);
            cmd.Parameters.AddWithValue("@pt", plannedTime);
            cmd.Parameters.AddWithValue("@at", actualTime);
            cmd.Parameters.AddWithValue("@psd", plannedStartDate.HasValue ? (object)plannedStartDate.Value.ToString("yyyy-MM-dd") : DBNull.Value);
            cmd.Parameters.AddWithValue("@ped", plannedEndDate.HasValue ? (object)plannedEndDate.Value.ToString("yyyy-MM-dd") : DBNull.Value);
            cmd.Parameters.AddWithValue("@asd", actualStartDate.HasValue ? (object)actualStartDate.Value.ToString("yyyy-MM-dd") : DBNull.Value);
            cmd.Parameters.AddWithValue("@aed", actualEndDate.HasValue ? (object)actualEndDate.Value.ToString("yyyy-MM-dd") : DBNull.Value);
            cmd.Parameters.AddWithValue("@diff", difficulty);
            cmd.Parameters.AddWithValue("@cat", categoryLabels ?? "");
            cmd.Parameters.AddWithValue("@s", state.ToString());
            cmd.Parameters.AddWithValue("@id", id);
            bool changed = cmd.ExecuteNonQuery() > 0;
            cmd.Dispose();
            return changed;
        }

        public bool DeleteTask(int id)
        {
            var cmd = CreateCommand("DELETE FROM Tasks WHERE Id=@id");
            cmd.Parameters.AddWithValue("@id", id);
            bool changed = cmd.ExecuteNonQuery() > 0;
            cmd.Dispose();
            return changed;
        }

        public ScrumTask GetTask(int id)
        {
            var cmd = CreateCommand("SELECT * FROM Tasks WHERE Id=@id");
            cmd.Parameters.AddWithValue("@id", id);
            var r = cmd.ExecuteReader();
            ScrumTask t = null;
            if (r.Read())
                t = MapTask(r);
            r.Dispose();
            cmd.Dispose();
            if (t != null)
                t.AssignedPersonIds = GetPersonIdsForTask(id);
            return t;
        }

        public List<ScrumTask> GetTasksForUserStory(int userStoryId)
        {
            var cmd = CreateCommand("SELECT * FROM Tasks WHERE UserStoryId=@id");
            cmd.Parameters.AddWithValue("@id", userStoryId);
            var r = cmd.ExecuteReader();
            var raw = new List<ScrumTask>();
            while (r.Read())
                raw.Add(MapTask(r));
            r.Dispose();
            cmd.Dispose();

            var list = new List<ScrumTask>();
            foreach (var t in raw)
            {
                t.AssignedPersonIds = GetPersonIdsForTask(t.Id);
                list.Add(t);
            }
            return list;
        }

        private ScrumTask MapTask(SQLiteDataReader r)
        {
            return new ScrumTask
            {
                Id = r.GetInt32(0),
                UserStoryId = r.GetInt32(1),
                Title = r.GetString(2),
                Description = r.IsDBNull(3) ? "" : r.GetString(3),
                State = (TaskState)Enum.Parse(typeof(TaskState), r.GetString(4)),
                Priority = r.GetInt32(5),
                PlannedTime = r.GetDouble(6),
                ActualTime = r.GetDouble(7),
                PlannedStartDate = r.IsDBNull(8) ? (DateTime?)null : DateTime.Parse(r.GetString(8)),
                PlannedEndDate = r.IsDBNull(9) ? (DateTime?)null : DateTime.Parse(r.GetString(9)),
                ActualStartDate = r.IsDBNull(10) ? (DateTime?)null : DateTime.Parse(r.GetString(10)),
                ActualEndDate = r.IsDBNull(11) ? (DateTime?)null : DateTime.Parse(r.GetString(11)),
                Difficulty = r.GetInt32(12),
                CategoryLabels = r.IsDBNull(13) ? "" : r.GetString(13)
            };
        }

        private List<int> GetPersonIdsForTask(int taskId)
        {
            var ids = new List<int>();
            var cmd = CreateCommand("SELECT PersonId FROM TaskPersons WHERE TaskId=@id");
            cmd.Parameters.AddWithValue("@id", taskId);
            var r = cmd.ExecuteReader();
            while (r.Read())
                ids.Add(r.GetInt32(0));
            r.Dispose();
            cmd.Dispose();
            return ids;
        }

        public bool AssignPersonToTask(int taskId, int personId)
        {
            var cmd = CreateCommand("INSERT OR IGNORE INTO TaskPersons (TaskId, PersonId) VALUES (@t, @p)");
            cmd.Parameters.AddWithValue("@t", taskId);
            cmd.Parameters.AddWithValue("@p", personId);
            bool changed = cmd.ExecuteNonQuery() > 0;
            cmd.Dispose();
            return changed;
        }

        public bool RemovePersonFromTask(int taskId, int personId)
        {
            var cmd = CreateCommand("DELETE FROM TaskPersons WHERE TaskId=@t AND PersonId=@p");
            cmd.Parameters.AddWithValue("@t", taskId);
            cmd.Parameters.AddWithValue("@p", personId);
            bool changed = cmd.ExecuteNonQuery() > 0;
            cmd.Dispose();
            return changed;
        }

        public List<Person> GetPersonsForTask(int taskId)
        {
            var list = new List<Person>();
            var cmd = CreateCommand("SELECT p.Id, p.Name, p.Role FROM Persons p JOIN TaskPersons tp ON p.Id=tp.PersonId WHERE tp.TaskId=@id");
            cmd.Parameters.AddWithValue("@id", taskId);
            var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new Person(r.GetInt32(0), r.GetString(1), r.GetString(2)));
            r.Dispose();
            cmd.Dispose();
            return list;
        }

        public List<ScrumTask> GetTasksForPersonInProject(int personId, int projectId)
        {
            var list = new List<ScrumTask>();
            var cmd = CreateCommand(@"SELECT t.* FROM Tasks t
                JOIN TaskPersons tp ON t.Id=tp.TaskId
                JOIN UserStories us ON t.UserStoryId=us.Id
                WHERE tp.PersonId=@pid AND us.ProjectId=@proj");
            cmd.Parameters.AddWithValue("@pid", personId);
            cmd.Parameters.AddWithValue("@proj", projectId);
            var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(MapTask(r));
            r.Dispose();
            cmd.Dispose();
            return list;
        }

        public bool UpdateTaskState(int taskId, TaskState state)
        {
            var cmd = CreateCommand("UPDATE Tasks SET State=@s WHERE Id=@id");
            cmd.Parameters.AddWithValue("@s", state.ToString());
            cmd.Parameters.AddWithValue("@id", taskId);
            bool changed = cmd.ExecuteNonQuery() > 0;
            cmd.Dispose();
            return changed;
        }

        // ---- teams ----

        public int AddTeam(string name, int projectId)
        {
            var cmd = CreateCommand("INSERT INTO Teams (Name, ProjectId) VALUES (@n, @pid); SELECT last_insert_rowid();");
            cmd.Parameters.AddWithValue("@n", name);
            cmd.Parameters.AddWithValue("@pid", projectId);
            int newId = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.Dispose();
            return newId;
        }

        public bool UpdateTeam(int id, string name)
        {
            var cmd = CreateCommand("UPDATE Teams SET Name=@n WHERE Id=@id");
            cmd.Parameters.AddWithValue("@n", name);
            cmd.Parameters.AddWithValue("@id", id);
            bool changed = cmd.ExecuteNonQuery() > 0;
            cmd.Dispose();
            return changed;
        }

        public bool DeleteTeam(int id)
        {
            var cmd = CreateCommand("DELETE FROM Teams WHERE Id=@id");
            cmd.Parameters.AddWithValue("@id", id);
            bool changed = cmd.ExecuteNonQuery() > 0;
            cmd.Dispose();
            return changed;
        }

        public Team GetTeam(int id)
        {
            var cmd = CreateCommand("SELECT Id, Name, ProjectId FROM Teams WHERE Id=@id");
            cmd.Parameters.AddWithValue("@id", id);
            var r = cmd.ExecuteReader();
            Team team = null;
            if (r.Read())
                team = new Team(r.GetInt32(0), r.GetString(1), r.GetInt32(2));
            r.Dispose();
            cmd.Dispose();
            if (team != null)
                team.MemberIds = GetPersonIdsForTeam(id);
            return team;
        }

        public List<Team> GetTeamsForProject(int projectId)
        {
            var list = new List<Team>();
            var cmd = CreateCommand("SELECT Id, Name, ProjectId FROM Teams WHERE ProjectId=@pid");
            cmd.Parameters.AddWithValue("@pid", projectId);
            var r = cmd.ExecuteReader();
            var raw = new List<Team>();
            while (r.Read())
                raw.Add(new Team(r.GetInt32(0), r.GetString(1), r.GetInt32(2)));
            r.Dispose();
            cmd.Dispose();
            foreach (var t in raw)
            {
                t.MemberIds = GetPersonIdsForTeam(t.Id);
                list.Add(t);
            }
            return list;
        }

        private List<int> GetPersonIdsForTeam(int teamId)
        {
            var ids = new List<int>();
            var cmd = CreateCommand("SELECT PersonId FROM TeamPersons WHERE TeamId=@id");
            cmd.Parameters.AddWithValue("@id", teamId);
            var r = cmd.ExecuteReader();
            while (r.Read())
                ids.Add(r.GetInt32(0));
            r.Dispose();
            cmd.Dispose();
            return ids;
        }

        public bool AddPersonToTeam(int teamId, int personId)
        {
            var cmd = CreateCommand("INSERT OR IGNORE INTO TeamPersons (TeamId, PersonId) VALUES (@t, @p)");
            cmd.Parameters.AddWithValue("@t", teamId);
            cmd.Parameters.AddWithValue("@p", personId);
            bool changed = cmd.ExecuteNonQuery() > 0;
            cmd.Dispose();
            return changed;
        }

        public bool RemovePersonFromTeam(int teamId, int personId)
        {
            var cmd = CreateCommand("DELETE FROM TeamPersons WHERE TeamId=@t AND PersonId=@p");
            cmd.Parameters.AddWithValue("@t", teamId);
            cmd.Parameters.AddWithValue("@p", personId);
            bool changed = cmd.ExecuteNonQuery() > 0;
            cmd.Dispose();
            return changed;
        }

        public List<Person> GetPersonsForTeam(int teamId)
        {
            var list = new List<Person>();
            var cmd = CreateCommand("SELECT p.Id, p.Name, p.Role FROM Persons p JOIN TeamPersons tp ON p.Id=tp.PersonId WHERE tp.TeamId=@id");
            cmd.Parameters.AddWithValue("@id", teamId);
            var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new Person(r.GetInt32(0), r.GetString(1), r.GetString(2)));
            r.Dispose();
            cmd.Dispose();
            return list;
        }
    }
}