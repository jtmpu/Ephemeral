# Ephemeral

![](https://github.com/jtmpu/Ephemeral/workflows/.github/workflows/main.yml/badge.svg)

A suite of tools for lateral movements within a windows environment.

# Solution description

The solution Ephemeral is a suite of sub-projects with the intention of simplifying lateral movements within larger
Windows enterprise environments. The solution builds several executables and shared libraries which can be used
in various situtations to perform privilege escalations or setting up a C2 infrastructure. 

The purpose of all tools is to leave a minimal footprint, preferably never leaving any footprint on the disk. 
Obviously, this could prove to be impossible - but it's currently an experiment. It's all written in C#, using 
the native Windows library instead of importing 100000000 frameworks. I might be forced to write in C++ in the
future, due to some limitations. We will see.

All projects are compiled targeting the .NET 4.0, .NET 4.5 and .NET Core 3.0 frameworks. This list will probably expand
in the future.

# Dependencies

The only two dependencies are currently CommandLineParser and ILMerge. CommandLineParser has been used due to laziness,
because i could not be bothered to write a simple command line argument parser. I will probably write something minimalistic
for this in the future, to further reduce the amount of external dependencies.

ILMerge is used to embedd all require (and project generated) shared libraries into the executables. ILMerge is hosted at
github.com/dotnet/ILMerge. I regard this as trustworthy, and i can't be bothered to write any shared library embedders myself.
The shared libraries are automatically embedd in the executable when compiling with "Release". The merged executable will end
up in the folder e.g. "bin/Release/net40/merged/...". No merging is performed for "Debug" builds.

# Projects

## AccessTokenAPI and AccessTokenCLI

These two projects aim at abusing access tokens for processes on a local system. The executable and the DLL can be used to 
borrow these tokens. The DLL has partially been designed to be loaded directly in powershell, to simplify its distribution,
and hopefully avoid writing anything to disk.

## GhostAPI, GhostNode and GhostCLI

These three projects are under development. The intention is to create a solid foundation for remote shell on computers
within the environment, which can be used to load other useful assemblies such as AccessTokenAPI.

## WinAPI

This project contains all the Native functions, structures and constants used for integration with Windows. Almost everything
here has been borrow from pinvoke.net.
