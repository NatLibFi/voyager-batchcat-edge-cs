#!/usr/bin/env csharp

using System.Diagnostics;
using System.IO;

public delegate void procDelegate (string cmd, string args);
public delegate void copyAssembliesDelegate ();

var strDotnetFramework = "net45";
var strWorkDirectory = "work";
var strCommonAssemblyDirectory = "work/lib";
var strInstallDirectory = strWorkDirectory + "/nuget";
var strNugetPrimarySource = "https://www.nuget.org/api/v2/";
Process proc = new Process();
procDelegate StartProcess = (cmd, args) => {

  string strStdout, strStderr;
  Process proc = new Process();
  
  proc.StartInfo.FileName = cmd;
  proc.StartInfo.RedirectStandardOutput = true;
  proc.StartInfo.RedirectStandardError = true;
  proc.StartInfo.UseShellExecute = false;
  proc.StartInfo.Arguments = args;
  
  proc.Start();

  strStdout = proc.StandardOutput.ReadToEnd();
  strStderr = proc.StandardError.ReadToEnd();
  
  proc.WaitForExit();

  if (proc.ExitCode != 0) {
    Console.WriteLine("ERROR: Command " + cmd + " failed:");
    Console.WriteLine(strStdout);
    Console.WriteLine(strStderr);
    Environment.Exit(-1);
  }

};
copyAssembliesDelegate CopyAssemblies = () => {
  foreach(var strDirectory in Directory.GetDirectories(strInstallDirectory)) {

    var strAssemblyDirectory = strDirectory + "/lib/" + strDotnetFramework;

    if (Directory.Exists(strAssemblyDirectory)) {
      foreach (var strFile in Directory.GetFiles(strAssemblyDirectory, "*.dll")) {
        File.Copy(strFile, strCommonAssemblyDirectory + "/" + Path.GetFileName(strFile));
      }
    }
  }
};

  if (String.IsNullOrEmpty(Environment.GetEnvironmentVariable("NUGET_PACKAGE_ID"))) {
  Console.WriteLine("ERROR: Environment variable NUGET_PACKAGE_ID is not defined");
  Environment.Exit(-1);
} else {
      
    Console.WriteLine("Creating a package");
    StartProcess("nuget.exe", "pack -OutputDirectory " + strWorkDirectory);
    
    Console.WriteLine("Installing the package");
    StartProcess("nuget.exe", "install " + Environment.GetEnvironmentVariable("NUGET_PACKAGE_ID") + " -OutputDirectory " + strInstallDirectory + " -Source " + Path.GetFullPath(strWorkDirectory) + " -FallbackSource " + strNugetPrimarySource);

    Console.WriteLine("Copying assemblies under a common directory");

    if (!Directory.Exists(strCommonAssemblyDirectory)) {
      Directory.CreateDirectory(strCommonAssemblyDirectory);
    }

    CopyAssemblies();
    
}
