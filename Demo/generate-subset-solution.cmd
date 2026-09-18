@echo off
REM Generates the SOC124 customer demo solution.

cd /d "%~dp0"

REM Remove a solution left by an earlier run. Anyone who generated with a .NET 10 SDK
REM before the --format flag was pinned has a stale SOC124.slnx here, and leaving both
REM files side by side invites tooling to open the wrong one.
if exist "SOC124.slnx" del /q "SOC124.slnx"
if exist "SOC124.sln" del /q "SOC124.sln"

REM Force the classic .sln format: nuget.exe restore and MSBuild.exe (used to build the
REM non-SDK-style PortBridgeUtils/Demo_PB .NET Framework 4.8 projects) only understand
REM .slnx starting with MSBuild 17.12+, which is newer than what CI/customer agents have.
dotnet new sln -n SOC124 -o "." --force --format sln
set "SLN=SOC124.sln"

dotnet sln "%SLN%" add ..\Core\Csra\Csra.csproj --solution-folder Core
dotnet sln "%SLN%" add ..\Core\Tol\Tol.csproj --solution-folder Core
dotnet sln "%SLN%" add ..\ReferenceLibraries\PortBridge\PortBridgeUtils\PortBridgeUtils.csproj --solution-folder Core

dotnet sln "%SLN%" add ..\ReferenceLibraries\CsTestMethods\CsTestMethods.csproj --solution-folder Examples
dotnet sln "%SLN%" add ..\ReferenceLibraries\CsraTestMethods\CsraTestMethods.csproj --solution-folder Examples
dotnet sln "%SLN%" add ..\ReferenceLibraries\UnitTests\UnitTestExample.csproj --solution-folder Examples
dotnet sln "%SLN%" add ..\ReferenceLibraries\PortBridge\Demo_PB\Demo_PB.csproj --solution-folder Examples

if "%CI%"=="" pause
