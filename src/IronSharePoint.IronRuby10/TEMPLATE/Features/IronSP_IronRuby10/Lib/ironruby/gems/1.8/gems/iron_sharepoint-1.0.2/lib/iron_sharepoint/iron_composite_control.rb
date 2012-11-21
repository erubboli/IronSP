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

    def render_context
      { }
    end

    def CreateChildControls
      unless self.page.nil?
        ctrl = self.page.parse_control(to_html)
        self.controls.add(ctrl)
      end
    end
  end
end
