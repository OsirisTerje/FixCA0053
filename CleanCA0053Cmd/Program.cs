using System;

namespace CleanCA0053Cmd
{
    using System.IO;
    using System.Reflection;

    class Program
    {
        static void Main()
        {
            Console.WriteLine("Version: " + Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine("FixCA0053: Fixes erroneous content in fields in the csproj files from current location and all subdirectories");
            Console.WriteLine("by Terje Sandstrom, Inmeta Consulting, 2012");
            Console.WriteLine("For instructions see blogpost at http://geekswithblogs.net/Terje/How to fix the CA0053 error in Code Analysis in Visual Studio 2012");
            Console.WriteLine();
            var ca = new CA0053();
            ca.Execute();
        }


    }


    class CA0053
    {
        private bool changed;
        public void Execute()
        {
            int skipped = 0;
            int fixedup = 0;
            int nowrite = 0;

            string here = Directory.GetCurrentDirectory();
            string[] filePaths = Directory.GetFiles(here, "*.csproj",
                                         SearchOption.AllDirectories);
            var carsd = new SearchTerms("<CodeAnalysisRuleSetDirectories>", "</CodeAnalysisRuleSetDirectories>", @"$(DevEnvDir)\..\..\Team Tools\Static Analysis Tools\Rule Sets");
            var card = new SearchTerms("<CodeAnalysisRuleDirectories>", "</CodeAnalysisRuleDirectories>", @"$(DevEnvDir)\..\..\Team Tools\Static Analysis Tools\FxCop\Rules");
            foreach (var file in filePaths)
            {
                changed = false;
                string text = File.ReadAllText(file);

                text = this.Change2(text, carsd);
                text = this.Change2(text, card);

                try
                {

                    if (changed)
                    {
                        File.WriteAllText(file, text);
                        Console.WriteLine("Fixed   :" + file);
                        fixedup++;
                    }
                    else
                    {
                        Console.WriteLine("Skipped :" + file);
                        skipped++;
                    }
                }
                catch
                {
                    Console.WriteLine("Unable to write to :" + file);
                    nowrite++;
                }

            }
            Console.WriteLine("Fixed : " + fixedup);
            Console.WriteLine("Skipped : " + skipped);
            if (nowrite>0)
                Console.WriteLine("Unable to write :" + nowrite);
            int total = fixedup + skipped;
            Console.WriteLine("Total files checked : " + total);
        }

        private string Change2(string text, SearchTerms terms)
        {
            const int NotFound = -1;
            int index = 0;
            do
            {
                index = text.IndexOf(terms.Start, index, StringComparison.CurrentCultureIgnoreCase);
                if (index != NotFound)
                {
                    int indexend = text.IndexOf(terms.Stop, index, StringComparison.CurrentCultureIgnoreCase);
                    string tobechecked = text.Substring(index, indexend - index);
                    if (tobechecked.IndexOf(@"Microsoft Visual Studio 10.0", StringComparison.CurrentCultureIgnoreCase) != NotFound)
                    {
                        int start = index + terms.Start.Length;
                        int length = indexend - start;
                        text = text.Remove(start, length);
                        text = text.Insert(start, terms.Content);
                        index = indexend;
                        changed = true;
                    }
                    else
                        index = indexend;
                }
            } while (index != NotFound);
            return text;
        }
    }


    struct SearchTerms
    {
        public string Start { get; private set; }

        public string Stop { get; private set; }

        public string Content { get; private set; }

        public SearchTerms(string start, string stop, string content)
            : this()
        {
            Start = start;
            Stop = stop;
            Content = content;
        }
    }


}
