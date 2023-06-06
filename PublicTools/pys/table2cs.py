from sheet_read import Sheet, Table, TableCol

def gen_cs(table):
    class_temp ='''
/// <summary>
/// [CLASS_DES]
/// </summary>    
[ConfigFile("[TABLE_FILE]","[TABLE_NAME]")]    
class [TBALE_NAME] : JSONConfigBase
{
    [PROPERTIES]
}  
    '''
    property_temp='''
    /// <summary>
    /// [DES]
    /// </summary>
    [ExcelConfigColIndex([COL_INDEX])]public [TYPE] [NAME] {set;get;}'''

    properties = ""
    index = 0
    for col in table.cols:
        if col.col_name=="ID": continue
        index = index + 1
        p = property_temp.replace("[COL_INDEX]",f"{index}").replace("[TYPE]", col.get_value_type()).replace("[NAME]",col.col_name).replace("[DES]",col.col_des)
        #print(f"gen:{p}")
        properties = properties+p

    return class_temp.replace("[CLASS_DES]",table.des).replace("[TBALE_NAME]", table.name).replace("[PROPERTIES]",properties).replace("[TABLE_FILE]",table.file).replace("[TABLE_NAME]",table.name)  

if __name__=="__main__":
    s = Sheet(f"../econfigs/1_StatData.xlsx")
    for t in s.tables:
        print(gen_cs(t))
