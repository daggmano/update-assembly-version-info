using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace UpdateAssemblyVersionInfo
{
    class Program
    {
        private static readonly Regex AssemblyRegex = new Regex(@"(\[assembly\:\s*)(Assembly(File)?Version)(\(""\d+\.\d+\.\d+)((\.(\d+|\*))?)(""\)])");

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                PrintUsage();
                return;
            }

            var filepath = args[0];
            if (!File.Exists(filepath))
            {
                Console.WriteLine("Unable to find file '{0}'.\n", filepath);
                PrintUsage();
                return;
            }

            int build;
            if (!Int32.TryParse(args[1], out build))
            {
                Console.WriteLine("'{0}' does not appear to be a build number.\n");
                PrintUsage();
                return;
            }

            var sb = new StringBuilder();

            using (var sr = File.OpenText(filepath))
            {
                while (!sr.EndOfStream)
                {
                    var s = sr.ReadLine();
                    if (s == null)
                    {
                        continue;
                    }

                    if (AssemblyRegex.IsMatch(s))
                    {
                        s = ProcessLine(s, build);
                    }

                    sb.AppendLine(s);
                }
            }

            File.WriteAllText(filepath, sb.ToString());
        }

        static void PrintUsage()
        {
            Console.WriteLine("Usage: UpdateAssemblyVersionInfo <filepath> <buildnumber>");
            Console.WriteLine("\nNote: filepath should point to AssemblyInfo.cs (or shared equivalent).");
        }

        static string ProcessLine(string s, int buildnumber)
        {
            var matches = AssemblyRegex.Matches(s);
            if (matches.Count == 1)
            {
                var groups = matches[0];
                if (groups.Groups.Count == 9)
                {
                    return String.Format("{0}{1}{2}.{3}{4}", groups.Groups[1].Value, groups.Groups[2].Value, groups.Groups[4].Value, buildnumber, groups.Groups[8].Value);
                }
            }

            return s;
        }
    }
}
