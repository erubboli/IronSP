require 'lib/iron_sharepoint/iron_view'

module IronSharePoint::Mixins
  module ControlView
    module ClassMethods
      def with_template template_file
        @template_file = template_file
      end

      def template_file
        @template_file || default_template
      end

      protected

      def default_template
        @default_template ||= begin
          m = self.name.match(/Controls::((\w+::)*(\w+))/)
          template_name = m[1].gsub("::","/").to_snake_case
          @default_template = "#{template_name}.haml"
        end
      end
    end

    def self.included klass
      klass.extend ClassMethods
    end

    def render_context
      { }
    end
    alias_method :view_context, :render_context

    def to_html
      view.render :file => self.class.template_file
    end

    def view
      @view ||= begin
        @view = IronView.new ["app/templates/"]
        @view.context = view_context
        @view
      end
    end
  end
end
