using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Config;

namespace CrossWord
{
    static class CrossWordProgram
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CrossWordProgram));

        public static void Main(string[] args)
        {
            InitLog();
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Log.InfoFormat("CrossWord ver. {0} ", version);

            string inputFile, outputFile, puzzle;
            if (!ParseInput(args, out inputFile, out outputFile, out puzzle))
            {
                Environment.Exit(1);
                return;
            }

        }

        static bool ParseInput(IEnumerable<string> args, out string inputFile, out string outputFile, out string puzzle)
        {
            bool help = false;
            string i = null, o = null, p = null;
            var optionSet = new NDesk.Options.OptionSet
                                {
                                    { "i|input=", "(input file)", v => i = v },
                                    { "o|output=", "(output file)", v => o = v },
                                    { "p|puzzle=", "(puzze)", v => p = v },
                                    { "h|?|help", "(help)", v => help = v != null },
                                };
            var unparsed = optionSet.Parse(args);
            inputFile = i;
            outputFile = o;
            puzzle = p;
            if (help || unparsed.Count > 1 || string.IsNullOrEmpty(inputFile) ||
                string.IsNullOrEmpty(outputFile) || string.IsNullOrEmpty(puzzle))
            {
                optionSet.WriteOptionDescriptions(Console.Out);
                return false;
            }
            return true;
        }

        static void InitLog()
        {
            var x = new log4net.Appender.ConsoleAppender { Layout = new log4net.Layout.PatternLayout("%message%newline") };
            BasicConfigurator.Configure(x);
        }
    }
}