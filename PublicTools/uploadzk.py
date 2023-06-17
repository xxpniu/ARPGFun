
import os
import io
import sys
from kazoo.client import KazooClient
import argparse


parser = argparse.ArgumentParser(
                prog='process execl',
                description='excel 解析 to cs',
                epilog='')

parser.add_argument("-r","--root")
parser.add_argument("-o","--host")
parser.add_argument("-d","--dir")


def upload(host, root, dir):
    print(root,host,dir)
    zk = KazooClient(host, 3000)
    zk.start()

    zroot = zk.exists("%s" % root)
    if not zroot:
        zk.create(root,b"")
    dir_root = os.path.join(os.getcwd(),dir)
    files = os.listdir(dir_root)
    for n in files:
        f = open("%s/%s" % (dir_root, n), "rb")
        path = "%s/%s" % (root, n)
        exs = zk.exists(path)
        if exs:
            zk.set(path, f.read())
        else:
            zk.create(path, f.read())
        print("Upload file:%s"%(path ))
    zk.stop()


 

if __name__ == "__main__":
   args = parser.parse_args()
   root = args.root or "/configs"
   dir = args.dir or "../client/Assets/Resources/Json"
   host = args.host or "andew25a.synology.me:1001"
   print(f"{args}")
   upload(host,root, dir)
 
