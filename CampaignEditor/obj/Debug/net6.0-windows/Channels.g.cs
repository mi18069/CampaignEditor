﻿#pragma checksum "..\..\..\Channels.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "34F5D11CCFF9B39087313012C49BE5218467AD53"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using CampaignEditor;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace CampaignEditor {
    
    
    /// <summary>
    /// Channels
    /// </summary>
    public partial class Channels : System.Windows.Window, System.Windows.Markup.IComponentConnector, System.Windows.Markup.IStyleConnector {
        
        
        #line 41 "..\..\..\Channels.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListView lvChannels;
        
        #line default
        #line hidden
        
        
        #line 45 "..\..\..\Channels.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox lbChannelGroups;
        
        #line default
        #line hidden
        
        
        #line 49 "..\..\..\Channels.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnEditChannelGroups;
        
        #line default
        #line hidden
        
        
        #line 57 "..\..\..\Channels.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListView lvPricelists;
        
        #line default
        #line hidden
        
        
        #line 61 "..\..\..\Channels.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnEditPricelist;
        
        #line default
        #line hidden
        
        
        #line 67 "..\..\..\Channels.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnNewPricelist;
        
        #line default
        #line hidden
        
        
        #line 75 "..\..\..\Channels.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListView lvActivities;
        
        #line default
        #line hidden
        
        
        #line 81 "..\..\..\Channels.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnToSelected;
        
        #line default
        #line hidden
        
        
        #line 87 "..\..\..\Channels.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnFromSelected;
        
        #line default
        #line hidden
        
        
        #line 95 "..\..\..\Channels.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid dgSelected;
        
        #line default
        #line hidden
        
        
        #line 121 "..\..\..\Channels.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnCancel;
        
        #line default
        #line hidden
        
        
        #line 124 "..\..\..\Channels.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnSave;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "7.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/CampaignEditor;component/channels.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Channels.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "7.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.lvChannels = ((System.Windows.Controls.ListView)(target));
            
            #line 44 "..\..\..\Channels.xaml"
            this.lvChannels.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.lvChannels_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 2:
            this.lbChannelGroups = ((System.Windows.Controls.ListBox)(target));
            
            #line 47 "..\..\..\Channels.xaml"
            this.lbChannelGroups.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.lbChannelGroups_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 3:
            this.btnEditChannelGroups = ((System.Windows.Controls.Button)(target));
            
            #line 51 "..\..\..\Channels.xaml"
            this.btnEditChannelGroups.Click += new System.Windows.RoutedEventHandler(this.btnEditChannelGroups_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.lvPricelists = ((System.Windows.Controls.ListView)(target));
            return;
            case 5:
            this.btnEditPricelist = ((System.Windows.Controls.Button)(target));
            
            #line 64 "..\..\..\Channels.xaml"
            this.btnEditPricelist.Click += new System.Windows.RoutedEventHandler(this.btnEditPricelist_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.btnNewPricelist = ((System.Windows.Controls.Button)(target));
            
            #line 69 "..\..\..\Channels.xaml"
            this.btnNewPricelist.Click += new System.Windows.RoutedEventHandler(this.btnNewPricelist_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.lvActivities = ((System.Windows.Controls.ListView)(target));
            return;
            case 8:
            this.btnToSelected = ((System.Windows.Controls.Button)(target));
            
            #line 84 "..\..\..\Channels.xaml"
            this.btnToSelected.Click += new System.Windows.RoutedEventHandler(this.btnToSelected_Click);
            
            #line default
            #line hidden
            return;
            case 9:
            this.btnFromSelected = ((System.Windows.Controls.Button)(target));
            
            #line 90 "..\..\..\Channels.xaml"
            this.btnFromSelected.Click += new System.Windows.RoutedEventHandler(this.btnFromSelected_Click);
            
            #line default
            #line hidden
            return;
            case 10:
            this.dgSelected = ((System.Windows.Controls.DataGrid)(target));
            return;
            case 12:
            this.btnCancel = ((System.Windows.Controls.Button)(target));
            
            #line 123 "..\..\..\Channels.xaml"
            this.btnCancel.Click += new System.Windows.RoutedEventHandler(this.btnCancel_Click);
            
            #line default
            #line hidden
            return;
            case 13:
            this.btnSave = ((System.Windows.Controls.Button)(target));
            
            #line 126 "..\..\..\Channels.xaml"
            this.btnSave.Click += new System.Windows.RoutedEventHandler(this.btnSave_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "7.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        void System.Windows.Markup.IStyleConnector.Connect(int connectionId, object target) {
            System.Windows.EventSetter eventSetter;
            switch (connectionId)
            {
            case 11:
            eventSetter = new System.Windows.EventSetter();
            eventSetter.Event = System.Windows.Controls.Control.MouseDoubleClickEvent;
            
            #line 108 "..\..\..\Channels.xaml"
            eventSetter.Handler = new System.Windows.Input.MouseButtonEventHandler(this.DataGridRow_MouseDoubleClick);
            
            #line default
            #line hidden
            ((System.Windows.Style)(target)).Setters.Add(eventSetter);
            break;
            }
        }
    }
}

