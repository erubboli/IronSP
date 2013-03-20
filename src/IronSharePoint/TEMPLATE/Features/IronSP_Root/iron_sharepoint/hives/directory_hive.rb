module IronSharePoint::Hives
  class DirectoryHive
    def file_stat path
      path = get_full_path path
      return File::Stat.new path
    end
  end
end

