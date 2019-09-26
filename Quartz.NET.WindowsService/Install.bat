%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\installutil.exe E:\Quartz.NET\Quartz.NET.WindowsService\bin\Debug\Quartz.NET.WindowsService.exe

Net Start Quartz.Net

sc config Quartz.Net start= auto