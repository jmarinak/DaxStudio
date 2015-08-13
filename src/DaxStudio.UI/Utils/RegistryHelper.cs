﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
//using System.Windows.Forms;
using Microsoft.Win32;
//using ComboBox = System.Windows.Forms.ComboBox;
using System;
using DaxStudio.UI.Model;
using System.Threading.Tasks;

namespace DaxStudio.UI
{
    public static class RegistryHelper
    {
    
        private const string registryRootKey = "SOFTWARE\\DaxStudio";
        private const string REGISTRY_LAST_VERSION_CHECK_SETTING_NAME = "LastVersionCheckUTC";
        private const string REGISTRY_DISMISSED_VERSION_SETTING_NAME = "DismissedVersion";
        private const int MAX_MRU_SIZE = 25;

        public static ObservableCollection<string> GetServerMRUListFromRegistry()
        {
            return GetMRUListFromRegistry("Server");
        }

        public static void SaveServerMRUListToRegistry(string currentServer, ObservableCollection<string> servers)
        {
            SaveListToRegistry("Server", currentServer, servers);
        }

        public static ObservableCollection<DaxFile> GetFileMRUListFromRegistry()
        {
            var lst = new ObservableCollection<DaxFile>();
            foreach (var itm in  GetMRUListFromRegistry("File"))
            {
                lst.Add(new DaxFile(itm));
            }
            return lst;
        }

        public static void SaveFileMRUListToRegistry( IEnumerable<object> files)
        {
            SaveListToRegistry("File","", files);
        }

        public static T GetValue<T>(string subKey, T defaultValue )
        {
            var regDaxStudio = Registry.CurrentUser.OpenSubKey(registryRootKey);
            
            return (T)Convert.ChangeType(regDaxStudio.GetValue(subKey, defaultValue), typeof(T) );
        }

        public static T GetValue<T>(string subKey)
        {
            var regDaxStudio = Registry.CurrentUser.OpenSubKey(registryRootKey);
            return (T)regDaxStudio.GetValue(subKey);
        }

        public static Task SetValueAsync<T>(string subKey, T value)
        {
            return Task.Factory.StartNew(()=>{
                var regDaxStudio = Registry.CurrentUser.OpenSubKey(registryRootKey, true);
                
                regDaxStudio.SetValue(subKey, value);
            });
        }

        internal static ObservableCollection<string> GetMRUListFromRegistry(string listName)
        {
            var list = new ObservableCollection<string>();
            var regDaxStudio = Registry.CurrentUser.OpenSubKey(registryRootKey);
            if (regDaxStudio != null)
            {
                var regListMRU = regDaxStudio.OpenSubKey(string.Format("{0}MRU",listName));
                if (regListMRU != null)
                    foreach (var svr in regListMRU.GetValueNames())
                    {
                        var itmName = regListMRU.GetValue(svr).ToString();
                        list.Add(itmName);
                    }
            }

            return list;
        }



        internal static void SaveListToRegistry(string listName, object currentItem, IEnumerable<object>itemList)
        {
            var listKey = string.Format("{0}MRU", listName);
            var regDaxStudio = Registry.CurrentUser.OpenSubKey(registryRootKey, RegistryKeyPermissionCheck.ReadWriteSubTree);
            if (regDaxStudio == null)
                Registry.CurrentUser.CreateSubKey(registryRootKey);
            if (regDaxStudio != null)
            {
                // clear existing data
                regDaxStudio.DeleteSubKeyTree(listKey, false);
                var regListMRU = regDaxStudio.CreateSubKey(listKey, RegistryKeyPermissionCheck.ReadWriteSubTree);
                if (regListMRU != null)
                {
                    int i = 1;
                    // set current item as item 1
                    if (currentItem.ToString().Length > 0)
                    {
                        regListMRU.SetValue(string.Format("{0}{1}", listName, i), currentItem);
                        i++;
                    }
                    foreach (object listItem in itemList)
                    {
                        if (i > MAX_MRU_SIZE) break; // don't save more than the max mru size
                        var str = listItem as string;
                        if (str == null) str = listItem.ToString();
                        if (string.Equals(str, currentItem.ToString(),StringComparison.CurrentCultureIgnoreCase)) continue;
                        regListMRU.SetValue(string.Format("{0}{1}",listName, i), str );
                        i++;
                    }
                }
            }
        }


        public static void SetLastVersionCheck(DateTime value)
        {
            string path = registryRootKey;
            RegistryKey settingKey = Registry.CurrentUser.OpenSubKey(path, true);
            if (settingKey == null) settingKey = Registry.CurrentUser.CreateSubKey(path);
            using (settingKey)
            {
                var strDate = value.ToUniversalTime().ToString("s", System.Globalization.CultureInfo.InvariantCulture);
                settingKey.SetValue(REGISTRY_LAST_VERSION_CHECK_SETTING_NAME, strDate, RegistryValueKind.String);
            }
        }

        public static DateTime GetLastVersionCheck()
        {
            DateTime dtReturnVal = DateTime.MinValue;
            RegistryKey rk = Registry.CurrentUser.OpenSubKey(registryRootKey);
            if (rk != null)
            {
                using (rk)
                {
                    DateTime.TryParse((string)rk.GetValue(REGISTRY_LAST_VERSION_CHECK_SETTING_NAME, DateTime.MinValue.ToShortDateString()), out dtReturnVal);
                }
            }

            return dtReturnVal.ToLocalTime();
        }


        public static string GetDismissedVersion()
        {
            string sReturnVal = string.Empty;
            RegistryKey rk = Registry.CurrentUser.OpenSubKey(registryRootKey);
            if (rk != null)
            {
                sReturnVal = (string)rk.GetValue(REGISTRY_DISMISSED_VERSION_SETTING_NAME, string.Empty);
                rk.Close();
            }

            return string.IsNullOrEmpty(sReturnVal)?"0.0.0.0":sReturnVal;
        }

        public static void SetDismissedVersion(string value)
        {
            string path = registryRootKey;
            RegistryKey settingKey = Registry.CurrentUser.OpenSubKey(path, true);
            if (settingKey == null) settingKey = Registry.CurrentUser.CreateSubKey(path);
            settingKey.SetValue(REGISTRY_DISMISSED_VERSION_SETTING_NAME, value, RegistryValueKind.String);
            settingKey.Close();
        }

    }
}
