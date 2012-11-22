require 'active_support'
require 'action_controller'
require 'action_view'

base_dir = File.join(File.dirname(__FILE__), "iron_templates")
Dir["#{base_dir}/*.rb"].each do |file|
  require file
end
