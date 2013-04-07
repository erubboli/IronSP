require 'singleton'
require 'rack'

module IronSharePoint
  class << self
    attr_accessor :application
  end

  class Application
    include Singleton

    class << self
      def inherited base
        raise "You cannot have more than one IronSharePoint::Application" if IronSharePoint.application
        super
        IronSharePoint.application = base.instance
      end
    end

    attr_reader :assets, :rack_handler

    def initialize
      @initialized = false
    end

    def initialized?
      @initialized
    end

    def initialize!
      raise "Application has already been initialized." if @initialized

      @rack_handler = Rack::Handlers::IronSP.run(self.to_app @assets)

      @initialized = true
      self
    end

    def to_app assets
      Rack::Builder.new do
        use Rack::ContentLength
        use Rack::ShowExceptions

        map '/_assets' do
          run assets
        end

        map '/' do
          run lambda{|env| [200, {"Content-Type" => "text/html"}, ["Hello IronSP"]]}
        end
      end
    end
  end
end
