require 'mustache'

module ActionView
  module TemplateHandlers
    module Mustache
      class Handler < TemplateHandler
        include Compilable

        def compile(template)
          <<-RUBY
            mustache = ActionView::TemplateHandlers::Mustache::View.new
            mustache.template = '#{template.source}'
            mustache.view = self
            mustache[:yield] = content_for(:layout)
            mustache.context.update(local_assigns)

            mustache.render
          RUBY
        end
      end

      class View < ::Mustache
        attr_accessor :view

        def method_missing(method, *args, &block)
          view.send(method, *args, &block)
        end

        def respond_to?(method, include_private=false)
          super(method, include_private) || view.respond_to?(method, include_private)
        end

        def partial(name)
          template = view.view_paths.find_template name.to_s, :mustache, false
          template.source
        end
      end
    end
  end

  Template.register_template_handler(:mustache, TemplateHandlers::Mustache::Handler)
end
