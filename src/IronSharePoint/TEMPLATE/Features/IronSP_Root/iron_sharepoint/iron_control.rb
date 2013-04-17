require 'iron_sharepoint/iron_view'

module IronSharePoint
  class IronControl
    include IronView
    include Mixins::Logging
    include Mixins::ParentAttributes
    extend Mixins::ShortTermMemory

    def Render(writer)
      monitor "Render #{self.class.name}" do
        begin
          writer.Write render_template
        rescue Exception => ex
          logger.error ex
          raise ex
        end
      end
    end
  end
end
