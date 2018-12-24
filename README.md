# DotnetEfDbcontextConverter

.NET Core/Standard Entity Framework Database-First Context Converter

# Description 
Optimizes generated DbContext output of "dotnet ef dbcontext scaffold".
- Makes DB schema changeable at runtime" + Environment.NewLine +
- Removes OnConfiguring method (including connectionString), so you can implement your own partial OnConfiguring method outside the generated context.

#Usage

`dotnet DotnetEfDbcontextConverter.exe path\myDbContext.cs`

Or in combination with your scaffolding command:

```
C:\projects\myproject\projectDb\> 
dotnet ef dbcontext scaffold "server=mysqltest;port=3306;user=myuser;password=mypw;database=projectDb" MySql.Data.EntityFrameworkCore -o Models -f && dotnet C:\DotnetEfDbcontextConverter.dll Models\projectDbContext.cs
```

Using .NET Core, you can run the DLL directly via "dotnet" command.
If you want an EXE file, run

`dotnet publish -c Release -r win10-x64`

within the project's directory.


#Download
- coreapp-dll.zip contains the compiled DLL (recommended)
- coreapp-exe.zip contains the release Exe (quite large)


# Links
- Website http://www.dxsdata.com/2018/02/net-clipboard-tools/