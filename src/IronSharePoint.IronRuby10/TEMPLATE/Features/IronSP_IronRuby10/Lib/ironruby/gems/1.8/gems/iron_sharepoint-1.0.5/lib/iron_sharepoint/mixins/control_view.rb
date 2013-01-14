require 'lib/iron_sharepoint/iron_view'

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

    def template_path
      return @template_path unless @template_path.nil?
      if ActionView::Base.cache_template_loading?
        @@cached_template_path ||= ActionView::PathSet.new [ActionView::Template::EagerPath.new(DEFAULT_TEMPLATE_PATH)]
      else
        @@reloadable_template_path ||= ActionView::PathSet.new [ActionView::ReloadableTemplate::ReloadablePath.new(DEFAULT_TEMPLATE_PATH)]
      end
    end

    def view
      @view ||= begin
        @view = IronSharePoint::IronView.new template_path
        @view.parent = self
        @view.context = view_context
        @view
      end
    end

    def to_html
      view.render :file => template
    rescue Exception => ex
      logger.error ex if respond_to? :logger
      if (SPContext.current && SPContext.current.web.current_user)
        error_template ex
      else
        ""
      end
    end

    def error_template ex
      backtrace = ex.backtrace[0..2].map{|x| "<div>#{x}</div>"}
      <<-HTML
        <div class="ironsp-error" style="display: inline-block; color: red; font-size: 10px;line-height: 110%;text-align: left;">
          <p>#{ex.message}</p>
          #{backtrace}
        </div>
      HTML
    end

    def render_context
      { }
    end
    alias_method :view_context, :render_context
  end
end
