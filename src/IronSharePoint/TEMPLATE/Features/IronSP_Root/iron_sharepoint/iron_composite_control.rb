module IronSharePoint
  class IronCompositeControl
    include IronView
    include Mixins::Logging
    include Mixins::ParentAttributes
    extend Mixins::ShortTermMemory

    def CreateChildControls
      monitor "Render #{self.class.name}" do
        begin
          unless self.page.nil?
            ctrl = self.page.parse_control(render_template)
            self.controls.add(ctrl)
          end
        rescue Exception => ex
          logger.error ex
        end
      end
    end

    def Render writer
      super
    end
  end
end
