using System.Diagnostics;
using System.IO;
using System.Net.Http;
using NuGet.Packaging;

public delegate void procDelegate (string cmd, string args);

HttpResponseMessage httpResponse;
var strMsiExtractDirectory = "work/msi";
var strBuildDirectory = "work/build";
var strDistDirectory = "work/dist";
var strBatchCatDLL = strDistDirectory + "/batchcat.dll";
var strBatchCatEdgeDLL = strDistDirectory + "/batchcat-edge.dll";
var strVoyagerMsi = strBuildDirectory + "/voyager.msi";
var strVoyagerInstallUrl = "http://knowledge.exlibrisgroup.com/@api/deki/files/42346/VoyagerInstall.msi";
var strMsiBatchCatDLL = "work/msi/Voyager/System/BatchCat2009-21.dll";

NuspecReader reader = new NuspecReader("Package.nuspec");
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
  strStderr = proc.StandardError.xReadToEnd();
  
  proc.WaitForExit();

  if (proc.ExitCode != 0) {
    Console.WriteLine("ERROR: Command " + cmd + " failed:");
    Console.WriteLine(strStdout);
    Console.WriteLine(strStderr);
    Environment.Exit(-1);
  }

};

if (!Directory.Exists(strBuildDirectory)) {
  Directory.CreateDirectory(strBuildDirectory);
}

if (!Directory.Exists(strDistDirectory)) {
  Directory.CreateDirectory(strDistDirectory);
}

if (!Directory.Exists(strMsiExtractDirectory)) {
  Directory.CreateDirectory(strMsiExtractDirectory);
}

Console.WriteLine("Downloading Voyager installation package");
httpResponse = await new HttpClient().GetAsync(strVoyagerInstallUrl);
await httpResponse.Content.CopyToAsync(new FileStream(strVoyagerMsi, FileMode.CreateNew));

Console.WriteLine("Extracting BatchCat DLL from Voyager installation package");

StartProcess("msiexec", "/qn /a " + strVoyagerMsi + " TARGETDIR=" + File.GetFullPath(strMsiExtractDirectory));

Console.WriteLine("Importing typelib from the BatchCat DLL");

StartProcess("tlbimp2", "/out:" + strBatchCatDLL + " " + strMsiBatchCatDLL);

Environment.Exit(0);

Console.WriteLine("Building BatchCatEdge DLL");

StartProcess("mcs", "-target:library -platform:x86 -reference:" + strBatchCatDLL + " -out:" + strBatchCatEdgeDLL + " contentFiles/App_Packages/" + reader.GetId() + "." + reader.GetVersion() + "/" + "BatchCatEdge.cs");
