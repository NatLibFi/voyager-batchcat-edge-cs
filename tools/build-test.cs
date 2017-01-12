using System.Diagnostics;
using System.IO;
using NuGet.Packaging;

public delegate void procDelegate (string cmd, string args);

var strBuildDirectory = "work/build";
var strBatchCatMockDLL = strBuildDirectory + "/batchcat-mock.dll";
var strBatchCatEdgeDLL = strBuildDirectory + "/batchcat-edge.dll"; 

NuspecReader reader = new NuspecReader("Package.nuspec");
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

if (!Directory.Exists(strBuildDirectory)) {
  Directory.CreateDirectory(strBuildDirectory);
}

Console.WriteLine("Building BatchCatMock DLL");

StartProcess("mcs", "-target:library -platform:x86 -out:" + strBatchCatMockDLL + " contentFiles/App_Packages/" + reader.GetId() + "." + reader.GetVersion() + "/" + "BatchCatMock.cs");

Console.WriteLine("Building BatchCatEdge DLL");

StartProcess("mcs", "-target:library -platform:x86 -reference:" + strBatchCatMockDLL + " -out:" + strBatchCatEdgeDLL + " contentFiles/App_Packages/" + reader.GetId() + "." + reader.GetVersion() + "/" + "BatchCatEdge.cs");
