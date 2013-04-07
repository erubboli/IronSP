require 'hike/index'

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

module IronSharePoint
  class FileStat
    attr_accessor :mtime, :size, :is_file

    def file?
      self.is_file
    end

    def directory?
      !self.is_file
    end
  end

  module Hives
    class DirectoryHive
      def file_stat path
        path = get_full_path path
        return File::Stat.new path
      end
    end

    class HiveComposite
      def file_stat path
        handler = find_handler path
        return nil if handler.nil?
        handler.file_stat path
      end
    end

    class SPDocumentHive
      def file_stat path
        path = get_partial_path path
        if file_exists path
          listItem = get_sp_list_item path
          return nil if listItem.nil?

          stat = IronSharePoint::FileStat.new
          stat.mtime = listItem["Modified"]
          stat.size = listItem["File_x0020_Size"].to_i
          stat.is_file = true
          return stat
        else
          return nil
        end
      end
    end
  end
end
