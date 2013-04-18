require 'rack'
require 'iron_sharepoint/ext/rack/log4r_adapters'

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

          begin
            env = build_env req
            status, headers, body = @app.call env

            write_response res, status, headers, body
          ensure
            body.close  if body.respond_to? :close
          end
        else
          res.status_code = 404
          res.write "Not started"
        end
      rescue Exception => ex
        logger.error ex
        res.status_code = 500
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
          "rack.errors" => Rack::Log4rErrorStreamAdapter.new(::IronSP::RACK_LOGGER),
          "rack.logger" => ::IronSP::RACK_LOGGER,
          "rack.multithread" => true,
          "rack.multiprocess" => false,
          "rack.run_once" => false,
          "rack.url_scheme" => req.url.scheme.downcase,
          "rack.input" => read_body(req)
        }

        req.server_variables.all_keys.each do |key|
          value = truncate_path(key, req.server_variables[key])
          env[key.to_s] = value.nil? ? nil : value.to_s
        end

        return env
      end

      def read_body req
        using(System::IO::StreamReader.new req.input_stream) do |reader|
          StringIO.new(reader.read_to_end)
        end
      end

      def write_response res, status, headers, body
        res.status_code = status
        if (ct_header = headers.delete "Content-Type")
          res.content_type, charset = ct_header.split ';'
          charset.match(/charset=\s*\"?([^\s;\"]+)\"?/) do |m|
            begin
              encoding = Sytem::Text::Encoding.get_encoding m[1]
              res.content_encoding = encoding
            rescue
            end
          end unless charset.nil?
        end
        headers.each{|k,v| res.headers[k] = v}
        if body.is_a?(Rack::BodyProxy)
          inner = body.instance_variable_get :@body
          if inner.is_a?(System::Array[System::Byte])
            res.binary_write inner
            return
          end
        end
        if body.respond_to?(:each)
          body.each do |part|
            res.write part.to_s
          end
        else
          res.write body.to_s
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
    end
  end
end
