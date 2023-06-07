import xlrd 
import io 
import sys 


class TableCol:
    index : int
    col_name: str
    col_des: str
    value_type: str 
    def __str__(self):
        return f"{self.index}.{self.col_name} {self.value_type} {self.col_des}"
    def __repr__(self):
        return self.__str__()
    
    def __init__(self,index,name,des,v_type):
        self.index = index
        self.col_name = name
        self.col_des = des
        self.value_type = v_type  

    def get_value_type(self):
        if self.value_type == "Int": return "int" 
        elif self.value_type == "String": return "string"
        elif self.value_type == "Float": return "float"
        else: raise TypeError(f"type of {self.value_type} not supported!")     

class Table:
    cols : list[TableCol]
    row_datas: list[list]
    def __init__(self, data, name, file_name, des):
        self.name = name 
        self.file = file_name
        self.des = des
        self.data = data
        self.read_cols()
        self.read_rows()

    def read_cols(self):
        self.cols = []
        ncols = self.data.ncols
        for col in range(1,ncols):
            name , v_type , des = self.data.cell_value(0,col),self.data.cell_value(1,col),self.data.cell_value(2,col)
            if name == "" or name == 0: break
            c = TableCol(col,name,des,v_type)
            self.cols.append(c)
            
    def read_rows(self):
        nrows = self.data.nrows 
        self.row_datas = []
        for r in range(3,nrows):
            row  = []
            for c in self.cols:
                row.append(self.data.cell_value(r,c.index))
                pass
            self.row_datas.append(row)

    def __str__(self):
        return f"{self.name} of export {self.file} {self.des} of cols length:{len(self.cols)}"   


class Sheet:
     
    def __init__(self, file):
        self.file_name = file
        self.process()

    def process(self):
        data = xlrd.open_workbook(self.file_name)
        index_table = data.sheet_by_name("__Base")
        ncols = index_table.ncols
        nrows = index_table.nrows
        if ncols <4 or nrows <1 : raise ValueError("index table row must >= 1 and col must >=4")
        self.tables = []
        for i in range(0,nrows):
            table_sheet, table_name , export_file , table_des = index_table.cell_value(i,0) , index_table.cell_value(i,1) ,index_table.cell_value(i,2),index_table.cell_value(i,3)
            t = Table(data.sheet_by_name(table_sheet),table_name, export_file, table_des)
            self.tables.append(t)
            print(f"[{i}] process:{t}")
            print(t.cols)
            #print(t.row_datas)    
        

if __name__ == "__main__" :
    t = Sheet(f"../econfigs/1_StatData.xlsx")
  