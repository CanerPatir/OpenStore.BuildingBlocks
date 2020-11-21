#!/usr/bin/env bash

dotnet pack -c Release -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg
dotnet nuget push **/*.nupkg -s /Users/caner.patir/local_nuget --skip-duplicate
dotnet nuget push **/*.snupkg -s /Users/caner.patir/local_nuget --skip-duplicate
