Iron SharePoint Alpha 1.0

Install IronRuby 1.0 (default installation) (Install HAML Gem; and fix RubyGems config; 32bit Bug=> thx Kevin :-)
Copy Ruby and Scripting Assemblies to the GAC (RubyHome\bin)
Deploy IronSharePoint

###IronApps####
Go to sitecollection/_catalogs/IronApps
Add New IronApp Item
Go to SharePoint Designer navigate to sitecollection/_catalogs/IronApps/mayapp
add app.rb 
Content: "Hello #{ctx.Web.Title}" 
Go to the team site add IronPart; edit WebPart; Choose myapp

###IronControl####
User IronPart WebPart

Go to Masterpage and add 
<%@ Register Tagprefix="Iron" Namespace="IronSharePoint" Assembly="IronSharePoint, Version=1.0.0.0, Culture=neutral, PublicKeyToken=6f476c86fea3892b" %>

Add the IronControl
<Iron:IronControl Language="Ruby" runat="server">
		<config>
			this is a config string;
		</config>
		<script>
			"Hello #{ctx.Web.Title} => #{this.Language}"
		</script>
</Iron:IronControl>
