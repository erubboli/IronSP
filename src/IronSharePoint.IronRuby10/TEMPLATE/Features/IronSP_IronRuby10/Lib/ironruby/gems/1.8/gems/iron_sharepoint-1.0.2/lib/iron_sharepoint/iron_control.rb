require 'lib/iron_sharepoint/mixins/logging'
require 'lib/iron_sharepoint/mixins/type_registration'
require 'lib/iron_sharepoint/mixins/control_view'
require 'lib/iron_sharepoint/mixins/parent_attributes'

module IronSharePoint
  class IronControl
    include Mixins::Logging
    include Mixins::ControlView
    include Mixins::ParentAttributes

    def self.inherited child
      child.send :include, Mixins::TypeRegistration
    end

    def Render(writer)
      scope = Microsoft::SharePoint::Utilities::SPMonitoredScope.new "Render #{self.class.name}"
      begin
        writer.Write(to_html)
      rescue Exception => ex
        logger.error ex
        writer.Write(ex.message)
      ensure
        scope.dispose
      end
    end
  end
end
