require 'iron_sharepoint/file_stat'

module IronSharePoint::Hives
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

  class DirectoryHive
    def file_stat path
	  path = get_full_path path
	  return File::Stat.new path
	end
  end

  class SystemHive
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
end
	  