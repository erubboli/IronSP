require 'rubygems'
require 'log4r'
require 'action_controller'
require 'action_view'
require 'lib/log4r/outputter/iron_logs_outputter'
require 'lib/log4r/outputter/iron_memory_outputter'

formatter = Log4r::PatternFormatter.new(:pattern => "[%l] %d - %c@%t :: %M")

default_log = Log4r::Logger.new "default"
default_log.outputters << (Log4r::IronLogsOutputter.new "iron_logs", $RUNTIME, :formatter => formatter, :level => Log4r::WARN)
default_log.outputters << (Log4r::IronMemoryOutputter.new "iron_default", $RUNTIME, :formatter => formatter)

internal_log = Log4r::Logger.new "log4r"
internal_log.outputters << (Log4r::IronMemoryOutputter.new "iron_internal", $RUNTIME, :formatter => formatter)

IRON_INTERNAL_LOGGER = internal_log
IRON_DEFAULT_LOGGER = default_log

base_dir = File.dirname(__FILE__)
Dir["#{base_dir}/**/*.rb"].each do |file|
  require file
end
