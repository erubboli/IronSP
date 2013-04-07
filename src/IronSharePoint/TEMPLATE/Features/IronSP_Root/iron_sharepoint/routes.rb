require 'sinatra/base'
require 'iron_sharepoint/ext/rack/handlers/iron_sp'

module IronSP
  class << self
    attr_accessor :routes
  end

  class Routes < Sinatra::Base

    class << self
      def inherited base
        raise "You cannot have more than one IronSP::Routes" unless IronSP.routes.nil?
        super
        IronSP.routes = base.new
        @handler = Rack::Handlers::IronSP.run(IronSP.routes)
      end

      def process http_context
        handler.process http_context
      end

      private

      def handler
        @handler || default_handler
      end

      def default_handler
        @default_handler ||= Rack::Handlers::IronSP.run(Sinatra::Base.new)
      end
    end
  end
end
