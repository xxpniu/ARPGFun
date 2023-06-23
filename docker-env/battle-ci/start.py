#pip install pyyaml

import yaml
import argparse
import os
import io

battle_service_port_range = [1800,1899]
battle_port_range = [2400,2499]
server_port_file="port.yaml"


def get_ports(dir:str):
   file_name = os.path.join(dir,server_port_file)
   
   if not os.path.exists(file_name):
     used_ports = {
        "server_port" :[battle_port_range[0]],
        "service_port":[battle_service_port_range[0]]
     }
     with open(file_name,"w") as file:
        yaml.dump(used_ports, file)   
     return battle_port_range[1],battle_service_port_range[1]
   else:
        with open(file_name,"r") as file:
            file_str = file.read()
            print(file_str)
        used_ports = yaml.safe_load(file_str)
        server_port = None
        service_port = None
        for port in range(battle_port_range[0],battle_port_range[1],1):
            if port not in used_ports["server_port"]:
                server_port = port
                break
        for port in range(battle_service_port_range[0],battle_service_port_range[1],1):
            if port not in used_ports["service_port"]:
                service_port = port
                break 
        if server_port and service_port:
            used_ports["server_port"].append(server_port)
            used_ports["service_port"].append(service_port)
            with open(file_name,"w+") as file:
                yaml.dump(used_ports,file)
            print(used_ports)
            return server_port, service_port   
        else:
            raise SystemError("all port is inused")
        pass
   pass

def setup_env(map_id:str, iamge_name:str, dir:str, server_id:str, server_ip:str, server_port:int, service_ip:str, service_port:int, zk:setattr):
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
            - MAP_ID={map_id}
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

parser.add_argument("-d", "--dir")
parser.add_argument("-s", "--serverid") 
parser.add_argument("-z", "--zookeeper")
parser.add_argument("-i", "--image")
parser.add_argument("-ip","--serverip")
parser.add_argument("-si","--serviceip")
parser.add_argument("-m", "--map")


if __name__ == "__main__" :
   args = parser.parse_args()
   dir = args.dir or "."
   zk = args.zookeeper or "andew25a.synology.me:1001"
   image = args.image or "battle:latest"
   server_id = args.serverid or "test"
   server_ip = args.serverip or "andew25a.synology.me"
   service_ip = args.serviceip or "192.168.1.8"
   map_id= args.map or "1"

   server_port,service_port = get_ports(dir)

   setup_env(map_id, image, dir, server_id,server_ip, server_port,service_ip, service_port, zk)

