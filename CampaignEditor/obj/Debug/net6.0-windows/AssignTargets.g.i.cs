// Updated by XamlIntelliSenseFileGenerator 2/6/2023 5:48:03 PM
#pragma checksum "..\..\..\AssignTargets.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "55F068766C2458F0BBECD3B0A1F22B228430A9BE"
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


namespace CampaignEditor
{


    /// <summary>
    /// AssignTargets
    /// </summary>
    public partial class AssignTargets : System.Windows.Window, System.Windows.Markup.IComponentConnector
    {


#line 13 "..\..\..\AssignTargets.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox lbTargets;

#line default
#line hidden


#line 16 "..\..\..\AssignTargets.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnToSelected;

#line default
#line hidden


#line 21 "..\..\..\AssignTargets.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnFromSelected;

#line default
#line hidden


#line 26 "..\..\..\AssignTargets.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnNewTarget;

#line default
#line hidden


#line 34 "..\..\..\AssignTargets.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox lbSelectedTargets;

#line default
#line hidden


#line 40 "..\..\..\AssignTargets.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock tbTargetFilters;

#line default
#line hidden


#line 42 "..\..\..\AssignTargets.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnSave;

#line default
#line hidden


#line 45 "..\..\..\AssignTargets.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnCancel;

#line default
#line hidden

        private bool _contentLoaded;

        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "7.0.0.0")]
        public void InitializeComponent()
        {
            if (_contentLoaded)
            {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/CampaignEditor;V1.0.0.0;component/assigntargets.xaml", System.UriKind.Relative);

#line 1 "..\..\..\AssignTargets.xaml"
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
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this.lbTargets = ((System.Windows.Controls.ListBox)(target));
                    return;
                case 2:
                    this.btnToSelected = ((System.Windows.Controls.Button)(target));
                    return;
                case 3:
                    this.btnFromSelected = ((System.Windows.Controls.Button)(target));
                    return;
                case 4:
                    this.btnNewTarget = ((System.Windows.Controls.Button)(target));
                    return;
                case 5:
                    this.lbSelectedTargets = ((System.Windows.Controls.ListBox)(target));
                    return;
                case 6:
                    this.tbTargetFilters = ((System.Windows.Controls.TextBlock)(target));
                    return;
                case 7:
                    this.btnSave = ((System.Windows.Controls.Button)(target));
                    return;
                case 8:
                    this.btnCancel = ((System.Windows.Controls.Button)(target));
                    return;
            }
            this._contentLoaded = true;
        }

        internal System.Windows.Controls.Button btnEditTarget;
    }
}

