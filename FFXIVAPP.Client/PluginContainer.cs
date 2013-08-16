// FFXIVAPP.Client
// PluginContainer.cs
// 
// � 2013 Ryan Wilson

#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using FFXIVAPP.Client.Helpers;
using FFXIVAPP.Client.Models;
using FFXIVAPP.Client.Properties;
using FFXIVAPP.Common.Models;
using FFXIVAPP.Common.Utilities;
using FFXIVAPP.IPluginInterface;
using NLog;

#endregion

namespace FFXIVAPP.Client
{
    internal class PluginContainer : IPluginHost
    {
        #region Property Bindings

        private PluginCollectionHelper _loaded;

        public PluginCollectionHelper Loaded
        {
            get { return _loaded ?? (_loaded = new PluginCollectionHelper()); }
        }

        #endregion

        #region Declarations

        #endregion

        /// <summary>
        /// </summary>
        /// <param name="path"> </param>
        public void LoadPlugins(string path = "")
        {
            path = (path == "") ? AppDomain.CurrentDomain.BaseDirectory : path;
            Loaded.Clear();
            if (!Directory.Exists(path))
            {
                return;
            }
            var directories = Directory.GetDirectories(path);
            foreach (var d in directories)
            {
                var settings = String.Format(@"{0}\PluginInfo.xml", d);
                if (!File.Exists(settings))
                {
                    continue;
                }
                var xDoc = XDocument.Load(settings);
                foreach (var xElement in xDoc.Descendants()
                    .Elements("Main"))
                {
                    var xKey = (string) xElement.Attribute("Key");
                    var xValue = (string) xElement.Element("Value");
                    if (String.IsNullOrWhiteSpace(xKey) || String.IsNullOrWhiteSpace(xValue))
                    {
                        return;
                    }
                    switch (xKey)
                    {
                        case "FileName":
                            VerifyPlugin(String.Format(@"{0}\{1}", d, xValue));
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// </summary>
        public void UnloadPlugins()
        {
            foreach (PluginInstance pInstance in Loaded)
            {
                if (pInstance.Instance != null)
                {
                    pInstance.Instance.Dispose();
                }
                pInstance.Instance = null;
            }
            Loaded.Clear();
        }

        /// <summary>
        /// </summary>
        /// <param name="fileName"> </param>
        private void VerifyPlugin(string fileName)
        {
            try
            {
                Logging.Log(LogManager.GetCurrentClassLogger(), String.Format("PluginFileName:{0}", fileName));
                var pAssembly = Assembly.LoadFile(fileName);
                var pType = pAssembly.GetType(pAssembly.GetName()
                    .Name + ".Plugin");
                var implementsIPlugin = typeof (IPlugin).IsAssignableFrom(pType);
                if (!implementsIPlugin)
                {
                    Logging.Log(LogManager.GetCurrentClassLogger(), "*IPlugin Not Implemented*");
                    return;
                }
                var plugin = new PluginInstance();
                plugin.Instance = (IPlugin) Activator.CreateInstance(pType);
                plugin.AssemblyPath = fileName;
                plugin.Instance.Host = this;
                plugin.Instance.Initialize();
                Logging.Log(LogManager.GetCurrentClassLogger(), String.Format("Added:{0}", plugin.Instance.Name));
                Loaded.Add(plugin);
            }
            catch (Exception ex)
            {
                Logging.Log(LogManager.GetCurrentClassLogger(), "", ex);
            }
        }

        #region Implementaion of IPluginHost

        /// <summary>
        /// </summary>
        /// <param name="pluginName"> </param>
        /// <param name="commands"> </param>
        public void Commands(string pluginName, IEnumerable<string> commands)
        {
            var pluginInstance = Loaded.Find(pluginName);
            if (pluginInstance == null)
            {
                return;
            }
            if (!Settings.Default.AllowPluginCommands)
            {
                var enumerable = commands as List<string> ?? commands.ToList();
                var commandlist = enumerable.Aggregate("", (current, s) => current + (s + ","));
                Logging.Log(LogManager.GetCurrentClassLogger(), String.Format("PluginCommandAborted: {0}: \n{1}", pluginName, commandlist.Substring(0, commandlist.Length - 1)));
                return;
            }
            // return for now as all commands are disabled
            return;
            foreach (var command in commands)
            {
                var ascii = Encoding.GetEncoding("utf-16");
                KeyBoardHelper.SendNotify(ascii.GetBytes(command));
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="pluginName"></param>
        /// <param name="displayed"> </param>
        /// <param name="content"> </param>
        public void PopupMessage(string pluginName, out bool displayed, object content)
        {
            var popupContent = content as PopupContent;
            var pluginInstance = Loaded.Find(popupContent.PluginName);
            if (pluginInstance == null || ShellView.View.Notify.IsOpen)
            {
                displayed = false;
                return;
            }
            PopupHelper.Toggle(content);
            displayed = true;
            EventHandler onClosed = null;
            onClosed = delegate
            {
                pluginInstance.Instance.PopupResult = PopupHelper.Result;
                PopupHelper.MessagePopup.Closed -= onClosed;
            };
            PopupHelper.MessagePopup.Closed += onClosed;
        }

        public void GetConstants(string pluginName)
        {
            var pluginInstance = Loaded.Find(pluginName);
            if (pluginInstance == null)
            {
                return;
            }
            pluginInstance.Instance.SetConstants(ConstantsType.AutoTranslate, Constants.AutoTranslate);
            pluginInstance.Instance.SetConstants(ConstantsType.ChatCodes, Constants.ChatCodes);
            pluginInstance.Instance.SetConstants(ConstantsType.ChatCodesXml, Constants.ChatCodesXml);
            pluginInstance.Instance.SetConstants(ConstantsType.Colors, Constants.Colors);
            pluginInstance.Instance.SetConstants(ConstantsType.CultureInfo, Constants.CultureInfo);
            pluginInstance.Instance.SetConstants(ConstantsType.CharacterName, Constants.CharacterName);
            pluginInstance.Instance.SetConstants(ConstantsType.ServerName, Constants.ServerName);
            pluginInstance.Instance.SetConstants(ConstantsType.GameLanguage, Constants.GameLanguage);
            //throw new NotImplementedException();
        }

        public void GetPlayerInfo(string pluginName)
        {
            var pluginInstance = Loaded.Find(pluginName);
            if (pluginInstance == null)
            {
                return;
            }
            //throw new NotImplementedException();
        }

        public void GetMapInfo(string pluginName)
        {
            var pluginInstance = Loaded.Find(pluginName);
            if (pluginInstance == null)
            {
                return;
            }
            //throw new NotImplementedException();
        }

        public void GetMonsterInfo(string pluginName)
        {
            var pluginInstance = Loaded.Find(pluginName);
            if (pluginInstance == null)
            {
                return;
            }
            //throw new NotImplementedException();
        }

        #endregion
    }
}