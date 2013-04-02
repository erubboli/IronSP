require 'sprockets/processed_asset'

module Sprockets
  class ProcessedAsset
    def source
      (@source || '').to_s
    end
  end
end
