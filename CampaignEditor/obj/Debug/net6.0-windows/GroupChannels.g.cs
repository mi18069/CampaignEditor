﻿#pragma checksum "..\..\..\GroupChannels.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "A55B35939BB6C75182C4ED794229B82D2CE2B6FC"
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
    /// GroupChannels
    /// </summary>
    public partial class GroupChannels : System.Windows.Window, System.Windows.Markup.IComponentConnector, System.Windows.Markup.IStyleConnector {
        
        
        #line 37 "..\..\..\GroupChannels.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListView lvChannels;
        
        #line default
        #line hidden
        
        
        #line 44 "..\..\..\GroupChannels.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListView lvChannelGroups;
        
        #line default
        #line hidden
        
        
        #line 49 "..\..\..\GroupChannels.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox tbNewGroup;
        
        #line default
        #line hidden
        
        
        #line 54 "..\..\..\GroupChannels.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnNewChhGr;
        
        #line default
        #line hidden
        
        
        #line 60 "..\..\..\GroupChannels.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnDeleteChhGr;
        
        #line default
        #line hidden
        
        
        #line 67 "..\..\..\GroupChannels.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnToAssigned;
        
        #line default
        #line hidden
        
        
        #line 73 "..\..\..\GroupChannels.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnFromAssigned;
        
        #line default
        #line hidden
        
        
        #line 81 "..\..\..\GroupChannels.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid dgAssigned;
        
        #line default
        #line hidden
        
        
        #line 105 "..\..\..\GroupChannels.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnCancel;
        
        #line default
        #line hidden
        
        
        #line 108 "..\..\..\GroupChannels.xaml"
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
            System.Uri resourceLocater = new System.Uri("/CampaignEditor;component/groupchannels.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\GroupChannels.xaml"
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
            return;
            case 2:
            this.lvChannelGroups = ((System.Windows.Controls.ListView)(target));
            
            #line 48 "..\..\..\GroupChannels.xaml"
            this.lvChannelGroups.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.lvChannelGroups_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 3:
            this.tbNewGroup = ((System.Windows.Controls.TextBox)(target));
            
            #line 52 "..\..\..\GroupChannels.xaml"
            this.tbNewGroup.GotFocus += new System.Windows.RoutedEventHandler(this.tbNewGroup_GotFocus);
            
            #line default
            #line hidden
            
            #line 53 "..\..\..\GroupChannels.xaml"
            this.tbNewGroup.LostFocus += new System.Windows.RoutedEventHandler(this.tbNewGroup_LostFocus);
            
            #line default
            #line hidden
            return;
            case 4:
            this.btnNewChhGr = ((System.Windows.Controls.Button)(target));
            
            #line 57 "..\..\..\GroupChannels.xaml"
            this.btnNewChhGr.Click += new System.Windows.RoutedEventHandler(this.btnNewChhGr_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.btnDeleteChhGr = ((System.Windows.Controls.Button)(target));
            
            #line 63 "..\..\..\GroupChannels.xaml"
            this.btnDeleteChhGr.Click += new System.Windows.RoutedEventHandler(this.btnDeleteChhGr_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.btnToAssigned = ((System.Windows.Controls.Button)(target));
            
            #line 70 "..\..\..\GroupChannels.xaml"
            this.btnToAssigned.Click += new System.Windows.RoutedEventHandler(this.btnToAssigned_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.btnFromAssigned = ((System.Windows.Controls.Button)(target));
            
            #line 76 "..\..\..\GroupChannels.xaml"
            this.btnFromAssigned.Click += new System.Windows.RoutedEventHandler(this.btnFromAssigned_Click);
            
            #line default
            #line hidden
            return;
            case 8:
            this.dgAssigned = ((System.Windows.Controls.DataGrid)(target));
            return;
            case 10:
            this.btnCancel = ((System.Windows.Controls.Button)(target));
            
            #line 107 "..\..\..\GroupChannels.xaml"
            this.btnCancel.Click += new System.Windows.RoutedEventHandler(this.btnCancel_Click);
            
            #line default
            #line hidden
            return;
            case 11:
            this.btnSave = ((System.Windows.Controls.Button)(target));
            
            #line 110 "..\..\..\GroupChannels.xaml"
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
            case 9:
            eventSetter = new System.Windows.EventSetter();
            eventSetter.Event = System.Windows.Controls.Control.MouseDoubleClickEvent;
            
            #line 94 "..\..\..\GroupChannels.xaml"
            eventSetter.Handler = new System.Windows.Input.MouseButtonEventHandler(this.DataGridRow_MouseDoubleClick);
            
            #line default
            #line hidden
            ((System.Windows.Style)(target)).Setters.Add(eventSetter);
            break;
            }
        }
    }
}

