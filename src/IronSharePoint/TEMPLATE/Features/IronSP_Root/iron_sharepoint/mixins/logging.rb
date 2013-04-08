require 'log4r'

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
        Log4r::Logger[name] || (Log4r::Logger.new "#{IronSP::DEFAULT_LOGGER}::#{name}")
      end
    end
  end
end
