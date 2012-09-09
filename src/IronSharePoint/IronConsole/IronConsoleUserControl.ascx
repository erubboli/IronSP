<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %> 
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="IronConsoleUserControl.ascx.cs" Inherits="IronSharePoint.IronConsole.IronConsoleUserControl" %>

<style type="text/css">
#ironSP-console-input {
  -webkit-box-sizing: border-box;
  -moz-box-sizing: border-box;
  box-sizing: border-box;
  border-width:0px;
  overflow:auto;
  width:90%;
  float:left;
}


#ironSP-console-container {
  padding:5px;
  max-height:500px;
  overflow:auto;
}

#ironSP-console {
}

.ironSP-console-edit {
  background-color: Yellow !important;
}

.ironSP-console-line td, .ironSP-console-prompt td, #ironSP-console-input {
  color: black !important;
  font-family: Consolas !important;
  font-size: 12pt;
}

.ironSP-console-prefix {
  padding-right: 5px;
}

.ironSP-console-prefix {
  padding-top: 2px;
  vertical-align: top;
}

.ironSP-console-error td {
  color: red !important;
  font-size: 10pt;
}

.ironSP-console-output {
  font-style: italic;
}

.ironSP-console-executing {
  background: url('_layouts/images/IronSP/ajax-loader.gif') no-repeat right top
}
</style>

<script type="text/javascript" src="/_layouts/IronSP/jquery.js" ></script>
<script type="text/javascript" src="/_layouts/IronSP/jquery-fieldselection.min.js" ></script>
<script type="text/javascript" src="/_layouts/IronSP/mustache.js" ></script>
<script type="text/javascript" src="/_layouts/IronSP/IronConsole.js" ></script>

<script type="text/javascript">
  $(document).ready(function () {
    var console = new IronConsole('<%= SPContext.Current.Web.Url  %>/_layouts/IronSP/IronConsoleService.ashx?ext=.rb')
    var consoleView = new IronConsoleView(console)
  });
</script>

<script id="ironSP-console-line-template" type='text/html'>
  <tr class='ironSP-console-line ironSP-console-{{type}}'>
    <td class='ironSP-console-prefix'>
      <span>{{prefix}}</span>
    </td>
    <td>
      <span>{{text}}</span>
    </td>
  </tr>
</script>
<script id="ironSP-console-template" type='text/html'>
  <table id="ironSP-console">
    <tr class='ironSP-console-prompt'>
      <td class='ironSP-console-prefix'>
        <span>{{prompt}}</span>
      </td>
      <td>
        <textarea id="ironSP-console-input" cols="80" rows="5"/>
      </td>
    </tr>
  </table>
</script>

<div id="ironSP-console-container">

</div>
