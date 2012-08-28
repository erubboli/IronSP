Iron SharePoint Alpha 0.1.1.1

Deploy IronSharePoint.IronRuby10
Deploy IronSharePoint

Activate "Iron Hive Site" and "Iron Site" Feature on you site collection.

###Iron WebPart ####
Navigate in SharePoint Designer to sitecollection/_catalogs/IronHive/
add controls.rb 
Content: 
class TestControl < IronSharePoint::DynamicControl
	def Render(writer)
		writer.Write("Hello IronSP!")
	end
end

Go to the team site add IronPart; edit WebPart as following:
ScriptName: controls.rb
ScriptClass: TestControl

###IronControl####
User IronPart WebPart

Go to Masterpage and add 
<%@ Register Tagprefix="Iron" Namespace="IronSharePoint" Assembly="IronSharePoint, Version=1.0.0.0, Culture=neutral, PublicKeyToken=6f476c86fea3892b" %>

Add the IronControl
<Iron:IronControl  runat="server" ScriptName="controls.rb" ScriptClass="TestControl" ></script>
</Iron:IronControl>
