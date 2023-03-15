using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;

namespace AddSnippet;

internal class Program
{
    private static void Main(string[] args)
    {
        try
        {

            var normalArgs = args.Where(x => x[0] != '-').ToArray();
            var optionArgs = args.Where(x => x[0] == '-').SelectMany(x => x.Skip(1)).ToHashSet();

            var clipboard = optionArgs.Contains('c');
            var setSnipDir = optionArgs.Contains('d');

            string snipDir = string.Empty;

            if (setSnipDir)
            {
                if (normalArgs.Length != 1)
                {
                    Console.Error.WriteLine($"to set the Snippets directory please specify only one argument");
                    Environment.Exit(1);
                }
                snipDir = GetOrSetSnipDir(normalArgs[0]);
            }
            else
            {
                snipDir = GetOrSetSnipDir(normalArgs[0]);
            }

            if (clipboard)
            {
                if (normalArgs.Length < 1)
                {
                    Console.Error.WriteLine("abbreviation for snippet not specified");
                    Environment.Exit(1);
                }

                string text = string.Empty;
                new Thread(() => text = Clipboard.GetText().Trim()).RunSta();

                if (string.IsNullOrEmpty(text))
                {
                    Console.Error.WriteLine("There is no valid text on the clipboard.");
                }

                text = text.Replace("$", "$$");

                var abbreviation = normalArgs[0];
                var title = normalArgs.Length > 1 ? normalArgs[1] : string.Empty;

                var snipdoc = GetSnipDoc(title: title, abbreviation: abbreviation, text: text);
                var filename = Path.Combine(snipDir, $"{abbreviation}.snippet");
                snipdoc.Save(filename);
            }

        }
        catch (Exception ex)
        {
            var fullname = System.Reflection.Assembly.GetEntryAssembly().Location;
            var progname = Path.GetFileNameWithoutExtension(fullname);
            Console.Error.WriteLine($"{progname} Error: {ex.Message}");
        }
    }

    private static XDocument GetSnipDoc(string title, string abbreviation, string text)
    {
        var doc = new XDocument(new XElement("CodeSnippets",
            new XElement("CodeSnippet", new XAttribute("Format", "1.0.0"),
                new XElement("Header",
                    new XElement("Title", title),
                    new XElement("ShortCut", abbreviation)),
            new XElement("Snippet",
                new XElement("Code", new XAttribute("Language", "CSharp"),
                    new XCData(text))))));
        return doc;
    }

    /// <summary>
    /// Currently SnipDir is the only setting. Gets the snipdir from the settings docs and
    /// create a default settings doc if it does not exist.
    /// </summary>
    /// <param name="snipDir">The code Snippets folder</param>
    private static string GetOrSetSnipDir(string dir)
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(userProfile, "AddSnippet");
        Directory.CreateDirectory(appFolder);
        var settingsFilename = Path.Combine(appFolder, "settings.xml");
        if (!File.Exists(settingsFilename) || !string.IsNullOrEmpty(dir))
        {
            var settingsDoc = new XDocument(new XElement("Settings", new XElement("SnipDir", dir)));
            settingsDoc.Save(settingsFilename);
            return dir;
        }
        else
        {
            var doc = XDocument.Load(settingsFilename);
            return doc.Root.Element("SnipDir")?.Value;
        }
    }
}
