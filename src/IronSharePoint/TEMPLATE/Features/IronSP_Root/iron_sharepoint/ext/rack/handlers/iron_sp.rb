require 'rack'
require 'iron_sharepoint/mixins/logging'

module Rack
  module Handlers
    class IronSP
      include IronSharePoint::Mixins::Logging

      attr_reader :app, :started

      alias_method :started?, :started

      def self.run app, options = {}
        @server = self.new app
        @server.start
        @server
      end

      def initialize app
        @app = app
      end

      def process ctx
        if started?
          req = ctx.request
          res = ctx.response

          env = build_env req
          log_env env

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
          env[key] = truncate_path(key, req.server_variables[key])
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

      def truncate_path(key, value)
        case key
        when "PATH_INFO"
          if (match = /\/_assets\/.*$/.match(value))
            match[0]
          elsif (match = /_iron(\/.*)$/.match(value))
            match[1]
          else
            value
          end
        when "SCRIPT_NAME"
          if value =~ /\/_assets\//
            "/_assets"
          else
            match = /_iron(\/[^\/]+)/.match(value)
            match.nil? ? value : match[1]
          end
        else
          value
        end
      end

      def log_env env
        env = env.sort.map {|k,v| "#{k} => #{v}"}.join("\n")
        logger.debug "Processing rack request:\n#{env}"
      end
    end
  end
end
