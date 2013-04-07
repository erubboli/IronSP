require 'active_support/core_ext'
require 'iron_sharepoint/ext/all'

module IronSharePoint
  def self.env
    @env ||= ActiveSupport::StringInquirer.new(IronConstant.IronEnv.to_s.downcase)
  end

  Dir["iron_sharepoint/**/*.rb"].each do |file|
    begin
      require file
    rescue Exception => ex
      DEFAULT_LOGGER.error ex
    end
  end
end

IronSP = IronSharePoint
