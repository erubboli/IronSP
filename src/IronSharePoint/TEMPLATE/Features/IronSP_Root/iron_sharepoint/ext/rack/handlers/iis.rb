require 'rack'

module Rack
  module Handlers
    class IIS
      attr_reader :app, :started

      alias_method :started?, :started

      def self.run app, options = {}
        @server = self.new app
        @server.start
      end

      def initialize app
        @app = app
      end

      def process ctx
        if started?
          req = ctx.request
          res = ctx.response

          env = build_env req

          result = @app.call env

          write_response res, *result
        else
          res.status_code = 404
          res.write "Not started"
        end
      end

      def start
        @started = true
      end

      def shutdown
        @started = false
      end

      private

      def build_env req
        env = {
          "rack.errors" => ::IronSharePoint::DEFAULT_LOGGER,
          "rack.multithread" => true,
          "rack.multiprocess" => false,
          "rack.run_once" => false,
          "rack.url_scheme" => req.url.scheme.downcase,
          "rack.input" => read_body(req)
        }

        req.server_variables.all_keys.each do |key|
          env[key] = trim_iron(req.server_variables[key])
        end

        return env
      end

      def read_body req
        using(System::IO::StreamReader.new req.input_stream) do |reader|
          reader.read_to_end
        end
      end

      def write_response res, status, headers, body
        res.status_code = status
        headers.each{|k,v| res.headers[k] = v}
        if body.respond_to? :to_str
          res.write body.to_str
        elsif body.respond_to?(:each)
          body.each do |part|
            res.write part.to_s
          end
        end
      end

      def trim_iron(s)
        match = /_iron(\/.*)$/.match s
        match.nil? ? s : match[1]
      end
    end
  end
end
