﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using ADOTabular;

namespace DaxStudio.Xmla
{
    class ExcelAmoWrapper
    {

        internal delegate void VoidDelegate();
        internal delegate T ReturnDelegate<T>();

        private static Assembly m_excelAmoAssembly;
        private static string m_excelAmoAssemblyPath;

        //[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        //private static extern uint GetModuleFileName([In] IntPtr hModule, [Out] StringBuilder lpFilename, [In, MarshalAs(UnmanagedType.U4)] int nSize);
        //[DllImport("Kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        //private static extern IntPtr GetModuleHandle(string lpModuleName);

        protected static string RetrieveExcelAmoAssemblyPath()
        {
            string directoryName = ADOTabular.AdomdClientWrappers.ExcelAdoMdConnections.RetrieveAdomdAssemblyFolder();
            Log.Debug("{Class} {Method} dir: {directoryName}", "ExcelAmoWrapper", "RetrieveExcelAmoAssemblyPath", directoryName);
            return Path.Combine(directoryName, "Microsoft.Excel.Amo.dll");
        }

        public static Assembly ExcelAmoAssembly
        {
            get
            {
                try
                {
                    if (m_excelAmoAssembly == null)
                    {
//                        try
//                        {
                            m_excelAmoAssembly = Assembly.LoadFrom(ExcelAdomdClientAssemblyPath);
//                        }
//                        catch (Exception ex)
//                        {    
//                            Log.Verbose("{Class} {Method} {message}", "ExcelAmoWrapper", "ExcelAmoAssembly", "Attempting hack of loading Microsoft.AnalysisServices");
//                            //TODO - Hack to try to get Excel 2010 working 
//                            m_excelAmoAssembly = Assembly.LoadFrom(AssemblyDirectory + "\\Microsoft.Excel.Amo.dll");
//                        }
                    }
                    return m_excelAmoAssembly;
                }
                catch (Exception e)
                {
                    throw new Exception(string.Format("Error loading AMO from '{0}'", ExcelAdomdClientAssemblyPath));
                }

            }
        }

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        protected static string ExcelAdomdClientAssemblyPath
        {
            get
            {
                if (m_excelAmoAssemblyPath == null)
                {
                    m_excelAmoAssemblyPath = RetrieveExcelAmoAssemblyPath();
                    Log.Debug("{class} {method} AssemblyPath: {path}", "ExcelAmoWrapper", "ExcelAdomdClientAssemblyPath", m_excelAmoAssemblyPath);
                }
                return m_excelAmoAssemblyPath;
            }
        }
    }
}
