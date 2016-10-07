using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml;
using System.Text.RegularExpressions;

namespace SindaSoft.ActiveRecordInspector
{
    public class Inspector 
    {
        public Dictionary<string, ARSingleClassInfo> arTypes = new Dictionary<string, ARSingleClassInfo>();
        public string file2inspect = null;
        public string directory2inspect = null;

        public event EventHandler OnProgress = null;
        public int currentTypeInspected = 0;
        public int maxTypeInspected = 0;

        public string error_log = "";

        public int percentage_progress = 0;
        public string percentage_message = "";

        /// <summary>
        /// Default constructor
        /// </summary>
        public Inspector()
        {
            file2inspect = null;
            error_log = "";
            directory2inspect = Directory.GetCurrentDirectory();
        }

        /// <summary>
        /// Constructor with initial path or file to be inspected
        /// </summary>
        /// <param name="path">Initial path or file to be inspected</param>
        public Inspector(string path_or_file)
        {
            file2inspect = null;
            error_log = "";
            directory2inspect = Directory.GetCurrentDirectory();

            if(Directory.Exists(path_or_file))
            {
                directory2inspect =  path_or_file;
            }
            else if (File.Exists(path_or_file))
            {
                file2inspect = path_or_file;
            }
        }

        public void InitInspector()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += new ResolveEventHandler(currentDomain_AssemblyResolve);

            try
            {
                List<string> files;

                if (file2inspect != null)
                    files = new List<string>() { file2inspect };
                else
                    files = new List<string>(Directory.GetFiles(directory2inspect, "*.dll"));

                // Initialize ActiveRecord
                string asm = String.Join(",", files.Select(x => Path.GetFileNameWithoutExtension(x)).ToArray());
                //InitHelper.InitializeActiveRecord(false, asm);

                //------------------------------------------------------------------
                // Load libs...
                
                currentTypeInspected = 0;

                List<Type> allTypes = new List<Type> (); 
                foreach (string an in files)
                {
                    try
                    {
                        Assembly a = Assembly.LoadFile(an);
                        List<Type> tt = new List<Type>(a.GetTypes());
                        allTypes.AddRange(tt);
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        error_log += "Error loading: " + Path.GetFileName(an) + "\n";
                        foreach(Exception e in ex.LoaderExceptions)
                            error_log += "\t" + e.Message + "\n";
                    }
                    catch (Exception ex)
                    {
                        error_log += "Error loading: " + Path.GetFileName(an) + "\n";
                        error_log += "\t" + ex.Message + "\n";
                    }

                    currentTypeInspected++;
                    percentage_progress = 100 * currentTypeInspected / files.Count;
                    percentage_message = String.Format("Pass 1 of 2. Inspecting {0} %", percentage_progress);

                    if (OnProgress != null)
                        OnProgress(this, EventArgs.Empty);

                }

                maxTypeInspected = allTypes.Count;
                currentTypeInspected = 0;

                foreach (string an in files)
                {
                        Assembly a = Assembly.LoadFile(an);
                        Type[] tt = a.GetTypes();
                        foreach (Type t in tt)
                        {
                            //if (t.IsSubclassOf(typeof(Castle.ActiveRecord.ActiveRecordBase)))
                            {
                                try
                                {
                                    object[] attrs = t.GetCustomAttributes(typeof(Castle.ActiveRecord.ActiveRecordAttribute), false);
                                    if (attrs != null && attrs.Length == 1)
                                    {
                                        ARSingleClassInfo info = new ARSingleClassInfo();
                                        Castle.ActiveRecord.ActiveRecordAttribute ara = attrs[0] as Castle.ActiveRecord.ActiveRecordAttribute;

                                        info.filename = an;
                                        info.type = t;
                                        info.table_name = String.IsNullOrEmpty(ara.Table) ? t.Name : ara.Table;
                                        info.derived = findAllDerivedTypes(allTypes, t);
                                        info.DiscriminatorColumn = ara.DiscriminatorColumn;
                                        info.DiscriminatorValue = ara.DiscriminatorValue;

                                        info.derived.Remove(typeof(Castle.ActiveRecord.ActiveRecordBase));
                                        info.derived.Remove(typeof(Castle.ActiveRecord.ActiveRecordBase<>));
                                        info.derived.Remove(typeof(Castle.ActiveRecord.ActiveRecordHooksBase));
                                        info.derived.Remove(typeof(Castle.ActiveRecord.ActiveRecordValidationBase));
                                        info.derived.Remove(typeof(Castle.ActiveRecord.ActiveRecordValidationBase<>));

                                        PropertyInfo[] pis = t.GetProperties();
                                        InspectClass(pis, ref info, "", "", an);
                                        arTypes[t.Name.Replace("`1", "<>")] = info;
                                    }
                                }

                                catch (ReflectionTypeLoadException ex)
                                {
                                    error_log += "Error loading class " + t.Name + " from " + Path.GetFileName(an) + "\n";
                                    foreach (Exception e in ex.LoaderExceptions)
                                        error_log += "\t" + e.Message + "\n";
                                }
                                catch (Exception ex)
                                {
                                    error_log += "Error loading: " + Path.GetFileName(an) + "\n";
                                    error_log += "\t" + ex.Message + "\n";
                                }
                            }
                            currentTypeInspected++;

                            percentage_progress = 100 * currentTypeInspected / maxTypeInspected;
                            percentage_message = String.Format("Pass 2 of 2. Inspecting {0} % ({1})", percentage_progress, t.Name);

                            if (percentage_progress % 5 == 0 && OnProgress != null)
                                OnProgress(this, EventArgs.Empty);

                        }
                }
                System.Diagnostics.Debug.WriteLine(arTypes.Count + " possible ActiveRecord classes located in " + files.Count + " modules ("+this.directory2inspect+")");
            }
            catch (Exception ex)
            { 
            }

            currentDomain.AssemblyResolve -= new ResolveEventHandler(currentDomain_AssemblyResolve);
        }

        Assembly currentDomain_AssemblyResolve(object sender, ResolveEventArgs e)
        {
            string assemblyPath;
            if (file2inspect != null)
                assemblyPath = Path.Combine(Path.GetDirectoryName(file2inspect), new AssemblyName(e.Name).Name + ".dll");
            else
                assemblyPath = Path.Combine(directory2inspect, new AssemblyName(e.Name).Name + ".dll");
            
            if (!File.Exists(assemblyPath)) 
                return null;
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            return assembly;
        }

        private void InspectClass(PropertyInfo[] pis, ref ARSingleClassInfo info, string prop_prefix, string col_prefix, string an)
        {
                foreach (PropertyInfo pi in pis)
                {
                    object[] attrs1 = pi.GetCustomAttributes(typeof(Castle.ActiveRecord.PrimaryKeyAttribute), false);
                    object[] attrs2 = pi.GetCustomAttributes(typeof(Castle.ActiveRecord.PropertyAttribute), false);
                    object[] attrs3 = pi.GetCustomAttributes(typeof(Castle.ActiveRecord.BelongsToAttribute), false);
                    object[] attrs4 = pi.GetCustomAttributes(typeof(Castle.ActiveRecord.NestedAttribute), false);
                    object[] attrs5 = pi.GetCustomAttributes(typeof(Castle.ActiveRecord.HasManyAttribute), false);

                    string name = prop_prefix + pi.Name;

                    if (attrs1 != null && attrs1.Length == 1)
                    {
                        Castle.ActiveRecord.PrimaryKeyAttribute pas = attrs1[0] as Castle.ActiveRecord.PrimaryKeyAttribute;
                        info.columns[name] = new ARPropertyInfo();
                        info.columns[name].isPrimaryKey = true;
                        info.columns[name].column_name = col_prefix; 
                        info.columns[name].column_name += String.IsNullOrEmpty(pas.Column) ? pi.Name : pas.Column;
                        info.columns[name].property_info = pi;
                        info.columns[name].XmlDoc = GetXmlDoc(an, pi);
                    }
                    else if (attrs2 != null && attrs2.Length == 1)
                    {
                        Castle.ActiveRecord.PropertyAttribute pas = attrs2[0] as Castle.ActiveRecord.PropertyAttribute;
                        info.columns[name] = new ARPropertyInfo();
                        info.columns[name].column_name = col_prefix; 
                        info.columns[name].column_name += String.IsNullOrEmpty(pas.Column) ? pi.Name : pas.Column;
                        info.columns[name].property_info = pi;
                        info.columns[name].XmlDoc = GetXmlDoc(an, pi);
                    }
                    else if (attrs3 != null && attrs3.Length == 1)
                    {
                        Castle.ActiveRecord.BelongsToAttribute pas = attrs3[0] as Castle.ActiveRecord.BelongsToAttribute;
                        info.columns[name] = new ARPropertyInfo();
                        info.columns[name].column_name = col_prefix; 
                        info.columns[name].column_name += String.IsNullOrEmpty(pas.Column) ? pi.Name : pas.Column;
                        info.columns[name].linked_to_table = findTableName(pi);
                        info.columns[name].property_info = pi;
                        info.columns[name].XmlDoc = GetXmlDoc(an, pi);
                    }
                    else if (attrs4 != null && attrs4.Length == 1)
                    {
                        Castle.ActiveRecord.NestedAttribute pas = attrs4[0] as Castle.ActiveRecord.NestedAttribute;
                        string cprefix = pas.ColumnPrefix;
                        string pprefix = name + ".";
                        // Check --> pi.PropertyType
                        InspectClass(pi.PropertyType.GetProperties(), ref info, pprefix,  cprefix, an);
                    }
                    else if (attrs5 != null && attrs5.Length == 1)
                    {
                    }
                }
        }

        private string findTableName(PropertyInfo pi)
        {
            Type t = pi.PropertyType;
            object[] a = t.GetCustomAttributes(typeof(Castle.ActiveRecord.ActiveRecordAttribute), false);
            if (a != null && a.Length > 0)
            {
                return (a[0] as Castle.ActiveRecord.ActiveRecordAttribute).Table;
            }
            else
                return "";
        }

        private List<Type> findAllDerivedTypes(List<Type> types, Type cls)
        {
            //return types.Where(t => t != cls && cls.IsSubclassOf(t)).ToList();
            return types.Where(t => t != cls && isSubclassOfRawGeneric(t, cls)).ToList();
        }

        private bool isSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }

        private string GetXmlDoc(string fname, PropertyInfo pi)
        {
#if false
            string docuPath = fname.Substring(0, fname.LastIndexOf(".")) + ".xml";
            string retval = "";
            if (File.Exists(docuPath))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(docuPath);

                string path = "P:" + pi.DeclaringType.FullName + "." + pi.Name;
                XmlNode row = doc.SelectSingleNode( "//member[starts-with(@name, '" + path + "')]");
                if(row != null)
                    retval = Regex.Replace(row.InnerXml, @"\s+", " ");
            }
            return retval;
#endif
            return String.Empty;
        }

    }
}
