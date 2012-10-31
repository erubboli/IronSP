require 'haml'
require 'iron_templates/mustache'

module ActionView
  module TemplateHandlers
    module Hamstache
      class Handler < TemplateHandler
        include Compilable

        def compile(template)
          <<-RUBY
            mustache = ActionView::TemplateHandlers::Hamstache::View.new
            mustache.template = '#{template.source}'
            mustache.view = self
            mustache[:yield] = content_for(:layout)
            mustache.context.update(local_assigns)

            mustache.render
          RUBY
        end
      end

      class View < ActionView::TemplateHandlers::Mustache::View
        def template= haml
          super Haml::Engine.new(haml).render
        end
      end
    end
  end

  Template.register_template_handler(:hamstache, TemplateHandlers::Hamstache::Handler)
end

module Haml
  module Precompiler
    alias_method :process_line_without_mustache, :process_line

    def process_line(text, index)
      if text =~ /^\{\{.*\}\}$/
        @index = index + 1
        push_merged_text(text, 1, true)
        concat_merged_text("\n")
        return
      else
        process_line_without_mustache(text, index)
      end
    end
  end
end

