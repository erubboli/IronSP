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
end    