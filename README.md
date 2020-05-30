# 运行环境
.net core3.1
# 开箱即用
直接运行Quartz.NetUI\Quartz.NET.Web目录下run.bat文件或部署项目。
登陆token存放于appsettings.json节点token
管理员帐号位于节点superToken
# 在线演示地址
http://task.volcore.xyz/  登陆口令为task123456
# 如有问题+群：45221949
# 码云地址：https://gitee.com/x_discoverer/Quartz.NetUI
  # 作业列表
 ![作业列表](https://github.com/cq-panda/Quartz.NetUI/blob/master/Quartz.NET.Web/wwwroot/images/example/tasklist.png)
   # 新建作业
 ![新建作业](https://github.com/cq-panda/Quartz.NetUI/blob/master/Quartz.NET.Web/wwwroot/images/example/add.png)
 # 修改作业
  ![修改作业](https://github.com/cq-panda/Quartz.NetUI/blob/master/Quartz.NET.Web/wwwroot/images/example/update.png)
   # 查看日志
  ![查看日志](https://github.com/cq-panda/Quartz.NetUI/blob/master/Quartz.NET.Web/wwwroot/images/example/log.png)
# 配置文件QuartzSettings
配置文件QuartzSettings由系统自动生成,所在位置与当前项目同级,生成文件包括作业参数配置及日志文件初始化。
# 配置文件目录结构
 ![配置目录结构](https://github.com/cq-panda/Quartz.NetUI/blob/master/Quartz.NET.Web/wwwroot/images/example/dir.png)
 
 # 项目结构
  ![项目结构](https://github.com/cq-panda/Quartz.NetUI/blob/master/Quartz.NET.Web/wwwroot/images/example/project1.png)
  ![项目结构](https://github.com/cq-panda/Quartz.NetUI/blob/master/Quartz.NET.Web/wwwroot/images/example/project2.png)
 
├─Constant
│      QuartzFileInfo.cs
│      
├─Controllers
│      HealthController.cs
│      HomeController.cs
│      TaskBackGroundController.cs
│      
├─Enum
│      JobAction.cs
│      
├─Extensions
│      ConvertPath.cs
│      QuartzNETExtension.cs
│      
├─Filters
│      TaskAuthorizeFilter.cs
│      
├─Models
│      TaskLog.cs
│      TaskOptions.cs
│      
├─Utility
│      FileHelper.cs
│      FileQuartz.cs
│      HttpContext.cs
│      HttpManager.cs
│      HttpResultful.cs
│      TaskCurrent.cs
│      
├─Views  
│─TaskBackGround
│          Index.cshtml
│          
└─wwwroot
    │      task_index.css       
    │      
    ├─iView
    │      iview.min.js
    │      
    ├─js
    │      task-index.js               
    └─vue
            vue.js
