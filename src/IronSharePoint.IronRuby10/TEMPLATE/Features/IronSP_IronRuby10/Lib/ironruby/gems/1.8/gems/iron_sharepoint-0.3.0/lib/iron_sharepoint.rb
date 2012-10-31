require 'rubygems'
require 'log4r'
require 'action_controller'
require 'action_view'
require 'lib/log4r/outputter/iron_logs_outputter'
require 'lib/log4r/outputter/iron_runtime_outputter'

log = Log4r::Logger.new "iron"
log.outputters = Log4r::IronLogsOutputter.new "iron"

internal_log = Log4r::Logger.new "log4r"
internal_log.outputters = Log4r::IronRuntimeOutputter.new "internal"

IRON_DEFAULT_LOGGER = log

base_dir = File.join(File.dirname(__FILE__), "iron_sharepoint")
Dir["#{base_dir}/**/*.rb"].each do |file|
  require file
end
