require "log4r"
require "log4r/outputter/outputter"
require "log4r/staticlogger"

module Log4r
  class IronMemoryOutputter < Outputter
    def initialize _name, opts = {}
      super _name, opts
    end

    def print opts = {}
      opts = {
        filter: //,
        count: 10
      }.merge opts

      filter = opts[:filter]
      filter = %r{#{filter}} unless filter.is_a? Regexp

      logs.select do |entry|
        (entry =~ filter) != nil
      end.first(opts[:count]).each do |entry|
        puts entry
      end
      nil
    end

    private

    def write data
      logs << data.encode
      @logs = logs.last 100 if logs.size > 100
      nil
    end

    def logs
      @logs ||= []
    end
  end
end
