using System;
using System.IO;
using System.IO.Compression;
using System.Xml.Linq;

class CheckEpub
{
    static void Main()
    {
        var filePath = @"d:\Projects\FileLancet\test.epub";
        using var stream = File.OpenRead(filePath);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        
        Console.WriteLine("Entries in test.epub:");
        foreach (var entry in archive.Entries)
        {
            Console.WriteLine($"  {entry.FullName}");
        }
        
        var containerEntry = archive.GetEntry("META-INF/container.xml") 
            ?? archive.GetEntry("META-INF\\container.xml");
        
        if (containerEntry != null)
        {
            Console.WriteLine($"\nFound container.xml at: {containerEntry.FullName}");
            using var containerStream = containerEntry.Open();
            var doc = XDocument.Load(containerStream);
            Console.WriteLine("\ncontainer.xml content:");
            Console.WriteLine(doc.ToString());
            
            var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;
            var rootfile = doc.Root?.Element(ns + "rootfiles")?.Element(ns + "rootfile");
            var opfPath = rootfile?.Attribute("full-path")?.Value;
            Console.WriteLine($"\nOPF path from container.xml: {opfPath}");
            
            if (!string.IsNullOrEmpty(opfPath))
            {
                var opfEntry = archive.GetEntry(opfPath.Replace("\\", "/"))
                    ?? archive.GetEntry(opfPath);
                Console.WriteLine($"OPF entry found: {opfEntry != null}");
                if (opfEntry == null)
                {
                    Console.WriteLine($"Tried paths: '{opfPath.Replace("\\", "/")}' and '{opfPath}'");
                }
            }
        }
        else
        {
            Console.WriteLine("container.xml not found!");
        }
    }
}
