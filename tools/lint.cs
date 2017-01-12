using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

public delegate string getStringDelegate();

Process proc;
var strBuildDirectory = "work/build";
var strPackageDirectory = "work/nuget";
getStringDelegate GetGendarmeExecutable = () => {
  
  var strGendarmeDirectory = Directory.GetDirectories(strPackageDirectory).Single((strDirectory) => new Regex("/Mono\\.Gendarme\\.").IsMatch(strDirectory));
  return strGendarmeDirectory + "/tools/gendarme.exe";
  
};

if (Directory.GetFiles(strBuildDirectory, "*.dll").Length == 0) {
  Console.WriteLine("No files to run lint for");
  Environment.Exit(-1);
} else {
  
  proc = new Process();
  proc.StartInfo.FileName = "mono";
  proc.StartInfo.RedirectStandardOutput = true;
  proc.StartInfo.RedirectStandardError = true;
  proc.StartInfo.UseShellExecute = false;
  proc.StartInfo.Arguments = GetGendarmeExecutable() + " " + strBuildDirectory + "/*.dll";

  proc.Start();
  proc.WaitForExit();

  if (proc.ExitCode != 0) {
    Console.WriteLine("ERROR: " + (proc.ExitCode == -1 ? "Executing Gendarme failed" : "Assemblies did not pass the check") + ":");
    Console.WriteLine(proc.StandardOutput.ReadToEnd());
    Console.WriteLine(proc.StandardError.ReadToEnd());
    Environment.Exit(-1);
  }
    
}
