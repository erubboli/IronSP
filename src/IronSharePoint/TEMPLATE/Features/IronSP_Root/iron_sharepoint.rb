IronSP = IronSharePoint

module IronSP
  def self.env
    @env ||= ActiveSupport::StringInquirer.new(IronConstant.IronEnv.to_s.downcase)
  end

  autoload :Mixins, 'iron_sharepoint/mixins/all'
  autoload :Assets, 'iron_sharepoint/assets'
  autoload :Routes, 'iron_sharepoint/routes'
  autoload :Variation, 'iron_sharepoint/variation'
end

require 'active_support/core_ext'
require 'iron_sharepoint/ext/all'
require 'iron_sharepoint/patches/core'
require 'iron_sharepoint/loggers'
require 'iron_sharepoint/version'

require 'iron_sharepoint/iron_view'
require 'iron_sharepoint/iron_control'
require 'iron_sharepoint/iron_composite_control'
