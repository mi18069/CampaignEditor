﻿#pragma checksum "..\..\..\CampaignForecast.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "3204F5174DBC12F970C4D8849564EA664AC58199"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using CampaignEditor.UserControls;
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


namespace CampaignEditor.UserControls {
    
    
    /// <summary>
    /// CampaignForecast
    /// </summary>
    public partial class CampaignForecast : System.Windows.Controls.Page, System.Windows.Markup.IComponentConnector {
        
        
        #line 12 "..\..\..\CampaignForecast.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid gridInit;
        
        #line default
        #line hidden
        
        
        #line 37 "..\..\..\CampaignForecast.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DatePicker dpFrom;
        
        #line default
        #line hidden
        
        
        #line 54 "..\..\..\CampaignForecast.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DatePicker dpTo;
        
        #line default
        #line hidden
        
        
        #line 61 "..\..\..\CampaignForecast.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Init;
        
        #line default
        #line hidden
        
        
        #line 66 "..\..\..\CampaignForecast.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid gridForecast;
        
        #line default
        #line hidden
        
        
        #line 83 "..\..\..\CampaignForecast.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListView lvChannels;
        
        #line default
        #line hidden
        
        
        #line 89 "..\..\..\CampaignForecast.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid dgSchema;
        
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
            System.Uri resourceLocater = new System.Uri("/CampaignEditor;component/campaignforecast.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\CampaignForecast.xaml"
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
            this.gridInit = ((System.Windows.Controls.Grid)(target));
            return;
            case 2:
            this.dpFrom = ((System.Windows.Controls.DatePicker)(target));
            return;
            case 3:
            this.dpTo = ((System.Windows.Controls.DatePicker)(target));
            return;
            case 4:
            this.Init = ((System.Windows.Controls.Button)(target));
            
            #line 64 "..\..\..\CampaignForecast.xaml"
            this.Init.Click += new System.Windows.RoutedEventHandler(this.Init_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.gridForecast = ((System.Windows.Controls.Grid)(target));
            return;
            case 6:
            this.lvChannels = ((System.Windows.Controls.ListView)(target));
            
            #line 86 "..\..\..\CampaignForecast.xaml"
            this.lvChannels.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.lvChannels_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 7:
            this.dgSchema = ((System.Windows.Controls.DataGrid)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

