require 'log4r'

module IronSP
  formatter = Log4r::PatternFormatter.new(:pattern => "[%l] %d %c :: %M")
  io_formatter = Log4r::PatternFormatter.new(:pattern => "[%l] %c :: %M")

  DEFAULT_LOGGER = ::IRON_DEFAULT_LOGGER = Log4r::Logger.new("Iron")
  DEFAULT_LOGGER.outputters << (Log4r::IronMemoryOutputter.new "iron_memory", :formatter => formatter)
  DEFAULT_LOGGER.outputters << (Log4r::StdoutOutputter.new "iron_stdout", :formatter => io_formatter, :only_at => [Log4r::DEBUG, Log4r::INFO, Log4r::WARN])
  DEFAULT_LOGGER.outputters << (Log4r::StderrOutputter.new "iron_stderr", :formatter => io_formatter, :level => Log4r::ERROR)

  RACK_LOGGER = ::IRON_RACK_LOGGER = Log4r::Logger.new("Rack")
  RACK_LOGGER.outputters << (Log4r::IronMemoryOutputter.new "rack_memory")

  def self.print_log opts = {}
    opts = { filter: opts } unless opts.is_a? Hash
    opts = {
      name: "iron_memory"
    }.merge opts
    name = opts.delete :name
    outputter = Log4r::Outputter[name]
    raise "Outputter '#{name}' not found." if outputter.nil?
    raise "Outputter must be of type Log4r::IronMemoryOutputter." unless outputter.is_a? Log4r::IronMemoryOutputter

    outputter.print opts
  end
end
