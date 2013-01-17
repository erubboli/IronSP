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

    alias_method :render_base, :Render
    def Render(writer)
      logger.error self.render_exception unless self.render_exception.nil?
      render_base writer
    end

    def ToHtml sp_context
      @sp_context = sp_context
      render_template
    end
  end
end
