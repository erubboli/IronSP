module IronSharePoint::Hives
  class HiveComposite
    def file_stat path
      handler = find_handler path
      return nil if handler.nil?
      handler.file_stat path
    end
  end
end

