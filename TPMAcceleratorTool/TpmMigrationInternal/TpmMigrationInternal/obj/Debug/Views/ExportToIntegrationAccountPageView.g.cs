﻿#pragma checksum "..\..\..\Views\ExportToIntegrationAccountPageView.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "5ED58E98C315984C01CC190ED0E2B20CC347D0C3"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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


namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration {
    
    
    /// <summary>
    /// ExportToIntegrationAccountPageView
    /// </summary>
    public partial class ExportToIntegrationAccountPageView : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 17 "..\..\..\Views\ExportToIntegrationAccountPageView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock CertificateMigrationWarning;
        
        #line default
        #line hidden
        
        
        #line 21 "..\..\..\Views\ExportToIntegrationAccountPageView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox SchemaExportCheckBox;
        
        #line default
        #line hidden
        
        
        #line 24 "..\..\..\Views\ExportToIntegrationAccountPageView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox CertExportCheckBox;
        
        #line default
        #line hidden
        
        
        #line 27 "..\..\..\Views\ExportToIntegrationAccountPageView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox PartnerExportCheckBox;
        
        #line default
        #line hidden
        
        
        #line 30 "..\..\..\Views\ExportToIntegrationAccountPageView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox AgmtExportCheckBox;
        
        #line default
        #line hidden
        
        
        #line 35 "..\..\..\Views\ExportToIntegrationAccountPageView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox OverwriteCheckBox;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/TpmMigration;component/views/exporttointegrationaccountpageview.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Views\ExportToIntegrationAccountPageView.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.CertificateMigrationWarning = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 2:
            this.SchemaExportCheckBox = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 3:
            this.CertExportCheckBox = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 4:
            this.PartnerExportCheckBox = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 5:
            this.AgmtExportCheckBox = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 6:
            this.OverwriteCheckBox = ((System.Windows.Controls.CheckBox)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}
