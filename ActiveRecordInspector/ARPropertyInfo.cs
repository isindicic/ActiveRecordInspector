using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SindaSoft.ActiveRecordInspector
{
    public class ARPropertyInfo
    {
        public PropertyInfo property_info;
        public bool isPrimaryKey = false;
        public string column_name = "";
        public string linked_to_table = "";
        public string XmlDoc = "";
    }

}
