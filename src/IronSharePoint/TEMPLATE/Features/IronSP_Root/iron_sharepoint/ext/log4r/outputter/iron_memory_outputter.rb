require "log4r"
require "log4r/outputter/outputter"
require "log4r/staticlogger"

module Log4r
  class IronMemoryOutputter < Outputter
    def initialize _name, runtime = $RUNTIME, hash = {}
      super _name, hash
      @runtime = runtime
    end

    def print_log n=10
      logs.last(n).each do |x|
        puts x
      end
      nil
    end

    private

    def write data
      logs << data.encode
      @logs = logs[900...1000] if logs.size > 1000
      nil
    end

    def logs
      @logs ||= []
    end
  end
end
