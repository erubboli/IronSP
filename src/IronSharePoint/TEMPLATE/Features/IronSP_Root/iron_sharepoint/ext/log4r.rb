require 'log4r'
require 'iron_sharepoint/patches/log4r/logger.rb'
require 'iron_sharepoint/ext/log4r/outputter/iron_logs_outputter'
require 'iron_sharepoint/ext/log4r/outputter/iron_memory_outputter'

formatter = Log4r::PatternFormatter.new(:pattern => "[%l] %d %c :: %M")
io_formatter = Log4r::PatternFormatter.new(:pattern => "[%l] %c :: %M")

default_log = Log4r::Logger.new "Iron"
default_log.outputters << (Log4r::IronMemoryOutputter.new "iron_default", $RUNTIME, :formatter => formatter)
default_log.outputters << (Log4r::StdoutOutputter.new "iron_stdout", :formatter => io_formatter, :only_at => [Log4r::DEBUG, Log4r::INFO, Log4r::WARN])
default_log.outputters << (Log4r::StderrOutputter.new "iron_stderr", :formatter => io_formatter, :level => Log4r::ERROR)

internal_log = Log4r::Logger.new "log4r"
internal_log.outputters << (Log4r::IronMemoryOutputter.new "iron_internal", $RUNTIME, :formatter => formatter)

::IronSP::DEFAULT_LOGGER = IRON_DEFAULT_LOGGER = default_log
