require 'sprockets'
require 'singleton'

require 'iron_sharepoint/patches/hike/index'
require 'iron_sharepoint/patches/sprockets/processed_asset'
require 'iron_sharepoint/patches/digest/md5'

module IronSP
  attr_accessor :assets

  class Assets
    include Singleton

    class << self
      def inherited base
        raise "You cannot have more than one IronSharePoint::Assets" if IronSP.assets
        super
        IronSP.assets = base.instance
      end
    end
  end
end
