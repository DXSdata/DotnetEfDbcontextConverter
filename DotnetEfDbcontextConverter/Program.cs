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
                    " - Removes OnConfiguring method (including connectionString), so you can implement your own partial OnConfiguring method outside the generated context." + Environment.NewLine + Environment.NewLine +
                    "Usage: " + Environment.NewLine +
                    "DotnetEfDbcontextConverter.exe path\\myDbContext.cs");

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
            origSchema = origSchema.Split(",")[1].Trim().Trim('"');
            var schemaEndPosition = origSchema.IndexOf('"');
            origSchema = origSchema.Substring(0, schemaEndPosition);


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

            File.WriteAllLines(file, lines, System.Text.Encoding.Default);

            Console.WriteLine("Converting finished.");
        }
    }
}
