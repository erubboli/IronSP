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
      monitor "Render #{self.class.name}" do
        writer.Write self.view.render
      end
    end
  end
end
