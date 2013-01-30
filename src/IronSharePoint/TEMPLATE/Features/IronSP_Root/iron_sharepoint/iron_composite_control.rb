require 'iron_sharepoint/mixins/logging'
require 'iron_sharepoint/mixins/type_registration'
require 'iron_sharepoint/mixins/control_view'
require 'iron_sharepoint/mixins/parent_attributes'
require 'iron_sharepoint/mixins/short_term_memory'

module IronSharePoint
  class IronCompositeControl
    include Mixins::Logging
    include Mixins::ControlView
    include Mixins::ParentAttributes
    extend Mixins::ShortTermMemory

    def self.inherited child
      child.send :include, Mixins::TypeRegistration
    end

    def CreateChildControls
      monitor "Render #{self.class.name}" do
        begin
          unless self.page.nil?
            self.view.context = view_context
            ctrl = self.page.parse_control(self.view.render)
            self.controls.add(ctrl)
          end
        rescue Exception => ex
          logger.error ex
        end
      end
    end
  end
end
