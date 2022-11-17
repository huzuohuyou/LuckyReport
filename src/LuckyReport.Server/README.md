## 数据库模型同步
- sqlite 安装 Microsoft.EntityFrameworkCore.Sqlit
```
Install-Package Microsoft.EntityFrameworkCore.Tools
Add-Migration InitialCreate[名称每次变动]
Update-Database
```