# :nodoc:
require "log4r"
require "log4r/outputter/outputter"
require "log4r/staticlogger"

module Log4r
  class IronLogsOutputter < Outputter
    def initialize _name, runtime = $RUNTIME, hash = {}
      super _name, hash
      @runtime = runtime
    end

    private

    def write data
      begin
        site = @runtime.iron_hive.site;

        site.AddWorkItem(System::Guid.NewGuid(),
                         System::DateTime.UtcNow,
                         IronSharePoint::IronLog::IronLogWorkItemJobDefinition.WorkItemGuid,
                         site.RootWeb.ID,
                         site.ID,
                         1,
                         false,
                         site.RootWeb.GetList(site.RootWeb.ServerRelativeUrl + "/" + IronSharePoint::IronConstant.IronLogsListPath).ID,
                         System::Guid.Empty,
                         site.SystemAccount.ID,
                         nil,
                         data,
                         System::Guid.Empty)
      rescue Exception => ex
        Logger.log_internal {"#{ex.class} in Outputter '#{@name}'!"}
        Logger.log_internal {ex}
      end
    end
  end
end
