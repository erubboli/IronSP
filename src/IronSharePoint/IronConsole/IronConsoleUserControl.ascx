<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %> 
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="IronConsoleUserControl.ascx.cs" Inherits="IronSharePoint.IronConsole.IronConsoleUserControl" %>

<style type="text/css">
.boxsizingBorder { 
  -webkit-box-sizing: border-box; 
  -moz-box-sizing: border-box; 
  box-sizing: border-box; 
  border-width:0px;
  overflow:auto;
  width:90%;
  float:left;
} 

#ironSP_Console_Out
{
  max-height:500px;
  overflow:auto;
}

#ironSP_Console_Container textarea { 
  color:Black !important;
  font-family:Consolas !important;
  font-size:12pt;
} 

.ironSP_Console_Alt{
  background-color:Yellow;
}

#ironSP_Console_Container{
  padding:5px;
  color:Black !important;
  font-family:Consolas !important;
  font-size:12pt;
}

#ironConsole-prompt {
  float: left;
}

</style>

<script type="text/javascript" src="/_layouts/IronSP/jquery.js" ></script>
<script type="text/javascript" src="/_layouts/IronSP/mustache.js" ></script>
<script type="text/javascript" src="/_layouts/IronSP/IronConsole.js" ></script>

<script type="text/javascript">
  $(document).ready(function () {
    var console = new IronConsole('<%= SPContext.Current.Web.Url  %>/_layouts/IronSP/IronConsoleService.ashx?ext=.rb')
    var consoleView = new IronConsoleView(console)

    return;

    var strg = false;
    var history = [];
    var historyIndex = 0;

    $("#ironSP_Console_Script").keyup(function (event) {

      var prompt = "&gt;&gt;&nbsp;";
      var command = $("#ironSP_Console_Script").val();

      if (event.keyCode == 13 && !strg) {

        if (command.trim() == 'clear') {
          $("#ironSP_Console_Out").empty();
          $('#ironSP_Console_Script').val("");
          return;
        }

        history.push(command.replace(new RegExp("\\n$"), ""));
        historyIndex = -1;

        $.ajax({
          type: 'POST',
          url: '<%= SPContext.Current.Web.Url %>/_layouts/IronSP/IronConsoleService.ashx?ext=.rb',
          dataType: 'text',
          data: {
            script: command
          },
          success: function (data) {
            $("#ironSP_Console_Out").append("<p>" + prompt + $('#ironSP_Console_Script').val().replace(/(\r\n|\n|\r)/gm, "<br/>") + "</p>");
            $('#ironSP_Console_Script').val("");
            $.each(data.replace( /(\r\n|\n|\r)/gm , "<br/>").split("<br/>"), function(i, s) {
              $("<p/>").text(s).appendTo("#ironSP_Console_Out");
            });
            $('#ironSP_Console_Out').scrollTop($('#ironSP_Console_Out')[0].scrollHeight);
          }
        });
      }

      if (event.keyCode == 38 && !strg) {
        historyIndex += 1;
        historyIndex = historyIndex % history.length;

        var index = (history.length - 1) - (historyIndex % history.length);
        $('#ironSP_Console_Script').val(history[index]);
        event.preventDefault();
        return false;
      }
      if (event.keyCode == 40 && !strg) {
        historyIndex -= 1;
        if (historyIndex < 0) historyIndex += history.length;
        historyIndex = historyIndex % history.length;

        var index = (history.length - 1) - (historyIndex % history.length);
        $('#ironSP_Console_Script').val(history[index]);
        event.preventDefault();
        return false;
      }
    }).keydown(function (event) {

      var TABKEY = 9;
      if (event.keyCode == TABKEY) {
        $('#ironSP_Console_Script').val($('#ironSP_Console_Script').val() + "    ");
        return false;
      }

      if (event.keyCode == 45) {
        strg = !strg;
        strg ? $("#ironSP_Console_Script").addClass('ironSP_Console_Alt') : $("#ironSP_Console_Script").removeClass('ironSP_Console_Alt');
        return false;
      };

      if (event.keyCode == 13 && !strg) {
        return false;
      }
    });

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
        <textarea id="ironSP-console-input"/>
      </td>
    </tr>
  </table>
</script>

<div id="ironSP-console-container">

</div>
