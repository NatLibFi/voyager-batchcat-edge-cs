#!/bin/bash

export NUGET_PACKAGE_ID="NatLibFi.Voyager.BatchCatEdge.Sources"

csharp tools/init.cs &&
csharp -lib:work/lib -r:NuGet.Packaging,System.Net.Http tools/build.cs
#csharp tools/lint.cs
