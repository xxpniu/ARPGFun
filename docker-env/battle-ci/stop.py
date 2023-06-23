import yaml
import argparse
import os
import io

server_port_file="port.yaml"

def release_port(dir, server_id):
   file_name = os.path.join(dir,server_port_file)
   docker_compose = os.path.join(dir, server_id,"docker-compose.yml")
   if not os.path.exists(file_name) or not os.path.exists(docker_compose):
      raise SystemError(f"not found {file_name}")
   with open(file_name,"r") as file:
      used_ports = yaml.safe_load(file.read())
      if not used_ports: raise SystemError(f"error data: {file_name}")
   with open(docker_compose,"r") as file:
      docker_yaml = yaml.safe_load(file.read())
      for port in docker_yaml["services"]["battle"]["ports"]:
        p = int(port.split(':') [0])
        if p in used_ports["server_port"]: used_ports["server_port"].remove(p)
        if p in used_ports["service_port"]: used_ports["service_port"].remove(p)
   with open(file_name,"w+") as file:
      yaml.dump(used_ports,file)

parser = argparse.ArgumentParser(
                prog='',
                description='启动battleserver',
                epilog='')

parser.add_argument("-d", "--dir")
parser.add_argument("-s", "--serverid") 

if __name__ == "__main__" :
   args = parser.parse_args()
   dir = args.dir or "."
   server_id = args.serverid or "test"
   release_port(dir, server_id)