﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MKSlideShop {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.3.0.0")]
    internal sealed partial class ShowSettings : global::System.Configuration.ApplicationSettingsBase {
        
        private static ShowSettings defaultInstance = ((ShowSettings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new ShowSettings())));
        
        public static ShowSettings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.Collections.Specialized.StringCollection LastPaths {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["LastPaths"]));
            }
            set {
                this["LastPaths"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("10")]
        public int ShowTime {
            get {
                return ((int)(this["ShowTime"]));
            }
            set {
                this["ShowTime"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string BrowserPath {
            get {
                return ((string)(this["BrowserPath"]));
            }
            set {
                this["BrowserPath"] = value;
            }
        }
    }
}
