﻿#pragma checksum "..\..\..\..\UserControls\SpotItem.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "0BDD89762A8246B0D0E2733412A51D214E6E54AC"
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
    /// SpotItem
    /// </summary>
    public partial class SpotItem : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 25 "..\..\..\..\UserControls\SpotItem.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label lblCode;
        
        #line default
        #line hidden
        
        
        #line 28 "..\..\..\..\UserControls\SpotItem.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox tbName;
        
        #line default
        #line hidden
        
        
        #line 32 "..\..\..\..\UserControls\SpotItem.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox tbLength;
        
        #line default
        #line hidden
        
        
        #line 38 "..\..\..\..\UserControls\SpotItem.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnDelete;
        
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
            System.Uri resourceLocater = new System.Uri("/CampaignEditor;component/usercontrols/spotitem.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\UserControls\SpotItem.xaml"
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
            this.lblCode = ((System.Windows.Controls.Label)(target));
            return;
            case 2:
            this.tbName = ((System.Windows.Controls.TextBox)(target));
            
            #line 30 "..\..\..\..\UserControls\SpotItem.xaml"
            this.tbName.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.tb_TextChanged);
            
            #line default
            #line hidden
            return;
            case 3:
            this.tbLength = ((System.Windows.Controls.TextBox)(target));
            
            #line 35 "..\..\..\..\UserControls\SpotItem.xaml"
            this.tbLength.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.tb_TextChanged);
            
            #line default
            #line hidden
            
            #line 35 "..\..\..\..\UserControls\SpotItem.xaml"
            this.tbLength.AddHandler(System.Windows.Input.CommandManager.PreviewExecutedEvent, new System.Windows.Input.ExecutedRoutedEventHandler(this.tbPreviewExecuted_PreviewExecuted));
            
            #line default
            #line hidden
            
            #line 36 "..\..\..\..\UserControls\SpotItem.xaml"
            this.tbLength.PreviewTextInput += new System.Windows.Input.TextCompositionEventHandler(this.tbLength_PreviewTextInput);
            
            #line default
            #line hidden
            return;
            case 4:
            this.btnDelete = ((System.Windows.Controls.Button)(target));
            
            #line 40 "..\..\..\..\UserControls\SpotItem.xaml"
            this.btnDelete.Click += new System.Windows.RoutedEventHandler(this.btnDelete_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

