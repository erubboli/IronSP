require 'tilt'
require 'iron_sharepoint/mixins/logging'
require 'iron_sharepoint/mixins/view_helpers'
require 'active_support/core_ext/module/delegation'

module IronSharePoint
  class IronView
    include Mixins::Logging
    include ::IronSharePoint::ViewHelpers

    attr_accessor :parent, :template, :context
    delegate :attributes, :has_attribute?, :to => :parent

    class << self
      attr_accessor :cache_template, :template_paths

      def cache_template?
        @cache_template
      end

      def cache
        @cache ||= Tilt::Cache.new
      end

      def template_paths
        @templates_path ||= ["app/templates", "iron_sharepoint/rescues"]
      end
    end

    def initialize options = {}
      @context = options[:context]
      @template = options[:template]
      @parent = options[:parent]
    end

    def render
      compiled = compiled_template(@template)
      unless compiled.nil?
        compiled.render self, @context
      else
        compiled_template("template_not_found.haml").render self
      end
    rescue Exception => ex
      logger.error ex if respond_to? :logger
      if (SPContext.current && SPContext.current.web.current_user)
        compiled_template("template_error.haml").render(self, {:message => ex.message, :backtrace => ex.backtrace})
      else
        ""
      end
    end

    private

    def compiled_template template_name
      if self.class.cache_template?
        self.class.cache.fetch template_name do
          load_template template_name
        end
      else
        load_template template_name
      end
    end

    def load_template template_name
      full_paths = self.class.template_paths.map do |template_path|
        template_name.starts_with?(template_path) ? template_name : File.join(template_path, template_name)
      end
      valid_path = full_paths.find do |full_path|
        File.exist? full_path
      end
      Tilt.new valid_path unless valid_path.nil?
    end
  end
end
