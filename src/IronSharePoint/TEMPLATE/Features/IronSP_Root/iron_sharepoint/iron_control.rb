require 'iron_sharepoint/mixins/logging'
require 'iron_sharepoint/mixins/type_registration'
require 'iron_sharepoint/mixins/control_view'
require 'iron_sharepoint/mixins/parent_attributes'

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
        html = to_html
        writer.Write(html)
      rescue Exception => ex
        logger.error ex
        writer.Write("<div class='iron-control-error'>Error in #{self.class.name}</div>")
      ensure
        scope.dispose
      end
    end
  end
end