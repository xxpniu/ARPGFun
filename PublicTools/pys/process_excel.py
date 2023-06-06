from table2cs import gen_cs
from sheet_read import Sheet, Table, TableCol
import io
import os
import argparse
import json





parser = argparse.ArgumentParser(
                prog='process execl',
                description='excel 解析 to cs',
                epilog='')

parser.add_argument("-f","--dir")
parser.add_argument("-o","--output")
parser.add_argument("-n","--namespace")
parser.add_argument("-d","--filepath")

if __name__ == "__main__":
   args = parser.parse_args()
   relevant_path = args.dir or "../econfigs/"
   namespace = args.namespace or "EConfig"
   output = args.output or "../src/csharp/ExcelConfig.cs"
   file_path = args.filepath or "../output/"
   included_extensions =  ['xlsx']
   file_names = [fn for fn in os.listdir(relevant_path)
              if any(fn.endswith(ext) for ext in included_extensions)]
   
   cs_file_temp ='''
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExcelConfig;
namespace [NAMESPACE]
{
    [TABLES]
}
   '''
   cs_class=""
   for file in file_names:
        s = Sheet(f"{relevant_path}{file}")
        for t in s.tables:
            cs_class = cs_class + gen_cs(t)
            file_data = open(f"{file_path}/{t.file}", 'w', encoding='utf-8')
            file_data.write(json.dumps(t.row_datas))
            file_data.close()
   cs_file = cs_file_temp.replace("[NAMESPACE]",namespace).replace("[TABLES]",cs_class)
   print(f"output:{output} \n {cs_file}")
   file_object = open(output, 'w',encoding='utf-8')
   file_object.write(cs_file)
   file_object.close( )

