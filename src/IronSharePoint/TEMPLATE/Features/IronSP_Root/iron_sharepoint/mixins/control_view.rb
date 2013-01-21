require 'iron_sharepoint/iron_view'

module IronSharePoint::Mixins
  module ControlView
    attr_writer :template, :view

    DEFAULT_TEMPLATE_PATH = "app/templates"

    module ClassMethods
      def with_template template
        if template.is_a? Hash
          ext, name = template.to_a[0]
          @template = "#{name}.#{ext.to_s}"
        else
          @template = template.to_s
        end
      end

      def template
        @template || default_template
      end

      protected

      def default_template
        @default_template ||= begin
          m = self.name.match(/Controls::((\w+::)*(\w+))/)
          template_name = m[1].gsub("::","/").underscore
          @default_template = "#{template_name}.haml"
        end
      end
    end

    def self.included klass
      klass.extend ClassMethods
    end

    def template
      @template || self.class.template
    end

    def view
      @view ||= IronSharePoint::IronView.new({
        :template => template,
        :context => view_context,
        :parent => self
      })
    end

    private

    def view_context
      { }
    end
  end
end
