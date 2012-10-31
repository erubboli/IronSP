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

    def render_context
      { }
    end

    def Render(writer)
      begin
        writer.Write(to_html)
      rescue Exception => ex
        logger.error ex
        writer.Write(ex.message)
      end
    end
  end
end
