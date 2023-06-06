
import os
import io
import sys
from kazoo.client import KazooClient


HOST = "129.211.9.75:2181"
DIR = "./src/json"
ROOT = "/configs"

i = 1
while i < len(sys.argv):
    if sys.argv[i] == '--root':
        i += 1
        ROOT = sys.argv[i]
    elif sys.argv[i] == '--host':
        i += 1
        HOST = sys.argv[i]
    elif sys.argv[i] == '--DIR':
        i += 1
        HTTP_HOST = sys.argv[i]
    pass
        
    i += 1
    pass

zk = KazooClient(HOST, 3000)
zk.start()

zroot = zk.exists("%s" % ROOT)
if not zroot:
    zk.create(ROOT,"")

files = os.listdir(DIR)
for n in files:
    f = open("%s/%s" % (DIR, n), "rb")
    path = "%s/%s" % (ROOT, n)
    exs = zk.exists(path)
    if exs:
        zk.set(path, f.read())
    else:
        zk.create(path, f.read())
    print("Upload file:%s"%(path ))
zk.stop()
