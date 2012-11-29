require 'lib/iron_sharepoint/mixins/logging'
require 'lib/iron_sharepoint/mixins/type_registration'
require 'lib/iron_sharepoint/mixins/control_view'
require 'lib/iron_sharepoint/mixins/parent_attributes'

module IronSharePoint
  class IronCompositeControl
    include Mixins::Logging
    include Mixins::ControlView
    include Mixins::ParentAttributes

    def self.inherited child
      child.send :include, Mixins::TypeRegistration
    end

    def CreateChildControls
      scope = Microsoft::SharePoint::Utilities::SPMonitoredScope.new "CreateChildControls #{self.class.name}"
      unless self.page.nil?
        ctrl = self.page.parse_control(to_html)
        self.controls.add(ctrl)
      end
    rescue Exception => ex
      logger.error ex
    ensure
      scope.dispose
    end
  end
end
