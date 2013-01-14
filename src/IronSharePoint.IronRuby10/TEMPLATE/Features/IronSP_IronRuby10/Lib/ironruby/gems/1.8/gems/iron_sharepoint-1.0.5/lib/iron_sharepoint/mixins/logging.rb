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
    end

    def logger
      @logger ||= begin
        name = self.is_a?(Module) ? self.name : self.class.name
        @logger = Log4r::Logger[name] || (Log4r::Logger.new "default::#{name}")
        @logger
      end
    end
  end
end
