using System;
using System.IO;
using System.Linq;

namespace DotnetEfDbcontextConverter
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length == 0)
            {
                Console.WriteLine(
                    "Optimizes generated DbContext output of \"dotnet ef dbcontext scaffold\"." + Environment.NewLine +
                    " - Makes DB schema changeable at runtime" + Environment.NewLine +
                    " - Removes OnConfiguring method (including connectionString), so you can implement your own partial OnConfiguring method outside the generated context." + Environment.NewLine + 
                    " - Optional parameter --winforms optimizes all generated .cs files in the context file's folder for usage in Windows Forms (grids etc)." + Environment.NewLine +
                    Environment.NewLine +
                    "Usage: " + Environment.NewLine +
                    "DotnetEfDbcontextConverter.exe path\\myDbContext.cs [--winforms]");

                Console.ReadKey();

                return;
            }

            
            string file = args[0];

            String content = File.ReadAllText(file);
            File.Copy(file, file + ".backup", true);

            Console.WriteLine("Backup file generated.");

            //Get original schema name
            var schemaPosition = content.IndexOf("entity.ToTable(");
            var origSchema = content.Substring(schemaPosition, 50);
            try
            {
                origSchema = origSchema.Split(",")[1].Trim().Trim('"');
                var schemaEndPosition = origSchema.IndexOf('"');
                origSchema = origSchema.Substring(0, schemaEndPosition);
            }
            catch(IndexOutOfRangeException) //As an alternative method, use database name from connection string
            {
                schemaPosition = content.IndexOf("database=");
                origSchema = content.Substring(schemaPosition);
                origSchema = origSchema.Split(";", 2)[0].Trim().Replace("database=","").Replace("\"", "").Replace(")", "");
            }           


            //Insert static schema variable declaration
            var staticVarDecl = Environment.NewLine + "\t\tpublic static string Schema = \"" + origSchema + "\";" + Environment.NewLine + Environment.NewLine;
            if (!content.Contains(staticVarDecl))
            {
                var staticVarPosition = content.IndexOf(": DbContext") + 20;
                content = content.Insert(staticVarPosition, staticVarDecl);
            }

            //Replace hard-coded schema name with variable
            var lines = content.Split(Environment.NewLine).ToList();
            //foreach (var line in lines)
            //lines.ForEach(line =>
            lines = lines.Select(line => 
                line.Contains("entity.ToTable") ? line.Replace("\""+origSchema+"\"", "Schema") : line
            ).ToList();
            
            //Remove OnConfiguring method
            var onConfigStartpos = lines.FindIndex(o => o.Contains("protected override void OnConfiguring"));
            lines.RemoveRange(onConfigStartpos, 8);

            //For better WinForms / grid usage: Replace ICollection and HashSet with BindingList
            //(E.g. using parent/child relations in grid, ICollection might have only 2 grid columns like "Count" or "ReadOnly"
            //Alternative (untested): Use context.table.ToBindingList() as DataSource as described here: https://blogs.msdn.microsoft.com/efdesign/2010/09/08/data-binding-with-dbcontext/
            if (args.Any(a => a == "--winforms"))
            {
                var dir = new FileInfo(file).DirectoryName;
                foreach (var f in Directory.GetFiles(dir, "*.cs"))
                {
                    var fcontent = File.ReadAllText(f);
                    fcontent = "using System.ComponentModel;" + Environment.NewLine + fcontent.Replace("ICollection<", "IList<").Replace("HashSet<", "BindingList<");
                    File.WriteAllText(f, fcontent, System.Text.Encoding.Default);
                }
            }

            File.WriteAllLines(file, lines, System.Text.Encoding.Default);

            Console.WriteLine("Converting finished.");
        }
    }
}
