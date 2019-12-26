# AccessTokenCLI

A project used primarily as aid in learning Windows developement for future purposes. 
The project will contain one class library and one command line client. When the 
project is mature enough, i will start adding the compiled versions targeting several 
.NET frameworks to simplify the usage in real life situations.

Currently, both the library and the CLI application are compiled for .NET Framework 4.0, .NET Framework 4.5 and .NET Core.
This list of compilation targets is likely to expand in the future, as well as providing compiled binaries.

## TokenManage CLI

List processes on the system.

```powershell
PS Z:\repos\TokenManage\TokenManageCLI\bin\Debug\net40> .\TokenManageCLI.exe search -a
PID,   PROCESS,         USER
2132,  conhost,         BOMBCORP\Administrator
796,   svchost,         NT AUTHORITY\SYSTEM
616,   conhost,         BOMBCORP\test
524,   svchost,         NT AUTHORITY\SYSTEM
1324,  sppsvc,
1048,  svchost,         NT AUTHORITY\SYSTEM
1892,  powershell,      BOMBCORP\test
1580,  SppExtComObj,    NT AUTHORITY\NETWORK SERVICE
1040,  explorer,        BOMBCORP\test
2552,  powershell,      BOMBCORP\test
360,   TokenManageCLI,  BOMBCORP\test
228,   smss,
316,   csrss,
404,   winlogon,        NT AUTHORITY\SYSTEM
848,   svchost,         NT AUTHORITY\LOCAL SERVICE
1016,  svchost,         NT AUTHORITY\LOCAL SERVICE
668,   VBoxService,     NT AUTHORITY\SYSTEM
752,   svchost,         NT AUTHORITY\LOCAL SERVICE
1996,  taskhostex,      BOMBCORP\test
660,   dwm,             Window Manager\DWM-1
2172,  powershell,      BOMBCORP\Administrator
2076,  ServerManager,   BOMBCORP\test
2784,  msdtc,           NT AUTHORITY\NETWORK SERVICE
468,   lsass,           NT AUTHORITY\SYSTEM
912,   svchost,         NT AUTHORITY\NETWORK SERVICE
2424,  VBoxTray,        BOMBCORP\test
376,   wininit,         NT AUTHORITY\SYSTEM
552,   svchost,         NT AUTHORITY\NETWORK SERVICE
460,   services,
2060,  Taskmgr,         BOMBCORP\test
368,   csrss,
1344,  conhost,         BOMBCORP\test
1076,  wlms,            NT AUTHORITY\SYSTEM
184,   spoolsv,         NT AUTHORITY\SYSTEM
4,     System,
0,     Idle,
```

Start a new process (defaults to cmd.exe) using the access token from process ID 2172.

```powershell
PS Z:\repos\TokenManage\TokenManageCLI\bin\Debug\net40> .\TokenManageCLI.exe start -p 2172
```


## TokenManage class library and PowerShell

Some examples of how i envision the class library can be used with powershell in the future.
This may not be possible, we will see. I need to experiment with how process and thread tokens
are used.

Just look at the PS.cs file in the AccessTokenAPI project to see the shorthand commands available.

Currently, this works:

```powershell
Microsoft Windows [Version 10.0.18362.476]
(c) 2019 Microsoft Corporation. Med ensamrätt.

C:\Windows\system32>powershell -sta
Windows PowerShell
Copyright (C) Microsoft Corporation. All rights reserved.

Try the new cross-platform PowerShell https://aka.ms/pscore6

PS C:\Windows\system32> [System.Reflection.Assembly]::Load([IO.File]::ReadAllBytes("C:\Users\user\source\repos\TokenManage\TokenManageCLI\bin\Debug\net45\TokenManage.dll"))

GAC    Version        Location
---    -------        --------
False  v4.0.30319


PS C:\Windows\system32> get-process lsass

Handles  NPM(K)    PM(K)      WS(K)     CPU(s)     Id  SI ProcessName
-------  ------    -----      -----     ------     --  -- -----------
   1662      26     8424      17192      30,34    744   0 lsass


PS C:\Windows\system32> [TokenManage.PS]::ImpersonateProcessToken(744)
PS C:\Windows\system32> [Environment]::UserName
SYSTEM
```


```powershell
PS C:\Users\user> powershell -sta
Windows PowerShell
Copyright (C) Microsoft Corporation. All rights reserved.

Try the new cross-platform PowerShell https://aka.ms/pscore6

PS C:\Users\user> add-type -path C:\Users\user\source\repos\TokenManage\TokenManage\bin\Debug\netstandard2.0\TokenManage.dll

PS C:\Users\user> whoami /priv

PRIVILEGES INFORMATION
----------------------

Privilege Name                Description                            State
============================= ====================================== ========
SeShutdownPrivilege           Avsluta datorn                         Disabled
SeChangeNotifyPrivilege       Kringgå bläddringskontroll             Enabled
SeUndockPrivilege             Ta bort datorn från dockningsstationen Disabled
SeIncreaseWorkingSetPrivilege Öka allokerat minne för en process     Disabled
SeTimeZonePrivilege           Ändra tidszon                          Disabled

PS C:\Users\user> [TokenManage.PS]::EnablePrivilege("SeShutdownPrivilege")
True

PS C:\Users\user> whoami /priv

PRIVILEGES INFORMATION
----------------------

Privilege Name                Description                            State
============================= ====================================== ========
SeShutdownPrivilege           Avsluta datorn                         Enabled
SeChangeNotifyPrivilege       Kringgå bläddringskontroll             Enabled
SeUndockPrivilege             Ta bort datorn från dockningsstationen Disabled
SeIncreaseWorkingSetPrivilege Öka allokerat minne för en process     Disabled
SeTimeZonePrivilege           Ändra tidszon                          Disabled
```

```
powershell.exe -sta # Ensure single thread is used
[System.Reflection.Assembly]::Load([IO.File]::ReadAllBytes("path-to-dll-or-download-from-internet"))
[TokenManage.PS]::ListTokens()
[TokenManage.PS]::EnablePrivilege("SeDebugPrivilege")
[TokenManage.PS]::ImpersonateByPID(1064)
[TokenManage.PS]::GetSystem()
[TokenManage.PS]::Rev2self()
```
