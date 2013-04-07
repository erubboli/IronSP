IronSP = IronSharePoint

require 'active_support/core_ext'
require 'iron_sharepoint/ext/all'
require 'iron_sharepoint/patches/core'

module IronSP
  def self.env
    @env ||= ActiveSupport::StringInquirer.new(IronConstant.IronEnv.to_s.downcase)
  end

  autoload :Assets, 'iron_sharepoint/assets'
  autoload :Routes, 'iron_sharepoint/routes'
end
