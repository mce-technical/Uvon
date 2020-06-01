﻿#pragma checksum "..\..\..\ControlPanel.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "7E64F9B4DD71C4386F4482FD4D00417D0F895EA9"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

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
using Uvon_Desktop;


namespace Uvon_Desktop {
    
    
    /// <summary>
    /// ControlPanel
    /// </summary>
    public partial class ControlPanel : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 17 "..\..\..\ControlPanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button info;
        
        #line default
        #line hidden
        
        
        #line 18 "..\..\..\ControlPanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image info_image;
        
        #line default
        #line hidden
        
        
        #line 22 "..\..\..\ControlPanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Shapes.Ellipse motor_drivers;
        
        #line default
        #line hidden
        
        
        #line 23 "..\..\..\ControlPanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Shapes.Ellipse uv_light_1;
        
        #line default
        #line hidden
        
        
        #line 24 "..\..\..\ControlPanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Shapes.Ellipse uv_light_2;
        
        #line default
        #line hidden
        
        
        #line 25 "..\..\..\ControlPanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Shapes.Ellipse autopilot;
        
        #line default
        #line hidden
        
        
        #line 26 "..\..\..\ControlPanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Shapes.Ellipse emergency_status;
        
        #line default
        #line hidden
        
        
        #line 30 "..\..\..\ControlPanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image imagesource;
        
        #line default
        #line hidden
        
        
        #line 41 "..\..\..\ControlPanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Enable;
        
        #line default
        #line hidden
        
        
        #line 42 "..\..\..\ControlPanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button uv1;
        
        #line default
        #line hidden
        
        
        #line 43 "..\..\..\ControlPanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button uv2;
        
        #line default
        #line hidden
        
        
        #line 44 "..\..\..\ControlPanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button disconnect;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.8.1.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/Uvon_Desktop;component/controlpanel.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\ControlPanel.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.8.1.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 8 "..\..\..\ControlPanel.xaml"
            ((Uvon_Desktop.ControlPanel)(target)).KeyDown += new System.Windows.Input.KeyEventHandler(this.Window_KeyDown);
            
            #line default
            #line hidden
            return;
            case 2:
            this.info = ((System.Windows.Controls.Button)(target));
            
            #line 17 "..\..\..\ControlPanel.xaml"
            this.info.Click += new System.Windows.RoutedEventHandler(this.Info_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.info_image = ((System.Windows.Controls.Image)(target));
            return;
            case 4:
            this.motor_drivers = ((System.Windows.Shapes.Ellipse)(target));
            return;
            case 5:
            this.uv_light_1 = ((System.Windows.Shapes.Ellipse)(target));
            return;
            case 6:
            this.uv_light_2 = ((System.Windows.Shapes.Ellipse)(target));
            return;
            case 7:
            this.autopilot = ((System.Windows.Shapes.Ellipse)(target));
            return;
            case 8:
            this.emergency_status = ((System.Windows.Shapes.Ellipse)(target));
            return;
            case 9:
            this.imagesource = ((System.Windows.Controls.Image)(target));
            return;
            case 10:
            this.Enable = ((System.Windows.Controls.Button)(target));
            
            #line 41 "..\..\..\ControlPanel.xaml"
            this.Enable.Click += new System.Windows.RoutedEventHandler(this.On_Off_Click);
            
            #line default
            #line hidden
            return;
            case 11:
            this.uv1 = ((System.Windows.Controls.Button)(target));
            
            #line 42 "..\..\..\ControlPanel.xaml"
            this.uv1.Click += new System.Windows.RoutedEventHandler(this.uv1_Click);
            
            #line default
            #line hidden
            return;
            case 12:
            this.uv2 = ((System.Windows.Controls.Button)(target));
            
            #line 43 "..\..\..\ControlPanel.xaml"
            this.uv2.Click += new System.Windows.RoutedEventHandler(this.uv2_Click);
            
            #line default
            #line hidden
            return;
            case 13:
            this.disconnect = ((System.Windows.Controls.Button)(target));
            
            #line 44 "..\..\..\ControlPanel.xaml"
            this.disconnect.Click += new System.Windows.RoutedEventHandler(this.disconnect_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

