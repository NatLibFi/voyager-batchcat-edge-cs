set NUGET_PACKAGE_ID="NatLibFi.Voyager.BatchCatEdge.Sources"

csharp tools/init.cs &&
csharp -lib:work/lib -r:NuGet.Packaging.dll tools/compile-build.cs &&
#csharp tools/lint.cs
