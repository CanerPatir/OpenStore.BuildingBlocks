#!/usr/bin/env bash

dotnet pack -c Release -p:IncludeSymbols=false -p:SymbolPackageFormat=snupkg
dotnet nuget push **/*.nupkg -s $HOME/local_nuget --skip-duplicate
dotnet nuget push **/*.snupkg -s $HOME/local_nuget --skip-duplicate
