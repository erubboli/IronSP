require 'active_support/core_ext/module/delegation'

module Rack
  class Log4rErrorStreamAdapter
    attr_reader :logger
    delegate :flush, :to => :logger

    def initialize logger
      @logger = logger
    end

    def write data
      logger.error data
    end

    def puts data
      write data.to_s
    end
  end

  class Log4rCommonLoggerAdapter
    attr_reader :logger
    delegate :flush, :to => :logger

    def initialize logger
      @logger = logger
    end

    def write data
      logger.info data
    end

    def puts data
      write data.to_s
    end
  end
end
