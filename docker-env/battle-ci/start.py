#pip install pyyaml

import yaml
import argparse
import os
import io



def setup_env(iamge_name:str, dir:str, server_id:str, server_ip:str, server_port:int, service_ip:str, service_port:int, zk:setattr):
   root_dir = os.path.join(dir, server_id)
   if not os.path.exists(root_dir):
      os.makedirs(root_dir)
   file_name = os.path.join(root_dir, "docker-compose.yml")
   print(f"path:{file_name}")
   services = f"""version: '3'
services: 
    battle:
        image: '{iamge_name}'
        ports: 
            - '{server_port}:{server_port}'
            - '{service_port}:{service_port}' 
        environment: 
            - HOST_ADDRESS='{server_ip}'     
            - LISTEN_ADDRESS='{service_ip}'
            - HOST_PORT={service_port}
            - LISTEN_PORT={server_port}
            - ZK_SERVER='{zk}'
#            - KAFKA_SERVER= 
            - MAX_PLAYER=10
            - BATTLE_ID={server_id}
   """
   config = yaml.safe_load(services)
   print(config)

   with open(f"{dir}/{server_id}/docker-compose.yml","w") as file:
      #yaml.dump(config, file)
      file.write(services)
   pass   


parser = argparse.ArgumentParser(
                prog='',
                description='启动battleserver',
                epilog='')

parser.add_argument("-d","--dir")
parser.add_argument("-s","--serverid") 
parser.add_argument("-z","--zookeeper")
parser.add_argument("-i","--image")
parser.add_argument("-ip","--serverip")
parser.add_argument("-p","--port")
parser.add_argument("-si","--serviceip")
parser.add_argument("-sp", "--serviceprot")


if __name__ == "__main__" :
   args = parser.parse_args()
   dir = args.dir or "."
   zk = args.zookeeper or "andew25a.synology.me:1001"
   image = args.image or "battle:latest"
   server_id = args.serverid or "test"
   server_ip = args.serverip or "andew25a.synology.me"
   server_port = args.port or 2000
   service_ip = args.serviceip or "192.168.1.8"
   service_port = args.serviceprot or 1400

   setup_env(image, dir, server_id,server_ip, server_port,service_ip, service_port, zk)

