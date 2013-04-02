require 'digest/md5'

module Digest
  class MD5
    def self.file path
      self.new.update(File.read path)
    end

    def file path
      update(File.read path)
    end
  end
end
