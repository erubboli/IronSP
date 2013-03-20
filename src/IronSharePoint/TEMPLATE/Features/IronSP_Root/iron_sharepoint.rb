require 'active_support/core_ext'
require 'iron_sharepoint/ext/log4r'
require 'iron_sharepoint/ext/rack'

module IronSharePoint
  Dir["iron_sharepoint/**/*.rb"].each do |file|
    begin
      require file
    rescue Exception => ex
      DEFAULT_LOGGER.error ex
    end
  end
end

