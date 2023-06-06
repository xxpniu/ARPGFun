
  # 多人在线的动作游戏 
 

 
  ## 技术特点:
  * 1.使用unity为服务器。<br/>
  * 2.协议使用Grpc自动生产，双向流支持的长链接服务器<br/>
  * 3.UI系统使用自动生产代码，分离业务和UI查找，对重构支持代码敏感<br/>
  * 4.使用mongodb作为数据储存<br/>
  * 5.使用AI行为树的AI逻辑处理，可以直接运行时查看状态和编辑行为<br/>
  * 6.技能编辑可视化，基于layout/timeline<br/>
  * 7.基于状态同步的技术，反射注入的方式自动化完成协议链接<br/>
  * 8.基于zookeeper的服务器发现机制
  

  ## 编辑效果查看
   [演示视频]( https://youtu.be/jZGbP2sA7vY )

  
  ## 项目运行需求
  *  mongodb 版本
  *  Unity发布的server版本
  *  Gprc
  *  Zookeeper
  
  ## 项目目录结构
  *  client 战斗服务器和游戏客户端
  *  Doc 是相关策划文档已经迁移到了 
  *  Server 服务器目录
  *  Server/GServer/LoginServer 是中心账号服务器
  *  Server/GServer/GServer 是网关服务器用来承载用户数据
  *  Server/GServer/ChatServer 是聊天服务器
  *  Server/GServer/NotifyServer 是通知推送服务器 用来服务器消息推送 再由每个聊天服务器负责广播转发
  *  Server/GServer/MatchServer 战斗副本匹配服务器
  
  *  PublicTools 相关的工具目录
  *  PublicTools/econfigs 游戏的数据配表
  *  PublicTools/proto 游戏的网络协议
  *  PublicTools/src 自动编译工具源码输出目录
  *  PublicTools/toolssrc 工具的源码
  
  
  ## 项目启动
  *  编译服务器 buildstart.sh 
  *  发布unity客户端 和 服务器配置相关服务器参数

  ## 自动生成协议
  ···
    cd PublicTools 
    docker build -t build-ci . && docker run --rm -it build-ci 
  ···
  
  
  
  
  
