# :nodoc:
require "log4r"
require "log4r/outputter/outputter"
require "log4r/staticlogger"

module Log4r
  class IronRuntimeOutputter < Outputter
    include System

    def initialize _name, runtime = $RUNTIME, hash = {}
      super _name, hash
      @runtime = runtime
    end

    private

    def write data
      unless @runtime.respond_to? :logs
        def @runtime.logs
          @logs ||= []
        end
      end
      @runtime.logs << data
    end
  end
end
