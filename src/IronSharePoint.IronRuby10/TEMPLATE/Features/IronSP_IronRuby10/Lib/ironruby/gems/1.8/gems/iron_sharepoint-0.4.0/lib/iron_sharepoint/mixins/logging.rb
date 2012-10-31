require 'log4r'
require 'lib/log4r/outputter/iron_logs_outputter'

include Log4r

module IronSharePoint::Mixins
  module Logging

    def self.included klass
      klass.extend self
    end

    def log message, level = :info
      logger.send level, message
      # @logger_deprecated ||= IronSharePoint::IronLog::IronLogger.new $RUNTIME
      # level_enum = level_to_enum level
      # if message.is_a? Exception
      #   ex = message
      #   backtrace = ex.backtrace.map{|x| "\t#{x}"}.join "\n"
      #   message = "#{ex.class}: #{ex.message}\n#{backtrace}"
      # end
      # @logger_deprecated.log message, level_enum
    end

    def logger
      @logger ||= begin
        name = self.class.name
        @logger = Log4r::Logger[name] || (Log4r::Logger.new name)
        @logger.outputters = Log4r::Outputter[name] || (Log4r::IronLogsOutputter.new name)
        @logger
      end
    end
  end
end
