
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
parser.add_argument("-h","--host")
parser.add_argument("-d","--dir")


def upload(host, root, dir):
    zk = KazooClient(host, 3000)
    zk.start()

    zroot = zk.exists("%s" % root)
    if not zroot:
        zk.create(root,"")

    files = os.listdir(dir)
    for n in files:
        f = open("%s/%s" % (dir, n), "rb")
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
   dir = args.dir or "./src/json"
   host = args.host or "localhost:2181"

   upload(host,root, dir)
 
