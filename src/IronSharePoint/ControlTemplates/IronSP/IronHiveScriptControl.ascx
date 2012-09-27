<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %> 
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="IronHiveScriptControl.ascx.cs" Inherits="IronSharePoint.ControlTemplates.IronSharePoint.CodeEditorDelegateControl" %>

<script type="text/javascript" src="/_layouts/IronSP/jquery.js"></script>
<script type="text/javascript" src="/_layouts/IronSP/jquery-fieldselection.min.js" > </script>
<script type="text/javascript" src="/_layouts/IronSP/mustache.js" ></script>
<script type="text/javascript" src="/_layouts/IronSP/IronConsole.js" > </script>

<script type="text/javascript">

$(document).ready(function () {
    
    var webUrl = '<%= SPContext.Current.Web.Url %>';

    $("a[href$='.rb']").each(function(){

        var fileUrl = encodeURI($(this).attr("href"))

        var newUrl = webUrl + "/_layouts/IronSP/IronEditor.aspx?file=" +  fileUrl;

        $(this).attr("href", newUrl);
    });
});
</script>