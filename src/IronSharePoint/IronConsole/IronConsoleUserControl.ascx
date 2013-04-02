<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Assembly Name="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="IronConsoleUserControl.ascx.cs" Inherits="IronSharePoint.IronConsoleUserControl" %>
<style type="text/css">
    #ironSP-console-input {
        -moz-box-sizing: border-box;
        -webkit-box-sizing: border-box;
        border-width: 0px;
        box-sizing: border-box;
        float: left;
        overflow: auto;
        width: 90%;
    }

    #ironSP-console-container {
        max-height: 500px;
        overflow: auto;
        padding: 5px;
    }

    #ironSP-console {
    }

    .ironSP-console-edit {
        background-color: Yellow !important;
    }

    .ironSP-console-line td, .ironSP-console-prompt td, #ironSP-console-input {
        color: black !important;
        font-family: "Droid Sans Mono", Consolas, Courier !important;
        font-size: 12pt;
    }

    .ironSP-console-prefix {
        padding-right: 5px;
    }

    .ironSP-console-prefix {
        padding-top: 2px;
        vertical-align: top;
    }

    .ironSP-console-error td, .ironSP-console-stackTrace td {
        color: red !important;
        font-size: 10pt;
    }

    .ironSP-console-output {
        font-style: italic;
    }

    .ironSP-console-executing {
        background: url('_layouts/images/IronSP/ajax-loader.gif') no-repeat right top;
    }
</style>
<script src="http://ajax.aspnetcdn.com/ajax/jQuery/jquery-1.9.1.min.js" type="text/javascript"> </script>
<script src="/_layouts/15/IronSP/mustache.js" type="text/javascript"> </script>
<script src="/_layouts/15/IronSP/IronConsole.js" type="text/javascript"> </script>
<script type="text/javascript">$(document).ready(function () { var console = new IronConsole('<%= Microsoft.SharePoint.SPContext.Current.Web.Url %>/_layouts/15/IronSP/IronConsoleService.ashx?lang=ruby'); var consoleView = new IronConsoleView(console); });</script>
<script id="ironSP-console-line-template" type='text/html'><tr class='ironSP-console-line ironSP-console-{{type}}'><td class='ironSP-console-prefix'><span>{{prefix}}</span></td><td><span>{{text}}</span></td></tr></script>
<script id="ironSP-console-template" type='text/html'><table id="ironSP-console"><tr class='ironSP-console-prompt'><td class='ironSP-console-prefix'><span>{{prompt}}</span></td><td><textarea id="ironSP-console-input" cols="80" rows="5" /></td></tr></table></script>
<div id="ironSP-console-container"></div>
