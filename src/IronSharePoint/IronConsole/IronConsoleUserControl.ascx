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

</style>

 <script type="text/javascript" src="/_layouts/IronSP/jquery.js" ></script>

 <script type="text/javascript">
     $(document).ready(function () {
        
        var strg = false;

         $("#ironSP_Console_Script").keyup(function (event) {
             
             var prompt = "&gt;&gt;&nbsp;"

             if (event.keyCode == 13 && !strg) {

                if($("#ironSP_Console_Script").val().trim()=='clear'){
                     $("#ironSP_Console_Out").empty();
                     $('#ironSP_Console_Script').val("");
                     return;
                }

                 $.ajax({
                     type: 'POST',
                     url: '<%= SPContext.Current.Web.Url %>/_layouts/IronSP/IronConsoleService.ashx?ext=.rb',
                     dataType: 'text',
                     data: {
                         script: $('#ironSP_Console_Script').val(),
                     },
                     success: function (data) {
                         $("#ironSP_Console_Out").append("<p>" + prompt + $('#ironSP_Console_Script').val().replace(/(\r\n|\n|\r)/gm, "<br/>") + "</p>");
                         $('#ironSP_Console_Script').val("");
                         $("#ironSP_Console_Out").append("<p>" + data.replace(/(\r\n|\n|\r)/gm, "<br/>") + "</p>");
                         $('#ironSP_Console_Out').scrollTop($('#ironSP_Console_Out')[0].scrollHeight);
                     }
                 });
             }
         }).keydown(function (event){
                
            var TABKEY = 9; 
            if(event.keyCode == TABKEY) { 
                $('#ironSP_Console_Script').val( $('#ironSP_Console_Script').val() + "    ") ;            
                return false; 
            } 
            
            if (event.keyCode == 45) {
                strg = !strg;
                strg?$("#ironSP_Console_Script").addClass('ironSP_Console_Alt'):$("#ironSP_Console_Script").removeClass('ironSP_Console_Alt');
                return false;
             };
         }); 

     });

    

 </script>

<div id="ironSP_Console_Container">
    <div id='ironSP_Console_Out'></div>
    <span style="float:left">&gt;&gt;&nbsp;</span><textarea id="ironSP_Console_Script" rows="5" cols="50" class="boxsizingBorder" ></textarea>
</div>
