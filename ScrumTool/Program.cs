using ScrumTool.Controller;
using ScrumTool.Model;
using ScrumTool.View;

namespace ScrumTool
{
    class Program
    {
        static void Main(string[] args)
        {
            // these are just practice dbs
            //SqliteDatabase database = new SqliteDatabase("scrum.db");
            //SqliteDatabase database = new SqliteDatabase("agile.db");
            SqliteDatabase database = new SqliteDatabase("groupOK.db");
            database.Initialize();

            ScrumController controller = new ScrumController(database);
            var view = new ConsoleView(controller);

            view.Run();
        }
    }
}