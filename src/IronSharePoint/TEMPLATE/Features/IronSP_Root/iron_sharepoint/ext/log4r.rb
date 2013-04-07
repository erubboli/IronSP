require 'log4r'
require 'iron_sharepoint/patches/log4r/logger.rb'
require 'iron_sharepoint/ext/log4r/outputter/iron_logs_outputter'
require 'iron_sharepoint/ext/log4r/outputter/iron_memory_outputter'

formatter = Log4r::PatternFormatter.new(:pattern => "[%l] %d - %c@%t :: %M")

default_log = Log4r::Logger.new "default"
default_log.outputters << (Log4r::IronLogsOutputter.new "iron_logs", $RUNTIME, :formatter => formatter, :level => Log4r::WARN)
default_log.outputters << (Log4r::IronMemoryOutputter.new "iron_default", $RUNTIME, :formatter => formatter)

internal_log = Log4r::Logger.new "log4r"
internal_log.outputters << (Log4r::IronMemoryOutputter.new "iron_internal", $RUNTIME, :formatter => formatter)

::IronSharePoint::INTERNAL_LOGGER = IRON_INTERNAL_LOGGER = internal_log
::IronSharePoint::DEFAULT_LOGGER = IRON_DEFAULT_LOGGER = default_log

