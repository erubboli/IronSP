require 'iron_sharepoint/mixins/logging'

module IronSharePoint::HttpHandlers
  def self.routes
    @routes ||= {}
  end

  class MainHttpHandler
    include System::Web::IHttpHandler
    include IronSharePoint::Mixins::Logging

    def IsReusable
      false
    end

    def ProcessRequest(context)
      path = context.Request.Path
      _, handler = IronSharePoint::HttpHandlers.routes.select {|k,v| k.match(path) }.to_a.last

      unless handler.nil?
        handler.new.ProcessRequest(context)
      else
        logger.error "No handler registered for #{path}"
        context.Response.write("No handler has been registered for this URL")
      end
    end
  end
end

$RUNTIME.HttpHandlerClass = "IronSharePoint.HttpHandlers.MainHttpHandler"
