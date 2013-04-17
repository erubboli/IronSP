require 'tilt'
#require 'iron_sharepoint/mixins/logging'
require 'iron_sharepoint/mixins/view_helpers'
require 'sinatra/base'
require 'active_support/core_ext/module/delegation'

module IronSP
  module IronView
    include Sinatra::Templates
    include Mixins::ViewHelpers

    delegate :settings, :template_cache, to: IronSP::IronView

    def default_template
      match = self.class.name.match(/Controls::((\w+::)*(\w+))/)
      template = match.nil? ? self.class.name : match[1]
      template.gsub("::","/").underscore.to_sym
    end

    def template
      @template || default_template
    end

    def render_template
      locals = view_context if respond_to? :view_context
      haml template, {}, locals
    end

    class << self
      def settings
        self
      end

      def template_cache
        @template_cache ||= Tilt::Cache.new
        @template_cache.clear if settings.reload_templates
        @template_cache
      end

      def templates
        @templates ||= {}
      end

      # Define a named template. The block must return the template source.
      def template(name, &block)
        filename, line = caller_locations.first
        templates[name] = [block, filename, line.to_i]
      end

      # Define the layout template. The block must return the template source.
      def layout(name = :layout, &block)
        template name, &block
      end

      CALLERS_TO_IGNORE = [ # :nodoc:
        /\/sinatra(\/(base|main|showexceptions))?\.rb$/,    # all sinatra code
        /lib\/tilt.*\.rb$/,                                 # all tilt code
        /^\(.*\)$/,                                         # generated code
        /rubygems\/(custom|core_ext\/kernel)_require\.rb$/, # rubygems require hacks
        /active_support/,                                   # active_support require hacks
        /bundler(\/runtime)?\.rb/,                          # bundler require hacks
        /<internal:/,                                       # internal in ruby >= 1.9.2
        /src\/kernel\/bootstrap\/[A-Z]/                     # maglev kernel files
      ]

      def caller_locations
        cleaned_caller 2
      end

      def cleaned_caller(keep = 3)
        caller(1).
          map    { |line| line.split(/:(?=\d|in )/, 3)[0,keep] }.
          reject { |file, *_| CALLERS_TO_IGNORE.any? { |pattern| file =~ pattern } }
      end

      # Sets an option to the given value.  If the value is a proc,
      # the proc will be called every time the option is accessed.
      def set(option, value = (not_set = true), ignore_setter = false, &block)
        raise ArgumentError if block and !not_set
        value, not_set = block, false if block

        if not_set
          raise ArgumentError unless option.respond_to?(:each)
          option.each { |k,v| set(k, v) }
          return self
        end

        if respond_to?("#{option}=") and not ignore_setter
          return __send__("#{option}=", value)
        end

        setter = proc { |val| set option, val, true }
        getter = proc { value }

        case value
        when Proc
          getter = value
        when Symbol, Fixnum, FalseClass, TrueClass, NilClass
          getter = value.inspect
        when Hash
          setter = proc do |val|
            val = value.merge val if Hash === val
            set option, val, true
          end
        end

        define_singleton("#{option}=", setter) if setter
        define_singleton(option, getter) if getter
        define_singleton("#{option}?", "!!#{option}") unless method_defined? "#{option}?"
        self
      end

      # Same as calling `set :option, true` for each of the given options.
      def enable(*opts)
        opts.each { |key| set(key, true) }
      end

      # Same as calling `set :option, false` for each of the given options.
      def disable(*opts)
        opts.each { |key| set(key, false) }
      end

      private

      # Dynamically defines a method on settings.
      def define_singleton(name, content = Proc.new)
        # replace with call to singleton_class once we're 1.9 only
        (class << self; self; end).class_eval do
          undef_method(name) if method_defined? name
          String === content ? class_eval("def #{name}() #{content}; end") : define_method(name, &content)
        end
      end
    end
  end

  IronView.set :views, File.join(IronSP::IronConstant.HiveWorkingDirectory, 'app/templates')
  IronView.set :default_encoding, 'utf-8'
  IronView.set :reload_templates, Proc.new { !IronSP.env.production? }
end
