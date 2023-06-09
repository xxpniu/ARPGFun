
  # 部署流程
  ### 需求环境
  - docker & docker compose 
      下载网址  https://docs.docker.com/get-docker/
 
  - Unity 2022.3.1 +
  - python3 & pip

      ```
       pip install kazoo
      ```

  ### 编译
  - 使用 buildforci.sh  编译服务器代码

    ```
    cd arpgfun 
    mkdir publish 
    sh buildforci.sh 
    ```
  - 启动环境 mongo 和 zookeeper
   docker-env/server-env中修改
  ```
  ADDRESS=localhost  #your ip
  REMOTE=$ADDRESS
  MONGO=$ADDRESS
  ZK=$ADDRESS:2181
  KAFKA=$ADDRESS:9092
  ``` 

  ```
   cd docker-env/server-env
   sh run.sh
  ```
  - 首次需要上传配置到zookeeper服务器
```python
if __name__ == "__main__":
   args = parser.parse_args()
   root = args.root or "/configs"
   dir = args.dir or "./client/Assets/Resources/Json"
   host = args.host or "localhost:2181"  # your zookeeper ip
   print(f"{args}")
   upload(host,root, dir)
 
```

   ```
   cd PublicTools 
   python3 uploadzk.py
   ```

- 同样是方式启动 server-nobattle

- 修改文件
  client/Assets/StreamingAssets/client.json

  ```js
  {
    "LoginServerHost": "localhost",
    "LoginServerPort": 9000
  }

  ```

- 启动Unity打开client 
- 打开场景 
  client/Assets/Scenes/Launch.unity
  ps:
   需要注意模式 

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
  * 9.全docker化
  

  ## 编辑效果查看
   [演示视频]( https://youtu.be/jZGbP2sA7vY )

  
  ## 项目运行需求
  *  mongodb 版本
  *  Unity发布的server版本
  *  Gprc
  *  Zookeeper
  *  docker
  
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
 
  
  
  ## 自动生成协议
   使用github CI 完成自动处理
  
  
  
  
