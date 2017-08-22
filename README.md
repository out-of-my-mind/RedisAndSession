# RedisAndSession
#### 简介
>##### 这个项目重载的session基于底层的会话类，通过创建一个继承 SessionStateStoreProviderBase 类的类，来实现自定义会话状态存储到Redis中。
##### 在本项目中，我会尽量把注释写的详细些，方便我后期查看和改进

#### 文件结构
>Client //客户端   
>>RedisSessionStateProvider.cs //重载 session会话类
>>WebForm1.aspx //用户登入会话首页
>  
>Example  //一些在使用到的源代码，也就是使用到的重载类 
>   
>RedisStudy  //ServiceStack.Redis 封装的类  
>  
>WindowsFormsRedisApp //相当于服务器，展示目前客户端用户会话     
>>Form1.cs    //主窗口，负责展示     
> 
>RedisAndSession.sln            
#### 更新日志
>##### 2017-08-22 上传了简单的demo，实现用户会话保存到Redis，并展示会话
