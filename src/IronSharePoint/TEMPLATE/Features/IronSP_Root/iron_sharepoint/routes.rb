require 'sinatra/base'
require 'iron_sharepoint/ext/rack/handlers/iron_sp'
require 'rack/body_proxy'
require 'iron_sharepoint/view_helpers/translation_helper'
require 'iron_sharepoint/view_helpers/debug_helper'

module IronSP
  class << self
    attr_accessor :routes
  end

  class Routes < Sinatra::Base
    class << self
      def inherited base
        #raise "You cannot have more than one IronSP::Routes" unless IronSP.routes.nil?
        super
        if IronSP.routes.nil?
          IronSP.routes = base.new
          @handler = Rack::Handlers::IronSP.run(IronSP.routes)
        end
      end

      def process http_context
        handler.process http_context
      end

      private

      def handler
        @handler || default_handler
      end

      def default_handler
        @default_handler ||= Rack::Handlers::IronSP.run(self.new)
      end
    end

    use Rack::CommonLogger, Rack::Log4rCommonLoggerAdapter.new(::IronSP::RACK_LOGGER)

    set :logging, nil # Loggers are set by the handler
    set :environment, IronSP.env.to_sym
    set :views, File.join(IronSP::IronConstant.HiveWorkingDirectory, 'app/templates')

    configure :development do
      enable :show_exceptions
      enable :dump_exceptions
      disable :raise_exceptions
    end

    configure :production do
      disable :show_exceptions
      enable :dump_exceptions
      disable :raise_exceptions
    end

    helpers ActionView::Helpers::TranslationHelper
    helpers ActionView::Helpers::DebugHelper
  end
end
