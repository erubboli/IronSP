require 'hike/index'
require 'iron_sharepoint/file_stat'

module Hike
  class Index
    def stat path
      path = trim(path)
      $RUNTIME.hive.file_stat path
    end

    def trim path
      $RUNTIME.platform_adaptation_layer.trim_path(path.to_s).to_s
    end
  end
end
