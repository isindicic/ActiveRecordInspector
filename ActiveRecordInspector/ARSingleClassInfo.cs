using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SindaSoft.ActiveRecordInspector
{
    public class ARSingleClassInfo
    {
        public Type type = null;
        public List<Type> derived = null;

        public string filename = "";
        public string table_name = "";

        public Dictionary<string, ARPropertyInfo> columns = new Dictionary<string, ARPropertyInfo>();

        public string DiscriminatorColumn = null;
        public string DiscriminatorValue = null;
    }

}
